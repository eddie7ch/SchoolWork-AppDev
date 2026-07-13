namespace WinFormsApp1.Forms
{
    partial class ChatForm
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
            lblUserInfo = new Label();
            rtbHistory = new RichTextBox();
            lblMessage = new Label();
            txtMessage = new TextBox();
            btnSend = new Button();
            lblStatus = new Label();
            SuspendLayout();

            // lblUserInfo — Shows logged-in username at top
            lblUserInfo.AutoSize = true;
            lblUserInfo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblUserInfo.Location = new Point(12, 12);
            lblUserInfo.Text = "Logged in as: ";

            // rtbHistory — Read-only chat history field (FR-07, NFR requirement: non-editable)
            rtbHistory.BackColor = Color.White;
            rtbHistory.BorderStyle = BorderStyle.FixedSingle;
            rtbHistory.Font = new Font("Segoe UI", 10F);
            rtbHistory.Location = new Point(12, 38);
            rtbHistory.ReadOnly = true;
            rtbHistory.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtbHistory.Size = new Size(576, 370);
            rtbHistory.TabStop = false;

            // lblMessage
            lblMessage.AutoSize = true;
            lblMessage.Font = new Font("Segoe UI", 10F);
            lblMessage.Location = new Point(12, 422);
            lblMessage.Text = "Message:";

            // txtMessage — Message input field (FR-06)
            txtMessage.Font = new Font("Segoe UI", 10F);
            txtMessage.Location = new Point(12, 442);
            txtMessage.MaxLength = 500;
            txtMessage.Size = new Size(468, 25);
            txtMessage.KeyDown += txtMessage_KeyDown;

            // btnSend — Send button (FR-06)
            btnSend.Font = new Font("Segoe UI", 10F);
            btnSend.Location = new Point(490, 440);
            btnSend.Size = new Size(98, 29);
            btnSend.Text = "Send";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;

            // lblStatus — Connection/send status indicator
            lblStatus.AutoSize = false;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.Gray;
            lblStatus.Location = new Point(12, 478);
            lblStatus.Size = new Size(576, 18);
            lblStatus.Text = "Ready";

            // ChatForm
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(600, 504);
            Controls.AddRange(new Control[] { lblUserInfo, rtbHistory, lblMessage, txtMessage, btnSend, lblStatus });
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "ChatForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "AWE Chat";

            ResumeLayout(false);
            PerformLayout();
        }

        private Label lblUserInfo;
        private RichTextBox rtbHistory;
        private Label lblMessage;
        private TextBox txtMessage;
        private Button btnSend;
        private Label lblStatus;
    }
}
