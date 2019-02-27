using Intel.RealSense.Frames;
using Intel.RealSense.Types;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Intel.RealSense.Processing
{
    public interface IProcessingBlock : IOptions
    {
        Task<Frame> Process(Frame original, CancellationToken token);
        Task<FrameSet> Process(FrameSet original, CancellationToken token);
    }
}
