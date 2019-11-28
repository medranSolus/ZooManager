using System.Windows;

namespace ZooManager.Windows.TabCash
{
    public partial class AddOperationWindow : Window
    {
        MainWindow parentWindow;

        public AddOperationWindow(MainWindow parent)
        {
            InitializeComponent();
            parentWindow = parent;
            dataGridTypes.DataContext = parentWindow.ZooConnection.GetModelType(ModelType.BalanceType);
            lock (parentWindow.BalanceTypes)
                dataGridTypes.ItemsSource = parentWindow.BalanceTypes;
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            if(IsDataInputValid())
            {
                object operation = parentWindow.ZooConnection.CreateModel(ModelType.CashBalance);
                ZooClient.SetProperty(operation, "SubmitDate", datePickerSubmit.SelectedDate);
                ZooClient.SetProperty(operation, "Money", decimal.Parse(textBoxMoney.Text));
                ZooClient.SetProperty(operation, "BalanceTypeID", ZooClient.GetProperty(dataGridTypes.SelectedItem, "ID"));
                if (string.IsNullOrWhiteSpace(textBoxDescription.Text))
                    ZooClient.SetProperty(operation, "DetailedDescription", textBoxDescription.Text);
                if (!parentWindow.ZooConnection.AddModel(operation))
                {
                    MessageBox.Show("[ERROR] Cannot add new finance operation to database, check log for detailed cause.");
                    DialogResult = false;
                }
                else
                    DialogResult = true;
                Close();
            }
        }

        bool IsDataInputValid()
        {
            if (!decimal.TryParse(textBoxMoney.Text, out decimal val))
            {
                MessageBox.Show("[ERROR] Operation's money must be specified!");
                return false;
            }
            else if (dataGridTypes.SelectedItems.Count <= 0)
            {
                MessageBox.Show("[ERROR] Must specify operation type first, select one from the list!");
                return false;
            }
            return true;
        }
    }
}
