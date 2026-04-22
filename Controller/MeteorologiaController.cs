using AppDemo_Selenium_IPMA.Service;
using AppDemo_Selenium_IPMA.View;
using OpenQA.Selenium;

namespace AppDemo_Selenium_IPMA.Controller
{
    public class MeteorologiaController
    {
        private readonly MeteorologiaView _view;
        private readonly ServicoIPMA _servicoIPMA;

        public MeteorologiaController(MeteorologiaView view, ServicoIPMA servicoIPMA)
        {
            _view = view;
            _servicoIPMA = servicoIPMA;
        }

        public void Executar()
        {
            string distrito = _view.PedirDistrito();
            string cidade = _view.PedirCidade();

            try
            {
                var dados = _servicoIPMA.ObterDados(distrito, cidade);
                _view.MostrarDados(dados);
            }
            catch (LocalNaoEncontradoException ex)
            {
                _view.MostrarErro(ex.Message);
            }
            catch (WebDriverTimeoutException)
            {
                _view.MostrarErro("Não foi possível carregar as cidades para o distrito indicado.");
            }
            catch (Exception)
            {
                _view.MostrarErro("Ocorreu um erro ao obter os dados meteorológicos.");
            }
        }
    }
}
