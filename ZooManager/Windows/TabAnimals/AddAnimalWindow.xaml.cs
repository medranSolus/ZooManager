using System.Windows;

namespace ZooManager.Windows.TabAnimals
{
    public partial class AddAnimalWindow : Window
    {
        MainWindow parentWindow;

        public AddAnimalWindow(MainWindow parent)
        {
            InitializeComponent();
            parentWindow = parent;
            dataGridPlaces.DataContext = parentWindow.ZooConnection.GetModelType(ModelType.Place);
            lock (parentWindow.Places)
                dataGridPlaces.ItemsSource = parentWindow.Places;
            dataGridFoods.DataContext = parentWindow.ZooConnection.GetModelType(ModelType.Food);
            lock (parentWindow.Foods)
                dataGridFoods.ItemsSource = parentWindow.Foods;
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            if (IsDataInputValid())
            {
                object animal = parentWindow.ZooConnection.CreateModel(ModelType.Animal);
                ZooClient.SetProperty(animal, "Name", textBoxName.Text);
                ZooClient.SetProperty(animal, "Count", int.Parse(textBoxCount.Text));
                ZooClient.SetProperty(animal, "PlaceID", ZooClient.GetProperty(dataGridPlaces.SelectedItem, "ID"));
                ZooClient.SetProperty(animal, "FoodID", ZooClient.GetProperty(dataGridFoods.SelectedItem, "ID"));
                ZooClient.SetProperty(animal, "MaintenanceCost", decimal.Parse(textBoxCost.Text));
                if (!parentWindow.ZooConnection.AddModel(animal))
                {
                    MessageBox.Show("[ERROR] Cannot add new animal to database, check log for detailed cause.");
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
                MessageBox.Show("[ERROR] Animal's name must be specified!");
                return false;
            }
            else if (!int.TryParse(textBoxCount.Text, out int value) || value < 0)
            {
                MessageBox.Show("[ERROR] Animal's count must be specified and be at least 0!");
                return false;
            }
            else if (dataGridPlaces.SelectedItems.Count <= 0)
            {
                MessageBox.Show("[ERROR] Must assign animal to place, select one from the list!");
                return false;
            }
            else if (dataGridFoods.SelectedItems.Count <= 0)
            {
                MessageBox.Show("[ERROR] Must assign food to animal, select one from the list!");
                return false;
            }
            else if (!decimal.TryParse(textBoxCost.Text, out decimal val))
            {
                MessageBox.Show("[ERROR] Animal's maintenance cost must be specified!");
                return false;
            }
            return true;
        }
    }
}
