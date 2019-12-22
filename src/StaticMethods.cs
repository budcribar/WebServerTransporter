using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
#if DEBUG
using OpenTelemetry.Trace;
using OpenTelemetry.Trace.Configuration;
#endif
using Microsoft.Extensions.Hosting;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Net.Http;

namespace PeakSWC.WebServerTransporter
{
    public static class StaticMethods
    {
#if DEBUG
        public static IServiceCollection AddZipkin(this IServiceCollection services, string serviceName)
        {
            return services.AddOpenTelemetry(() =>
            {
                var factory = TracerFactory.Create(b =>

            b.UseZipkin(o =>
            {
                o.ServiceName = serviceName;
                o.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
            }).AddRequestCollector());
                TracerFactoryBase.Default = factory;
                return factory;

            });
        }
#endif
        public static IEnumerable<ReadOnlyMemory<byte>> SplitByLength(this byte[] buffer, int maxLength)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
          
            for (int index = 0; index < buffer.Length; index += maxLength)
            {
                var chunkLength = Math.Min(maxLength, buffer.Length - index);
                              
                yield return new ReadOnlyMemory<byte>(buffer, index, chunkLength);
            }
        }

        public static IEnumerable<ReadOnlyMemory<byte>> SplitByLength(this ImmutableArray<byte> buffer, int maxLength)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            var mem = buffer.ToArray();

            for (int index = 0; index < buffer.Length; index += maxLength)
            {
                var chunkLength = Math.Min(maxLength, buffer.Length - index);

                yield return new ReadOnlyMemory<byte>(mem, index, chunkLength);
            }
        }

#if DEBUG
        public static SpanContext Context(this ITracer tracer, TextMapCarrier carrier)
        {
            if (tracer == null)
                throw new ArgumentNullException(nameof(tracer));
            if (carrier == null)
                throw new ArgumentNullException(nameof(carrier));

            var carrierMap = new Dictionary<string, IEnumerable<string>>();

            foreach (var entry in carrier.Map)
            {
                carrierMap.Add(entry.Key, new[] { entry.Value });
            }

            static IEnumerable<string> GetCarrierKeyValue(Dictionary<string, IEnumerable<string>> source, string key)
            {
                if (key == null || !source.TryGetValue(key, out var value))
                {
                    return new List<string>();
                }

                return value;
            }

            return tracer.TextFormat.Extract<Dictionary<string, IEnumerable<string>>>(carrierMap, GetCarrierKeyValue);
        }
#endif
#region Unused
        public static IEnumerable<string> SplitByLength(this string str, int maxLength)
        {
            for (int index = 0; index < str.Length; index += maxLength)
            {
                yield return str.Substring(index, Math.Min(maxLength, str.Length - index));
            }
        }
        // Unused
        public static async IAsyncEnumerable<byte[]> ClientStreamData(string data)

        {
            foreach (var s in data.SplitByLength(16 * 1024))
            {
                yield return UTF8Encoding.UTF8.GetBytes(s);
                await Task.Delay(1).ConfigureAwait(false);
            }

        }

        public static byte[] Combine(byte[][] arrays)
        {
            if (arrays == null)
                throw new ArgumentNullException(nameof(arrays));

            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
        // Unused
        public static async IAsyncEnumerable<byte[]> ClientStreamData(HttpContent content)

        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            byte[] buffer = await content.ReadAsByteArrayAsync().ConfigureAwait(false);

            foreach (var s in buffer.SplitByLength(16 * 1024 * 1024))
            {
                yield return s.ToArray();
            }

        }
        // Unused
        public static async IAsyncEnumerable<byte[]> ClientStreamDataString(string data)

        {
            foreach (var s in data.SplitByLength(16 * 1024))  
            {
                yield return UTF8Encoding.UTF8.GetBytes(s);
                await Task.Delay(1).ConfigureAwait(true);   
            }

        }
#endregion




    }
}
