namespace AppDemo_Selenium_IPMA.Service
{
    public sealed class LocalNaoEncontradoException : Exception
    {
        public LocalNaoEncontradoException(string message) : base(message) { }
    }
}
