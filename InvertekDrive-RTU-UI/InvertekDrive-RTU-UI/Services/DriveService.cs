using System.Diagnostics;
using InvertekDrive_RTU_UI.Model;
using Modbus.Device;

namespace InvertekDrive_RTU_UI.Services;

public static class DriveService
{
    #region Parameters read array

    public static ushort[] ParametersRead { get; private set; }

    #endregion

    public static bool Run(
        IModbusSerialMaster master,
        byte slaveAddress,
        ushort registerAddress,
        ushort value)
    {
        try
        {
            master.WriteSingleRegister(slaveAddress, registerAddress, value);
            Debug.WriteLine("DriveService.Run()");
            return true;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return false;
        }
    }

    public static bool Stop(
        IModbusSerialMaster master,
        byte slaveAddress,
        ushort registerAddress,
        ushort value)
    {
        try
        {
            master.WriteSingleRegister(slaveAddress, registerAddress, value);
            Debug.WriteLine("DriveService.Stop()");
            return true;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return false;
        }
    }

    public static ushort[] ReadVoltCurrent(
        IModbusSerialMaster master
        , byte slaveAddress,
        ushort startAddress,
        ushort numberOfParameters)
    {
        return master.ReadHoldingRegisters(slaveAddress, startAddress, numberOfParameters);
    }
}