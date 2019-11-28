using System.Windows;

namespace ZooManager.Windows.TabAnimals
{
    public partial class AddFoodWindow : Window
    {
        MainWindow parentWindow;

        public AddFoodWindow(MainWindow parent)
        {
            InitializeComponent();
            parentWindow = parent;
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            if(IsDataInputValid())
            {
                object food = parentWindow.ZooConnection.CreateModel(ModelType.Food);
                ZooClient.SetProperty(food, "Name", textBoxName.Text);
                ZooClient.SetProperty(food, "Amount", double.Parse(textBoxAmount.Text));
                if (!parentWindow.ZooConnection.AddModel(food))
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
