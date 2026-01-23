namespace InvertekDrive_RTU_UI.Model;

public enum DriveStatus : ushort
{
    Running = 0,
    Tripped = 1,
    StandBy = 5,
    Ready = 6,
    Stopped = 10,
    Disable = 11
}