using Microsoft.AspNetCore.Components;
using MudBlazor;
using Color = MudBlazor.Color;

namespace AvaliaRBI.Shared.Functions;

public static class UtilCommands
{
    [Inject]
    private static IDialogService DialogService { get; set; }

    public static async Task ShowError(string message)
    {
        var parameters = new DialogParameters();
        parameters.Add("Color", Color.Error);

        List<string> listErrors = new()
        {
            message
        };

        parameters.Add("Errors", listErrors!.ToArray());

        await DialogService.Show<ErrorDialog>("Erro!", parameters).Result;
    }

}

public static class DialogServiceExtensions
{
    public static async Task ShowError(this IDialogService DialogService, string message)
    {
        var parameters = new DialogParameters
        {
            { "Color", Color.Error }
        };

        var errors = message.Split("\n");

        List<string> listErrors = new(errors);

        parameters.Add("Errors", listErrors!.ToArray());

        await DialogService.Show<ErrorDialog>("Erro!", parameters).Result;
    }

    public static async Task<DialogResult> ShowQuestion(this IDialogService DialogService, string title, string message, DialogParameters parameters)
    {
        var errors = message.Split("\n");

        List<string> listErrors = new(errors);

        parameters.Add("Errors", listErrors!.ToArray());
        parameters.Add("IsQuestion", true);

        return await DialogService.Show<ErrorDialog>(title, parameters).Result;
    }

    public static async Task ShowCannotDeleteByDependenciesError(this IDialogService DialogService, string entity, string prep = "esse")
    {
        var parameters = new DialogParameters
        {
            { "Color", Color.Error }
        };

        var errors = $"Não foi possivel deletar {prep} {entity}, pois ele possui dependências na aplicação";

        List<string> listErrors = new() { errors };

        parameters.Add("Errors", listErrors!.ToArray());

        await DialogService.Show<ErrorDialog>("Erro!", parameters).Result;
    }
}

