using MudBlazor;

namespace AvaliaRBI.Shared
{
    public static class MudGridConstants
    {
        public static DataGridFilterMode FilterMode { get; set; } = DataGridFilterMode.ColumnFilterMenu;
        public static MudBlazor.ResizeMode ResizeMode { get; set; } = MudBlazor.ResizeMode.Column;
        public static MudBlazor.DataGridFilterCaseSensitivity FilterCaseSensitivity { get; set; } = MudBlazor.DataGridFilterCaseSensitivity.CaseInsensitive;
        public static bool DragDropReorderingEnabled { get; set; } = false;
        public static bool ColumnsPanelReorderingEnabled { get; set; } = false;
        public static bool Hideable { get; set; } = true;
        public static bool Filterable { get; set; } = true;
        public static bool Groupable { get; set; } = true;
    }

    public static class ThemeConstants
    {
        public static bool _isDarkMode = false;
    }
}
