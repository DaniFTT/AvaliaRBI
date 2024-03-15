using AvaliaRBI.Shared.Extensions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AvaliaRBI._2___Application.Shared.Notification;

namespace AvaliaRBI.Shared.Validation;

public class ExcelField<T> where T : class
{
    public int Col { get; set; }
    public string Header { get; set; }
    public string FieldName { get; set; }
    public string Comment { get; set; }
    public List<IValidationRule<string>> Rules { get; set; }

    private Action<T, string> _setValueAction;

    public ExcelField(int col, string header, string comment = null, string fieldName = null)
    {
        Col = col;
        Header = header;
        Comment = comment;
        FieldName = fieldName ?? header;
        Rules = new List<IValidationRule<string>>();
    }

    public ExcelField<T> WithRequiredRule()
    {
        Rules.Add(new NotEmptyValidationRule());
        Comment += " - Este campo é obrigatório para a inserção no sistema. \n";
        return this;
    }

    public ExcelField<T> WithMaxLengthRule(int maxLength)
    {
        Rules.Add(new MaxLengthValidationRule(maxLength));
        Comment += $" - Este campo deve ter no máximo {maxLength} caracteres. \n";
        return this;
    }

    public ExcelField<T> WithRGRule()
    {
        Rules.Add(new RGValidationRule());
        Comment += $" - Este campo de RG deve ser preenchido apenas com os 9 digitos númericos, ou no formatado de 12 digitos: 00.000.000.-0. \n";
        return this;
    }

    public ExcelField<T> WithNoFutureDateRule()
    {
        Rules.Add(new NotFutureDateValidationRule());
        Comment += $" - Este campo deve ser preenchido com uma data no formato dd/mm/yyyy, não podendo ser uma data futura.  \n";
        return this;
    }

    public ExcelField<T> SetAction(Action<T, string> setValueAction)
    {
        _setValueAction = setValueAction;
        return this;
    }

    public void SetValueIfValid(T targetObject, string value, int row, ImportNotificationModel importModel)
    {
        if (_setValueAction == null || !Rules.Any())
            return;

        var validationResult = value.ApplyValidationRules(this);
        if (validationResult.IsValid)
        {
            _setValueAction?.Invoke(targetObject, value);
            return;
        }

        importModel.AddNota(row, validationResult.ErrorMessage);  
    }
}

public static class ImportExcelUtil
{
    //private readonly ImportNotificationModel _importModel;
    //private readonly ExcelWorksheet _worksheet;
    //public ImportExcelUtil(ImportNotificationModel importModel, ExcelWorksheet worksheet)
    //{
    //    _importModel = importModel;
    //    _worksheet = worksheet;
    //}



    public static ValidationResult ApplyValidationRules<T>(this string value, ExcelField<T> field) where T : class
    {
        foreach (var rule in field.Rules)
        {
            var result = rule.Validate(value, field.FieldName);
            if (!result.IsValid)
            {
                return result;
            }
        }

        return new ValidationResult { IsValid = true };
    }


    //public string GetRequiredString(int row, int col, string fieldName)
    //{
    //    var result = _worksheet.Cells.GetString(row, col);
    //    if (string.IsNullOrWhiteSpace(result))
    //        _importModel.AddNota(row, $"O campo {fieldName} não pode ser vazio", NotaType.Error);

    //    return result;
    //}

    //public string GetString(int row, int col)
    //{
    //    return _worksheet.Cells.GetString(row, col);
    //}
}


public class ValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }
}

public interface IValidationRule<T>
{
    ValidationResult Validate(T value, string row);
}

public class NotEmptyValidationRule : IValidationRule<string>
{
    public ValidationResult Validate(string value, string fieldName)
    {
        return new ValidationResult
        {
            IsValid = !string.IsNullOrWhiteSpace(value),
            ErrorMessage = !string.IsNullOrWhiteSpace(value) ? "" : $"O campo {fieldName} precisa ser informado."
        };
    }
}

public class MaxLengthValidationRule : IValidationRule<string>
{
    private readonly int _maxLength;

    public MaxLengthValidationRule(int maxLength)
    {
        _maxLength = maxLength;
    }

    public ValidationResult Validate(string value, string fieldName)
    {
        return new ValidationResult
        {
            IsValid = value.Length <= _maxLength,
            ErrorMessage = value.Length <= _maxLength ? "" : $"O campo {fieldName} deve conter no máximo {_maxLength} caracteres."
        };
    }
}

public class NotFutureDateValidationRule : IValidationRule<string>
{
    public ValidationResult Validate(string value, string fieldName)
    {
        var validationResult = new ValidationResult();

        validationResult.IsValid = DateTime
            .TryParseExact(value, "dd/MM/yyyy", CultureInfo.GetCultureInfo("pt-BR"), DateTimeStyles.AssumeLocal, out var date);

        if(!validationResult.IsValid)
        {
            validationResult.ErrorMessage = $" {fieldName} está inválida. A data deve ser no formato dd/mm/yyyy";
            return validationResult;
        }

        validationResult.IsValid = date.Date <= DateTime.Now.Date;
        validationResult.ErrorMessage = validationResult.IsValid ? "" : $"A {fieldName} não pode ser maior do que hoje";

        return validationResult;
    }
}

public class RGValidationRule : IValidationRule<string>
{
    public RGValidationRule()
    {
    }

    public ValidationResult Validate(string value, string fieldName = "RG")
    {
        var rg = value.NormalizeRG();

        return new ValidationResult
        {
            IsValid = !string.IsNullOrEmpty(rg),
            ErrorMessage = !string.IsNullOrEmpty(rg) ? "" : $"{fieldName} inválido. Deve ser informado apenas 9 dígitos, ou formatado com 12 caracteres: 00.000.000-0."
        };
    }
}

public static class ValidationExtensions
{
    public static object GetCell(ExcelRange cells, int row, int col)
    {
        return cells[row, col].Text;
    }
    public static string GetString(this ExcelRange cells, int row, int col)
    {
        return GetCell(cells, row, col).ToString();
    }
    public static double ObterDecimal(this ExcelRange cells, int row, int col)
    {
        return Convert.ToDouble(GetCell(cells, row, col));
    }
    public static int ObterInteger(this ExcelRange cells, int row, int col)
    {
        return Convert.ToInt32(GetCell(cells, row, col));
    }
    public static DateTime ObterData(this ExcelRange cells, int row, int col, string format)
    {
        return DateTime.ParseExact(GetString(cells, row, col), format, CultureInfo.GetCultureInfo("pt-BR"));
    }
    public static Dictionary<int, string> ValuesOfLine(this ExcelRange cells, int row)
    {
        var valuesOfLine = new Dictionary<int, string>();

        var columnLimit = cells.Columns;
        for (int columnPositionNumber = 1; columnPositionNumber <= columnLimit; columnPositionNumber++)
        {
            var valueOfColumn = GetString(cells, row, columnPositionNumber);

            if (!string.IsNullOrEmpty(valueOfColumn))
            {
                valuesOfLine.Add(columnPositionNumber, valueOfColumn);
            }
        }

        return valuesOfLine;
    }
}