using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace PanelApp;

public sealed class MainForm : Form
{
    private readonly WebView2 webView = new();
    private readonly ComboBox companiesBox = new();
    private readonly ToolStripStatusLabel statusText = new("آماده");
    private readonly List<CompanyProfile> companies = [];
    private readonly string dataDirectory;
    private readonly string settingsPath;

    public MainForm()
    {
        Text = "Panel";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(900, 620);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(7, 16, 27);
        Font = new Font("Tahoma", 9);
        KeyPreview = true;

        dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Panel");
        settingsPath = Path.Combine(dataDirectory, "companies.json");
        Directory.CreateDirectory(dataDirectory);

        var top = BuildToolbar();
        var status = new StatusStrip { SizingGrip = false, BackColor = Color.FromArgb(8, 19, 31), ForeColor = Color.White };
        status.Items.Add(statusText);

        webView.Dock = DockStyle.Fill;
        webView.DefaultBackgroundColor = Color.FromArgb(7, 16, 27);

        Controls.Add(webView);
        Controls.Add(status);
        Controls.Add(top);

        Load += async (_, _) => await InitializeAsync();
        KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.F5) { webView.Reload(); e.Handled = true; }
            else if (e.Alt && e.KeyCode == Keys.Left) { if (webView.CanGoBack) webView.GoBack(); e.Handled = true; }
            else if (e.KeyCode == Keys.F11) { ToggleFullscreen(); e.Handled = true; }
        };
    }

    private Panel BuildToolbar()
    {
        var panel = new Panel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(8, 7, 8, 6), BackColor = Color.FromArgb(8, 19, 31) };
        var title = new Label { Text = "Panel", AutoSize = false, Width = 90, Dock = DockStyle.Left, ForeColor = Color.FromArgb(231, 203, 121), Font = new Font("Segoe UI", 14, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
        var actions = new FlowLayoutPanel { Dock = DockStyle.Right, Width = 440, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, BackColor = Color.Transparent };

        companiesBox.Width = 190;
        companiesBox.Height = 32;
        companiesBox.DropDownStyle = ComboBoxStyle.DropDownList;
        companiesBox.SelectedIndexChanged += (_, _) => NavigateSelectedCompany();

        actions.Controls.Add(MakeButton("تازه‌سازی", (_, _) => webView.Reload()));
        actions.Controls.Add(MakeButton("بازگشت", (_, _) => { if (webView.CanGoBack) webView.GoBack(); }));
        actions.Controls.Add(MakeButton("ویرایش", (_, _) => EditCompany()));
        actions.Controls.Add(MakeButton("+ شرکت", (_, _) => AddCompany()));
        actions.Controls.Add(companiesBox);
        panel.Controls.Add(actions);
        panel.Controls.Add(title);
        return panel;
    }

    private static Button MakeButton(string text, EventHandler click)
    {
        var button = new Button { Text = text, AutoSize = true, Height = 32, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(16, 34, 56), ForeColor = Color.White, Margin = new Padding(4, 0, 0, 0) };
        button.FlatAppearance.BorderColor = Color.FromArgb(45, 74, 102);
        button.Click += click;
        return button;
    }

    private async Task InitializeAsync()
    {
        try
        {
            LoadCompanies();
            RefreshCompanyList();

            var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: Path.Combine(dataDirectory, "WebView2"));
            await webView.EnsureCoreWebView2Async(environment);

            webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
            webView.CoreWebView2.Settings.IsZoomControlEnabled = true;
            webView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = true;

            webView.CoreWebView2.NewWindowRequested += (sender, e) =>
            {
                e.Handled = true;
                if (Uri.TryCreate(e.Uri, UriKind.Absolute, out var targetUri))
                    webView.CoreWebView2.Navigate(targetUri.AbsoluteUri);
            };
            webView.CoreWebView2.NavigationStarting += (_, _) => statusText.Text = "در حال اتصال…";
            webView.CoreWebView2.NavigationCompleted += (_, e) => statusText.Text = e.IsSuccess ? "متصل" : "اتصال برقرار نشد";
            NavigateSelectedCompany();
        }
        catch (Exception ex)
        {
            MessageBox.Show("اجرای Panel ممکن نشد. Microsoft Edge WebView2 Runtime را نصب یا به‌روزرسانی کنید.\n\n" + ex.Message, "Panel", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadCompanies()
    {
        companies.Clear();
        try
        {
            if (File.Exists(settingsPath))
            {
                var loaded = JsonSerializer.Deserialize<List<CompanyProfile>>(File.ReadAllText(settingsPath));
                if (loaded is { Count: > 0 })
                    companies.AddRange(loaded.Where(x => !string.IsNullOrWhiteSpace(x.Name) && !string.IsNullOrWhiteSpace(x.Url)));
            }
        }
        catch { }

        if (companies.Count == 0)
        {
            companies.Add(new CompanyProfile { Name = "گلچین", Url = "http://100.79.217.1:8790/" });
            SaveCompanies();
        }
    }

    private void SaveCompanies()
    {
        Directory.CreateDirectory(dataDirectory);
        File.WriteAllText(settingsPath, JsonSerializer.Serialize(companies, new JsonSerializerOptions { WriteIndented = true }));
    }

    private void RefreshCompanyList(int selectedIndex = 0)
    {
        companiesBox.DataSource = null;
        companiesBox.DisplayMember = nameof(CompanyProfile.Name);
        companiesBox.DataSource = companies.ToList();
        if (companies.Count > 0) companiesBox.SelectedIndex = Math.Clamp(selectedIndex, 0, companies.Count - 1);
    }

    private void NavigateSelectedCompany()
    {
        if (webView.CoreWebView2 is null || companiesBox.SelectedItem is not CompanyProfile company) return;
        if (Uri.TryCreate(company.Url, UriKind.Absolute, out var uri)) webView.CoreWebView2.Navigate(uri.AbsoluteUri);
    }

    private void AddCompany()
    {
        using var dialog = new CompanyDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        companies.Add(new CompanyProfile { Name = dialog.CompanyName, Url = dialog.CompanyUrl });
        SaveCompanies();
        RefreshCompanyList(companies.Count - 1);
    }

    private void EditCompany()
    {
        if (companiesBox.SelectedIndex < 0 || companiesBox.SelectedItem is not CompanyProfile current) return;
        var index = companiesBox.SelectedIndex;
        using var dialog = new CompanyDialog(current.Name, current.Url);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        current.Name = dialog.CompanyName;
        current.Url = dialog.CompanyUrl;
        SaveCompanies();
        RefreshCompanyList(index);
    }

    private void ToggleFullscreen()
    {
        FormBorderStyle = FormBorderStyle == FormBorderStyle.None ? FormBorderStyle.Sizable : FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
    }
}
