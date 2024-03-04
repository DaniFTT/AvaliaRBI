using AvaliaRBI._1___Presentation.AssessmentCollection;
using AvaliaRBI._1___Presentation.DepartmentsPositions;
using AvaliaRBI._1___Presentation.Employees;
using AvaliaRBI._1___Presentation.MonthlyAssessments;
using AvaliaRBI._2___Application;
using AvaliaRBI._2___Application.Shared;
using AvaliaRBI._3___Domain;
using AvaliaRBI._3___Domain.Abstractions;
using AvaliaRBI._4___Repository;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;

namespace AvaliaRBI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Roboto-Regular.ttf", "RobotoRegular");
            });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddTransient<MudLocalizer, DictionaryMudLocalizer>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<EmployeePage>();
        builder.Services.AddSingleton<EmployeeService>();
        builder.Services.AddSingleton<IBaseRepository<Employee>, EmployeeRepository>();

        builder.Services.AddSingleton<OrganizationalStructurePage>();

        builder.Services.AddSingleton<SectorService>();
        builder.Services.AddSingleton<IBaseRepository<Sector>, SectorRepository>();
        builder.Services.AddSingleton<DepartmentService>();
        builder.Services.AddSingleton<IBaseRepository<Department>, DepartmentRepository>();
        builder.Services.AddSingleton<PositionService>();
        builder.Services.AddSingleton<IBaseRepository<PositionJob>, PositionRepository>();

        builder.Services.AddSingleton<AssessmentCollectionPage>();
        builder.Services.AddSingleton<AssessmentCollectionService>();
        builder.Services.AddSingleton<IBaseRepository<AssessmentCollection>, AssessmentCollectionRepository>();

        builder.Services.AddSingleton<MonthlyAssessmentPage>();
        builder.Services.AddSingleton<MonthlyAssessmentService>();
        builder.Services.AddSingleton<IBaseRepository<MonthlyAssessment>, MonthlyAssessmentRepository>();

        builder.Services.AddSingleton<AssessmentEmployeeService>();
        builder.Services.AddSingleton<IBaseRepository<AssessmentEmployee>, AssessmentEmployeeRepository>();

        builder.Services.AddSingleton<MonthlyAssessmentUpsertPage>();

        builder.Services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;

            config.SnackbarConfiguration.PreventDuplicates = false;
            config.SnackbarConfiguration.NewestOnTop = false;
            config.SnackbarConfiguration.ShowCloseIcon = true;
            config.SnackbarConfiguration.VisibleStateDuration = 3000;
            config.SnackbarConfiguration.HideTransitionDuration = 200;
            config.SnackbarConfiguration.ShowTransitionDuration = 200;
            config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
        });

        builder.Services.AddLogging();


        builder.Services.AddSingleton<EmailService>();
        builder.Services.AddSingleton<NotificationsService>();
        builder.Services.AddSingleton<PdfService>();
        builder.Services.AddSingleton<ExcelService>();

        return builder.Build();
    }
}

internal class DictionaryMudLocalizer : MudLocalizer
{
    private Dictionary<string, string> _localization;

    public DictionaryMudLocalizer()
    {
        _localization = new()
        {
            { "MudDataGrid.is empty", "Vazio" },
            { "MudDataGrid.is not empty", "Não vazio" },
            { "MudDataGrid.contains", "Contém" },
            { "MudDataGrid.not contains", "Não contém" },
            { "MudDataGrid.AddFilter", " Filtro" },
            { "MudDataGrid.Apply", "Aplicar" },
            { "MudDataGrid.Cancel", "Cancelar" },
            { "MudDataGrid.CollapseAllGroups", "Colapsar agrupamentos" },
            { "MudDataGrid.ExpandAllGroups", "Expandir agrupamentos" },
            { "MudDataGrid.Column", "Coluna" },
            { "MudDataGrid.Columns", "Colunas" },
            { "MudDataGrid.equals", "Igual á" },
            { "MudDataGrid.Filter", "Filtrar" },
            { "MudDataGrid.False", "Falso" },
            { "MudDataGrid.True", "Verdadeiro" },
            { "MudDataGrid.Hide", "Ocultar" },
            { "MudDataGrid.HideAll", "Ocultar Tudo" },
            { "MudDataGrid.is", "É" },
            { "MudDataGrid.is after", "Está depois" },
            { "MudDataGrid.is before", "Está antes" },
            { "MudDataGrid.is not", "Não é" },
            { "MudDataGrid.Sort", "Ordernar" },
            { "MudDataGrid.Ungroup", "Desagrupar" },
            { "MudDataGrid.Unsort", "Desordenar" },
            { "MudDataGrid.Value", "Desordenar" },
            { "MudDataGrid.starts with", "Começa com" },
            { "MudDataGrid.ends with", "Termina com" },
            { "MudDataGrid.ShowAll", "Mostrar Tudo" },
            { "MudDataGrid.Save", "Salvar" },
            { "MudDataGrid.RefreshData", "Atualizar" },
            { "MudDataGrid.Operator", "Operador" },
            { "MudDataGrid.not equals", "Não igual á" },
            { "MudDataGrid.MoveUp", "Mover acima" },
            { "MudDataGrid.MoveDown", "Mover Abaixo" },
            { "MudDataGrid.FilterValue", "Valor do Filtro" },
            { "MudDataGrid.Clear", "Limpar" },
        };
    }

    public override LocalizedString this[string key]
    {
        get
        {
            var currentCulture = Thread.CurrentThread.CurrentUICulture.Parent.TwoLetterISOLanguageName;
            if (currentCulture.Equals("pt", StringComparison.InvariantCultureIgnoreCase)
                && _localization.TryGetValue(key, out var res))
            {
                return new(key, res);
            }
            else
            {
                return new(key, key, true);
            }
        }
    }
}