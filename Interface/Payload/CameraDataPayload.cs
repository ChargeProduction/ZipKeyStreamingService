using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroFormatter;

namespace ZipKeyStreamingService.Interface.Payload
{
    [ZeroFormattable]
    public class CameraDataPayload
    {
        [Index(0)]
        public virtual byte[] JpegData { get; set; }
    }
}
