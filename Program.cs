using AppDemo_Selenium_IPMA.Controller;
using AppDemo_Selenium_IPMA.View;

MeteorologiaView view = new MeteorologiaView();
MeteorologiaController controller = new MeteorologiaController(view);

controller.Executar();
