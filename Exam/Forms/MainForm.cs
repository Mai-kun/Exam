using Exam.Models;
using System;
using System.Linq;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace Exam.Forms
{
    public partial class MainForm : Form
    {
        public MainForm(int role)
        {
            InitializeComponent();

            // Настройка ограничений роли
            switch (role)
            {
                case 3:
                    AddButton.Enabled = false;
                    DelButton.Enabled = false;
                    groupBox1.Visible = false;
                    break;

                default:
                    break;
            }

            // Получение названия роли
            var roleName = LibraryDBEntities.GetContext().user
                .Join(LibraryDBEntities.GetContext().role,
                x => x.role_id,
                y => y.role_id,
                (x, y) => y.role_name).ToList();

            toolStripStatusLabel.Text = "Ваша роль: " + roleName[0];

            GetData();

            var filterData = LibraryDBEntities.GetContext().user.ToList();

            filterData.Insert(0, new user
            {
                username = "Все"
            });

            FilterComboBox.DataSource = filterData;
            FilterComboBox.DisplayMember = "username";
            FilterComboBox.ValueMember = "user_id";
        }


        private void GetData()
        {
            var datas = LibraryDBEntities.GetContext().books
                .Join(LibraryDBEntities.GetContext().user,
                x => x.user_id,
                y => y.user_id,
                (x, y) => new { x.book_name, y.username }).ToList();

            dataGrid.DataSource = datas;

            dataGrid.Columns[0].HeaderText = "Название";
            dataGrid.Columns[1].HeaderText = "Пользователь";
        }

        private void UpdateData()
        {
            var datas = LibraryDBEntities.GetContext().books
                .Join(LibraryDBEntities.GetContext().user,
                x => x.user_id,
                y => y.user_id,
                (x, y) => new { x.book_name, y.username }).ToList();

            // Сортировка
            switch (SortComboBox.SelectedIndex)
            {
                case 0:
                    datas = datas.OrderBy(x => x.book_name).ToList();
                    break;
                case 1:
                    datas = datas.OrderByDescending(x => x.book_name).ToList();
                    break;
                default:
                    break;
            }

            // Фильтрация
            if (FilterComboBox.SelectedIndex != 0)
            {
                datas = datas.Where(x => x.username == (FilterComboBox.SelectedItem as user).username).ToList();
            }

            // Поиск в таблице
            if (SearchTextBox.Text != "")
            {
                datas = datas.Where(x => x.book_name.ToLower().Contains(SearchTextBox.Text.ToLower())).ToList();
            }

            dataGrid.DataSource = datas;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Owner.Close();
            System.Windows.Forms.Application.Exit();
        }

        private void ExitButton_Click(object sender, System.EventArgs e)
        {
            Owner.Show();
            Close();
        }

        private void SortComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            UpdateData();
        }

        private void FilterComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            UpdateData();
        }

        private void SearchTextBox_TextChanged(object sender, System.EventArgs e)
        {
            UpdateData();
        }

        private void AddButton_Click(object sender, System.EventArgs e)
        {
            new AddForm().ShowDialog();
            UpdateData();
        }

        private void DelButton_Click(object sender, System.EventArgs e)
        {
            // Количество выделенных строк
            var rows = dataGrid.SelectedRows;

            for (int i = 0; i < rows.Count; i++)
            {
                // Получение первой ячейки строки выделенной строки
                string name = dataGrid.SelectedRows[i].Cells[0].Value.ToString();

                var book = LibraryDBEntities.GetContext().books.FirstOrDefault(x => x.book_name == name);

                LibraryDBEntities.GetContext().books.Remove(book);
                LibraryDBEntities.GetContext().SaveChanges();
            }

            UpdateData();
        }

        private void ListButton_Click(object sender, System.EventArgs e)
        {
            new ListForm().ShowDialog();
        }

        private void ImportButton_Click(object sender, System.EventArgs e)
        {
            // Выбор пути
            OpenFileDialog open = new OpenFileDialog();
            open.Title = "Выберите файла для импорта";

            if (DialogResult.OK != open.ShowDialog())
                return;

            Excel.Application app = new Excel.Application();
            Excel.Workbook workbook = app.Workbooks.Open(open.FileName);
            // Получение 1-ого листа
            Excel.Worksheet worksheet = workbook.Sheets[1];

            try
            {
                int row = 2;

                while (true)
                {
                    int column = 1;

                    if (string.IsNullOrEmpty(app.Cells[row, column].Text))
                        break;

                    var book = new books();
                    book.book_name = app.Cells[row, column++].Text;

                    if (int.TryParse(app.Cells[row, column].Text, out int id))
                    {
                        book.user_id = id;
                    }
                    else
                    {
                        string cell = app.Cells[row, column].Text.ToString();
                        var userid = LibraryDBEntities.GetContext().user.FirstOrDefault(x => x.username == cell);
                        book.user_id = userid.user_id;
                    }

                    // Добавление и сохранение данных
                    LibraryDBEntities.GetContext().books.Add(book);
                    LibraryDBEntities.GetContext().SaveChanges();

                    row++;
                }
            }
            catch (NullReferenceException)
            {
                // Пустой
            }
            catch (Exception)
            {
                MessageBox.Show("Неверный данные или неверное заполнение файла");
            }
            finally
            {
                // Закрытие листа
                workbook.Close();
                // Завершение приложения
                app.Quit();
                // Вызов очистки
                GC.Collect();

                UpdateData();
            }
        }

        private void ExportButton_Click(object sender, System.EventArgs e)
        {
            var app = new Excel.Application();
            app.SheetsInNewWorkbook = 1;
            Excel.Workbook workbook = app.Workbooks.Add(Type.Missing);

            Excel.Worksheet worksheet = app.Worksheets.Item[1];
            worksheet.Name = "Книги";

            // Загаловки
            worksheet.Cells[1, 1] = dataGrid.Columns[0].HeaderCell.Value;
            worksheet.Cells[1, 2] = dataGrid.Columns[1].HeaderCell.Value;

            // Столбец в эксел
            for (int i = 0; i < dataGrid.ColumnCount; i++)
            {
                // Строка в эксел
                for (int j = 0; j < dataGrid.RowCount; j++)
                {
                    worksheet.Cells[j + 2, i + 1] = dataGrid.Rows[j].Cells[i].Value.ToString();
                }
            }

            app.Visible = true;
        }
    }
}
