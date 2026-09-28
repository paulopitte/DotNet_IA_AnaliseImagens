using AnaliseImagemIA;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// ==========================
// Configuração
// ==========================
IConfigurationRoot config =
    new ConfigurationBuilder()
        .AddUserSecrets<Program>()
        .Build();

var credential = new ApiKeyCredential(
    config["PAT_TOKEN"]
    ?? throw new InvalidOperationException("Falta a definição do token: PAT_TOKEN")
);

var options = new OpenAIClientOptions
{
    Endpoint = new Uri("https://models.github.ai/inference")
};

IChatClient client =
    new OpenAIClient(credential, options)
        .GetChatClient("openai/gpt-4.1")
        .AsIChatClient();

// ==========================
// UI inicial
// ==========================
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("======================================");
Console.WriteLine("  CENTRAL DE MONITORAMENTO DE TRÁFEGO ");
Console.WriteLine("======================================");
Console.ResetColor();

Console.WriteLine("\nSistema de visão computacional iniciado.");
Console.WriteLine("📡 Conectando ao agente multimodal...");
Console.WriteLine("\nPressione qualquer tecla para iniciar a análise...");
Console.ReadKey();

Console.WriteLine("\n🧠 Processando imagens...\n");

// ==========================
// Processamento
// ==========================
var resultados = new List<TrafegoResult>();

foreach (var caminhoImagem in Directory.GetFiles("Imagens", "*.jpg"))
{
    var nomeCamera = Path.GetFileNameWithoutExtension(caminhoImagem);

    var mensagem = new ChatMessage(ChatRole.User, $$"""
        Extraia informações desta imagem da câmera {{nomeCamera}}.

        Responda SOMENTE com um objeto JSON neste formato:
        {
            "Status": string, // "Limpo", "Fluindo", "Congestionado", "Bloqueado"
            "NumeroCarros": number,
            "NumeroCaminhoes": number
        }
        """);

    mensagem.Contents.Add(
        new DataContent(File.ReadAllBytes(caminhoImagem), "image/jpg"));

    var response =
        await client.GetResponseAsync<TrafegoResult>([mensagem]);

    if (response.TryGetResult(out var result))
    {
        resultados.Add(result);
        ExibirResultado(nomeCamera, result);
    }

    await Task.Delay(600); // efeito visual/didático
}

// ==========================
// Resumo final
// ==========================
Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine("\n📊 RESUMO GERAL");
Console.WriteLine("--------------------------------------");
Console.WriteLine($"Câmeras analisadas: {resultados.Count}");
Console.WriteLine($"🟢 Limpo: {resultados.Count(r => r.Status == TrafegoResult.TrafegoStatus.Limpo)}");
Console.WriteLine($"🔵 Fluindo: {resultados.Count(r => r.Status == TrafegoResult.TrafegoStatus.Fluindo)}");
Console.WriteLine($"🟡 Congestionado: {resultados.Count(r => r.Status == TrafegoResult.TrafegoStatus.Congestionado)}");
Console.WriteLine($"🔴 Bloqueado: {resultados.Count(r => r.Status == TrafegoResult.TrafegoStatus.Bloqueado)}");
Console.ResetColor();

Console.WriteLine("\nAnálise concluída. Pressione qualquer tecla para encerrar.");
Console.ReadKey();

// ==========================
// Função auxiliar
// ==========================
static void ExibirResultado(string camera, TrafegoResult r)
{
    ConsoleColor cor = r.Status switch
    {
        TrafegoResult.TrafegoStatus.Limpo => ConsoleColor.Green,
        TrafegoResult.TrafegoStatus.Fluindo => ConsoleColor.Cyan,
        TrafegoResult.TrafegoStatus.Congestionado => ConsoleColor.Yellow,
        TrafegoResult.TrafegoStatus.Bloqueado => ConsoleColor.Red,
        _ => ConsoleColor.White
    };

    Console.ForegroundColor = cor;

    Console.WriteLine(
        $"{("Câmera nº " + camera).PadRight(14)} | " +
        $"{r.Status.ToString().PadRight(15)} | " +
        $"🚗 {r.NumeroCarros.ToString().PadLeft(3)} | " +
        $"🚚 {r.NumeroCaminhoes.ToString().PadLeft(3)}"
    );

    Console.ResetColor();
}
