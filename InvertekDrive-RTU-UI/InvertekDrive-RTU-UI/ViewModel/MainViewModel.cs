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
    #region Class Instances

    private readonly ModbusModel _modbusModel = new();
    private readonly DriveModel _driveModel = new();

    #endregion

    #region Relay Commands

    public RelayCommand ConnectModbusDevice { get; }
    public RelayCommand DisconnectModbusDevice { get; }
    public RelayCommand UpdateLocalPorts { get; }
    public RelayCommand ChangeToRunDrive { get; }
    public RelayCommand ChangeToStopDrive { get; }
    public RelayCommand WriteAcceleration { get; }
    public RelayCommand WriteDeceleration { get; }
    public RelayCommand WriteFrequency { get; }
    public RelayCommand WriteMinFrequency { get; }
    public RelayCommand WriteMaxFrequency { get; }

    #endregion

    #region Collections

    public ObservableCollection<string> ComPorts
    {
        get => _modbusModel.ComPort;
        set => _modbusModel.ComPort = value;
    }

    public ObservableCollection<int> BaudRate => _modbusModel.BaudRate;

    #endregion

    #region Properties

    // Select Com Port from ComboBox
    private string _selectedComPort;

    public string SelectedComPort
    {
        get => _selectedComPort;
        set
        {
            _selectedComPort = value;
            OnPropertyChanged();
        }
    }

    // Select BaudRate from ComboBox
    private int _selectedBaudRate;

    public int SelectedBaudRate
    {
        get => _selectedBaudRate;
        set
        {
            _selectedBaudRate = value;
            OnPropertyChanged();
        }
    }

    // Select SlaveId from TextBox
    public byte SelectedSlaveId
    {
        get => _modbusModel.SlaveId;
        set
        {
            _modbusModel.SlaveId = value;
            OnPropertyChanged();
        }
    }

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

    public bool DriveRunning
    {
        get => _driveModel.IsRunning;
        set
        {
            _driveModel.IsRunning = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DriveStatus));
            ChangeToRunDrive.RaiseCanExecuteChanged();
            ChangeToStopDrive.RaiseCanExecuteChanged();
        }
    }

    public ushort OutFrequency
    {
        get => _driveModel.OutputFrequency;
        set
        {
            _driveModel.OutputFrequency = value;
            OnPropertyChanged();
        }
    }
    
    public ushort OutCurrent
    {
        get => _driveModel.OutputCurrent;
        set
        {
            _driveModel.OutputCurrent = value;
            OnPropertyChanged();
        }
    }
    #endregion

    #region String Status Indicators

    public string ModbusConnectionStatus => ModbusConnected ? "Connected" : "Disconnected";
    public string DriveStatus => DriveRunning ? "Running" : "Stopped";

    #endregion


    public MainViewModel()
    {
        ConnectModbusDevice = new RelayCommand(_ => ConnectSerialModbusDevice(), _ => !ModbusConnected);
        DisconnectModbusDevice = new RelayCommand(_ => DisconnectSerialModbusDevice(), _ => ModbusConnected);
        UpdateLocalPorts = new RelayCommand(_ => GetPorts(), _ => !ModbusConnected);
        ChangeToRunDrive = new RelayCommand(_ => RunDrive(), _ => !DriveRunning);
        ChangeToStopDrive = new RelayCommand(_ => StopDrive(), _ => DriveRunning);
    }

    #region Obtain Com Ports From Local PC

    public void GetPorts()
    {
        ModbusService.GetComPorts(ComPorts);
    }

    #endregion

    #region Connect and Disconnect Modbus Device

    // Connect
    private void ConnectSerialModbusDevice()
    {
        bool connected = ModbusService.ConnectModbusMaster(
            _selectedComPort,
            SelectedBaudRate,
            (Int32)DataBits.Eight,
            nameof(Parity.None),
            (int)StopBits.One);

        if (connected)
            ModbusConnected = true;
    }

    // Disconnect
    private void DisconnectSerialModbusDevice()
    {
        bool disconnected = ModbusService.DisconnectModbusMaster();
        if (disconnected)
            ModbusConnected = false;
    }

    #endregion

    #region Run or Stop Drive

    // Run Method
    private void RunDrive()
    {
        bool driveRun = DriveService.Run(
            ModbusService.Master,
            SelectedSlaveId,
            (ushort)MasterAddresses.ControlWord,
            (ushort)DriveCommands.Run);

        if (driveRun)
            DriveRunning = true;
    }

    // Stop Method
    private void StopDrive()
    {
        bool driveStop = DriveService.Stop(
            ModbusService.Master,
            SelectedSlaveId,
            (ushort)MasterAddresses.ControlWord,
            (ushort)DriveCommands.Stop);

        if (driveStop)
            DriveRunning = false;
    }

    #endregion

    #region Read Output Frequency and Output Current

    private void ReadOutVoltCurrent()
    {
        DriveService.ReadVoltCurrent(
            ModbusService.Master,
            SelectedSlaveId,
            (ushort)MasterAddresses.OutputFrequency,
            (ushort)RegistersToRead.Three);
    }
    #endregion
}