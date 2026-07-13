using WinFormsApp1.Services;

namespace WinFormsApp1.Forms
{
    /// <summary>
    /// Login form — the application's entry point.
    /// Responsibilities:
    ///   - Collect and validate the user's name (FR-02, FR-03)
    ///   - Handle the Remember Me feature (FR-04, FR-05)
    ///   - Open the Chat window on successful login (FR-03)
    /// Design: Separation of Concerns — UI logic only; persistence delegated to UserService.
    /// </summary>
    public partial class LoginForm : Form
    {
        private readonly UserService _userService;

        public LoginForm()
        {
            InitializeComponent();
            _userService = new UserService();
            LoadSavedUsername();
        }

        /// <summary>
        /// Pre-populates the name field if a username was previously saved (Remember Me).
        /// Called on startup to satisfy FR-05.
        /// </summary>
        private void LoadSavedUsername()
        {
            string savedName = _userService.LoadUsername();
            if (!string.IsNullOrEmpty(savedName))
            {
                txtName.Text = savedName;
                chkRememberMe.Checked = true;
            }
        }

        /// <summary>
        /// Login button click handler.
        /// Validates name is not empty (NFR-07), persists if Remember Me checked,
        /// then opens the Chat window.
        /// </summary>
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtName.Text.Trim();

            // Validation: name field must not be empty (FR-03, NFR-07)
            if (string.IsNullOrEmpty(username))
            {
                lblError.Text = "Name cannot be empty. Please enter your name.";
                txtName.Focus();
                return;
            }

            lblError.Text = "";

            // Save or clear username based on Remember Me checkbox (FR-04, FR-05)
            if (chkRememberMe.Checked)
                _userService.SaveUsername(username);
            else
                _userService.ClearUsername();

            // Open Chat window and close Login window (FR-03)
            var chatForm = new ChatForm(username);
            chatForm.Show();
            this.Hide();

            // Close the entire application when the chat window is closed
            chatForm.FormClosed += (s, args) => this.Close();
        }
    }
}
