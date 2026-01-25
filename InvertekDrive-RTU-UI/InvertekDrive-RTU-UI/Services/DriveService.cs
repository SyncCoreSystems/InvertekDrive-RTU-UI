using Modbus.Device;

namespace InvertekDrive_RTU_UI.Services;

public static class DriveService
{
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

    #region Read DC Bus Voltage and Drive Status

    public static async Task<ushort> ReadAsyncRegister(
        IModbusSerialMaster master,
        byte slaveAddress,
        ushort registerAddress,
        ushort registersToRead)
    {
        ushort[] values =
            await master.ReadHoldingRegistersAsync(slaveAddress, registerAddress, registersToRead);

        return values[0];
    }

    #endregion
}