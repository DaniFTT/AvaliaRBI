using System.ComponentModel;

namespace AvaliaRBI._3___Domain;

public enum CriteriaType
{
    [Description("Nenhum")]
    None = 0,
    [Description("Inteiro")]
    Integer = 1,
    [Description("Decimal")]
    Decimal = 2,
    [Description("Porcentagem")]
    Percentage = 3,
    [Description("Tempo")]
    Time = 4
}