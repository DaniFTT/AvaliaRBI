using AvaliaRBI.Shared.Extensions;

namespace AvaliaRBI._4___Repository.Shared;

public static class DataModel
{
    private static string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"AvaliaRBI.db3");

    public static byte[] GetDbFile()
    {
        try
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"AvaliaRBI.db3");

            File.Copy(dbPath, tempPath, true);

            var bytes = File.ReadAllBytes(tempPath);

            File.Delete(tempPath);

            return bytes;
        }
        catch (Exception e)
        {
            return Array.Empty<byte>();
        }

    }

    public static void ExportData()
    {
        var filePath = "AvaliaRBI".GetFileName("db3");

        File.Copy(dbPath, filePath, true);
    }

    public static async Task ImportData()
    {
        var filePickerOptions = new PickOptions
        {
            PickerTitle = "Por favor, selecione o arquivo Excel para importar",
        };

        var result = await FilePicker.Default.PickAsync(filePickerOptions);
        if (result != null)
        {
            var fileName = Path.GetFileName(result.FullPath);
            if (!fileName.Contains("db3"))
                throw new Exception("Banco de Dados Inválido");

            if (!fileName.Equals("AvaliaRBI.db3"))
                throw new Exception("O nome do Banco de Dados deve ser 'AvaliaRBI.db3'");

            File.Copy(result.FullPath, dbPath, true);
        }

    }
}

