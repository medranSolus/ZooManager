using System.Windows;

namespace ZooManager.Windows.TabPlaces
{
    public partial class ModifyPlaceWindow : Window
    {
        MainWindow parentWindow;
        object place;

        public ModifyPlaceWindow(MainWindow parent, object selectedPlace)
        {
            InitializeComponent();
            if (selectedPlace == null)
            {
                Logger.LogWarning("Error selecting place to modify, process terminated.", GetType(), "ModifyPlaceWindow(parent: MainWindow, selectedPlace: null)");
                Close();
            }
            place = selectedPlace;
            parentWindow = parent;
            textBoxName.Text = (string)ZooClient.GetProperty(place, "Name");
            timePickerOpen.Value = System.DateTime.Now.Date + ((System.TimeSpan)ZooClient.GetProperty(place, "OpenTime"));
            timePickerClose.Value = System.DateTime.Now.Date + ((System.TimeSpan)ZooClient.GetProperty(place, "CloseTime"));
            textBoxCost.Text = ((decimal)ZooClient.GetProperty(place, "MaintenanceCost")).ToString();
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            if (IsDataInputValid())
            {
                object place = parentWindow.ZooConnection.CreateModel(ModelType.Place);
                ZooClient.SetProperty(place, "Name", textBoxName.Text);
                ZooClient.SetProperty(place, "OpenTime", timePickerOpen.Value.Value.TimeOfDay);
                ZooClient.SetProperty(place, "CloseTime", timePickerClose.Value.Value.TimeOfDay);
                ZooClient.SetProperty(place, "MaintenanceCost", decimal.Parse(textBoxCost.Text));
                if (!parentWindow.ZooConnection.ModifyModel(place))
                {
                    MessageBox.Show("[ERROR] Cannot modify place, check log for detailed cause.");
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
