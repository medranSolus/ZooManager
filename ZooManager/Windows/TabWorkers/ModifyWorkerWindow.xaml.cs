using System;
using System.Linq;
using System.Windows;

namespace ZooManager.Windows.TabWorkers
{
    public partial class ModifyWorkerWindow : Window
    {
        MainWindow parentWindow;
        object worker;

        public ModifyWorkerWindow(MainWindow parent, object selectedWorker)
        {
            InitializeComponent();
            if (selectedWorker == null)
            {
                Logger.LogWarning("Error selecting worker to modify, process terminated.", GetType(), "ModifyWorkerWindow(parent: MainWindow, selectedWorker: null)");
                Close();
            }
            worker = selectedWorker;
            parentWindow = parent;
            dataGridPlaces.DataContext = parentWindow.ZooConnection.GetModelType(ModelType.Place);
            lock (parentWindow.Places)
            {
                dataGridPlaces.ItemsSource = parentWindow.Places;
                int id = (int)ZooClient.GetProperty(worker, "PlaceID");
                dataGridPlaces.SelectedItem = parentWindow.Places.FirstOrDefault(place => (int)ZooClient.GetProperty(place, "ID") == id);
            }
            textBoxSurname.Text = (string)ZooClient.GetProperty(worker, "Surname");
            textBoxName.Text = (string)ZooClient.GetProperty(worker, "Name");
            textBoxAge.Text = ((int)ZooClient.GetProperty(worker, "Age")).ToString();
            textBoxSalary.Text = ((decimal)ZooClient.GetProperty(worker, "Salary")).ToString();
            datePickerStartDate.SelectedDate = (DateTime)ZooClient.GetProperty(worker, "StartDate");
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            if (IsDataInputValid())
            {
                ZooClient.SetProperty(worker, "Surname", textBoxSurname.Text);
                ZooClient.SetProperty(worker, "Name", textBoxName.Text);
                ZooClient.SetProperty(worker, "Age", int.Parse(textBoxAge.Text));
                ZooClient.SetProperty(worker, "Salary", decimal.Parse(textBoxSalary.Text));
                ZooClient.SetProperty(worker, "StartDate", datePickerStartDate.SelectedDate);
                ZooClient.SetProperty(worker, "PlaceID", ZooClient.GetProperty(dataGridPlaces.SelectedItem, "ID"));
                if (!parentWindow.ZooConnection.ModifyModel(worker))
                {
                    MessageBox.Show("[ERROR] Cannot modify worker, check log for detailed cause.");
                    DialogResult = false;
                }
                else
                    DialogResult = true;
                Close();
            }
        }

        bool IsDataInputValid()
        {
            if (string.IsNullOrWhiteSpace(textBoxName.Text))
            {
                MessageBox.Show("[ERROR] Worker's name must be specified!");
                return false;
            }
            else if (string.IsNullOrWhiteSpace(textBoxSurname.Text))
            {
                MessageBox.Show("[ERROR] Worker's surname must be specified!");
                return false;
            }
            else if (!int.TryParse(textBoxAge.Text, out int value) || value < 16)
            {
                MessageBox.Show("[ERROR] Worker's age must be specified and be at least 16!");
                return false;
            }
            else if (!decimal.TryParse(textBoxSalary.Text, out decimal val))
            {
                MessageBox.Show("[ERROR] Worker's salary must be specified!");
                return false;
            }
            else if (dataGridPlaces.SelectedItems.Count <= 0)
            {
                MessageBox.Show("[ERROR] Must assign worker to working place, select one from the list!");
                return false;
            }
            return true;
        }
    }
}
