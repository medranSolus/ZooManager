using System.Windows;

namespace ZooManager.Windows.TabWorkers
{
    public partial class AddWorkerWindow : Window
    {
        MainWindow parentWindow;

        public AddWorkerWindow(MainWindow parent)
        {
            InitializeComponent();
            parentWindow = parent;
            dataGridPlaces.DataContext = parentWindow.ZooConnection.GetModelType(ModelType.Place);
            lock (parentWindow.Places)
                dataGridPlaces.ItemsSource = parentWindow.Places;
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            if (IsDataInputValid())
            {
                object worker = parentWindow.ZooConnection.CreateModel(ModelType.Worker);
                ZooClient.SetProperty(worker, "Surname", textBoxSurname.Text);
                ZooClient.SetProperty(worker, "Name", textBoxName.Text);
                ZooClient.SetProperty(worker, "Age", int.Parse(textBoxAge.Text));
                ZooClient.SetProperty(worker, "Salary", decimal.Parse(textBoxSalary.Text));
                ZooClient.SetProperty(worker, "StartDate", datePickerStartDate.SelectedDate);
                ZooClient.SetProperty(worker, "PlaceID", ZooClient.GetProperty(dataGridPlaces.SelectedItem, "ID"));
                if (!parentWindow.ZooConnection.AddModel(worker))
                {
                    MessageBox.Show("[ERROR] Cannot add new worker to database, check log for detailed cause.");
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
