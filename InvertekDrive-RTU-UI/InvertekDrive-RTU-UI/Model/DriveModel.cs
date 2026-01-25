
namespace InvertekDrive_RTU_UI.Model;

public class DriveModel
{
    public const double RawValueFrequency = 10.0;
    public const double RawValueRampTime = 100.0;
    public const int MaxRampTime = 600;
    public const double MinRampTime = 0.0;
    public const int  MaxRatedVolt= 120;
    public const int  MinRatedVolt= 20;
    public const int  MaxRatedCurrent = 105;
    public const int  MinRatedCurrent = 26;
    public const int  MaxRatedFrequencyOne = 60;
    public const int  MaxRatedFrequencyTwo = 50;
    public const int  MaxRatedSpeedOne = 3600;
    public const int  MaxRatedSpeedTwo = 3000;
    public const double MinRatedSpeed = 0.0;
    
    

    public bool 
        IsRunning { get; set; }
    public double
        SetFrequency { get; set; }
    public double
        OutputFrequency { get; set; }
    public double
        OutputCurrent {get; set; }
    public ushort
        BusVoltage { get; set; }
    public double
        AccelerationTime { get; set; }
    public double
        DecelerationTime { get; set; }
    public double
        MaxFrequency { get; set; }
    public double
        MinFrequency { get; set; }
    public bool
        RunningStatus { get; set; }
    public bool
        ReadyStatus { get; set; }
    public bool
        TrippedStatus { get; set; }
        
}