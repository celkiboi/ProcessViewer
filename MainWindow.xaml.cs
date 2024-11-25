using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProcessViewer;

public partial class MainWindow : Window
{
    public bool IsProcessSelected { get; private set; }
    Process? SelectedProcess { get; set; }
    ProcessPriority SelectedPriority { get; set; }

    private static readonly Dictionary<string, ProcessPriority> PriorityMapping = new()
    {
        { "Idle", ProcessPriority.Idle },
        { "Below Normal", ProcessPriority.BelowNormal },
        { "Normal", ProcessPriority.Normal },
        { "Above Normal", ProcessPriority.AboveNormal },
        { "High", ProcessPriority.High },
        { "Real-time", ProcessPriority.Realtime }
    };

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        ProcessManager.Instance.RefreshProcesses();
        ProcessesDataGrid.ItemsSource = ProcessManager.Instance.Processes;
    }

    void ProcessDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedProcess = (Process)ProcessesDataGrid.SelectedItem;
        IsProcessSelected = SelectedProcess != null;
        SetPriorityButton.IsEnabled = IsProcessSelected;

        if (SelectedProcess != null)
        {
            foreach (object? child in LogicalTreeHelper.GetChildren(ChangingPriorityPanel))
            {
                if (child is RadioButton radioButton &&
                    PriorityMapping.TryGetValue(radioButton.Content.ToString()!, out var priority)
                    && (ProcessPriority)priority == SelectedProcess.Priority)
                {
                    radioButton.IsChecked = true;
                }

            }
        }
    }

    private void Reload()
    {
        ProcessManager.Instance.RefreshProcesses();
        ProcessesDataGrid.ItemsSource = ProcessManager.Instance.Processes;
        SelectedProcess = null;
        IsProcessSelected = false;
        SetPriorityButton.IsEnabled = false;
    }

    void SetPriorityButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedProcess == null) return;

        if (!ProcessManager.Instance.SetPriority(SelectedProcess, SelectedPriority))
            MessageBox.Show(
                $"Cannot change the priority of {SelectedProcess} to {SelectedPriority}", 
                "Error", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        else
            MessageBox.Show(
               $"Changed the priority of {SelectedProcess} to {SelectedPriority}",
               "Info",
               MessageBoxButton.OK,
               MessageBoxImage.Information);

        Reload();
    }

    void PriorityRadioButton_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton radioButton && PriorityMapping.TryGetValue(radioButton.Content.ToString()!, out var priority))
        {
            SelectedPriority = (ProcessPriority)priority;
            SetPriorityButton.IsEnabled = SelectedProcess?.Priority != SelectedPriority && IsProcessSelected;
        }
    }

    void ReloadProcessesButton_Click(object sender, RoutedEventArgs e)
    {
        Reload();
    }
}