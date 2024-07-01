using Exam.Models;
using System.Linq;
using System.Windows.Forms;

namespace Exam.Forms
{
    public partial class ListForm : Form
    {
        public ListForm()
        {
            InitializeComponent();

            var datas = LibraryDBEntities.GetContext().books
                .Join(LibraryDBEntities.GetContext().user,
                x => x.user_id,
                y => y.user_id,
                (x, y) => new { x.book_name, y.username }).ToList();

            int index = 1;

            foreach (var item in datas)
            {
                dataListBox.Items.Add($"{index++})  " + item.book_name + " - " + item.username);
            }
        }
    }
}
