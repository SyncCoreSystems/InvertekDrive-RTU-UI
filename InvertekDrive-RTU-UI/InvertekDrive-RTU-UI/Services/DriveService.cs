using System.Diagnostics;
using Modbus.Device;

namespace InvertekDrive_RTU_UI.Services;

public static class DriveService
{
    #region General Status Vars

    public static bool IsRunning { get; private set; }

    #endregion

    public static void Run(IModbusSerialMaster master, byte slaveAddress, ushort registerAddress, ushort value)
    {
        master.WriteSingleRegister(slaveAddress, registerAddress, value);

        Debug.WriteLine("DriveService.Run()");
    }

    public static void Stop(IModbusSerialMaster master, byte slaveAddress, ushort registerAddress, ushort value)
    {
        master.WriteSingleRegister(slaveAddress, registerAddress, value);

        Debug.WriteLine("DriveService.Stop()");
    }

    public static void ReadParameters(IModbusSerialMaster master, byte slaveAddress, ushort startAddress,
        ushort numberOfParameters)
    {
        ushort[] values = master.ReadHoldingRegisters(slaveAddress, startAddress, numberOfParameters);
    }
}