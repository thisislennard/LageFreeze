using System.Drawing;
using Forms = System.Windows.Forms;

namespace LageFreeze.Services;

/// <summary>
/// Owns the optional notification-area icon. Command events intentionally carry
/// no application state; the application coordinator decides what each action does.
/// </summary>
public sealed class SystemTrayService : IDisposable
{
    private readonly Forms.NotifyIcon _notifyIcon;
    private readonly Forms.ContextMenuStrip _menu;
    private readonly Forms.ToolStripMenuItem _freezeItem;
    private readonly Forms.ToolStripMenuItem _refreshItem;
    private readonly Forms.ToolStripMenuItem _liveItem;
    private readonly Icon _applicationIcon;
    private bool _disposed;

    public SystemTrayService(Icon? applicationIcon = null)
    {
        _applicationIcon = applicationIcon is null
            ? LoadApplicationIcon()
            : (Icon)applicationIcon.Clone();
        _menu = new Forms.ContextMenuStrip();

        var openItem = new Forms.ToolStripMenuItem("Öffnen");
        openItem.Click += (_, _) => OpenRequested?.Invoke(this, EventArgs.Empty);

        _freezeItem = new Forms.ToolStripMenuItem("Bild einfrieren");
        _freezeItem.Click += (_, _) => FreezeRequested?.Invoke(this, EventArgs.Empty);

        _refreshItem = new Forms.ToolStripMenuItem("Standbild aktualisieren");
        _refreshItem.Click += (_, _) => RefreshRequested?.Invoke(this, EventArgs.Empty);

        _liveItem = new Forms.ToolStripMenuItem("Live-Bild wiederherstellen");
        _liveItem.Click += (_, _) => LiveRequested?.Invoke(this, EventArgs.Empty);

        var settingsItem = new Forms.ToolStripMenuItem("Einstellungen");
        settingsItem.Click += (_, _) => SettingsRequested?.Invoke(this, EventArgs.Empty);

        var exitItem = new Forms.ToolStripMenuItem("Beenden");
        exitItem.Click += (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty);

        _menu.Items.Add(openItem);
        _menu.Items.Add(new Forms.ToolStripSeparator());
        _menu.Items.Add(_freezeItem);
        _menu.Items.Add(_refreshItem);
        _menu.Items.Add(_liveItem);
        _menu.Items.Add(new Forms.ToolStripSeparator());
        _menu.Items.Add(settingsItem);
        _menu.Items.Add(new Forms.ToolStripSeparator());
        _menu.Items.Add(exitItem);

        _notifyIcon = new Forms.NotifyIcon
        {
            ContextMenuStrip = _menu,
            Icon = _applicationIcon,
            Text = "LageFreeze – LIVE",
            Visible = false,
        };
        _notifyIcon.DoubleClick += (_, _) => OpenRequested?.Invoke(this, EventArgs.Empty);

        UpdateFrozenState(isFrozen: false);
    }

    public event EventHandler? OpenRequested;

    public event EventHandler? FreezeRequested;

    public event EventHandler? RefreshRequested;

    public event EventHandler? LiveRequested;

    public event EventHandler? SettingsRequested;

    public event EventHandler? ExitRequested;

    public bool IsVisible => _notifyIcon.Visible;

    public void Show()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _notifyIcon.Visible = true;
    }

    public void Hide()
    {
        if (!_disposed)
        {
            _notifyIcon.Visible = false;
        }
    }

    public void UpdateFrozenState(bool isFrozen)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _freezeItem.Enabled = !isFrozen;
        _refreshItem.Enabled = isFrozen;
        _liveItem.Enabled = isFrozen;
        _notifyIcon.Text = isFrozen
            ? "LageFreeze – EINGEFROREN"
            : "LageFreeze – LIVE";
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _menu.Dispose();
        _applicationIcon.Dispose();
        _disposed = true;
    }

    private static Icon LoadApplicationIcon()
    {
        try
        {
            var resourceUri = new Uri(
                "pack://application:,,,/LageFreeze;component/Assets/LageFreeze.ico",
                UriKind.Absolute);
            var resource = System.Windows.Application.GetResourceStream(resourceUri);
            if (resource is not null)
            {
                using var stream = resource.Stream;
                using var icon = new Icon(stream);
                return (Icon)icon.Clone();
            }
        }
        catch (IOException)
        {
            // The system icon below remains a safe fallback.
        }
        catch (ArgumentException)
        {
            // Invalid icon data must not prevent the application from starting.
        }

        return (Icon)SystemIcons.Application.Clone();
    }
}
