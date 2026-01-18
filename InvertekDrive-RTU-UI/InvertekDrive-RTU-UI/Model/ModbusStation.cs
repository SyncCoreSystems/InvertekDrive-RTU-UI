using System.Collections.ObjectModel;
using System.IO.Ports;

namespace InvertekDrive_RTU_UI.Model;

public class ModbusStation
{
    public ObservableCollection<string> ComPort { get; set; } = new();

    public ObservableCollection<int> BaudRate { get; } = new()
    {
        9600,
        19200,
        38400,
        57600
    };

    public byte SlaveId { get; set; } = 1;

    public ObservableCollection<string> Parity { get; } = new()
    {
        "None",
        "Odd",
        "Even"
    };

    public ObservableCollection<int> DataBits { get; } = new()
    {
        7,
        8
    };

    public ObservableCollection<int> StopBits { get; } = new()
    {
        0,
        1,
        2
    };
}