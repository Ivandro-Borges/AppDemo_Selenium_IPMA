namespace AppDemo_Selenium_IPMA.View
{
    public class MeteorologiaView
    {

        public string PedirDistrito()
        {
            while (true)
            {
                Console.Write("Introduza o distrito (ex: Lisboa): ");
                string? input = Console.ReadLine()?.Trim();

                if (!string.IsNullOrWhiteSpace(input))
                    return input;

                Console.WriteLine("[ERRO] O distrito não pode estar vazio.");
            }
        }

        public string PedirCidade()
        {
            while (true)
            {
                Console.Write("Introduza a cidade (ex: Sintra): ");
                string? input = Console.ReadLine()?.Trim();

                if (!string.IsNullOrWhiteSpace(input))
                    return input;

                Console.WriteLine("[ERRO] A cidade não pode estar vazia.");
            }
        }

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