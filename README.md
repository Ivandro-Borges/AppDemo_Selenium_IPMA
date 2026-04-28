# AppDemo_Selenium_IPMA
Uma aplicação demonstradora que extrai os dados do site de meteorologia IPMA usando a API Selenium WebDriver com base na localização(distrito/cidade) de Portugal fornecida pelo utilizador.

## Descrição
Aplicação demonstradora desenvolvida no âmbito da unidade curricular **Laboratório de Desenvolvimento de Software** da Universidade Aberta.

A aplicação automatiza a navegação no site do [IPMA](https://www.ipma.pt) e extrai dados meteorológicos relevantes (temperatura, vento, precipitação), apresentando-os ao utilizador na consola.

## Tecnologias utilizadas
- C# / .NET 8
- Selenium WebDriver
- Padrão arquitetural MVC

## Estrutura do projeto
```
AppDemo_Selenium_IPMA/
├── Controller/
├── Model/
├── View/
└── Program.cs
```

## Equipa
| Nome | Papel |
|---|---|
| Ivandro | Líder |
| Nathalia | Verificadora |
| António | Desenvolvedor |
| Eber Felipe | Desenvolvedor |
## Como executar
1. Clonar o repositório
2. Garantir que o .NET 8 está instalado
3. Executar:
```bash
   dotnet restore
   dotnet run
```
4. Certificar-se de que o Google Chrome está instalado

## Modo debug (browser visível)
Por omissão, o `ServicoIPMA` corre o Chrome em modo *headless* (sem janela), de forma a que o utilizador final não veja o browser a abrir durante a recolha dos dados.

Para efeitos de **debug** ou demonstração — observar a navegação no site do IPMA passo a passo — basta instanciar o serviço com `headless: false` no `Program.cs`:

```csharp
// Program.cs
var view = new MeteorologiaView();
var servicoIPMA = new ServicoIPMA(headless: false); // browser visível
var controller = new MeteorologiaController(view, servicoIPMA);

controller.Executar();
```

Voltar a `new ServicoIPMA()` (ou `new ServicoIPMA(headless: true)`) para o comportamento normal.

## Estado do projeto
🚧 Em desenvolvimento