using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;


namespace System_Programming_Semaphore;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    int counter = 1;
    private int semaphoreCount = 0;
    private Semaphore Semaphore { get; set; }
    private ObservableCollection<Thread> workingThreads = [];
    private ObservableCollection<Thread> createdThreads = [];
    private ObservableCollection<Thread> waitingThreads = [];
    public ObservableCollection<Thread> CreatedThreads { get => createdThreads; set { createdThreads = value; OnPropertyChanged(); } }
    public ObservableCollection<Thread> WaitingThreads { get => waitingThreads; set { waitingThreads = value; OnPropertyChanged(); } }
    public ObservableCollection<Thread> WorkingThreads { get => workingThreads; set { workingThreads = value; OnPropertyChanged(); } }
    public int SemaphoreCount { get => semaphoreCount; set { semaphoreCount = value; OnPropertyChanged(); SemaphoreCountChange(); } }
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        Semaphore = new Semaphore(SemaphoreCount, 10);
        _ = Task.Run(Run);
    }

    void Run()
    {
        while (true)
        {
            if (SemaphoreCount > WorkingThreads.Count && WaitingThreads.Count > 0)
            {
                var thread = WaitingThreads[0];
                Application.Current.Dispatcher.Invoke(() => {
                    WaitingThreads.Remove(thread);
                    WorkingThreads.Add(thread);
                });
                //SemaphoreCountChange();
            }
            Thread.Sleep(100);
        }
    }

    private void UpArrow_Click(object sender, RoutedEventArgs e)
    {
        if (SemaphoreCount < 10)
            SemaphoreCount++;
    }

    private void DownArrow_Click(object sender, RoutedEventArgs e)
    {
        if (SemaphoreCount > WorkingThreads.Count)
            SemaphoreCount--;
    }

    private void CreateBtn_Click(object sender, RoutedEventArgs e)
    {
        var thread = new Thread(() => { });
        thread.Name = $"Thread {counter++}";
        CreatedThreads.Add(thread);
    }

    private void CreatedList_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var list = sender as ListView;
        if (list is null) return;
        if (list.SelectedItem is Thread a)
        {
            WaitingThreads.Add(a);
            CreatedThreads.Remove(a);
        }
    }

    private void WaitingList_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var list = sender as ListView;
        if (list is null) return;
        if (list.SelectedItem is Thread a)
        {
            var result = MessageBox.Show($"Do you want to release {a.Name}?", "Relase", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
                WaitingThreads.Remove(a);
        }
    }

    private void WorkingList_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var list = sender as ListView;
        if (list is null) return;

        if (list.SelectedItem is Thread thread)
        {
            WorkingThreads.Remove(thread);
            Semaphore.Release();
            if (WaitingThreads.Count > 0)
            {
                var waitingThread = WaitingThreads[0];
                WaitingThreads.Remove(waitingThread);
                WorkingThreads.Add(waitingThread);
                waitingThread.Start();
            }
        }
    }

    private void SemaphoreCountChange()
    {
        while (WorkingThreads.Count < SemaphoreCount && WaitingThreads.Count > 0)
        {
            var thread = WaitingThreads[0];
            WaitingThreads.Remove(thread);
            WorkingThreads.Add(thread);
            thread.Start();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}