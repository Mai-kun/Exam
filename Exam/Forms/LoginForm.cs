using Exam.Models;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Exam.Forms
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void EnterButton_Click(object sender, System.EventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordTextBox.Text;

            var users = LibraryDBEntities.GetContext().user.Select(x => new { x.username, x.password, x.role_id });

            var currentUser = users.FirstOrDefault(x => x.username == login || x.password == password);

            try
            {
                if (currentUser == null)
                {
                    throw new InvalidDataException();
                }
            }
            catch (InvalidDataException)
            {
                MessageBox.Show("Неверные данных для входа");
            }

            OpenNewWindow((int)currentUser.role_id);
        }

        private void OpenNewWindow(int role)
        {
            MainForm form = new MainForm(role);
            form.Owner = this;
            form.Show();
            this.Hide();
        }
    }
}
