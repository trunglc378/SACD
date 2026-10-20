using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SACD.Desktop;

/// <summary>Local file selection with Vietnamese controls, independent of Windows display language.</summary>
public sealed class VietnameseFileDialog : Window
{
    public bool SaveMode { get; init; }
    public bool ConfirmOverwrite { get; init; } = true;
    public string Filter { get; init; } = "";
    public string FileName { get; set; } = "";
    private string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    private readonly TextBox address = new(), name = new();
    private readonly ListBox entries = new();
    private readonly TextBlock feedback = new() { TextWrapping = TextWrapping.Wrap };
    private sealed record Entry(string Path, bool IsDirectory)
    {
        public string Label => (IsDirectory ? "Thư mục · " : "Tệp · ") + System.IO.Path.GetFileName(Path);
    }

    public VietnameseFileDialog()
    {
        Width = 760; Height = 540; MinWidth = 620; MinHeight = 430;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var grid = new Grid { Margin = new Thickness(16) };
        foreach (var height in new[] { GridLength.Auto, GridLength.Auto, new GridLength(1, GridUnitType.Star), GridLength.Auto, GridLength.Auto, GridLength.Auto })
            grid.RowDefinitions.Add(new RowDefinition { Height = height });
        var navigation = new DockPanel { Margin = new Thickness(0, 0, 0, 10) };
        var up = Button("Lên thư mục trên", () => { var parent = Directory.GetParent(directory); if (parent != null) Browse(parent.FullName); });
        var go = Button("Đi tới", () => Browse(address.Text));
        DockPanel.SetDock(up, Dock.Right); DockPanel.SetDock(go, Dock.Right);
        navigation.Children.Add(up); navigation.Children.Add(go); navigation.Children.Add(address);
        address.VerticalContentAlignment = VerticalAlignment.Center;
        address.KeyDown += (_, e) => { if (e.Key == Key.Enter) { Browse(address.Text); e.Handled = true; } };
        grid.Children.Add(navigation);
        var drives = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        foreach (var drive in DriveInfo.GetDrives())
            drives.Children.Add(Button("Ổ " + drive.Name, () => Browse(drive.RootDirectory.FullName)));
        Grid.SetRow(drives, 1); grid.Children.Add(drives);
        entries.DisplayMemberPath = nameof(Entry.Label);
        entries.SelectionChanged += (_, _) => { if (entries.SelectedItem is Entry { IsDirectory: false } item) name.Text = Path.GetFileName(item.Path); };
        entries.MouseDoubleClick += (_, _) => OpenSelectedEntry();
        entries.KeyDown += (_, e) => { if (e.Key == Key.Enter) { OpenSelectedEntry(); e.Handled = true; } };
        Grid.SetRow(entries, 2); grid.Children.Add(entries);
        var label = new TextBlock { Margin = new Thickness(0, 10, 0, 3) };
        Grid.SetRow(label, 3); grid.Children.Add(label);
        name.Margin = new Thickness(0, 0, 0, 8);
        Grid.SetRow(name, 4); grid.Children.Add(name);
        var bottom = new DockPanel();
        var buttons = new StackPanel { Orientation = Orientation.Horizontal };
        var accept = Button("", Accept);
        var cancel = Button("Hủy", () => DialogResult = false); cancel.IsCancel = true;
        buttons.Children.Add(accept); buttons.Children.Add(cancel);
        DockPanel.SetDock(buttons, Dock.Right); bottom.Children.Add(buttons); bottom.Children.Add(feedback);
        Grid.SetRow(bottom, 5); grid.Children.Add(bottom);
        Content = grid;
        if (System.Windows.Application.Current?.MainWindow is { IsVisible: true } main && main != this) Owner = main;
        Loaded += (_, _) => {
            accept.Content = SaveMode ? "Lưu tệp" : "Chọn tệp";
            label.Text = "Tên tệp — " + Filter.Split('|')[0]; name.Text = Path.GetFileName(FileName);
            Browse(directory);
        };
    }

    private static Button Button(string text, Action action)
    {
        var button = new Button { Content = text, Padding = new Thickness(10, 6, 10, 6), Margin = new Thickness(3, 0, 0, 0) };
        button.Click += (_, _) => action(); return button;
    }

    private string[] Extensions => Filter.Split('|').ElementAtOrDefault(1)?.Split(';').Select(x => Path.GetExtension(x)).ToArray() ?? [];
    private void Browse(string path)
    {
        try {
            var folder = Path.GetFullPath(path);
            var items = Directory.GetDirectories(folder).OrderBy(x => x, StringComparer.CurrentCultureIgnoreCase).Select(x => new Entry(x, true))
                .Concat(Directory.GetFiles(folder).Where(x => Extensions.Length == 0 || Extensions.Contains(Path.GetExtension(x), StringComparer.OrdinalIgnoreCase))
                .OrderBy(x => x, StringComparer.CurrentCultureIgnoreCase).Select(x => new Entry(x, false))).ToArray();
            entries.ItemsSource = items; directory = folder; address.Text = folder; feedback.Text = "";
        } catch (Exception ex) { feedback.Text = VietnameseDisplay.Error(ex); }
    }
    private void OpenSelectedEntry()
    {
        if (entries.SelectedItem is not Entry entry) return;
        if (entry.IsDirectory) Browse(entry.Path); else { name.Text = Path.GetFileName(entry.Path); Accept(); }
    }
    private void Accept()
    {
        try {
            if (string.IsNullOrWhiteSpace(name.Text)) { feedback.Text = "Nhập tên tệp hoặc chọn một tệp trong danh sách."; return; }
            var path = Path.GetFullPath(Path.Combine(directory, name.Text.Trim()));
            if (Directory.Exists(path)) { Browse(path); return; }
            if (SaveMode && string.IsNullOrEmpty(Path.GetExtension(path)) && Extensions.Length == 1) path += Extensions[0];
            if (!Extensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase)) { feedback.Text = "Chọn đúng loại tệp: " + Filter.Split('|')[0]; return; }
            if (!SaveMode && !File.Exists(path)) { feedback.Text = "Không tìm thấy tệp đã chọn."; return; }
            if (SaveMode && !Directory.Exists(Path.GetDirectoryName(path))) { feedback.Text = "Thư mục lưu chưa tồn tại. Chọn thư mục khác."; return; }
            if (SaveMode && File.Exists(path) && ConfirmOverwrite) { feedback.Text = "Tệp đã tồn tại. Chọn tên mới để giữ bản đã lưu."; return; }
            FileName = path; DialogResult = true;
        } catch (Exception ex) { feedback.Text = VietnameseDisplay.Error(ex); }
    }
}
