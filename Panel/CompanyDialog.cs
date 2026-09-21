namespace PanelApp;

public sealed class CompanyDialog : Form
{
    private readonly TextBox nameBox = new();
    private readonly TextBox urlBox = new();

    public string CompanyName => nameBox.Text.Trim();
    public string CompanyUrl => urlBox.Text.Trim();

    public CompanyDialog(string? name = null, string? url = null)
    {
        Text = string.IsNullOrWhiteSpace(name) ? "افزودن شرکت" : "ویرایش شرکت";
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(500, 220);
        BackColor = Color.FromArgb(8, 19, 31);
        ForeColor = Color.White;
        Font = new Font("Tahoma", 10);

        var nameLabel = new Label { Text = "نام شرکت", AutoSize = true, Location = new Point(390, 26) };
        nameBox.SetBounds(30, 50, 440, 34);
        nameBox.Text = name ?? "";

        var urlLabel = new Label { Text = "آدرس داشبورد", AutoSize = true, Location = new Point(360, 98) };
        urlBox.SetBounds(30, 122, 440, 34);
        urlBox.RightToLeft = RightToLeft.No;
        urlBox.Text = url ?? "";

        var save = new Button { Text = "ذخیره", DialogResult = DialogResult.None, BackColor = Color.FromArgb(231, 203, 121), ForeColor = Color.Black };
        save.SetBounds(365, 172, 105, 34);
        save.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(CompanyName))
            {
                MessageBox.Show("نام شرکت را وارد کنید.", "Panel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Uri.TryCreate(CompanyUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                MessageBox.Show("آدرس داشبورد معتبر نیست.", "Panel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        };

        var cancel = new Button { Text = "انصراف", DialogResult = DialogResult.Cancel };
        cancel.SetBounds(250, 172, 105, 34);

        Controls.AddRange([nameLabel, nameBox, urlLabel, urlBox, save, cancel]);
        AcceptButton = save;
        CancelButton = cancel;
    }
}
