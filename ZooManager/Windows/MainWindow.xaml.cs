using System;
using System.Windows;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using ZooManager.Views;
using ZooManager.Windows.TabAnimals;
using ZooManager.Windows.TabPlaces;
using ZooManager.Windows.TabWorkers;
using ZooManager.Windows.TabCash;

namespace ZooManager.Windows
{
    public partial class MainWindow : Window
    {
        Thread threadUpdate;
        volatile bool update = true;
        volatile bool dataGridWorkersShouldClear = false;
        volatile bool dataGridAnimalsShouldClear = false;
        volatile bool dataGridFoodsShouldClear = false;
        volatile bool dataGridPlacesShouldClear = false;
        volatile bool dataGridAttractionsShouldClear = false;

        List<object> animals = new List<object>();
        List<object> attractions = new List<object>();
        List<object> cashBalances = new List<object>();
        List<object> overtimes = new List<object>();
        List<object> workers = new List<object>();

        public List<object> BalanceTypes { get; private set; } = new List<object>();
        public List<object> Foods { get; private set; } = new List<object>();
        public List<object> Places { get; private set; } = new List<object>();
        public ZooClient ZooConnection { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            const int port = 2560;
            try
            {
                ZooConnection = new ZooClient(port);
            }
            catch (Exception e)
            {
                Logger.LogError($"Cannot load ZooCom.dll: {e.ToString()}", GetType(), "MainWindow()");
                Application.Current.Shutdown(-1);
            }
            dataGridFoods.DataContext = ZooConnection.GetModelType(ModelType.Food);
            dataGridPlaces.DataContext = ZooConnection.GetModelType(ModelType.Place);
            threadUpdate = new Thread(() =>
            {
                TimeSpan waitTime = new TimeSpan(0, 5, 0);
                while (update)
                {
                    UpdateAnimals();
                    UpdateAttractions();
                    UpdateCashBalances();
                    UpdateFoods();
                    UpdateOvertimes();
                    UpdatePlaces();
                    UpdateWorkers();
                    Thread.Sleep(waitTime);
                }
            });
            threadUpdate.Start();
        }

        public IEnumerable<WorkerView> GetWorkerViews()
        {
            lock (Places)
            {
                return workers.Join(Places,
                    worker => ZooClient.GetProperty(worker, "PlaceID"), place => ZooClient.GetProperty(place, "ID"),
                    (worker, place) => new WorkerView()
                    {
                        ID = (int)ZooClient.GetProperty(worker, "ID"),
                        Surname = (string)ZooClient.GetProperty(worker, "Surname"),
                        Name = (string)ZooClient.GetProperty(worker, "Name"),
                        Age = (int)ZooClient.GetProperty(worker, "Age"),
                        Salary = (decimal)ZooClient.GetProperty(worker, "Salary"),
                        StartDate = (DateTime)ZooClient.GetProperty(worker, "StartDate"),
                        Place = (string)ZooClient.GetProperty(place, "Name")
                    });
            }
        }

        #region Update
        void UpdateAnimals()
        {
            new Thread(() =>
            {
                IEnumerable<AnimalView> source;
                lock (animals)
                {
                    animals = ZooConnection.GetModels(ModelType.Animal);
                    lock (Places)
                        lock (Foods)
                        {
                            source = animals.Join(Places,
                                animal => ZooClient.GetProperty(animal, "PlaceID"), place => ZooClient.GetProperty(place, "ID"),
                                (animal, place) => new
                                {
                                    ID = (int)ZooClient.GetProperty(animal, "ID"),
                                    Name = (string)ZooClient.GetProperty(animal, "Name"),
                                    Count = (int)ZooClient.GetProperty(animal, "Count"),
                                    MaintenanceCost = (decimal)ZooClient.GetProperty(animal, "MaintenanceCost"),
                                    Place = (string)ZooClient.GetProperty(place, "Name"),
                                    FoodID = (int)ZooClient.GetProperty(animal, "FoodID")
                                }).Join(Foods,
                                anon => ZooClient.GetProperty(anon, "FoodID"), food => ZooClient.GetProperty(food, "ID"),
                                (anon, food) => new AnimalView()
                                {
                                    ID = anon.ID,
                                    Name = anon.Name,
                                    Count = anon.Count,
                                    MaintenanceCost = anon.MaintenanceCost,
                                    Place = anon.Place,
                                    Food = (string)ZooClient.GetProperty(food, "Name")
                                });
                        }
                }
                dataGridAnimals.Dispatcher.Invoke(() =>
                {
                    buttonModifyAnimal.IsEnabled = false;
                    buttonRemoveAnimal.IsEnabled = false;
                    dataGridAnimals.ItemsSource = source;
                    labelAnimalSpeciesCount.Content = source.Count().ToString();
                    labelAnimalsCount.Content = source.Sum(animal => animal.Count).ToString();
                });
            }).Start();
        }

        void UpdateFoods(int? foodId = null)
        {
            new Thread(() =>
            {
                IEnumerable<object> filtered;
                lock (Foods)
                {
                    Foods = ZooConnection.GetModels(ModelType.Food);
                    if (foodId.HasValue)
                        filtered = Foods.Where(food => (int)ZooClient.GetProperty(food, "ID") == foodId.Value);
                    else
                        filtered = Foods;
                }
                dataGridFoods.Dispatcher.Invoke(() =>
                {
                    buttonModifyFood.IsEnabled = false;
                    buttonRemoveFood.IsEnabled = false;
                    dataGridFoods.ItemsSource = filtered;
                    labelFoodsCount.Content = filtered.Sum(food => (double)ZooClient.GetProperty(food, "Amount")).ToString();
                });
            }).Start();
        }

        void UpdateWorkers()
        {
            new Thread(() =>
            {
                IEnumerable<WorkerView> source;
                lock (workers)
                {
                    workers = ZooConnection.GetModels(ModelType.Worker);
                    source = GetWorkerViews();
                }
                dataGridWorkers.Dispatcher.Invoke(() =>
                {
                    buttonModifyWorker.IsEnabled = false;
                    buttonRemoveWorker.IsEnabled = false;
                    buttonSubmitOvertime.IsEnabled = false;
                    dataGridWorkers.ItemsSource = source;
                    labelWorkersCount.Content = source.Count().ToString();
                });
            }).Start();
        }

        void UpdateOvertimes(int? workerId = null)
        {
            new Thread(() =>
            {
                IEnumerable<object> filtered;
                lock (overtimes)
                {
                    overtimes = ZooConnection.GetModels(ModelType.Overtime);
                    if (workerId.HasValue)
                        filtered = overtimes.Where(overtime => (int)ZooClient.GetProperty(overtime, "WorkerID") == workerId.Value);
                    else
                        filtered = overtimes;
                    lock (workers)
                    {
                        filtered = filtered.Join(workers,
                            overtime => ZooClient.GetProperty(overtime, "WorkerID"), worker => ZooClient.GetProperty(worker, "ID"),
                            (overtime, worker) => CreateOvertimeView(overtime, worker));
                    }
                }
                dataGridOvertimes.Dispatcher.Invoke(() =>
                {
                    dataGridOvertimes.ItemsSource = filtered;
                });
            }).Start();
        }

        void UpdateCashBalances()
        {
            new Thread(() =>
            {
                IEnumerable<CashBalanceView> source;
                lock (BalanceTypes)
                {
                    BalanceTypes = ZooConnection.GetModels(ModelType.BalanceType);
                    lock (cashBalances)
                    {
                        cashBalances = ZooConnection.GetModels(ModelType.CashBalance);
                        source = cashBalances.Join(BalanceTypes,
                            balance => ZooClient.GetProperty(balance, "BalanceTypeID"), type => ZooClient.GetProperty(type, "ID"),
                            (balance, type) => new CashBalanceView()
                            {
                                ID = (int)ZooClient.GetProperty(balance, "ID"),
                                SubmitDate = (DateTime)ZooClient.GetProperty(balance, "SubmitDate"),
                                Money = (decimal)ZooClient.GetProperty(balance, "Money"),
                                Type = (string)ZooClient.GetProperty(type, "Description"),
                                DetailedDescription = (string)ZooClient.GetProperty(balance, "DetailedDescription")
                            });
                    }
                }
                dataGridCashBalances.Dispatcher.Invoke(() =>
                {
                    dataGridCashBalances.ItemsSource = source.OrderByDescending(balance => balance.SubmitDate);
                });
            }).Start();
        }

        void UpdatePlaces()
        {
            new Thread(() =>
            {
                lock (Places)
                    Places = ZooConnection.GetModels(ModelType.Place);
                dataGridPlaces.Dispatcher.Invoke(() =>
                {
                    buttonModifyPlace.IsEnabled = false;
                    buttonRemovePlace.IsEnabled = false;
                    dataGridPlaces.ItemsSource = Places;
                    labelPlacesCount.Content = Places.Count.ToString();
                });
            }).Start();
        }

        void UpdateAttractions(int? placeId = null)
        {
            new Thread(() =>
            {
                IEnumerable<AttractionView> source;
                lock (attractions)
                {
                    attractions = ZooConnection.GetModels(ModelType.Attraction);
                    IEnumerable<object> filtered;
                    if (placeId.HasValue)
                        filtered = attractions.Where(attraction => (int)ZooClient.GetProperty(attraction, "PlaceID") == placeId.Value);
                    else
                        filtered = attractions;
                    lock (Places)
                        lock (workers)
                        {
                            source = filtered.Join(Places,
                                attraction => ZooClient.GetProperty(attraction, "PlaceID"), place => ZooClient.GetProperty(place, "ID"),
                                (attraction, place) => new
                                {
                                    ID = (int)ZooClient.GetProperty(attraction, "ID"),
                                    Name = (string)ZooClient.GetProperty(attraction, "Name"),
                                    Description = (string)ZooClient.GetProperty(attraction, "Description"),
                                    AttractionManagerID = (int)ZooClient.GetProperty(attraction, "AttractionManagerID"),
                                    Place = (string)ZooClient.GetProperty(place, "Name")
                                }).Join(workers,
                                anon => ZooClient.GetProperty(anon, "AttractionManagerID"), worker => ZooClient.GetProperty(worker, "ID"),
                                (anon, worker) => new AttractionView()
                                {
                                    ID = anon.ID,
                                    Name = anon.Name,
                                    Description = anon.Description,
                                    AttractionManager = (string)ZooClient.GetProperty(worker, "Name") + " " + (string)ZooClient.GetProperty(worker, "Surname"),
                                    Place = anon.Place
                                });
                        }
                    dataGridAttractions.Dispatcher.Invoke(() =>
                    {
                        buttonModifyAttraction.IsEnabled = false;
                        buttonRemoveAttraction.IsEnabled = false;
                        dataGridAttractions.ItemsSource = source;
                        labelAttractionsCount.Content = source.Count();
                    });
                }
            }).Start();
        }
        #endregion Update

        OvertimeView CreateOvertimeView(object overtime, object worker)
        {
            return new OvertimeView()
            {
                ID = (int)ZooClient.GetProperty(overtime, "ID"),
                Date = (DateTime)ZooClient.GetProperty(overtime, "Date"),
                Hours = (int)ZooClient.GetProperty(overtime, "Hours"),
                PaymentPercentage = (int)ZooClient.GetProperty(overtime, "PaymentPercentage"),
                Employee = (string)ZooClient.GetProperty(worker, "Name") + " " + (string)ZooClient.GetProperty(worker, "Surname")
            };
        }

        #region Animals Tab
        void ButtonRefreshAnimals_Click(object sender, RoutedEventArgs e)
        {
            UpdateFoods();
            UpdatePlaces();
            UpdateAnimals();
        }

        void DataGridAnimals_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dataGridAnimalsShouldClear && dataGridAnimals.SelectedItem != null)
            {
                buttonModifyAnimal.IsEnabled = false;
                buttonRemoveAnimal.IsEnabled = false;
                dataGridAnimals.SelectedItem = null;
                DataGridFoods_MouseLeftButtonUp(sender, e);
                UpdateFoods();
            }
            else
                dataGridAnimalsShouldClear = true;
        }

        void DataGridAnimals_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dataGridAnimals.SelectedItem != null)
            {
                buttonModifyAnimal.IsEnabled = true;
                buttonRemoveAnimal.IsEnabled = true;
                dataGridAnimalsShouldClear = false;
                DataGridFoods_SelectionChanged(sender, e);
                int id = (int)ZooClient.GetProperty(dataGridAnimals.SelectedItem, "ID");
                int foodId = 0;
                lock (animals)
                    foodId = (int)ZooClient.GetProperty(animals.FirstOrDefault(animal => (int)ZooClient.GetProperty(animal, "ID") == id), "FoodID");
                UpdateFoods(foodId);
            }
        }

        void DataGridFoods_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dataGridFoodsShouldClear && dataGridFoods.SelectedItem != null)
            {
                buttonModifyFood.IsEnabled = false;
                buttonRemoveFood.IsEnabled = false;
                dataGridFoods.SelectedItem = null;
            }
            else
                dataGridFoodsShouldClear = true;
        }

        void DataGridFoods_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dataGridFoods.SelectedItem != null)
            {
                buttonModifyFood.IsEnabled = true;
                buttonRemoveFood.IsEnabled = true;
                dataGridFoodsShouldClear = false;
            }
        }

        void ButtonAddAnimal_Click(object sender, RoutedEventArgs e)
        {
            AddAnimalWindow addAnimalWindow = new AddAnimalWindow(this);
            bool? isAdded = addAnimalWindow.ShowDialog();
            if (isAdded.HasValue && isAdded.Value)
            {
                UpdateAnimals();
                UpdateFoods();
            }
        }

        void ButtonModifyAnimal_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridAnimals.SelectedItem != null)
            {
                object animal = null;
                lock (animals)
                    animal = animals.SingleOrDefault(a => (int)ZooClient.GetProperty(a, "ID") == ((AnimalView)dataGridAnimals.SelectedItem).ID);
                ModifyAnimalWindow modifyAnimalWindow = new ModifyAnimalWindow(this, animal);
                bool? isModified = modifyAnimalWindow.ShowDialog();
                if (isModified.HasValue && isModified.Value)
                {
                    UpdateAnimals();
                    UpdateFoods();
                }
            }
        }

        void ButtonRemoveAnimal_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridAnimals.SelectedItem != null)
            {
                int id = ((AnimalView)dataGridAnimals.SelectedItem).ID;
                new Thread(() =>
                {
                    Tuple<bool, byte> status = ZooConnection.DeleteModel(ModelType.Animal, id);
                    if (status == null)
                        MessageBox.Show("[ERROR] Cannot remove animal from Zoo, server connection error!");
                    else if (!status.Item1)
                        MessageBox.Show(ZooClient.DecodeDeleteError(status.Item2));
                    else
                    {
                        UpdateAnimals();
                        UpdateFoods();
                    }
                }).Start();
            }
        }

        void ButtonAddFood_Click(object sender, RoutedEventArgs e)
        {
            AddFoodWindow addFoodWindow = new AddFoodWindow(this);
            bool? isAdded = addFoodWindow.ShowDialog();
            if (isAdded.HasValue && isAdded.Value)
            {
                UpdateAnimals();
                UpdateFoods();
            }
        }

        void ButtonModifyFoodCount_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridFoods.SelectedItem != null)
            {
                object food = null;
                lock (Foods)
                    food = Foods.SingleOrDefault(f => (int)ZooClient.GetProperty(f, "ID") == (int)ZooClient.GetProperty(dataGridFoods.SelectedItem, "ID"));
                ModifyFoodWindow modifyFoodWindow = new ModifyFoodWindow(this, food);
                bool? isModified = modifyFoodWindow.ShowDialog();
                if (isModified.HasValue && isModified.Value)
                    UpdateFoods();
            }
        }

        void ButtonRemoveFood_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridFoods.SelectedItem != null)
            {
                int id = (int)ZooClient.GetProperty(dataGridFoods.SelectedItem, "ID");
                new Thread(() =>
                {
                    Tuple<bool, byte> status = ZooConnection.DeleteModel(ModelType.Food, id);
                    if (status == null)
                        MessageBox.Show("[ERROR] Cannot remove food type, server connection error!");
                    else if (!status.Item1)
                        MessageBox.Show(ZooClient.DecodeDeleteError(status.Item2));
                    else
                    {
                        UpdateAnimals();
                        UpdateFoods();
                    }
                }).Start();
            }
        }
        #endregion Animals Tab

        #region Workers Tab
        void ButtonRefreshWorkers_Click(object sender, RoutedEventArgs e)
        {
            UpdateWorkers();
            UpdateOvertimes();
        }

        void DataGridWorkers_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dataGridWorkersShouldClear && dataGridWorkers.SelectedItem != null)
            {
                buttonModifyWorker.IsEnabled = false;
                buttonRemoveWorker.IsEnabled = false;
                buttonSubmitOvertime.IsEnabled = false;
                dataGridWorkers.SelectedItem = null;
                UpdateOvertimes();
            }
            else
                dataGridWorkersShouldClear = true;
        }

        void DataGridWorkers_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dataGridWorkers.SelectedItem != null)
            {
                buttonModifyWorker.IsEnabled = true;
                buttonRemoveWorker.IsEnabled = true;
                buttonSubmitOvertime.IsEnabled = true;
                dataGridWorkersShouldClear = false;
                UpdateOvertimes(((WorkerView)dataGridWorkers.SelectedItem).ID);
            }
        }

        void ButtonAddWorker_Click(object sender, RoutedEventArgs e)
        {
            AddWorkerWindow addWorkerWindow = new AddWorkerWindow(this);
            bool? isAdded = addWorkerWindow.ShowDialog();
            if (isAdded.HasValue && isAdded.Value)
            {
                UpdateWorkers();
                UpdateOvertimes();
            }
        }

        void ButtonModifyWorker_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridWorkers.SelectedItem != null)
            {
                object worker = null;
                lock (workers)
                    worker = workers.SingleOrDefault(w => (int)ZooClient.GetProperty(w, "ID") == ((WorkerView)dataGridWorkers.SelectedItem).ID);
                ModifyWorkerWindow modifyWorkerWindow = new ModifyWorkerWindow(this, worker);
                bool? isModified = modifyWorkerWindow.ShowDialog();
                if (isModified.HasValue && isModified.Value)
                {
                    UpdateWorkers();
                    UpdateOvertimes();
                }
            }
        }

        void ButtonRemoveWorker_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridWorkers.SelectedItem != null)
            {
                int id = ((WorkerView)dataGridWorkers.SelectedItem).ID;
                new Thread(() =>
                {
                    Tuple<bool, byte> status = ZooConnection.DeleteModel(ModelType.Worker, id);
                    if (status == null)
                        MessageBox.Show("[ERROR] Cannot delete worker, server connection error!");
                    else if (!status.Item1)
                        MessageBox.Show(ZooClient.DecodeDeleteError(status.Item2));
                    else
                    {
                        UpdateOvertimes();
                        UpdateWorkers();
                    }
                }).Start();
            }
        }

        void ButtonSubmitOvertime_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridWorkers.SelectedItem != null)
            {
                int id = ((WorkerView)dataGridWorkers.SelectedItem).ID;
                SubmitOvertimeWindow submitOvertimeWindow = new SubmitOvertimeWindow(this, id);
                bool? isAdded = submitOvertimeWindow.ShowDialog();
                if (isAdded.HasValue && isAdded.Value)
                    UpdateOvertimes(id);
            }
        }
        #endregion Workers Tab

        #region Cash Tab
        void ButtonRefreshCash_Click(object sender, RoutedEventArgs e)
        {
            UpdateCashBalances();
        }

        void ButtonAddOperation_Click(object sender, RoutedEventArgs e)
        {
            AddOperationWindow addOperationWindow = new AddOperationWindow(this);
            bool? isAdded = addOperationWindow.ShowDialog();
            if (isAdded.HasValue && isAdded.Value)
                UpdateCashBalances();
        }

        void ButtonPayMonthSalary_Click(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                object operation = ZooConnection.CreateModel(ModelType.CashBalance);
                ZooClient.SetProperty(operation, "BalanceTypeID", 0);
                if (!ZooConnection.AddModel(operation))
                    MessageBox.Show("[ERROR] Cannot pay month salary to employees, cannot add new operation to database, check log for detailed cause.");
                else
                    MessageBox.Show("Succesfully pay month salary to employees.");
            }).Start();
        }
        #endregion Cash Tab

        #region Places Tab
        void ButtonRefreshPlaces_Click(object sender, RoutedEventArgs e)
        {
            UpdatePlaces();
            UpdateWorkers();
            UpdateAttractions();
        }
        void DataGridPlaces_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dataGridPlacesShouldClear && dataGridPlaces.SelectedItem != null)
            {
                buttonModifyPlace.IsEnabled = false;
                buttonRemovePlace.IsEnabled = false;
                dataGridPlaces.SelectedItem = null;
                DataGridAttractions_MouseLeftButtonUp(sender, e);
                UpdateAttractions();
            }
            else
                dataGridPlacesShouldClear = true;
        }

        void DataGridPlaces_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dataGridPlaces.SelectedItem != null)
            {
                buttonModifyPlace.IsEnabled = true;
                buttonRemovePlace.IsEnabled = true;
                dataGridPlacesShouldClear = false;
                DataGridAttractions_SelectionChanged(sender, e);
                UpdateAttractions((int)ZooClient.GetProperty(dataGridPlaces.SelectedItem, "ID"));
            }
        }

        void DataGridAttractions_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dataGridAttractionsShouldClear && dataGridAttractions.SelectedItem != null)
            {
                buttonModifyAttraction.IsEnabled = false;
                buttonRemoveAttraction.IsEnabled = false;
                dataGridAttractions.SelectedItem = null;
            }
            else
                dataGridAttractionsShouldClear = true;
        }

        void DataGridAttractions_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dataGridAttractions.SelectedItem != null)
            {
                buttonModifyAttraction.IsEnabled = true;
                buttonRemoveAttraction.IsEnabled = true;
                dataGridAttractionsShouldClear = false;
            }
        }

        void ButtonAddPlace_Click(object sender, RoutedEventArgs e)
        {
            AddPlaceWindow addPlaceWindow = new AddPlaceWindow(this);
            bool? isAdded = addPlaceWindow.ShowDialog();
            if (isAdded.HasValue && isAdded.Value)
            {
                UpdatePlaces();
                UpdateAttractions();
            }
        }

        void ButtonModifyPlace_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridPlaces.SelectedItem != null)
            {
                object place = null;
                lock (Places)
                    place = Places.SingleOrDefault(p => (int)ZooClient.GetProperty(p, "ID") == (int)ZooClient.GetProperty(dataGridPlaces.SelectedItem, "ID"));
                ModifyPlaceWindow modifyPlaceWindow = new ModifyPlaceWindow(this, place);
                bool? isModified = modifyPlaceWindow.ShowDialog();
                if (isModified.HasValue && isModified.Value)
                {
                    UpdatePlaces();
                    UpdateAttractions();
                }
            }
        }

        void ButtonRemovePlace_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridPlaces.SelectedItem != null)
            {
                int id = (int)ZooClient.GetProperty(dataGridPlaces.SelectedItem, "ID");
                new Thread(() =>
                {
                    Tuple<bool, byte> status = ZooConnection.DeleteModel(ModelType.Place, id);
                    if (status == null)
                        MessageBox.Show("[ERROR] Cannot remove place from Zoo, server connection error!");
                    else if (!status.Item1)
                        MessageBox.Show(ZooClient.DecodeDeleteError(status.Item2));
                    else
                    {
                        UpdatePlaces();
                        UpdateAttractions();
                    }
                }).Start();
            }
        }

        void ButtonAddAttraction_Click(object sender, RoutedEventArgs e)
        {
            AddAttractionWindow addAttractionWindow = new AddAttractionWindow(this);
            bool? isAdded = addAttractionWindow.ShowDialog();
            if (isAdded.HasValue && isAdded.Value)
            {
                UpdatePlaces();
                UpdateAttractions();
            }
        }

        void ButtonModifyAttraction_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridAttractions.SelectedItem != null)
            {
                object attraction = null;
                lock (attractions)
                    attraction = attractions.SingleOrDefault(a => (int)ZooClient.GetProperty(a, "ID") == (int)ZooClient.GetProperty(dataGridAttractions.SelectedItem, "ID"));
                ModifyAttractionWindow modifyAttractionWindow = new ModifyAttractionWindow(this, attraction);
                bool? isModified = modifyAttractionWindow.ShowDialog();
                if (isModified.HasValue && isModified.Value)
                    UpdateAttractions();
            }
        }

        void ButtonRemoveAttraction_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridAttractions.SelectedItem != null)
            {
                int id = (int)ZooClient.GetProperty(dataGridAttractions.SelectedItem, "ID");
                new Thread(() =>
                {
                    Tuple<bool, byte> status = ZooConnection.DeleteModel(ModelType.Attraction, id);
                    if (status == null)
                        MessageBox.Show("[ERROR] Cannot remove attraction from Zoo, server connection error!");
                    else if (!status.Item1)
                        MessageBox.Show(ZooClient.DecodeDeleteError(status.Item2));
                    else
                    {
                        UpdatePlaces();
                        UpdateAttractions();
                    }
                }).Start();
            }
        }
        #endregion Places Tab

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            update = false;
            if (threadUpdate.ThreadState == ThreadState.WaitSleepJoin)
                threadUpdate.Abort();
        }
    }
}
