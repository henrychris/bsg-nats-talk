using System.Buffers;
using System.Text;
using System.Text.Json;
using NATS.Client.Core;

namespace FinalNatsDemo.Common.Nats.Serializer
{
    public class NatsSerializer<T> : INatsSerializer<T>
    {
        public void Serialize(IBufferWriter<byte> bufferWriter, T value)
        {
            var jsonString = JsonSerializer.Serialize(value);
            var utf8Bytes = Encoding.UTF8.GetBytes(jsonString);
            bufferWriter.Write(utf8Bytes);
        }

        public T? Deserialize(in ReadOnlySequence<byte> buffer)
        {
            var utf8Bytes = buffer.ToArray();
            var jsonString = Encoding.UTF8.GetString(utf8Bytes);
            var obj = JsonSerializer.Deserialize<T>(jsonString);
            return obj;
        }
    }
}
