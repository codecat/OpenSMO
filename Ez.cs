using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OpenSMO
{
  public static class StreamHelper
  {
    private static void ErrorHandler()
    {
      MainClass.AddLog("Error in StreamHelper", true);
    }

    private static void CloseHandler()
    {
      MainClass.AddLog("Connection closed during StreamHelper byte reading", true);
    }

    public static byte ReadByte(BinaryReader stream)
    {
      try {
        return stream.ReadByte();
      } catch { CloseHandler(); try { stream.Close(); } catch { ErrorHandler(); } return 0; }
    }

    public static byte[] ReadBytes(BinaryReader stream, int amount)
    {
      try {
        return stream.ReadBytes(amount);
      } catch { CloseHandler(); try { stream.Close(); } catch { ErrorHandler(); } return new byte[0]; }
    }

    public static void WriteNT(BinaryWriter stream, string Str)
    {
      byte[] writeBytes = new byte[Str.Length + 1];
      for (int i = 0; i < Str.Length; i++)
        writeBytes[i] = (byte)Str[i];
      writeBytes[Str.Length] = 0;
      stream.Write(writeBytes);
    }

    public static string ReadNT(BinaryReader stream)
    {
      string ret = "";
      byte addByte = 0x00;
      do {
        addByte = ReadByte(stream);
        if (addByte != 0x00)
          ret += (char)addByte;
      } while (addByte != 0x00);
      return ret;
    }

    public static void WriteInt16Regular(BinaryWriter stream, short data)
    {
      byte[] arr = BitConverter.GetBytes(data);
      Array.Reverse(arr);
      stream.Write(arr);
    }

    public static short ReadInt16Regular(BinaryReader stream)
    {
      byte[] arr = ReadBytes(stream, 2);
      if (arr.Length == 0) {
        // NOTE: This is a special case when ReadBytes returns an empty array, this can happen in case of a connection close.
        return 0;
      } else {
        Array.Reverse(arr);
        return BitConverter.ToInt16(arr, 0);
      }
    }

    public static ushort ReadUInt16Regular(BinaryReader stream)
    {
      byte[] arr = ReadBytes(stream, 2);
      if (arr.Length == 0) {
        // NOTE: This is a special case when ReadBytes returns an empty array, this can happen in case of a connection close.
        return 0;
      } else {
        Array.Reverse(arr);
        return BitConverter.ToUInt16(arr, 0);
      }
    }

    public static void WriteInt32Regular(BinaryWriter stream, int data)
    {
      byte[] arr = BitConverter.GetBytes(data);
      Array.Reverse(arr);
      stream.Write(arr);
    }

    public static int ReadInt32Regular(BinaryReader stream)
    {
      byte[] arr = ReadBytes(stream, 4);
      if (arr.Length == 0) {
        // NOTE: This is a special case when ReadBytes returns an empty array, this can happen in case of a connection close.
        return 0;
      } else {
        Array.Reverse(arr);
        return BitConverter.ToInt32(arr, 0);
      }
    }
  }

  public class Ez
  {
    public User user;
    public List<byte> Buffer = new List<byte>();
    public int LastPacketSize = 0;

    public Ez(User user)
    {
      this.user = user;
    }

    public void SendPack()
    {
      if (!this.user.Connected)
        return;

      byte[] arr = new byte[Buffer.Count + 4];

      byte[] arrCount = BitConverter.GetBytes(Buffer.Count);
      Array.Reverse(arrCount);
      arrCount.CopyTo(arr, 0);

      Buffer.ToArray().CopyTo(arr, 4);

      try {
        user.tcpWriter.Write(arr);
        user.tcpWriter.Flush();
      } catch { this.user.Disconnect(); }

      Buffer.Clear();
    }

    public int ReadPack()
    {
      LastPacketSize = Read4();

      // Check for invalid protocol requests (or `cat /dev/urandom | nc <ip> 8765`)
      if (LastPacketSize < 0 || LastPacketSize > Int16.MaxValue) { // Higher than an Int16 it probably will never be... Might have to make this an advanced setting though.
        user.Disconnect();
        return -1;
      }

      return LastPacketSize;
    }

    public void Discard()
    {
      if (LastPacketSize > 0)
        ReadArr(LastPacketSize);
    }

    public void WriteArr(byte[] data)
    {
      foreach (byte b in data)
        Buffer.Add(b);
    }

    public void Write1(byte data)
    {
      Buffer.Add(data);
    }

    public void Write2(short data)
    {
      byte[] arr = BitConverter.GetBytes(data);
      Array.Reverse(arr);
      WriteArr(arr);
    }

    public void Write4(int data)
    {
      byte[] arr = BitConverter.GetBytes(data);
      Array.Reverse(arr);
      WriteArr(arr);
    }

    public void WriteNT(string data)
    {
      byte[] writeBytes = new byte[data.Length + 1];
      for (int i = 0; i < data.Length; i++)
        writeBytes[i] = (byte)data[i];
      writeBytes[data.Length] = 0;
      WriteArr(writeBytes);
    }

    public byte[] ReadArr(int count)
    {
      try {
        return user.tcpReader.ReadBytes(count);
      } catch { MainClass.AddLog("Reading while socket closed!", true); return new byte[0]; }
    }

    public byte Read1()
    {
      LastPacketSize--;
      return user.tcpReader.ReadByte();
    }

    public short Read2()
    {
      LastPacketSize -= 2;
      return StreamHelper.ReadInt16Regular(user.tcpReader);
    }

    public ushort ReadU2()
    {
      LastPacketSize -= 2;
      return StreamHelper.ReadUInt16Regular(user.tcpReader);
    }

    public int Read4()
    {
      LastPacketSize -= 4;
      return StreamHelper.ReadInt32Regular(user.tcpReader);
    }

    public string ReadNT()
    {
      string ret = StreamHelper.ReadNT(user.tcpReader);
      LastPacketSize -= ret.Length + 1;
      return ret;
    }
  }
}
