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

        public MeteorologiaController()
        {
            _view = new MeteorologiaView();
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

                // Selecionar distrito "Porto"
                var selectDistrict = new SelectElement(driver.FindElement(By.Id("district")));
                selectDistrict.SelectByValue("Porto");

                // Aguarda atualização 2ª dropdown até obter valor da localidade pretendida
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

                var selectLocation = wait.Until(driver =>
                {
                    var select = new SelectElement(driver.FindElement(By.Id("locations")));

                    if (select.Options.Any(opt => opt.GetAttribute("Value") == "1131100"))
                        return select;

                    return null;

                });

                var oldHeader = driver.FindElement(By.CssSelector(".local-header")).Text;

                // Selecionar localidade "Penafiel"
                selectLocation.SelectByValue("1131100");

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

                // Obter CIDADE
                var headerText = driver.FindElement(By.CssSelector(".local-header")).Text;
                string city = headerText.Split(',').Last().Trim();
                city = string.IsNullOrWhiteSpace(city) ? "Indisponível" : city;


                // Obter elemento referente ao dia atual
                var activeWeekElement = driver.FindElement(By.CssSelector(".weekly-column.active"));

                // Obter DATA
                var date = activeWeekElement.FindElement(By.ClassName("date")).Text;
                date = string.IsNullOrWhiteSpace(date) ? "Indisponível" : date;


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

                // Fechar o browser
                driver.Quit();

                DadosMeteorologicos dados = new DadosMeteorologicos()
                {
                    Cidade = city,
                    Temperatura = temps,
                    Vento = wind,
                    Precipitacao = rain
                };

                _view.MostrarDados(dados);


            }
            catch (Exception ex)
            {
                _view.MostrarErro(ex.Message);
            }
            finally { driver?.Quit(); }
        }
    }
}