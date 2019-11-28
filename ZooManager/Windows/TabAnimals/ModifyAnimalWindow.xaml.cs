using System.Linq;
using System.Windows;

namespace ZooManager.Windows.TabAnimals
{
    public partial class ModifyAnimalWindow : Window
    {
        MainWindow parentWindow;
        object animal;

        public ModifyAnimalWindow(MainWindow parent, object selectedAnimal)
        {
            InitializeComponent();
            if (selectedAnimal == null)
            {
                Logger.LogWarning("Error selecting animal to modify, process terminated.", GetType(), "ModifyAnimalWindow(parent: MainWindow, selectedAnimal: null)");
                Close();
            }
            animal = selectedAnimal;
            parentWindow = parent;
            textBoxName.Text = (string)ZooClient.GetProperty(animal, "Name");
            textBoxCount.Text = ((int)ZooClient.GetProperty(animal, "Count")).ToString();
            textBoxCost.Text = ((decimal)ZooClient.GetProperty(animal, "MaintenanceCost")).ToString();
            dataGridPlaces.DataContext = parentWindow.ZooConnection.GetModelType(ModelType.Place);
            lock (parentWindow.Places)
            {
                dataGridPlaces.ItemsSource = parentWindow.Places;
                int id = (int)ZooClient.GetProperty(animal, "PlaceID");
                dataGridPlaces.SelectedItem = parentWindow.Places.FirstOrDefault(place => (int)ZooClient.GetProperty(place, "ID") == id);
            }
            dataGridFoods.DataContext = parentWindow.ZooConnection.GetModelType(ModelType.Food);
            lock (parentWindow.Foods)
            {
                dataGridFoods.ItemsSource = parentWindow.Foods;
                int id = (int)ZooClient.GetProperty(animal, "FoodID");
                dataGridFoods.SelectedItem = parentWindow.Foods.FirstOrDefault(food => (int)ZooClient.GetProperty(food, "ID") == id);
            }
            }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            if (IsDataInputValid())
            {
                ZooClient.SetProperty(animal, "Name", textBoxName.Text);
                ZooClient.SetProperty(animal, "Count", int.Parse(textBoxCount.Text));
                ZooClient.SetProperty(animal, "MaintenanceCost", decimal.Parse(textBoxCost.Text));
                ZooClient.SetProperty(animal, "PlaceID", ZooClient.GetProperty(dataGridPlaces.SelectedItem, "ID"));
                ZooClient.SetProperty(animal, "FoodID", ZooClient.GetProperty(dataGridFoods.SelectedItem, "ID"));
                if (!parentWindow.ZooConnection.ModifyModel(animal))
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
