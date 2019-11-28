using System;
using System.Windows;

namespace ZooManager.Windows.TabWorkers
{
    public partial class SubmitOvertimeWindow : Window
    {
        MainWindow parentWindow;
        int workerID;

        public SubmitOvertimeWindow(MainWindow parent, int id)
        {
            InitializeComponent();
            parentWindow = parent;
            workerID = id;
            datePickerDate.DisplayDateStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            datePickerDate.SelectedDate = null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (IsDataInputValid())
            {
                object overtime = parentWindow.ZooConnection.CreateModel(ModelType.Overtime);
                ZooClient.SetProperty(overtime, "Date", datePickerDate.SelectedDate);
                ZooClient.SetProperty(overtime, "Hours", int.Parse(textBoxHours.Text));
                ZooClient.SetProperty(overtime, "WorkerID", workerID);
                if (!parentWindow.ZooConnection.AddModel(overtime))
                {
                    MessageBox.Show("[ERROR] Cannot submit new overtime to database, check log for detailed cause.");
                    DialogResult = false;
                }
                else
                    DialogResult = true;
                Close();
            }
        }

        bool IsDataInputValid()
        {
            if (!int.TryParse(textBoxHours.Text, out int value))
            {
                MessageBox.Show(this, "[ERROR] Overtime hours must be specified!");
                return false;
            }
            else if (datePickerDate.SelectedDate == null)
            {
                MessageBox.Show(this, "[ERROR] Overtime date must be specified!");
                return false;
            }
            return true;
        }
    }
}
