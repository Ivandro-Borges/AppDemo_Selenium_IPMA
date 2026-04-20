using AppDemo_Selenium_IPMA.Controller;
using AppDemo_Selenium_IPMA.View;

MeteorologiaView view = new MeteorologiaView();
MeteorologiaController controller = new MeteorologiaController(view);

controller.DadosObtidos += view.MostrarDados;
controller.ErroOcorrido += view.MostrarErro;

controller.Executar();