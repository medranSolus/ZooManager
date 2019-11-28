using System.Windows;

namespace ZooManager.Windows.TabPlaces
{
    public partial class AddAttractionWindow : Window
    {
        MainWindow parentWindow;

        public AddAttractionWindow(MainWindow parent)
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
                object attraction = parentWindow.ZooConnection.CreateModel(ModelType.Attraction);
                ZooClient.SetProperty(attraction, "Name", textBoxName.Text);
                if (!string.IsNullOrWhiteSpace(textBoxDescription.Text))
                    ZooClient.SetProperty(attraction, "Description", textBoxDescription.Text);
                ZooClient.SetProperty(attraction, "PlaceID", ZooClient.GetProperty(dataGridPlaces.SelectedItem, "ID"));
                if (!parentWindow.ZooConnection.AddModel(attraction))
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
