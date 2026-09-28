namespace AnaliseImagemIA;

internal class TrafegoResult
{
    public TrafegoStatus Status { get; set; }
    public int NumeroCarros { get; set; }
    public int NumeroCaminhoes { get; set; }

    public enum TrafegoStatus { Limpo, Fluindo, Congestionado, Bloqueado };
}
