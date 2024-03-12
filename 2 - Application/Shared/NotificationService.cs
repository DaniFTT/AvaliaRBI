using MudBlazor;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Text.Json.Serialization;
using static AvaliaRBI._2___Application.Shared.Notification;

namespace AvaliaRBI._2___Application.Shared;


public class NotificationsService
{
    private readonly string _filePath;
    public List<Notification> Notifications { get; set; } = new List<Notification>();

    public int UnreadedNotifications { get => Notifications.Count(c => !c.Readed); }

    public NotificationsService()
    {
        try
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "AvaliaRBI");

            if (!Directory.Exists(appFolder))
                Directory.CreateDirectory(appFolder);

            _filePath = Path.Combine(appFolder, "Notifications.txt");

            LoadNotifications();
            RemoveOldNotifications();
        }
        catch (Exception)
        {

        }
    }

    public void LoadNotifications()
    {
        Notifications.Clear();
        if (File.Exists(_filePath))
        {
            var lines = File.ReadAllLines(_filePath);
            foreach (var line in lines)
            {
                var parts = line.Split(new[] { " --- " }, StringSplitOptions.None);
                if (parts.Length >= 4 && DateTime.TryParse(parts[0], out var dateTime))
                {
                    var notification = new Notification { DateTime = dateTime, Message = parts[1], Readed = parts[2] == "true" ? true : false, Type = (NotificationType)(int.Parse(parts[3])) };

                    if(parts.Length == 5)
                    {
                        notification.ImportModel = JsonConvert.DeserializeObject<ImportNotificationModel>(parts[4]);
                    }

                    Notifications.Add(notification);
                }
                
            }
        }
    }

    public void UpdateNotificationsReaded()
    {
        for (int i = 0; i < Notifications.Count; i++)
        {
            Notifications[i].Readed = true;
        }
        SaveNotifications();
    }

    private void RemoveOldNotifications()
    {
        Notifications = Notifications.Where(n => n.DateTime.Date >= DateTime.Now.Date).ToList();
        SaveNotifications();
    }

    public void RemoveNotification(Notification notification)
    {
        Notifications.Remove(notification);
        if (notification.Type == NotificationType.Processing)
            return;

        SaveNotifications();
    }

    public void AddNotification(string message, NotificationType notificationType = NotificationType.Success)
    {
        var notification = new Notification(message, notificationType);
        Notifications.Insert(0, notification);
        File.AppendAllText(_filePath, notification.ToString() + Environment.NewLine);

        OnNotificationsChanged?.Invoke();
    }

    public void AddNotification(Notification notification)
    {
        Notifications.Insert(0, notification);
        OnNotificationsChanged?.Invoke();
    }

    public bool ProcessAlreadyRunning(string processId)
    {
        return Notifications.Any(n => n.Type == NotificationType.Processing && n.Processing.ProcessId == processId);
    }

    public void UpdateProgressNotification(Notification notification, double currentValue)
    {
        notification.Processing.CurrentValue = currentValue;
        OnNotificationsChanged?.Invoke();
    }

    private void SaveNotifications()
    {
        Notifications = Notifications.OrderByDescending(c => c.DateTime).ToList();
        var lines = Notifications.Where(n => n.Type != NotificationType.Processing).Select(n => n.ToString());
        File.WriteAllLines(_filePath, lines);
    }

    public event Action OnNotificationsChanged;
}

public class Notification
{
    public string Message { get; set; }
    public DateTime DateTime { get; set; }
    public bool Readed { get; set; }
    public NotificationType Type { get; set; }

    public ProcessingModel Processing { get; set; }
    public ImportNotificationModel ImportModel { get; set; }

    public Notification(string message, NotificationType type)
    {
        Message = message;
        DateTime = DateTime.Now;
        Readed = false;
        Type = type;
    }

    public Notification(string message, double total, string processId)
    {
        Message = message;
        DateTime = DateTime.Now;
        Readed = false;
        Type = NotificationType.Processing;
        Processing = new Notification.ProcessingModel
        {
            ProcessId = processId,
            TotalValue = total,
            CurrentValue = 0
        };
    }

    public Notification(ImportNotificationModel importModel)
    {
        importModel.FinishDate = DateTime.Now;

        Message = importModel.Title;
        DateTime = DateTime.Now;
        Readed = false;
        ImportModel = importModel;
        Type = NotificationType.Success;

        if (importModel.Notas.Any(c => c.Type == NotaType.Error))
            Type = NotificationType.Error;    
    }   

    public Notification()
    {
    }

    public override string ToString()
    {
        var value = $"{DateTime:yyyy-MM-dd HH:mm:ss} --- {Message} --- {(Readed ? "true" : "false")} --- {(int)Type}";

        if (ImportModel != null)
        {
            var jsonImportModel = JsonConvert.SerializeObject(ImportModel);
            value += $" --- {jsonImportModel}";
        }

        return value;
    }

    public MudBlazor.Color GetColor()
    {
        switch (Type)
        {
            case NotificationType.Success:
                return MudBlazor.Color.Success;
            case NotificationType.Error:
                return MudBlazor.Color.Error;
            case NotificationType.Warning:
                return MudBlazor.Color.Warning;
            case NotificationType.Processing:
                return MudBlazor.Color.Primary;
        }

        return MudBlazor.Color.Primary;
    }

    public string GetIcon()
    {
        if (ImportModel != null)
        {
            return "";
        }

        switch (Type)
        {
            case NotificationType.Success:
                return Icons.Material.Filled.Bookmark;
            case NotificationType.Error:
                return Icons.Material.Filled.Error;
            case NotificationType.Warning:
                return Icons.Material.Filled.Warning;
            case NotificationType.Processing:
                return Icons.Material.Filled.Downloading;
        }

        return Icons.Material.Filled.Bookmark;
    }

    public enum NotificationType
    {

        Success,
        Error,
        Warning,
        Processing,
    }

    public class ProcessingModel
    {
        public string ProcessId { get; set; }
        public double TotalValue { get; set; }
        public double CurrentValue { get; set; }

        public double GetPercentage()
        {
            return (CurrentValue / (float)TotalValue) * 100;
        }
    }

    public class ImportNotificationModel
    {
        public string FileName { get; set; }
        public int ProcessedCount { get; set; }
        public int InsertedCount { get; set; }
        public int UpdatedCount { get; set; }
        public DateTime FinishDate { get; set; }

        public string Title { get; set; }
        public List<NotaModel> Notas { get; set; } = new List<NotaModel>();

        public void AddNota(int linha, string message, NotaType type = NotaType.Error)
        {
            Notas.Add(new NotaModel(linha, message, type));
        }

        public ImportNotificationModel()
        {
            
        }

        public ImportNotificationModel(string fileName)
        {
            FileName = fileName;
        }

        public bool ContainsErrors { get => Notas.Any(n => n.Type == NotaType.Error); }
        public bool ContainsErrorsByRow(int row) => Notas.Any(n => n.Row == row.ToString() && n.Type == NotaType.Error);
        public bool ContainsWarnings{ get => Notas.Any(n => n.Type == NotaType.Warning); }
    }
    public class NotaModel
    {
        public string Row { get; set; }
        public string Message { get; set; }
        public NotaType Type { get; set; }

        public NotaModel(int linha, string message, NotaType type)
        {
            Row = linha.ToString();
            Message = message;
            Type = type;
        }

        public NotaModel()
        {
            
        }
    }
    public enum NotaType
    {
        [Description("Aviso")]
        Warning,
        [Description("Erro")]
        Error,
    }
}