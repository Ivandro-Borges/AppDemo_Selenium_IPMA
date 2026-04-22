namespace AppDemo_Selenium_IPMA.Service
{
    /// <summary>
    /// Exceção de domínio sinalizando que o distrito ou a cidade introduzidos
    /// pelo utilizador não existem nos dropdowns do site do IPMA. Preferida
    /// a retornar null para que o Controller possa traduzir a falha em
    /// mensagem ao utilizador usando o catch pattern idiomático do .NET.
    /// </summary>
    public sealed class LocalNaoEncontradoException : Exception
    {
        public LocalNaoEncontradoException(string message) : base(message) { }
    }
}
