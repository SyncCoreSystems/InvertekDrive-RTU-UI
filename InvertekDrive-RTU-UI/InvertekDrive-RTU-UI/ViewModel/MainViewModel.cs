using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using InvertekDrive_RTU_UI.Model;

namespace InvertekDrive_RTU_UI.ViewModel;

public class MainViewModel : ViewModelBase
{
    #region Relay Commands

    public IRelayCommand ConnectModbusDevice { get; }
    public IRelayCommand DisconnectModbusDevice { get; }
    public IRelayCommand UpdateLocalPorts { get; }

    #endregion

    public MainViewModel()
    {
    }

    #region Com Ports, BaudRate and Slave ID Collections

    private readonly ModbusStation _modbusStation = new();

    public ObservableCollection<int> BaudRate => _modbusStation.BaudRate;
   
    private int _selectedBaudRate;
    public int SelectedBaudRate
    {
        get => _selectedBaudRate;
        set
        {
            _selectedBaudRate = value;
            OnPropertyChanged();
            Debug.WriteLine(SelectedBaudRate);
        }
    }

    public byte SelectedSlaveId
    {
        get => _modbusStation.SlaveId;
        set
        {
            _modbusStation.SlaveId = value;
            OnPropertyChanged();
            Debug.WriteLine(SelectedSlaveId);
        }
    }

    #endregion
}