namespace AppDemo_Selenium_IPMA.View
{
    public class MeteorologiaView
    {
        public void MostrarDados(Model.DadosMeteorologicos dados)
        {
            Console.WriteLine("=== Dados Meteorológicos IPMA ===");
            Console.WriteLine($"Cidade: {dados.Cidade}");
            Console.WriteLine($"Temperatura: {dados.Temperatura}");
            Console.WriteLine($"Vento: {dados.Vento}");
            Console.WriteLine($"Precipitação: {dados.Precipitacao}");
            Console.WriteLine("=================================");
        }

        public void MostrarErro(string mensagem)
        {
            Console.WriteLine($"[ERRO] {mensagem}");
        }
    }
}