using WinFormsApp1.Services;

namespace WinFormsApp1.Forms
{
    /// <summary>
    /// Login form — collects username, handles Remember Me, opens ChatForm.
    /// Design: Separation of Concerns — UI only; persistence delegated to UserService.
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

        private void LoadSavedUsername()
        {
            string savedName = _userService.LoadUsername();
            if (!string.IsNullOrEmpty(savedName))
            {
                txtName.Text = savedName;
                chkRememberMe.Checked = true;
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtName.Text.Trim();

            if (string.IsNullOrEmpty(username))
            {
                lblError.Text = "Name cannot be empty. Please enter your name.";
                txtName.Focus();
                return;
            }

            lblError.Text = "";

            if (chkRememberMe.Checked)
                _userService.SaveUsername(username);
            else
                _userService.ClearUsername();

            var chatForm = new ChatForm(username);
            chatForm.Show();
            this.Hide();
            chatForm.FormClosed += (s, args) => this.Close();
        }
    }
}
