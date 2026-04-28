using AppDemo_Selenium_IPMA.Model;

namespace AppDemo_Selenium_IPMA.Service
{
    /// <summary>
    /// Abstração do serviço que obtém dados meteorológicos do IPMA. Permite que
    /// o Controller dependa apenas do contrato e não da implementação concreta
    /// baseada em Selenium, reduzindo o acoplamento e facilitando a substituição
    /// por mocks ou implementações alternativas em testes.
    /// </summary>
    public interface IServicoIPMA
    {
        DadosMeteorologicos ObterDados(string distrito, string cidade);
    }
}
