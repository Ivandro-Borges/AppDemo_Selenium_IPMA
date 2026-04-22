using AppDemo_Selenium_IPMA.Controller;
using AppDemo_Selenium_IPMA.Service;
using AppDemo_Selenium_IPMA.View;

var view = new MeteorologiaView();
var servicoIPMA = new ServicoIPMA();
var controller = new MeteorologiaController(view, servicoIPMA);

controller.Executar();
