using System.Collections;
using System.Collections.Generic;
using System.Text;
public class PlayerData : BaseData
{
    public string name;
    public int atk;
    public int lev;
    public override int GetBytesNum()
    {
        int num = 0;
        num += 4 + Encoding.UTF8.GetBytes(name).Length;
        num += 4;
        num += 4;
        return num;
    }
    public override int Reading(byte[] bytes, int beginIndex = 0)
    {
        int index = beginIndex;
        name = ReadString(bytes, ref index);
        atk = ReadInt(bytes, ref index);
        lev = ReadInt(bytes, ref index);
        return index - beginIndex;
    }
    public override byte[] Writing()
    {
        int index = 0;
        byte[] bytes = new byte[GetBytesNum()];
        WriteString(bytes, name, ref index);
        WriteInt(bytes, atk, ref index);
        WriteInt(bytes, lev, ref index);
        return bytes;
    }
}

