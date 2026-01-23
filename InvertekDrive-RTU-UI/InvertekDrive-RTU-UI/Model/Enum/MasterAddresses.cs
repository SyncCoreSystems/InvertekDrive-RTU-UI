namespace InvertekDrive_RTU_UI.Model;

public enum MasterAddresses : ushort
{
    ControlWord = 0,
    FrequencySetPoint = 1,
    DriveStatus = 5,
    OutputFrequency = 6,
    OutputCurrent = 7,
    DigitalInputStatus = 10,
    DriveTemperature = 23,
    AccelerationTime = 130,
    DecelerationTime = 131,
    MotorRatedVolt = 134,
    MotorRatedCurrent= 135,
    MotorRatedFrequency = 136
}