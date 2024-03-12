namespace AvaliaRBI._3___Domain.Models;

public class AssessmentCriteria
{
    public string Id { get; set; }

    public string Name { get; set; }

    public CriteriaType? CriteriaType { get; set; }

    public string AspectId { get; set; }

    public CriteriaValue ValueCriteria { get; set; }

    public CriteriaValue LimitValueCriteria { get; set; }

    public double Performance { get; set; }

    public bool IsPositive { get; set; }

    public AssessmentCriteria()
    {
        CriteriaType = null;
        ValueCriteria = new CriteriaValue();
        LimitValueCriteria = new CriteriaValue();
    }

    public AssessmentCriteria(string name, CriteriaType criteriaType, string limitValue, AssessmentAspect aspect)
    {
        Name = name;
        CriteriaType = criteriaType;
        AspectId = aspect.Id;
    }

    public AssessmentCriteria(AssessmentCriteria assessmentCriteria, bool updateValue = true)
    {
        Id = assessmentCriteria.Id;
        Name = assessmentCriteria.Name;
        CriteriaType = assessmentCriteria.CriteriaType;
        AspectId = assessmentCriteria.AspectId;
        LimitValueCriteria = new CriteriaValue(assessmentCriteria.LimitValueCriteria ?? new CriteriaValue());

        if (updateValue)
        {
            ValueCriteria = new CriteriaValue(ValueCriteria ?? (assessmentCriteria.ValueCriteria ?? new CriteriaValue()));
            Performance = assessmentCriteria.Performance;
        }

        IsPositive = assessmentCriteria.IsPositive;
    }


    public string GetValue(bool withAdornement = true)
    {
        return ReturnValueOnCriteria(ValueCriteria, withAdornement);
    }

    public string GetLimitValue(bool withAdornement = true)
    {
        return ReturnValueOnCriteria(LimitValueCriteria, withAdornement);
    }

    public bool IsValidLimitValue()
    {
        switch (CriteriaType)
        {
            case _3___Domain.CriteriaType.Integer:
                return LimitValueCriteria.ValueInt > 0;
            case _3___Domain.CriteriaType.Decimal:
                return LimitValueCriteria.ValueDecimal > 0.0;
            case _3___Domain.CriteriaType.Percentage:
                return LimitValueCriteria.ValuePercentage > 0.0;
            case _3___Domain.CriteriaType.Time:
                return (!string.IsNullOrWhiteSpace(LimitValueCriteria.ValueTime) && TimeSpan.TryParse(LimitValueCriteria.ValueTime, out _));
            default:
                return true;
        }
    }

    public void UpdateNote()
    {
        switch (CriteriaType)
        {
            case _3___Domain.CriteriaType.Integer:
                {
                    var value = int.Parse(GetValue(false));
                    var limitValue = int.Parse(GetLimitValue(false));

                    Performance = IsPositive
                    ? ((double)value / limitValue) * 100
                    : ((double)(limitValue - value) / limitValue) * 100;

                    break;
                }
            case _3___Domain.CriteriaType.Decimal:
            case _3___Domain.CriteriaType.Percentage:
                {
                    var value = double.Parse(GetValue(false));
                    var limitValue = double.Parse(GetLimitValue(false));

                    Performance = IsPositive
                    ? ((double)value / limitValue) * 100
                    : ((double)(limitValue - value) / limitValue) * 100;

                    break;
                }
            case _3___Domain.CriteriaType.Time:
                {
                    var value = TimeSpan.Parse(GetValue(false));
                    var limitValue = TimeSpan.Parse(GetLimitValue(false));

                    Performance = IsPositive
                        ? (value.TotalSeconds / limitValue.TotalSeconds) * 100
                        : ((limitValue.TotalSeconds - value.TotalSeconds) / limitValue.TotalSeconds) * 100;

                    break;
                }
            default:
                break;
        }

        if (Performance > 100)
            Performance = 100;

        if (Performance < 0)
            Performance = 0;
    }


    private string ReturnValueOnCriteria(CriteriaValue value, bool withAdornement = true)
    {
        switch (CriteriaType)
        {
            case _3___Domain.CriteriaType.Integer:
                return value.ValueInt == null ? "0" : value.ValueInt.ToString();
            case _3___Domain.CriteriaType.Decimal:
                return value.ValueDecimal == null ? "0.00" : value.ValueDecimal.Value.ToString("F2");
            case _3___Domain.CriteriaType.Percentage:
                return (value.ValuePercentage == null ? "0.00" : value.ValuePercentage.Value.ToString("F2")) + (withAdornement ? "%" : string.Empty);
            case _3___Domain.CriteriaType.Time:
                return (string.IsNullOrEmpty(value.ValueTime) ? "00:00" : (DateTime.TryParse(value.ValueTime, out var parsedTime) ? parsedTime.ToString("HH:mm") : value.ValueTime));
            default:
                return string.Empty;
        }
    }
}
public class CriteriaValue
{
    private int? _valueInt;
    public int? ValueInt
    {
        get => _valueInt;
        set => _valueInt = value ?? 0;
    }

    private double? _valueDecimal;
    public double? ValueDecimal
    {
        get => _valueDecimal;
        set => _valueDecimal = value ?? 0.0;
    }

    private double? _valuePercentage;
    public double? ValuePercentage
    {
        get => _valuePercentage;
        set => _valuePercentage = value ?? 0.0;
    }

    private string _valueTime;
    public string ValueTime
    {
        get => _valueTime;
        set => _valueTime = (string.IsNullOrEmpty(value) ? "00:00" : (DateTime.TryParseExact(value, "HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedTime) ? parsedTime.ToString("HH:mm") : value));
    }

    public CriteriaValue(CriteriaValue criteria)
    {
        ValueInt = criteria.ValueInt;
        ValueDecimal = criteria.ValueDecimal;
        ValuePercentage = criteria.ValuePercentage;
        ValueTime = (string.IsNullOrEmpty(criteria.ValueTime) ? "00:00" : (DateTime.TryParse(criteria.ValueTime, out var parsedTime) ? parsedTime.ToString("HH:mm") : criteria.ValueTime));
    }

    public CriteriaValue()
    {
        ValueTime = "00:00";
    }
}
