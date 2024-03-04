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
                var parts = line.Split(new[] { " - " }, 4, StringSplitOptions.None);
                if (parts.Length == 4 && DateTime.TryParse(parts[0], out var dateTime))
                {
                    var notification = new Notification { DateTime = dateTime, Message = parts[1], Readed = parts[2] == "true" ? true : false, Type = (NotificationType)(int.Parse(parts[3])) };

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

    public void AddProgressNotification(Notification notification)
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

    public Notification()
    {

    }
    public override string ToString()
    {
        return $"{DateTime:yyyy-MM-dd HH:mm:ss} - {Message} - {(Readed ? "true" : "false")} - {(int)Type}";
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
}