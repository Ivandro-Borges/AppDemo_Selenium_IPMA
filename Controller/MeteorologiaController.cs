using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using AppDemo_Selenium_IPMA.Model;
using AppDemo_Selenium_IPMA.View;

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
            try
            {
                // Iniciar o browser
                IWebDriver driver = new ChromeDriver();
                driver.Navigate().GoToUrl("https://www.ipma.pt");

                // Fechar o browser
                driver.Quit();
            }
            catch (Exception ex)
            {
                _view.MostrarErro(ex.Message);
            }
        }
    }
}