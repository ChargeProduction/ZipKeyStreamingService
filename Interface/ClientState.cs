using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipKeyStreamingService.Interface
{
    public class ClientState
    {
        private bool receiveCameraOutput;

        public bool ReceiveCameraOutput
        {
            get => receiveCameraOutput;
            set => receiveCameraOutput = value;
        }
    }
}
