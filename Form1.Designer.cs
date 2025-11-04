namespace MonthlyScheduler;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        monthSelect = new ComboBox();
        yearSelect = new ComboBox();
        btnUpload = new Button();
        btnGenerateSchedule = new Button();
        scheduleGrid = new DataGridView();
        SuspendLayout();

        // Form settings
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        WindowState = FormWindowState.Maximized;
        Text = "Monthly Scheduler";

        ResumeLayout(false);
    }

    #endregion
}
