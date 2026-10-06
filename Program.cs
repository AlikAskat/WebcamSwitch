using System.Drawing;
using System.Drawing.Drawing2D;
using System.Management;
using System.Diagnostics;

namespace WebcamSwitch;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new WebcamTrayContext());
    }
}

internal sealed class WebcamTrayContext : ApplicationContext
{
    private readonly NotifyIcon trayIcon;
    private readonly ContextMenuStrip menu;

    private bool webcamEnabled;
    private string language = "en";

    private const string TaskName = "WebcamSwitch";

    private readonly string webcamId =
        @"USB\VID_3277&PID_0096&MI_00\7&EAD33BB&0&0000";

    private string LanguageFile =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WebcamSwitch",
            "language.txt");

    public WebcamTrayContext()
    {
        Directory.CreateDirectory(
            Path.GetDirectoryName(LanguageFile)!);

        LoadLanguage();

        menu = new ContextMenuStrip();

        trayIcon = new NotifyIcon
        {
            Icon = CreateCameraIcon(Color.LimeGreen),
            Visible = true,
            ContextMenuStrip = menu
        };

        trayIcon.MouseClick += TrayIcon_MouseClick;

        BuildMenu();
        UpdateState();
    }

    private void BuildMenu()
    {
        menu.Items.Clear();

        var toggle = new ToolStripMenuItem(
            T("Turn Webcam On / Off", "Включить / выключить камеру"));
        toggle.Click += (_, _) => ToggleWebcam();

        var settings = new ToolStripMenuItem(
            T("Webcam Settings", "Параметры камеры"));
        settings.Click += (_, _) => OpenCameraSettings();

        var languageItem = new ToolStripMenuItem(
            this.language == "en"
                ? "Language: English"
                : "Язык: Русский");

        languageItem.Click += (_, _) =>
        {
            this.language = this.language == "en" ? "ru" : "en";
            SaveLanguage();
            BuildMenu();
            UpdateState();
        };

        var donatePictureBox = new PictureBox
        {
            Size = new Size(32, 32),
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = LoadDonateImage(),
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            Location = new Point(72, 3)
        };

        var aboutPictureBox = new PictureBox
        {
            Size = new Size(32, 32),
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = CreateAboutImage(),
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            Location = new Point(126, 3)
        };

        var servicePanel = new Panel
        {
            Width = 230,
            Height = 38,
            BackColor = Color.Transparent
        };

        servicePanel.Controls.Add(donatePictureBox);
        servicePanel.Controls.Add(aboutPictureBox);

        donatePictureBox.Click += (_, _) =>
            OpenUrl("https://winunbury-site.pages.dev/donate/");

        aboutPictureBox.Click += (_, _) => ShowAbout();

        var serviceHost = new ToolStripControlHost(servicePanel)
        {
            AutoSize = false,
            Size = new Size(230, 38),
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        var exit = new ToolStripMenuItem(
            T("Exit", "Выход"));
        exit.Click += (_, _) => Exit();

        menu.Items.Add(toggle);
        menu.Items.Add(settings);
        menu.Items.Add(languageItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(serviceHost);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exit);
    }

    private Image LoadDonateImage()
    {
        var assembly = typeof(WebcamTrayContext).Assembly;

        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name =>
                name.EndsWith("Donate.ico", StringComparison.OrdinalIgnoreCase));

        if (resourceName == null)
            throw new InvalidOperationException("Donate.ico resource not found.");

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException("Donate.ico resource stream not found.");

        using var icon = new Icon(stream);
        return icon.ToBitmap();
    }

    private Image CreateAboutImage()
    {
        const int size = 64;
        var bitmap = new Bitmap(size, size);

        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.Transparent);

            using var brush = new SolidBrush(Color.FromArgb(90, 90, 90));
            graphics.FillEllipse(brush, 3, 3, 58, 58);

            using var pen = new Pen(Color.White, 5)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            graphics.FillEllipse(
                Brushes.White,
                30,
                12,
                4,
                4);

            graphics.DrawLine(
                pen,
                32,
                23,
                32,
                46);
        }

        return bitmap;
    }
    private void TrayIcon_MouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            ToggleWebcam();
    }

    private void ToggleWebcam()
    {
        try
        {
            using var process = new Process();

            process.StartInfo = new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = $"/Run /TN \"{TaskName}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            process.Start();
            process.WaitForExit();

            Thread.Sleep(1500);
            UpdateState();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                T("WebcamSwitch error", "Ошибка WebcamSwitch"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void UpdateState()
    {
        webcamEnabled = IsWebcamEnabled();

        trayIcon.Icon = CreateCameraIcon(
            webcamEnabled ? Color.LimeGreen : Color.Red);

        trayIcon.Text = webcamEnabled
            ? T("WebcamSwitch - ON", "WebcamSwitch - ВКЛ")
            : T("WebcamSwitch - OFF", "WebcamSwitch - ВЫКЛ");
    }

    private bool IsWebcamEnabled()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                $"SELECT Status FROM Win32_PnPEntity WHERE DeviceID='{webcamId.Replace(@"\", @"\\")}'");

            foreach (ManagementObject device in searcher.Get())
            {
                string? status = device["Status"]?.ToString();

                return string.Equals(
                    status,
                    "OK",
                    StringComparison.OrdinalIgnoreCase);
            }
        }
        catch
        {
        }

        return false;
    }

    private static void OpenCameraSettings()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ms-settings:camera",
                UseShellExecute = true
            });
        }
        catch
        {
        }
    }
    private void ShowAbout()
    {
        using var form = new Form
        {
            Text = language == "en" ? "About WebcamSwitch" : "О программе",
            StartPosition = FormStartPosition.CenterScreen,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            ShowInTaskbar = false,
            ClientSize = new Size(360, 220),
            Font = new Font("Segoe UI", 9F)
        };

        var title = new Label
        {
            AutoSize = false,
            Text = "WebcamSwitch",
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(20, 18),
            Size = new Size(320, 30)
        };

        var version = new Label
        {
            AutoSize = false,
            Text = language == "en" ? "Version 1.0.0" : "Версия 1.0.0",
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(20, 48),
            Size = new Size(320, 24)
        };

        var description = new Label
        {
            AutoSize = false,
            Text = language == "en"
                ? "A WinUnbury utility\n\nQuick and simple webcam control."
                : "Утилита WinUnbury\n\nБыстрое и простое управление веб-камерой.",
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(25, 76),
            Size = new Size(310, 70)
        };

        var websiteLink = new LinkLabel
        {
            AutoSize = false,
            Text = language == "en"
                ? "Open WinUnbury website"
                : "Открыть сайт WinUnbury",
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(20, 148),
            Size = new Size(320, 25)
        };

        websiteLink.LinkClicked += (_, _) =>
            OpenUrl("https://winunbury-site.pages.dev/");

        form.Controls.AddRange(
        [
            title,
            version,
            description,
            websiteLink
        ]);

        form.ShowDialog();
    }
    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
        }
    }

    private void Exit()
    {
        trayIcon.Visible = false;
        trayIcon.Dispose();
        menu.Dispose();

        Application.Exit();
    }

    private void LoadLanguage()
    {
        try
        {
            if (File.Exists(LanguageFile))
            {
                string value = File.ReadAllText(LanguageFile).Trim().ToLowerInvariant();

                if (value == "ru" || value == "en")
                    language = value;
            }
        }
        catch
        {
        }
    }

    private void SaveLanguage()
    {
        try
        {
            File.WriteAllText(LanguageFile, language);
        }
        catch
        {
        }
    }

    private string T(string english, string russian)
    {
        return language == "ru" ? russian : english;
    }

    private static Icon CreateCameraIcon(Color color)
    {
        using Bitmap bitmap = new Bitmap(32, 32);

        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            using Brush bodyBrush = new SolidBrush(color);
            using Pen outline = new Pen(Color.Black, 2.2f);

            var body = new Rectangle(3, 8, 20, 16);

            using var path = RoundedRectangle(body, 4);

            g.FillPath(bodyBrush, path);
            g.DrawPath(outline, path);

            Point[] lens =
            {
                new Point(23, 12),
                new Point(30, 9),
                new Point(30, 23),
                new Point(23, 20)
            };

            g.FillPolygon(bodyBrush, lens);
            g.DrawPolygon(outline, lens);

            using Brush lensBrush = new SolidBrush(Color.White);

            g.FillEllipse(
                lensBrush,
                new Rectangle(9, 12, 7, 7));

            using Brush centerBrush = new SolidBrush(color);

            g.FillEllipse(
                centerBrush,
                new Rectangle(11, 14, 3, 3));
        }

        IntPtr handle = bitmap.GetHicon();

        return Icon.FromHandle(handle);
    }

    private static GraphicsPath RoundedRectangle(
        Rectangle rectangle,
        int radius)
    {
        int d = radius * 2;

        var path = new GraphicsPath();

        path.AddArc(
            rectangle.X,
            rectangle.Y,
            d,
            d,
            180,
            90);

        path.AddArc(
            rectangle.Right - d,
            rectangle.Y,
            d,
            d,
            270,
            90);

        path.AddArc(
            rectangle.Right - d,
            rectangle.Bottom - d,
            d,
            d,
            0,
            90);

        path.AddArc(
            rectangle.X,
            rectangle.Bottom - d,
            d,
            d,
            90,
            90);

        path.CloseFigure();

        return path;
    }
}




