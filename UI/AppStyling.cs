namespace MonthlyScheduler.UI;

public static class AppStyling
{
    // Typography
    public static readonly Font Font = new("Segoe UI", 9F, FontStyle.Regular);
    public static readonly Font FontBold = new("Segoe UI", 9F, FontStyle.Bold);
    
    // Modern, professional color scheme
    public static readonly Color Primary = Color.FromArgb(51, 122, 183);      // Bootstrap-like blue
    public static readonly Color Secondary = Color.FromArgb(108, 117, 125);   // Neutral gray
    public static readonly Color Success = Color.FromArgb(40, 167, 69);       // Green
    public static readonly Color Danger = Color.FromArgb(220, 53, 69);        // Red
    public static readonly Color Warning = Color.FromArgb(255, 193, 7);       // Yellow
    public static readonly Color Info = Color.FromArgb(23, 162, 184);         // Cyan
    public static readonly Color Light = Color.FromArgb(248, 249, 250);       // Light gray
    public static readonly Color Dark = Color.FromArgb(52, 58, 64);          // Dark gray
    
    // Background and text colors
    public static readonly Color WindowBackground = Color.White;    // Dark window background
    public static readonly Color Background = Color.White;                         // Light content background
    public static readonly Color BackgroundSecondary = Color.FromArgb(250, 250, 250); // Slightly off-white for contrast
    public static readonly Color Text = Color.Black;               // Dark text for content areas
    public static readonly Color LightText = Color.FromArgb(232, 234, 237);       // Light text for dark backgrounds
    public static readonly Color SubText = Color.FromArgb(108, 117, 125);         // Gray text for secondary info
    
    // Borders and separators
    public static readonly Color Border = Color.FromArgb(222, 226, 230);          // Light borders
}