using System.Windows;

namespace ZooManager.Windows.TabAnimals
{
    public partial class ModifyFoodWindow : Window
    {
        MainWindow parentWindow;
        object food;

        public ModifyFoodWindow(MainWindow parent, object selectedFood)
        {
            InitializeComponent();
            if (selectedFood == null)
            {
                Logger.LogWarning("Error selecting food to modify, process terminated.", GetType(), "ModifyFoodWindow(parent: MainWindow, selectedFood: null)");
                Close();
            }
            food = selectedFood;
            parentWindow = parent;
            textBoxName.Text = (string)ZooClient.GetProperty(food, "Name");
            textBoxAmount.Text = ((double)ZooClient.GetProperty(food, "Amount")).ToString();
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            if (IsDataInputValid())
            {
                ZooClient.SetProperty(food, "Name", textBoxName.Text);
                ZooClient.SetProperty(food, "Amount", double.Parse(textBoxAmount.Text));
                if (!parentWindow.ZooConnection.ModifyModel(food))
                {
                    MessageBox.Show("[ERROR] Cannot add new food to database, check log for detailed cause.");
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
                MessageBox.Show("[ERROR] Food's name must be specified!");
                return false;
            }
            else if (!double.TryParse(textBoxAmount.Text, out double value) || value < 0)
            {
                MessageBox.Show("[ERROR] Food's amouont must be specified and be at least 0!");
                return false;
            }
            return true;
        }
    }
}
