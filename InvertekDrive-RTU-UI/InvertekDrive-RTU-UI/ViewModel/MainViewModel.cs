using System.Collections.ObjectModel;
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

    private async Task StartPolling()
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
            MessageBox.Show($"Error: A timeout occurred during the operation.  {e.Message}");
            StopPolling();
            DisconnectSerialModbusDevice();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"{ex.Message}");
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
    public RelayCommand WriteMinSpeed { get; }
    public RelayCommand WriteMaxSpeed { get; }
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
    public string SelectedComPort
    {
        get => field;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    // Select BaudRate from ComboBox
    public int SelectedBaudRate
    {
        get => field;
        set
        {
            field = value;
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
                _ = StartPolling();
            else
                StopPolling();
        }
    }

    // Check if master is not null
    private bool MasterIsNotNull
    {
        get => field;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Slider Control

    public int SliderMaxValue
    {
        get => field;
        set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MidSliderValue));
        }
    }

    public int MidSliderValue => SliderMaxValue / 2;

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

    // RPM = outHz * RatedHzMotor - Displayed in Current Speed
    public double CurrentRpm => OutFrequency * MotorRatedFrequencyValue;

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

    public double MaxSpeedValue
    {
        get => _driveModel.MaxFrequency;
        set
        {
            _driveModel.MaxFrequency = value;
            OnPropertyChanged();
        }
    }

    public double MinSpeedValue
    {
        get => _driveModel.MinFrequency;
        set
        {
            _driveModel.MinFrequency = value;
            OnPropertyChanged();
        }
    }

    public int MotorRatedVoltValue
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

    public int MotorRatedFrequencyValue
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
        WriteMaxSpeed = new RelayCommand(_ => WriteMaxSpeedLimit());
        WriteMinSpeed = new RelayCommand(_ => WriteMinSpeedLimit());
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
        bool comNotNull = string.IsNullOrEmpty(SelectedComPort);


        bool baudRateNotNull = !BaudRate.Contains(SelectedBaudRate);

        bool slaveAddressInvalid = SelectedSlaveId <= 0;

        if (comNotNull)
        {
            MessageBox.Show("Please select COM Port");
            return;
        }

        if (baudRateNotNull)
        {
            MessageBox.Show("Please select correct BaudRate");
            return;
        }

        if (slaveAddressInvalid)
        {
            MessageBox.Show("Please enter the correct slave address. 1 - 32");
            return;
        }

        if (!comNotNull && !baudRateNotNull)
        {
            try
            {
                bool connected = ModbusService.ConnectModbusMaster(
                    SelectedComPort,
                    SelectedBaudRate,
                    (int)DataBits.Eight,
                    nameof(Parity.None),
                    (int)StopBits.One);

                if (connected)
                {
                    ModbusConnected = true;
                    MasterIsNotNull = ModbusService.MasterNotNull(ModbusService.Master);
                }
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
    }

    // Disconnect
    private void DisconnectSerialModbusDevice()
    {
        bool disconnected = ModbusService.DisconnectModbusMaster();

        if (disconnected)
        {
            ModbusConnected = false;
            MasterIsNotNull = ModbusService.MasterNotNull(ModbusService.Master);
        }
    }

    #endregion

    #region Run or Stop Drive

    // Run Method
    private void RunDrive()
    {
        const int zeroValue = 0;

        if (!MasterIsNotNull)
        {
            MessageBox.Show(ModbusService.MasterNullIndicator);
            return;
        }

        if (MaxSpeedValue != zeroValue && MotorRatedCurrentValue != zeroValue &&
            MotorRatedFrequencyValue != zeroValue && MotorRatedVoltValue
            != zeroValue)
        {
            try
            {
                DriveService.WriteRegister(
                    ModbusService.Master!,
                    SelectedSlaveId,
                    (ushort)MasterAddresses.ControlWord,
                    (ushort)DriveCommands.Run);
            }
            catch (TimeoutException e)
            {
                MessageBox.Show($"Check device connection. {e.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"The serial device has been disconnected. {ex.Message}");
            }
        }
        else
        {
            MessageBox.Show(
                "Enter the Max Speed, Motor Rated Current, Motor Rated Voltage and Motor Rated Frequency." +
                " This values can't be zero or negative.");
        }
    }

    // Stop Method
    private void StopDrive()
    {
        DriveService.WriteRegister(
            ModbusService.Master!,
            SelectedSlaveId,
            (ushort)MasterAddresses.ControlWord,
            (ushort)DriveCommands.Stop);
    }

    #endregion

    #region Read Output Frequency and Output Current

    private async Task ReadFrequencyAndCurrent()
    {
        ushort[] values = await DriveService.ReadFrequencyAndCurrent(
            ModbusService.Master!,
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
        DcBusVolt = await DriveService.ReadAsyncRegister(
            ModbusService.Master!,
            SelectedSlaveId,
            (ushort)MasterAddresses.DcBusVoltage,
            (ushort)RegistersToRead.Two);
    }

    #endregion

    #region Write Frequency SetPoint

    private void WriteSetPointFrequency()
    {
        if (!MasterIsNotNull)
        {
            MessageBox.Show(ModbusService.MasterNullIndicator);
            return;
        }

        try
        {
            // Convert user input to raw value readable for drive
            double setPointFrequency = SetFrequencyValue * DriveModel.RawValueFrequency;

            DriveService.WriteRegister(
                ModbusService.Master!,
                SelectedSlaveId,
                (ushort)MasterAddresses.FrequencySetPoint,
                (ushort)setPointFrequency);
        }
        catch (TimeoutException e)
        {
            MessageBox.Show($"Error: A timeout occurred during the operation.  {e.Message}");
        }
        catch (Exception ex) // Catch any other unexpected exceptions
        {
            MessageBox.Show($"An unexpected error occurred:  {ex.Message}");
        }
    }

    #endregion

    #region Write Acceleration and Deceleration Time

    private void WriteAccelerationTime()
    {
        if (!MasterIsNotNull)
        {
            MessageBox.Show("Master is null.");
            return;
        }

        try
        {
            // Convert user input to raw value readable for drive
            double accelerationTimeValue = AccelerationTimeValue * DriveModel.RawValueRampTime;

            if (AccelerationTimeValue is >= DriveModel.MinRampTime and <= DriveModel.MaxRampTime)
            {
                DriveService.WriteRegister(
                    ModbusService.Master!,
                    SelectedSlaveId,
                    (ushort)MasterAddresses.AccelerationTime,
                    (ushort)accelerationTimeValue);
            }
            else
            {
                MessageBox.Show($"The acceleration ramp time value is out of range. Range is Min {DriveModel
                    .MinRampTime}s - Max {DriveModel.MaxRampTime}s");
            }
        }
        catch (TimeoutException e)
        {
            MessageBox.Show($"Error: A timeout occurred during the operation.  {e.Message}");
        }
        catch (Exception ex) // Catch any other unexpected exceptions
        {
            MessageBox.Show($"An unexpected error occurred:  {ex.Message}");
        }
    }

    private void WriteDecelerationTime()
    {
        if (!MasterIsNotNull)
        {
            MessageBox.Show("Master is null.");
            return;
        }

        try
        {
            // Convert user input to raw value readable for drive
            double decelerationTimeValue = DecelerationTimeValue * DriveModel.RawValueRampTime;


            if (DecelerationTimeValue is >= DriveModel.MinRampTime and <= DriveModel.MaxRampTime)
            {
                DriveService.WriteRegister(
                    ModbusService.Master!,
                    SelectedSlaveId,
                    (ushort)MasterAddresses.DecelerationTime,
                    (ushort)decelerationTimeValue);
            }
            else
            {
                MessageBox.Show($"The acceleration ramp time value is out of range. Range is Min {DriveModel
                    .MinRampTime}s - Max {DriveModel.MaxRampTime}s");
            }
        }
        catch (TimeoutException e)
        {
            MessageBox.Show($"Error: A timeout occurred during the operation.  {e.Message}");
        }
        catch (Exception ex) // Catch any other unexpected exceptions
        {
            MessageBox.Show($"An unexpected error occurred:  {ex.Message}");
        }
    }

    #endregion

    #region Write Max and Min Speed

    private void WriteMaxSpeedLimit()
    {
        if (!MasterIsNotNull)
        {
            MessageBox.Show(ModbusService.MasterNullIndicator);
            return;
        }

        try
        {
            if (MaxSpeedValue is DriveModel.MaxRatedSpeedOne or DriveModel.MaxRatedSpeedTwo)
            {
                DriveService.WriteRegister(
                    ModbusService.Master!,
                    SelectedSlaveId,
                    (ushort)MasterAddresses.MaxSpeedLimit,
                    (ushort)MaxSpeedValue);
            }
            else
            {
                MessageBox.Show(
                    $"Maximum speed incorrect value. Min {DriveModel.MaxRatedSpeedTwo}RPM = 50Hz" +
                    $" - Max {DriveModel.MaxRatedSpeedOne}RPM = 60Hz");
            }
        }
        catch (TimeoutException e)
        {
            MessageBox.Show($"Error: A timeout occurred during the operation.  {e.Message}");
        }
        catch (Exception ex) // Catch any other unexpected exceptions
        {
            MessageBox.Show($"An unexpected error occurred:  {ex.Message}");
        }
    }

    private void WriteMinSpeedLimit()
    {
        if (!MasterIsNotNull)
        {
            MessageBox.Show(ModbusService.MasterNullIndicator);
            return;
        }

        try
        {
            if (MinSpeedValue is >= DriveModel.MinRatedSpeed and < DriveModel.MaxRatedSpeedTwo)
            {
                DriveService.WriteRegister(
                    ModbusService.Master!,
                    SelectedSlaveId,
                    (ushort)MasterAddresses.MinSpeedLimit,
                    (ushort)MinSpeedValue);
            }
            else
            {
                MessageBox.Show(
                    $"Minimum speed value is out of range. Min {DriveModel.MinRatedSpeed}RPM" +
                    $" - Max {DriveModel.MaxRatedSpeedTwo - 1}RPM");
            }
        }
        catch (TimeoutException e)
        {
            MessageBox.Show($"Error: A timeout occurred during the operation.  {e.Message}");
        }
        catch (Exception ex) // Catch any other unexpected exceptions
        {
            MessageBox.Show($"An unexpected error occurred:  {ex.Message}");
        }
    }

    #endregion

    #region Write Motor Rated Voltage, Current, Frequency

    private void MotorRatedVoltage()
    {
        if (!MasterIsNotNull)
        {
            MessageBox.Show(ModbusService.MasterNullIndicator);
            return;
        }

        try
        {
            if (MotorRatedVoltValue is >= DriveModel.MinRatedVolt and <= DriveModel.MaxRatedVolt)
            {
                DriveService.WriteRegister(
                    ModbusService.Master!,
                    SelectedSlaveId,
                    (ushort)MasterAddresses.MotorRatedVolt,
                    (ushort)MotorRatedVoltValue);
            }
            else
            {
                MessageBox.Show(
                    $"Rated voltage incorrect value. Range Min {DriveModel.MinRatedVolt}V - Max {DriveModel.MaxRatedVolt}V");
            }
        }
        catch (TimeoutException ex)
        {
            MessageBox.Show($"Error: A timeout occurred during the operation.  {ex.Message}");
        }
        catch (Exception ex) // Catch any other unexpected exceptions
        {
            MessageBox.Show($"An unexpected error occurred:  {ex.Message}");
        }
    }

    private void MotorRatedCurrent()
    {
        if (!MasterIsNotNull)
        {
            MessageBox.Show(ModbusService.MasterNullIndicator);
            return;
        }

        try
        {
            // Convert user input to raw value readable for drive
            double currentRatedValue = MotorRatedCurrentValue * DriveModel.RawValueFrequency;


            if (currentRatedValue is >= DriveModel.MinRatedCurrent and <= DriveModel.MaxRatedCurrent)
            {
                DriveService.WriteRegister(
                    ModbusService.Master!,
                    SelectedSlaveId,
                    (ushort)MasterAddresses.MotorRatedCurrent,
                    (ushort)currentRatedValue);
            }
            else
            {
                MessageBox.Show("The rated current is out of range. Range is Min 2.6A - Max 10.5A");
            }
        }
        catch (TimeoutException ex)
        {
            MessageBox.Show($"Error: A timeout occurred during the operation.  {ex.Message}");
        }
        catch (Exception ex) // Catch any other unexpected exceptions
        {
            MessageBox.Show($"An unexpected error occurred:  {ex.Message}");
        }
    }

    private void MotorRatedFrequency()
    {
        if (!MasterIsNotNull)
        {
            MessageBox.Show(ModbusService.MasterNullIndicator);
            return;
        }

        try
        {
            if (MotorRatedFrequencyValue is DriveModel.MaxRatedFrequencyTwo or DriveModel.MaxRatedFrequencyOne)
            {
                DriveService.WriteRegister(
                    ModbusService.Master!,
                    SelectedSlaveId,
                    (ushort)MasterAddresses.MotorRatedFrequency,
                    (ushort)MotorRatedFrequencyValue);

                SliderMaxValue = MotorRatedFrequencyValue;
            }
            else
            {
                MessageBox.Show("Rated frequency incorrect value. Value must be 50Hz or 60Hz");
            }
        }
        catch (TimeoutException ex)
        {
            MessageBox.Show($"Error: A timeout occurred during the operation.  {ex.Message}");
        }
        catch (Exception ex) // Catch any other unexpected exceptions
        {
            MessageBox.Show($"An unexpected error occurred:  {ex.Message}");
        }
    }

    #endregion

    #region Read Drive Status

    private async Task ReadDriveStatus()
    {
        if (!MasterIsNotNull)
        {
            MessageBox.Show(ModbusService.MasterNullIndicator);
            return;
        }

        ushort byteDriveStatus = await DriveService.ReadAsyncRegister(
            ModbusService.Master!,
            SelectedSlaveId,
            (ushort)MasterAddresses.DriveStatus,
            (ushort)RegistersToRead.Two);


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