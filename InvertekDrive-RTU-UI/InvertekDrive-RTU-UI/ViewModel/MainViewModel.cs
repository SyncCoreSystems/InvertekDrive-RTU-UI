using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using InvertekDrive_RTU_UI.Commands;
using InvertekDrive_RTU_UI.Helpers;
using InvertekDrive_RTU_UI.Model;
using InvertekDrive_RTU_UI.Services;

namespace InvertekDrive_RTU_UI.ViewModel;

public class MainViewModel : ViewModelBase
{
    #region Cancellation Token

    private CancellationTokenSource? _pollingCts;

    private async void StartPolling()
    {
        try
        {
            if (_pollingCts != null)
                return;

            _pollingCts = new CancellationTokenSource();

            
                while (!_pollingCts.Token.IsCancellationRequested)
                {
                    await ReadDcBusVoltage();
                    await ReadFrequencyAndCurrent();
                    await ReadDriveStatus();
                    await Task.Delay(400, _pollingCts.Token);
                }
            
        }
        catch (TimeoutException e)
        {
            MessageBox.Show($"Check device connection. {e.Message}");
            StopPolling();
            DisconnectSerialModbusDevice();
        }
        catch (Exception e)
        {
            MessageBox.Show($"The serial device has been disconnected. {e.Message}");
            StopPolling();
            DisconnectSerialModbusDevice();
        }
        
    }

    // Stop Polling
    private void StopPolling()
    {
        _pollingCts?.Cancel();
        _pollingCts = null;
    }

    #endregion

    #region Class Instances

    private readonly ModbusModel _modbusModel = new();
    private readonly DriveModel _driveModel = new();
    private readonly MotorModel _motorModel = new();

    #endregion

    #region Relay Commands

    public RelayCommand ConnectModbusDevice { get; }
    public RelayCommand DisconnectModbusDevice { get; }
    public RelayCommand UpdateLocalPorts { get; }
    public RelayCommand ChangeToRunDrive { get; }
    public RelayCommand ChangeToStopDrive { get; }
    public RelayCommand WriteAcceleration { get; }
    public RelayCommand WriteDeceleration { get; }
    public RelayCommand WriteMinFrequency { get; }
    public RelayCommand WriteMaxFrequency { get; }
    public RelayCommand WriteMotorRatedVolt { get; }
    public RelayCommand WriteMotorRatedCurrent { get; }
    public RelayCommand WriteMotorRatedFrequency { get; }
    public RelayCommand ResetDrive { get; }

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

    #region Modbus Properties

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
            if (ModbusService.IsConnected == value)
                return;

            ModbusService.IsConnected = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ModbusConnectionStatus));
            ConnectModbusDevice.RaiseCanExecuteChanged();
            DisconnectModbusDevice.RaiseCanExecuteChanged();
            UpdateLocalPorts.RaiseCanExecuteChanged();

            if (value)
                StartPolling();
            else
                StopPolling();
        }
    }

    #endregion

    public double OutFrequency
    {
        get => _driveModel.OutputFrequency;
        set
        {
            _driveModel.OutputFrequency = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CurrentRpm));
        }
    }

    public double CurrentRpm => OutFrequency * 60;

    public double OutCurrent
    {
        get => _driveModel.OutputCurrent;
        set
        {
            _driveModel.OutputCurrent = value;
            OnPropertyChanged();
        }
    }

    public ushort DcBusVolt
    {
        get => _driveModel.BusVoltage;
        set
        {
            _driveModel.BusVoltage = value;
            OnPropertyChanged();
        }
    }

    public double SetFrequencyValue
    {
        get => _driveModel.SetFrequency;
        set
        {
            _driveModel.SetFrequency = value;
            OnPropertyChanged();
            WriteSetPointFrequency();
        }
    }

    public double AccelerationTimeValue
    {
        get => _driveModel.AccelerationTime;
        set
        {
            _driveModel.AccelerationTime = value;
            OnPropertyChanged();
        }
    }

    public double DecelerationTimeValue
    {
        get => _driveModel.DecelerationTime;
        set
        {
            _driveModel.DecelerationTime = value;
            OnPropertyChanged();
        }
    }

    public int MaxFrequencyValue
    {
        get => _driveModel.MaxFrequency;
        set
        {
            _driveModel.MaxFrequency = value;
            OnPropertyChanged();
        }
    }

    public double MinFrequencyValue
    {
        get => _driveModel.MinFrequency;
        set
        {
            _driveModel.MinFrequency = value;
            OnPropertyChanged();
        }
    }

    public double MotorRatedVoltValue
    {
        get => _motorModel.RatedVoltage;
        set
        {
            _motorModel.RatedVoltage = value;
            OnPropertyChanged();
        }
    }

    public double MotorRatedCurrentValue
    {
        get => _motorModel.RatedCurrent;
        set
        {
            _motorModel.RatedCurrent = value;
            OnPropertyChanged();
        }
    }

    public double MotorRatedFrequencyValue
    {
        get => _motorModel.RatedFrequency;
        set
        {
            _motorModel.RatedFrequency = value;
            OnPropertyChanged();
        }
    }

    public string CurrentlyDriveStatus
    {
        get => field;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool RunningStatus
    {
        get => _driveModel.RunningStatus;
        set
        {
            _driveModel.RunningStatus = value;
            OnPropertyChanged();
            ChangeToStopDrive.RaiseCanExecuteChanged();
            ChangeToRunDrive.RaiseCanExecuteChanged();
        }
    }

    public bool ReadyStatus
    {
        get => _driveModel.ReadyStatus;
        set
        {
            _driveModel.ReadyStatus = value;
            OnPropertyChanged();
        }
    }

    public bool TrippedStatus
    {
        get => _driveModel.TrippedStatus;
        set
        {
            _driveModel.TrippedStatus = value;
            OnPropertyChanged();
            ResetDrive.RaiseCanExecuteChanged();
        }
    }

    #endregion

    #region String Status Indicators

    public string ModbusConnectionStatus => ModbusConnected ? "Connected" : "Disconnected";

    #endregion

// Constructor
    public MainViewModel()
    {
        ConnectModbusDevice = new RelayCommand(_ => ConnectSerialModbusDevice(), _ => !ModbusConnected);
        DisconnectModbusDevice = new RelayCommand(_ => DisconnectSerialModbusDevice(), _ => ModbusConnected);
        UpdateLocalPorts = new RelayCommand(_ => GetPorts(), _ => !ModbusConnected);
        ChangeToRunDrive = new RelayCommand(_ => RunDrive(), _ => !RunningStatus);
        ChangeToStopDrive = new RelayCommand(_ => StopDrive(), _ => RunningStatus);
        WriteAcceleration = new RelayCommand(_ => WriteAccelerationTime());
        WriteDeceleration = new RelayCommand(_ => WriteDecelerationTime());
        WriteMaxFrequency = new RelayCommand(_ => WriteMaxFrequencyRange());
        WriteMinFrequency = new RelayCommand(_ => WriteMinFrequencyRange());
        WriteMotorRatedVolt = new RelayCommand(_ => MotorRatedVoltage());
        WriteMotorRatedFrequency = new RelayCommand(_ => MotorRatedFrequency());
        WriteMotorRatedCurrent = new RelayCommand(_ => MotorRatedCurrent());
        ResetDrive = new RelayCommand(_ => ResetDriveCmd(), _ => TrippedStatus);
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
            (int)DataBits.Eight,
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
        try
        {
            bool driveRun = DriveService.Run(
                ModbusService.Master,
                SelectedSlaveId,
                (ushort)MasterAddresses.ControlWord,
                (ushort)DriveCommands.Run);
        }
        catch (TimeoutException ex)
        {
            MessageBox.Show($"Modbus TimeOut {ex.Message}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Serial error {ex.Message}");
        }
    }

// Stop Method
    private void StopDrive()
    {
        bool driveStop = DriveService.Stop(
            ModbusService.Master,
            SelectedSlaveId,
            (ushort)MasterAddresses.ControlWord,
            (ushort)DriveCommands.Stop);
    }

    #endregion

    #region Read Output Frequency and Output Current

    private async Task ReadFrequencyAndCurrent()
    {
            ushort[] values = await DriveService.ReadFrequencyAndCurrent(
                ModbusService.Master,
                SelectedSlaveId,
                (ushort)MasterAddresses.OutputFrequency,
                (ushort)RegistersToRead.Three);

            OutFrequency = values[0] / 10.0;
            OutCurrent = values[1] / 10.0;
        
    }

    #endregion

    #region Read DC Bus Voltage

    private async Task ReadDcBusVoltage()
    {
        try
        {
            DcBusVolt = await DriveService.ReadDcBusVoltage(
                ModbusService.Master,
                SelectedSlaveId,
                (ushort)MasterAddresses.DcBusVoltage,
                (ushort)RegistersToRead.Two);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
    }

// Start Polling

    #endregion

    #region Write Frequency SetPoint

    private void WriteSetPointFrequency()
    {
        double setPointFrequency = SetFrequencyValue * 10.0;

        DriveService.WriteRegister(
            ModbusService.Master,
            SelectedSlaveId,
            (ushort)MasterAddresses.FrequencySetPoint,
            (ushort)setPointFrequency);
    }

    #endregion

    #region Write Acceleration and Deceleration Time

    private void WriteAccelerationTime()
    {
        try
        {
            DriveService.WriteRegister(
                ModbusService.Master,
                SelectedSlaveId,
                (ushort)MasterAddresses.AccelerationTime,
                (ushort)AccelerationTimeValue);
        }
        catch (UnauthorizedAccessException ex)
        {
            MessageBox.Show($"Error: Access denied to {ex.Message}");
        }
        catch (IOException ex)
        {
            MessageBox.Show($"Error: An I/O error occurred with {ex.Message}");
        }
        catch (TimeoutException ex)
        {
            MessageBox.Show($"Error: A timeout occurred during the operation.  {ex.Message}");
        }
        catch (Exception ex) // Catch any other unexpected exceptions
        {
            MessageBox.Show($"An unexpected error occurred:  {ex.Message}");
        }
        finally
        {
            // Ensure the port is closed even if an exception occurs
            MessageBox.Show("finally");
        }
    }

    private void WriteDecelerationTime()
    {
        DriveService.WriteRegister(
            ModbusService.Master,
            SelectedSlaveId,
            (ushort)MasterAddresses.DecelerationTime,
            (ushort)DecelerationTimeValue);
    }

    #endregion

    #region Write Max and Min Frequency Range

    private void WriteMaxFrequencyRange()
    {
        if (MaxFrequencyValue > DriveModel.MaxLimitSpeed)
            MessageBox.Show("The Maximum speed value is out of range. The valid values is 3000 = 50Hz or 3600 = 60Hz");
        if (MaxFrequencyValue < DriveModel.MinLimitSpeed)
            MessageBox.Show("The Maximum speed value is out of range. The valid values is 3000 = 50Hz or 3600 = 60Hz");

        if (MaxFrequencyValue is >= DriveModel.MinLimitSpeed and <= DriveModel.MaxLimitSpeed)
        {
            DriveService.WriteRegister(
                ModbusService.Master,
                SelectedSlaveId,
                (ushort)MasterAddresses.MaxHz,
                (ushort)MaxFrequencyValue);
        }
    }

    private void WriteMinFrequencyRange()
    {
        DriveService.WriteRegister(
            ModbusService.Master,
            SelectedSlaveId,
            (ushort)MasterAddresses.MinHz,
            (ushort)MinFrequencyValue);
    }

    #endregion

    #region Write Motor Rated Voltage, Current, Frequency

    private void MotorRatedVoltage()
    {
        DriveService.WriteRegister(
            ModbusService.Master,
            SelectedSlaveId,
            (ushort)MasterAddresses.MotorRatedVolt,
            (ushort)MotorRatedVoltValue);
    }

    private void MotorRatedCurrent()
    {
        try
        {
            DriveService.WriteRegister(
                ModbusService.Master,
                SelectedSlaveId,
                (ushort)MasterAddresses.MotorRatedCurrent,
                (ushort)MotorRatedCurrentValue);
        }
        catch (TimeoutException ex)
        {
            // Log error or show message to user
            MessageBox.Show($"Modbus TimeOut: {ex.Message}");
        }
    }

    private void MotorRatedFrequency()
    {
        DriveService.WriteRegister(
            ModbusService.Master,
            SelectedSlaveId,
            (ushort)MasterAddresses.MotorRatedFrequency,
            (ushort)MotorRatedFrequencyValue);
    }

    #endregion

    #region Read Drive Status and Digital Input 1

    private async Task ReadDriveStatus()
    {
        ushort[] values = await DriveService.ReadStatusAndDigitalInput1(
            ModbusService.Master,
            SelectedSlaveId,
            (ushort)MasterAddresses.DriveStatus,
            (ushort)RegistersToRead.Seven);

        ushort byteDriveStatus = values[0];

        // Get each bit from drive status byte
        RunningStatus = GetBitFromByte.GetBitFromRegister(byteDriveStatus, (ushort)DriveStatus.Running);
        ReadyStatus = GetBitFromByte.GetBitFromRegister(byteDriveStatus, (ushort)DriveStatus.Ready);
        TrippedStatus = GetBitFromByte.GetBitFromRegister(byteDriveStatus, (ushort)DriveStatus.Tripped);

        // Change string status indicator
        if (RunningStatus)
            CurrentlyDriveStatus = nameof(DriveStatus.Running);
        if (!ReadyStatus)
            CurrentlyDriveStatus = nameof(DriveStatus.Disable);
        if (TrippedStatus)
            CurrentlyDriveStatus = nameof(DriveStatus.Tripped);
        if (!RunningStatus && ReadyStatus)
            CurrentlyDriveStatus = nameof(DriveStatus.Stopped);
    }

    #endregion

    #region Reset Drive

    private void ResetDriveCmd()
    {
        throw new NotImplementedException();
    }

    #endregion
}