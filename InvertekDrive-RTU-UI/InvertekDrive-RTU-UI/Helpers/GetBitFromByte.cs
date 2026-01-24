namespace InvertekDrive_RTU_UI.Helpers;

public static class GetBitFromByte
{
    public static bool GetBitFromRegister(ushort register, ushort bit)
    {
        return ((register >> bit) & 1) == 1;
    }
}