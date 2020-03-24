using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroFormatter;

namespace ZipKeyStreamingService.Interface.Payload
{
    [ZeroFormattable]
    public class CommandPayload
    {
        [Index(0)]
        public virtual String Command { get; set; }
    }
}
