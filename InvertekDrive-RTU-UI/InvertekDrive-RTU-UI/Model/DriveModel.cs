
namespace InvertekDrive_RTU_UI.Model;

public class DriveModel
{
    public const int MaxLimitSpeed = 3600;
    public const int MinLimitSpeed = 3000;

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
    public int
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