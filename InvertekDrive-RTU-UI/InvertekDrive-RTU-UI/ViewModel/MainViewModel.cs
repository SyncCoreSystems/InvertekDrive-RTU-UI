using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using InvertekDrive_RTU_UI.Commands;
using InvertekDrive_RTU_UI.Model;
using InvertekDrive_RTU_UI.Services;

namespace InvertekDrive_RTU_UI.ViewModel;

public class MainViewModel : ViewModelBase
{
    #region Relay Commands

    public ICommand ConnectModbusDevice { get; }
    public RelayCommand DisconnectModbusDevice { get; }
    public ICommand UpdateLocalPorts { get; }

    #endregion

    public MainViewModel()
    {
        UpdateLocalPorts = new RelayCommand(_ => GetPorts());
    }

    #region Com Ports, BaudRate and Slave ID Collections

    private readonly ModbusStation _modbusStation = new();

    public ObservableCollection<string> ComPorts
    {
        get => _modbusStation.ComPort;
        set => _modbusStation.ComPort = value;
    }

    private string _selectedComPort;

    public string SelectedComPort
    {
        get => _selectedComPort;
        set
        {
            _selectedComPort = value;
            OnPropertyChanged();
            Debug.WriteLine(SelectedComPort);
        }
    }

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

    public string ModbusConnectionStatus => ModbusService.IsConnected ? "Connected" : "Disconnected";
    private bool _modbusConnected;

    public bool ModbusConnected
    {
        get => _modbusConnected;
        set
        {
            _modbusConnected = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ModbusConnectionStatus));
        }
    }

    public void GetPorts()
    {
        ModbusService.GetComPorts(ComPorts);
    }

    #endregion

    #region Connect Modbus Device

    private void ConnectSerialModbusDevice()
    {
        bool connected = ModbusService.ConnectModbusMaster(SelectedComPort, SelectedBaudRate, 8, "None", 1);
        if (connected)
            ModbusConnected = true;

    }
    #endregion
}