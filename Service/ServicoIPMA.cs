using AppDemo_Selenium_IPMA.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;

namespace AppDemo_Selenium_IPMA.Service
{
    /// <summary>
    /// Encapsula a obtenção de dados meteorológicos a partir do site do IPMA
    /// via Selenium WebDriver. Mantém o Controller independente dos detalhes
    /// de scraping, reforçando o baixo acoplamento entre componentes.
    /// </summary>
    public class ServicoIPMA
    {
        private const string IpmaUrl = "https://www.ipma.pt";
        private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(5);

        public DadosMeteorologicos ObterDados(string distrito, string cidade)
        {
            IWebDriver driver = new ChromeDriver(CriarOpcoes());
            try
            {
                driver.Navigate().GoToUrl(IpmaUrl);
                driver.FindElement(By.ClassName("ic_target")).Click();

                var selectDistrict = new SelectElement(driver.FindElement(By.Id("district")));
                var distritoOption = selectDistrict.Options
                    .FirstOrDefault(opt => opt.Text.Trim()
                        .Equals(distrito, StringComparison.OrdinalIgnoreCase))
                    ?? throw new LocalNaoEncontradoException("Distrito não encontrado.");

                selectDistrict.SelectByText(distritoOption.Text);

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
                        opt.Text.Trim().Equals(cidade, StringComparison.OrdinalIgnoreCase))
                    ?? throw new LocalNaoEncontradoException(
                        "Cidade não encontrada para o distrito indicado.");

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
                driver.Quit();
            }
        }

        private static ChromeOptions CriarOpcoes()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless=new");
            options.AddArgument("--disable-gpu");
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
