using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace BluetoothDock;

enum TrayVisual
{
    Disconnected,
    Connected,
    Busy
}

static class TrayIcons
{
    public static Icon Disconnected { get; } = Create(TrayVisual.Disconnected);
    public static Icon Connected { get; } = Create(TrayVisual.Connected);
    public static Icon Busy { get; } = Create(TrayVisual.Busy);

    private static Icon Create(TrayVisual visual)
    {
        const int size = 32;
        using var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(Color.Transparent);

            Color fill;
            Color glyph;
            Color? ring = null;
            bool drawDot = false;

            switch (visual)
            {
                case TrayVisual.Connected:
                    fill = Color.FromArgb(255, 0, 120, 215);
                    glyph = Color.White;
                    break;
                case TrayVisual.Busy:
                    fill = Color.FromArgb(70, 0, 120, 215);
                    glyph = Color.FromArgb(230, 0, 120, 215);
                    ring = Color.FromArgb(255, 0, 120, 215);
                    drawDot = true;
                    break;
                default:
                    fill = Color.FromArgb(255, 112, 112, 112);
                    glyph = Color.White;
                    break;
            }

            var circle = new RectangleF(1.5f, 1.5f, size - 4f, size - 4f);
            using (var brush = new SolidBrush(fill))
                g.FillEllipse(brush, circle);

            if (ring is Color ringColor)
            {
                using var pen = new Pen(ringColor, 2.4f);
                g.DrawEllipse(pen, circle);
            }

            DrawHeadphones(g, new RectangleF(7f, 7.5f, 18f, 17f), glyph);

            if (drawDot)
            {
                using var dotBrush = new SolidBrush(Color.FromArgb(255, 255, 185, 0));
                g.FillEllipse(dotBrush, size - 12f, size - 12f, 8f, 8f);
                using var dotPen = new Pen(Color.FromArgb(255, 255, 255, 255), 1.2f);
                g.DrawEllipse(dotPen, size - 12f, size - 12f, 8f, 8f);
            }
        }

        return BitmapToIcon(bmp);
    }

    private static void DrawHeadphones(Graphics g, RectangleF bounds, Color color)
    {
        float thickness = Math.Max(2.2f, bounds.Width / 8.5f);
        using var pen = new Pen(color, thickness)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };

        var band = new RectangleF(
            bounds.X + bounds.Width * 0.08f,
            bounds.Y,
            bounds.Width * 0.84f,
            bounds.Height * 0.95f);
        g.DrawArc(pen, band, 200f, 140f);

        float cupW = bounds.Width * 0.28f;
        float cupH = bounds.Height * 0.42f;
        float cupY = bounds.Y + bounds.Height * 0.48f;

        using var brush = new SolidBrush(color);
        g.FillEllipse(brush, bounds.X, cupY, cupW, cupH);
        g.FillEllipse(brush, bounds.Right - cupW, cupY, cupW, cupH);
    }

    private static Icon BitmapToIcon(Bitmap bmp)
    {
        IntPtr handle = bmp.GetHicon();
        try
        {
            using var temp = Icon.FromHandle(handle);
            return (Icon)temp.Clone();
        }
        finally
        {
            NativeUser32.DestroyIcon(handle);
        }
    }
}
