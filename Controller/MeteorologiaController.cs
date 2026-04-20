using AppDemo_Selenium_IPMA.Model;
using AppDemo_Selenium_IPMA.View;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;

namespace AppDemo_Selenium_IPMA.Controller
{
    public class MeteorologiaController
    {
        private readonly MeteorologiaView _view;
        public event Action<DadosMeteorologicos>? DadosObtidos;
        public event Action<string>? ErroOcorrido;

        public MeteorologiaController(MeteorologiaView view)
        {
            _view = view;
        }


        public void Executar()
        {
            // Iniciar o browser
            IWebDriver driver = new ChromeDriver();

            try
            {
                // Abrir site IPMA
                driver.Navigate().GoToUrl("https://www.ipma.pt");

                // Clicar em "Previsão Localidade"
                driver.FindElement(By.ClassName("ic_target")).Click();

                // Selecionar distrito (input do utilizador)
                var selectDistrict = new SelectElement(driver.FindElement(By.Id("district")));
                string distritoInput = _view.PedirDistrito();

                var distritoOption = selectDistrict.Options
                    .FirstOrDefault(opt => opt.Text.Trim()
                    .Equals(distritoInput, StringComparison.OrdinalIgnoreCase));

                if (distritoOption is null)
                {
                    ErroOcorrido?.Invoke("Distrito não encontrado.");
                    return;
                }

                selectDistrict.SelectByText(distritoOption.Text);

                // Aguarda atualização 2ª dropdown até obter valor da localidade pretendida
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

                var selectLocation = wait.Until(d =>
                {
                    var select = new SelectElement(d.FindElement(By.Id("locations")));

                    return select.Options.Any(opt =>
                        !string.IsNullOrWhiteSpace(opt.GetAttribute("value")))
                        ? select
                        : null;
                });

                // Pedir cidade ao utilizador
                string cidadeInput = _view.PedirCidade();

                var cidadeOption = selectLocation.Options
                    .FirstOrDefault(opt =>
                        !string.IsNullOrWhiteSpace(opt.GetAttribute("value")) &&
                        opt.Text.Trim().Equals(cidadeInput, StringComparison.OrdinalIgnoreCase));

                if (cidadeOption is null)
                {
                    ErroOcorrido?.Invoke("Cidade não encontrada para o distrito indicado.");
                    return;
                }

                var oldHeader = driver.FindElement(By.CssSelector(".local-header")).Text;

                selectLocation.SelectByText(cidadeOption.Text);

                // Aguarda atualização do header com a localidade selecionada
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

                // Obter CIDADE (cidade selecionada e validada)
                string city = cidadeOption.Text.Trim(); city = string.IsNullOrWhiteSpace(city) ? "Indisponível" : city;


                // Obter elemento referente ao dia atual
                var activeWeekElement = driver.FindElement(By.CssSelector(".weekly-column.active"));

                // Obter TEMPERATURAS
                var tempMin = activeWeekElement.FindElement(By.ClassName("tempMin")).Text;
                tempMin = string.IsNullOrWhiteSpace(tempMin) ? "Indisponível" : tempMin;

                var tempMax = activeWeekElement.FindElement(By.ClassName("tempMax")).Text;
                tempMax = string.IsNullOrWhiteSpace(tempMax) ? "Indisponível" : tempMax;

                string temps = $"Mínima {tempMin} - Máxima {tempMax}";

                // Obter VENTO
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

                // Obter PRECIPITAÇÃO  
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

                DadosMeteorologicos dados = new DadosMeteorologicos()
                {
                    Cidade = city,
                    Temperatura = temps,
                    Vento = wind,
                    Precipitacao = rain
                };

                DadosObtidos?.Invoke(dados);

            }
            catch (WebDriverTimeoutException)
            {
                ErroOcorrido?.Invoke("Não foi possível carregar as cidades para o distrito indicado.");
            }
            catch (Exception)
            {
                ErroOcorrido?.Invoke("Ocorreu um erro ao obter os dados meteorológicos.");
            }
            finally
            {
                driver?.Quit(); // Fechar o browser
            }
        }
    }
}