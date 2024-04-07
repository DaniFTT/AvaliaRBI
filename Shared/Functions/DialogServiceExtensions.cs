using AvaliaRBI.Shared.Dialogs;
using MudBlazor;
using Color = MudBlazor.Color;

namespace AvaliaRBI.Shared.Functions;

public static class DialogServiceExtensions
{
    public static async Task ShowError(this IDialogService DialogService, string message, Exception exception = null, object obj = null)
    {
        var parameters = new DialogParameters
        {
            { "Color", Color.Error }
        };

        var errors = message.Split("\n");

        List<string> listErrors = new(errors);

        parameters.Add("Errors", listErrors!.ToArray());
        parameters.Add("Exception", exception);
        parameters.Add("Obj", obj);

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

