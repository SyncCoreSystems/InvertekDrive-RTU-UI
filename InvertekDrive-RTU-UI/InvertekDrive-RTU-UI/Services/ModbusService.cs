using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Ports;
using InvertekDrive_RTU_UI.ViewModel;
using Modbus.Device;
using Modbus.Serial;

namespace InvertekDrive_RTU_UI.Services;

public static class ModbusService
{
    #region Modbus Master Access

    public static SerialPortAdapter? Adapter { get; private set; }
    public static IModbusSerialMaster? Master { get; private set; }
    public static SerialPort? SerialStation { get; private set; }

    #endregion

    #region Check if Master is not null

    public static bool MasterNotNull(IModbusSerialMaster? master)
    {
        return master != null;
    }

    #endregion

    #region Is Connected Store State

    public static bool
        IsConnected { get; private set; }

    #endregion

    #region Connect Modbus Serial Master

    public static bool ConnectModbusMaster(
        string serialPort,
        int baudRate,
        int dataBits,
        string parity,
        int stopBits
    )
    {
        if (!IsConnected)
        {
            try
            {
                SerialStation = new SerialPort(serialPort);
                SerialStation.BaudRate = baudRate;
                SerialStation.DataBits = dataBits;
                SerialStation.Parity = parity switch
                {
                    "None" => Parity.None,
                    "Even" => Parity.Even,
                    "Odd" => Parity.Odd,
                    _ => Parity.None
                };
                SerialStation.StopBits = stopBits switch
                {
                    0 => StopBits.None,
                    1 => StopBits.One,
                    2 => StopBits.Two,
                    _ => StopBits.One
                };
                SerialStation.Open();
                Adapter = new SerialPortAdapter(SerialStation);
                Master = ModbusSerialMaster.CreateRtu(Adapter);

                IsConnected = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        return true;
    }

    #endregion

    #region Disconnect Modbus Serial Master

    public static bool DisconnectModbusMaster()
    {
        if (IsConnected)
        {
            try
            {
                Master?.Dispose();
                Adapter?.Dispose();
                SerialStation?.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        IsConnected = false;
        return true;
    }

    #endregion
    
    #region Get Com Ports from local PC

    public static void GetComPorts(ObservableCollection<string> ports)
    {
        string[] getPorts = SerialPort.GetPortNames();
        foreach (string port in getPorts)
        {
            if  (!ports.Contains(port))
                ports.Add(port);
        }
    }
    #endregion
}