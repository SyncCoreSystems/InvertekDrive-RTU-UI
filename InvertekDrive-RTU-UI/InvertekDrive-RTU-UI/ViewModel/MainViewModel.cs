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

    public RelayCommand ConnectModbusDevice { get; }
    public RelayCommand DisconnectModbusDevice { get; }
    public RelayCommand UpdateLocalPorts { get; }

    #endregion

    public MainViewModel()
    {
        ConnectModbusDevice = new RelayCommand(_ => ConnectSerialModbusDevice(), _ => !ModbusConnected);
        DisconnectModbusDevice = new RelayCommand(_ => DisconnectSerialModbusDevice(), _ => ModbusConnected);
        UpdateLocalPorts = new RelayCommand(_ => GetPorts(), _ => !ModbusConnected);
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

    public string ModbusConnectionStatus => ModbusConnected ? "Connected" : "Disconnected";

    // If modbus connection is successfully the ModbusConnected var will be true
    private bool _modbusConnected;

    public bool ModbusConnected
    {
        get => _modbusConnected;
        set
        {
            _modbusConnected = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ModbusConnectionStatus));
            ConnectModbusDevice.RaiseCanExecuteChanged();
            DisconnectModbusDevice.RaiseCanExecuteChanged();
            UpdateLocalPorts.RaiseCanExecuteChanged();
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

    #region Disconnect Modbus Device

    private void DisconnectSerialModbusDevice()
    {
        bool disconnected = ModbusService.DisconnectModbusMaster();
        if (disconnected)
            ModbusConnected = false;
    }

    #endregion
}