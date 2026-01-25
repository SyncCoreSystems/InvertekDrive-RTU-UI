namespace InvertekDrive_RTU_UI.Model;

public enum MasterAddresses : ushort
{
    ControlWord = 0,
    FrequencySetPoint = 1,
    DriveStatus = 5,
    OutputFrequency = 6,
    OutputCurrent = 7,
    DigitalInputStatus = 10,
    DcBusVoltage = 22,
    DriveTemperature = 23,
    MaxHz = 128,
    MinHz = 129,
    AccelerationTime = 130,
    DecelerationTime = 131,
    MotorRatedVolt = 134,
    MotorRatedCurrent= 135,
    MotorRatedFrequency = 136
}