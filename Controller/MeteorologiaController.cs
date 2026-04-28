using AppDemo_Selenium_IPMA.Service;
using AppDemo_Selenium_IPMA.View;
using OpenQA.Selenium;

namespace AppDemo_Selenium_IPMA.Controller
{
    public class MeteorologiaController
    {
        private readonly MeteorologiaView _view;
        private readonly IServicoIPMA _servicoIPMA;

        public MeteorologiaController(MeteorologiaView view, IServicoIPMA servicoIPMA)
        {
            _view = view;
            _servicoIPMA = servicoIPMA;
        }

        public void Executar()
        {
            // Captura dos inputs acontece antes do try para que eventuais problemas
            // de entrada (input vazio, p.ex.) não sejam mascarados como erros de
            // scraping do IPMA dentro do catch Exception genérico.
            string distrito = _view.PedirDistrito();
            string cidade = _view.PedirCidade();

            try
            {
                var dados = _servicoIPMA.ObterDados(distrito, cidade);
                _view.MostrarDados(dados);
            }
            catch (LocalNaoEncontradoException ex)
            {
                // Exceção de domínio lançada pelo ServicoIPMA quando distrito ou
                // cidade não existem no site do IPMA — mensagem pronta para o utilizador.
                _view.MostrarErro(ex.Message);
            }
            catch (WebDriverTimeoutException)
            {
                // Timeout na atualização dinâmica dos dropdowns do IPMA (rede lenta
                // ou o site demora a carregar a lista de localidades).
                _view.MostrarErro("Não foi possível carregar as cidades para o distrito indicado.");
            }
            catch (Exception)
            {
                // Rede de segurança: qualquer outra falha inesperada é traduzida numa
                // mensagem genérica, evitando expor stack traces ao utilizador.
                _view.MostrarErro("Ocorreu um erro ao obter os dados meteorológicos.");
            }
        }
    }
}
