using System;
using System.IO;

namespace Network
{
    public class PackageHandler : PackageHandler<object>
    {
        public PackageHandler(object sender) : base(sender) { }
    }
    public class PackageHandler<T>
    {
        private MemoryStream stream = new MemoryStream(64 * 1024);
        private int readOffset = 0;
        private T sender;

        public PackageHandler(T sender)
        {
            this.sender = sender;
        }

        public void ReceiveData(byte[] data, int offset, int count)
        {
            if (stream.Position + count > stream.Capacity)
                throw new Exception("PackageHandler write buffer overflow");
            stream.Write(data, offset, count);
            ParsePackage();
        }
        public static byte[] PackMessage(/*SkillBridge.Message.NetMessage message*/)
        {
            byte[] package = null;
            using (MemoryStream ms = new MemoryStream())
            {
                //ProtoBuf.Serializer.Serialize(ms, message);
                package = new byte[ms.Length + 4];
                Buffer.BlockCopy(BitConverter.GetBytes(ms.Length), 0, package, 0, 4);
                Buffer.BlockCopy(ms.GetBuffer(), 0, package, 4, (int)ms.Length);
            }
            return package;
        }
        /*public static SkillBridge.Message.NetMessage UnpackMessage(byte[] packet, int offset, int length)
        {
            SkillBridge.Message.NetMessage message = null;
            using (MemoryStream ms = new MemoryStream(packet, offset, length))
                message = ProtoBuf.Serializer.Deserialize<SkillBridge.Message.NetMessage>(ms);
            return message;
        }*/
        bool ParsePackage()
        {
            if (readOffset + 4 < stream.Position)
            {
                int packageSize = BitConverter.ToInt32(stream.GetBuffer(), readOffset);
                if (packageSize + readOffset + 4 <= stream.Position)
                {//包有效

                    /*SkillBridge.Message.NetMessage message = UnpackMessage(stream.GetBuffer(), this.readOffset + 4, packageSize);
                    if (message == null)
                        throw new Exception("PackageHandler ParsePackage faild,invalid package");
                    MessageDistributer<T>.Instance.ReceiveMessage(this.sender, message);*/
                    readOffset += (packageSize + 4);
                    return ParsePackage();
                }
            }
            if (readOffset > 0)
            {
                long size = stream.Position - readOffset;
                if (readOffset < stream.Position)
                    Array.Copy(stream.GetBuffer(), readOffset, stream.GetBuffer(), 0, stream.Position - readOffset);
                readOffset = 0;
                stream.Position = size;
                stream.SetLength(size);
            }
            return true;
        }
    }
}
