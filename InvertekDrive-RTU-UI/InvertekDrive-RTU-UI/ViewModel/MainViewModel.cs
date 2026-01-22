using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using InvertekDrive_RTU_UI.Commands;
using InvertekDrive_RTU_UI.Model;
using InvertekDrive_RTU_UI.Services;
using Modbus.Device;

namespace InvertekDrive_RTU_UI.ViewModel;

public class MainViewModel : ViewModelBase
{
    #region Accessors

    private IModbusSerialMaster MasterAccess => ModbusService.Master;

    #endregion

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

    private readonly ModbusModel _modbusModel = new();

    public ObservableCollection<string> ComPorts
    {
        get => _modbusModel.ComPort;
        set => _modbusModel.ComPort = value;
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

    public ObservableCollection<int> BaudRate => _modbusModel.BaudRate;

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
        get => _modbusModel.SlaveId;
        set
        {
            _modbusModel.SlaveId = value;
            OnPropertyChanged();
            Debug.WriteLine(SelectedSlaveId);
        }
    }

    public string ModbusConnectionStatus => ModbusService.IsConnected ? "Connected" : "Disconnected";

    // If modbus connection is successfully the ModbusConnected var will be true

    public bool ModbusConnected
    {
        get => ModbusService.IsConnected;
        set
        {
            ModbusService.IsConnected = value;
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
        bool connected = ModbusService.ConnectModbusMaster(_selectedComPort, SelectedBaudRate, 8, "None", 1);
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

    #region Run or Stop Drive

    public void RunDrive()
    {
        DriveService.Run(MasterAccess, _modbusModel.SlaveId, 1, 1);
    }
    
    public void StopDrive()
    {
        DriveService.Stop(MasterAccess, _modbusModel.SlaveId, 1, 1);
    }

    #endregion
    
    #region Read Drive Parameters
    
    
    
    #endregion
}