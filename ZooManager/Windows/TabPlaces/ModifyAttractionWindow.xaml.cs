using System.Windows;
using System.Linq;

namespace ZooManager.Windows.TabPlaces
{
    public partial class ModifyAttractionWindow : Window
    {
        MainWindow parentWindow;
        object attraction;

        public ModifyAttractionWindow(MainWindow parent, object selectedAttraction)
        {
            InitializeComponent();
            if (selectedAttraction == null)
            {
                Logger.LogWarning("Error selecting attraction to modify, process terminated.", GetType(), "ModifyAttractionWindow(parent: MainWindow, selectedAttraction: null)");
                Close();
            }
            attraction = selectedAttraction;
            parentWindow = parent;
            dataGridPlaces.DataContext = parentWindow.ZooConnection.GetModelType(ModelType.Place);
            lock (parentWindow.Places)
            {
                dataGridPlaces.ItemsSource = parentWindow.Places;
                int id = (int)ZooClient.GetProperty(attraction, "PlaceID");
                dataGridPlaces.SelectedItem = parentWindow.Places.FirstOrDefault(place => (int)ZooClient.GetProperty(place, "ID") == id);
            }
            textBoxName.Text = (string)ZooClient.GetProperty(attraction, "Name");
            textBoxDescription.Text = (string)ZooClient.GetProperty(attraction, "Description");
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            if (IsDataInputValid())
            {
                ZooClient.SetProperty(attraction, "Name", textBoxName.Text);
                if (!string.IsNullOrWhiteSpace(textBoxDescription.Text))
                    ZooClient.SetProperty(attraction, "Description", textBoxDescription.Text);
                ZooClient.SetProperty(attraction, "PlaceID", ZooClient.GetProperty(dataGridPlaces.SelectedItem, "ID"));
                if (!parentWindow.ZooConnection.ModifyModel(attraction))
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
                MessageBox.Show("[ERROR] Attraction's name must be specified!");
                return false;
            }
            else if (dataGridPlaces.SelectedItems.Count <= 0)
            {
                MessageBox.Show("[ERROR] Must assign attraction to place, select one from the list!");
                return false;
            }
            return true;
        }
    }
}
