using AppDemo_Selenium_IPMA.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AppDemo_Selenium_IPMA.Service
{
    /// <summary>
    /// Encapsula a obtenção de dados meteorológicos a partir do site do IPMA
    /// via Selenium WebDriver. Mantém o Controller independente dos detalhes
    /// de scraping, reforçando o baixo acoplamento entre componentes.
    /// </summary>
    public class ServicoIPMA : IServicoIPMA
    {
        private const string IpmaUrl = "https://www.ipma.pt";
        private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(5);

        // Comparador pt-PT que ignora maiúsculas/minúsculas e diacríticos
        // (IgnoreNonSpace) para que o utilizador possa escrever "agueda" e
        // ainda assim casar com "Águeda" no dropdown do IPMA. Resolve também
        // variações como "sao" vs "São" em localidades como "São João da Madeira".
        private static readonly CompareInfo PtCompare =
            CultureInfo.GetCultureInfo("pt-PT").CompareInfo;
        private const CompareOptions NomeCompareOpts =
            CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

        private readonly bool _headless;

        /// <summary>
        /// Cria o serviço IPMA. Por omissão o Chrome corre em modo headless para
        /// que o utilizador final não veja o browser a abrir (requisito de UX
        /// da equipa). Para efeitos de debug, passar <paramref name="headless"/>
        /// como <c>false</c> faz o Chrome abrir em modo visível.
        /// </summary>
        public ServicoIPMA(bool headless = true)
        {
            _headless = headless;
        }

        public DadosMeteorologicos ObterDados(string distrito, string cidade)
        {
            IWebDriver driver = new ChromeDriver(CriarOpcoes());
            try
            {
                driver.Navigate().GoToUrl(IpmaUrl);

                // "ic_target" é o ícone que encaminha para a secção de previsão
                // por localidade (entrada no fluxo de pesquisa distrito → cidade).
                driver.FindElement(By.ClassName("ic_target")).Click();

                var selectDistrict = new SelectElement(driver.FindElement(By.Id("district")));
                var distritoOption = selectDistrict.Options
                    .FirstOrDefault(opt => MesmoNome(opt.Text, distrito))
                    ?? throw new LocalNaoEncontradoException("Distrito não encontrado.");

                selectDistrict.SelectByText(distritoOption.Text);

                // O dropdown de cidades é preenchido via JavaScript após a escolha
                // do distrito, por isso é necessário aguardar até que surja pelo
                // menos uma opção com value não vazio (opções com value vazio são
                // apenas placeholders do estado inicial).
                var wait = new WebDriverWait(driver, WaitTimeout);
                var selectLocation = wait.Until(d =>
                {
                    var select = new SelectElement(d.FindElement(By.Id("locations")));
                    return select.Options.Any(opt =>
                        !string.IsNullOrWhiteSpace(opt.GetAttribute("value")))
                        ? select
                        : null;
                });

                var cidadeOption = selectLocation.Options
                    .FirstOrDefault(opt =>
                        !string.IsNullOrWhiteSpace(opt.GetAttribute("value")) &&
                        MesmoNome(opt.Text, cidade))
                    ?? throw new LocalNaoEncontradoException(
                        "Cidade não encontrada para o distrito indicado.");

                // Espera até que o cabeçalho ".local-header" reflita a nova
                // localidade selecionada, garantindo que os dados apresentados
                // correspondem efetivamente à cidade pedida pelo utilizador.
                var oldHeader = driver.FindElement(By.CssSelector(".local-header")).Text;
                selectLocation.SelectByText(cidadeOption.Text);

                wait.Until(d =>
                {
                    try
                    {
                        var newHeader = d.FindElement(By.CssSelector(".local-header")).Text;
                        return !string.IsNullOrEmpty(newHeader) && newHeader != oldHeader;
                    }
                    catch
                    {
                        return false;
                    }
                });

                return ExtrairDados(driver, cidadeOption.Text);
            }
            finally
            {
                // driver.Quit() apenas no finally: garante fecho do browser mesmo
                // quando as exceções de domínio (LocalNaoEncontradoException) ou
                // falhas do Selenium sobem para o Controller.
                driver.Quit();
            }
        }

        // Compara dois nomes de localidade ignorando capitalização e acentos,
        // recorrendo ao CompareInfo de pt-PT. Necessário porque o utilizador
        // pode escrever "agueda" sem acento, enquanto o IPMA expõe "Águeda".
        private static bool MesmoNome(string a, string b) =>
            PtCompare.Compare(a.Trim(), b.Trim(), NomeCompareOpts) == 0;

        // Configuração do Chrome. Em headless: "--headless=new" ativa o headless
        // moderno (Chrome 109+) que renderiza a página de forma equivalente ao
        // modo visível; "--disable-gpu" evita warnings em ambientes sem GPU
        // (ex.: CI); "--window-size" garante dimensões consistentes para que os
        // elementos do IPMA sejam encontrados independentemente do tamanho do ecrã.
        private ChromeOptions CriarOpcoes()
        {
            var options = new ChromeOptions();
            if (_headless)
            {
                options.AddArgument("--headless=new");
                options.AddArgument("--disable-gpu");
            }
            options.AddArgument("--window-size=1920,1080");
            return options;
        }

        private static DadosMeteorologicos ExtrairDados(IWebDriver driver, string cidadeTexto)
        {
            string city = cidadeTexto.Trim();
            city = string.IsNullOrWhiteSpace(city) ? "Indisponível" : city;

            var activeWeekElement = driver.FindElement(By.CssSelector(".weekly-column.active"));

            var tempMin = activeWeekElement.FindElement(By.ClassName("tempMin")).Text;
            tempMin = string.IsNullOrWhiteSpace(tempMin) ? "Indisponível" : tempMin;

            var tempMax = activeWeekElement.FindElement(By.ClassName("tempMax")).Text;
            tempMax = string.IsNullOrWhiteSpace(tempMax) ? "Indisponível" : tempMax;

            string temps = $"Mínima {tempMin} - Máxima {tempMax}";

            string wind = "Indisponível";
            try
            {
                var windElem = activeWeekElement.FindElement(By.ClassName("windImg"));
                wind = windElem.GetAttribute("title") ?? "Indisponível";
            }
            catch (NoSuchElementException)
            {
                wind = "Indisponível";
            }

            string rain = "Indisponível";
            try
            {
                var rainElem = activeWeekElement.FindElement(By.ClassName("precProb"));
                string rainTitle = rainElem.GetAttribute("title") ?? "";
                var match = Regex.Match(rainTitle, @"\d+%");
                rain = match.Success ? match.Value : "Indisponível";
            }
            catch (NoSuchElementException)
            {
                rain = "Indisponível";
            }

            return new DadosMeteorologicos
            {
                Cidade = city,
                Temperatura = temps,
                Vento = wind,
                Precipitacao = rain
            };
        }
    }
}
