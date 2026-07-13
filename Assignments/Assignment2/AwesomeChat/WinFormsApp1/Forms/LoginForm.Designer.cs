namespace WinFormsApp1.Forms
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblTitle = new Label();
            lblName = new Label();
            txtName = new TextBox();
            chkRememberMe = new CheckBox();
            btnLogin = new Button();
            lblError = new Label();
            SuspendLayout();

            // lblTitle — Application branding header
            lblTitle.AutoSize = false;
            lblTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblTitle.Location = new Point(0, 20);
            lblTitle.Size = new Size(400, 45);
            lblTitle.Text = "AWE Chat";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            // lblName
            lblName.AutoSize = true;
            lblName.Font = new Font("Segoe UI", 10F);
            lblName.Location = new Point(60, 90);
            lblName.Text = "Enter your name:";

            // txtName — Name input field (FR-02)
            txtName.Font = new Font("Segoe UI", 10F);
            txtName.Location = new Point(60, 114);
            txtName.Size = new Size(280, 25);
            txtName.MaxLength = 50;

            // chkRememberMe — Remember Me checkbox (FR-04, FR-05)
            chkRememberMe.AutoSize = true;
            chkRememberMe.Font = new Font("Segoe UI", 10F);
            chkRememberMe.Location = new Point(60, 152);
            chkRememberMe.Text = "Remember Me";

            // btnLogin — Login button with validation (FR-03)
            btnLogin.Font = new Font("Segoe UI", 10F);
            btnLogin.Location = new Point(150, 190);
            btnLogin.Size = new Size(100, 32);
            btnLogin.Text = "Login";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click;

            // lblError — Validation error message (NFR-07)
            lblError.AutoSize = false;
            lblError.Font = new Font("Segoe UI", 9F);
            lblError.ForeColor = Color.Red;
            lblError.Location = new Point(40, 232);
            lblError.Size = new Size(320, 20);
            lblError.Text = "";
            lblError.TextAlign = ContentAlignment.MiddleCenter;

            // LoginForm
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(400, 265);
            Controls.AddRange(new Control[] { lblTitle, lblName, txtName, chkRememberMe, btnLogin, lblError });
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "AWE Chat - Login";
            AcceptButton = btnLogin;

            ResumeLayout(false);
            PerformLayout();
        }

        private Label lblTitle;
        private Label lblName;
        private TextBox txtName;
        private CheckBox chkRememberMe;
        private Button btnLogin;
        private Label lblError;
    }
}
