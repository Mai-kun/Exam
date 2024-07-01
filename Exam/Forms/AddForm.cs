using Exam.Models;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Exam.Forms
{
    public partial class AddForm : Form
    {
        public AddForm()
        {
            InitializeComponent();

            var users = LibraryDBEntities.GetContext().user.ToList();

            UsersComboBox.DataSource = users;
            UsersComboBox.DisplayMember = "username";
            UsersComboBox.ValueMember = "user_id";
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            if (NameTextBox.Text == "")
                return;

            LibraryDBEntities.GetContext().books.Add(new books()
            {
                book_name = NameTextBox.Text,
                user_id = (UsersComboBox.SelectedItem as user).user_id,
            });

            LibraryDBEntities.GetContext().SaveChanges();

            Close();
        }
    }
}
