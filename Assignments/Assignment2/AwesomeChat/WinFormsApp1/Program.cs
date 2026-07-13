using WinFormsApp1.Forms;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("AwesomeChat.Tests")]

namespace WinFormsApp1
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new LoginForm());
        }
    }
}
