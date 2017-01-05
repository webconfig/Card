using System;
using System.IO;
using ProtoBuf;
using System.Net.Sockets;
using System.Collections.Generic;

public class NetHelp
{
    public static void Send<T>(int entity_id, int type, T t, Socket socket)
    {
        byte[] msg;
        using (MemoryStream ms = new MemoryStream())
        {
            Serializer.Serialize<T>(ms, t);
            msg = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(msg, 0, msg.Length);
        }
        byte[] entity_id_value = IntToBytes(entity_id);
        byte[] type_value = IntToBytes(type);
        byte[] Length_value = IntToBytes(msg.Length + type_value.Length + entity_id_value.Length);
        //消息体结构：消息体长度+消息体
        byte[] data = new byte[Length_value.Length + type_value.Length + msg.Length + entity_id_value.Length];
        Length_value.CopyTo(data, 0);
        entity_id_value.CopyTo(data, 4);
        type_value.CopyTo(data, 8);
        msg.CopyTo(data, 12);

        try
        {
            socket.Send(data);
        }
        catch (Exception ex)
        {
            //Log.Error("发送数据错误:" + ex.ToString());
        }
    }
    public static void Send<T>(int type, T t, Socket socket)
    {
        byte[] msg;
        using (MemoryStream ms = new MemoryStream())
        {
            Serializer.Serialize<T>(ms, t);
            msg = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(msg, 0, msg.Length);
        }
        byte[] type_value = IntToBytes(type);
        byte[] Length_value = IntToBytes(msg.Length + type_value.Length);
        //消息体结构：消息体长度+消息体
        byte[] data = new byte[Length_value.Length + type_value.Length + msg.Length];
        Length_value.CopyTo(data, 0);
        type_value.CopyTo(data, 4);
        msg.CopyTo(data, 8);

        try
        {
            socket.Send(data);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log("发送数据错误:" + ex.ToString());
        }
    }
    public static void RecvData<T>(byte[] data, out T t)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            ms.Write(data, 0, data.Length);
            ms.Position = 0;
            t = Serializer.Deserialize<T>(ms);
        }
    }
    public static int BytesToInt(byte[] data, int offset)
    {
        int num = 0;
        for (int i = offset; i < offset + 4; i++)
        {
            num <<= 8;
            num |= (data[i] & 0xff);
        }
        return num;
    }
    public static int BytesToInt(List<byte> data, int offset, ref int num)
    {
        for (int i = offset; i < offset + 4; i++)
        {
            num <<= 8;
            num |= (data[i] & 0xff);
        }
        return num;
    }
    public static int BytesToInt(List<byte> data, int offset)
    {
        int num = 0;
        for (int i = offset; i < offset + 4; i++)
        {
            num <<= 8;
            num |= (data[i] & 0xff);
        }
        return num;
    }
    public static byte[] IntToBytes(int num)
    {
        byte[] bytes = new byte[4];
        for (int i = 0; i < 4; i++)
        {
            bytes[i] = (byte)(num >> (24 - i * 8));
        }
        return bytes;
    }
}

