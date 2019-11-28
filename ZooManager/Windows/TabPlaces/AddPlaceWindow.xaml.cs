using System.Windows;

namespace ZooManager.Windows.TabPlaces
{
    public partial class AddPlaceWindow : Window
    {
        MainWindow parentWindow;

        public AddPlaceWindow(MainWindow parent)
        {
            InitializeComponent();
            parentWindow = parent;
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            if(IsDataInputValid())
            {
                object place = parentWindow.ZooConnection.CreateModel(ModelType.Place);
                ZooClient.SetProperty(place, "Name", textBoxName.Text);
                ZooClient.SetProperty(place, "OpenTime", timePickerOpen.Value.Value.TimeOfDay);
                ZooClient.SetProperty(place, "CloseTime", timePickerClose.Value.Value.TimeOfDay);
                ZooClient.SetProperty(place, "MaintenanceCost", decimal.Parse(textBoxCost.Text));
                if (!parentWindow.ZooConnection.AddModel(place))
                {
                    MessageBox.Show("[ERROR] Cannot add new place to database, check log for detailed cause.");
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
                MessageBox.Show("[ERROR] Place's name must be specified!");
                return false;
            }
            else if (timePickerClose.Value.Value.TimeOfDay <= timePickerOpen.Value.Value.TimeOfDay)
            {
                MessageBox.Show("[ERROR] Close time cannot be equal or bigger than open time!");
                return false;
            }
            else if (!decimal.TryParse(textBoxCost.Text, out decimal val))
            {
                MessageBox.Show("[ERROR] Place's maintenace cost must be specified!");
                return false;
            }
            return true;
        }
    }
}
