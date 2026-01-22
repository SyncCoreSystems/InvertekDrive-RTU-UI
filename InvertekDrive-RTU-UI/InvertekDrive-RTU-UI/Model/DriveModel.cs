
namespace InvertekDrive_RTU_UI.Model;

public class DriveModel
{
    public bool 
        IsRunning { get; set; }
    public ushort
        SetFrequency { get; set; }
    public ushort
        OutputFrequency { get; set; }
    public ushort
        OutputCurrent {get; set; }
    public ushort
        BusVoltage { get; set; }
        
}