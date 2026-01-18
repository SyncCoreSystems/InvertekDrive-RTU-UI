using System.Collections.ObjectModel;
using System.IO.Ports;

namespace InvertekDrive_RTU_UI.Model;

public class ModbusStation
{
    public string ComPort { get; set; } 

    public ObservableCollection<int> BaudRate { get; } = new()
    {
        9600,
        19200,
        38400,
        57600
    };

    public byte SlaveId { get; set; }
}