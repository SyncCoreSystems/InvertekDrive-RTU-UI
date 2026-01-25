using System;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Modbus.Device;

namespace InvertekDrive_RTU_UI.Services;

public static class DriveService
{
    #region Methods for Run or Stop Drive

    public static bool Run(
        IModbusSerialMaster master,
        byte slaveAddress,
        ushort registerAddress,
        ushort value)
    {
        try
        {
            master.WriteSingleRegister(slaveAddress, registerAddress, value);
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
            return true;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return false;
        }
    }

    #endregion

    #region Mehthod for Write Single Register

    public static void WriteRegister(
        IModbusSerialMaster master,
        byte slaveAddress,
        ushort registerAddress,
        ushort value)
    {
        master.WriteSingleRegister(slaveAddress, registerAddress, value);
    }

    #endregion

    #region Read Output Frequency and Current

    public static async Task<ushort[]> ReadFrequencyAndCurrent(
        IModbusSerialMaster master
        , byte slaveAddress,
        ushort startAddress,
        ushort numberOfParameters)
    {
        return await master.ReadHoldingRegistersAsync(slaveAddress, startAddress, numberOfParameters);
    }

    #endregion

    #region Read DC Bus Voltage

    public static async Task<ushort> ReadDcBusVoltage(
        IModbusSerialMaster master,
        byte slaveAddress,
        ushort registerAddress,
        ushort registersToRead)
    {
        try
        {
            ushort[] values =
                await master.ReadHoldingRegistersAsync(slaveAddress, registerAddress, registersToRead);
            
            return values[0];
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            throw;
        }
    }

    #endregion
    
    #region Read Drive Status and Digital Input 1

    public static async Task<ushort[]> ReadStatusAndDigitalInput1(
        IModbusSerialMaster master,
        byte slaveAddress,
        ushort registerAddress,
        ushort registersToRead)
    {
return await master.ReadHoldingRegistersAsync(slaveAddress, registerAddress, registersToRead);
    }
    #endregion
}