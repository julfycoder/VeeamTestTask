using Cmprsr.Common.Entity;
using Cmprsr.Configuration;
using Cmprsr.Parallelization;
using Cmprsr.Segmentation;
using System.IO;
using System.Linq;

namespace Cmprsr.IO
{
	/// <summary>
	/// Represents operations for block-by-block writing from one stream to another.
	/// </summary>
	public class BlockByBlockStreamWriter : IBlockByBlockStreamWriter
	{
		private readonly ISegmentStreamWriter segmentStreamWriter;
		private readonly IDataSegmenter dataSegmenter;
		private readonly SegmentalCompressionConfiguration configuration;
		private readonly IParallelProcessor parallelProcessor;

		private Stream source;
		private Stream destination;

		private int segmentId;

		public BlockByBlockStreamWriter(
			ISegmentStreamWriter segmentStreamWriter,
			IDataSegmenter dataSegmenter,
			IParallelProcessor parallelProcessor,
			SegmentalCompressionConfiguration configuration)
		{
			this.segmentStreamWriter = segmentStreamWriter;
			this.dataSegmenter = dataSegmenter;
			this.configuration = configuration;
			this.parallelProcessor = parallelProcessor;
		}

		/// <summary>
		/// Writes data from one stream to another using block-by-block algorithm.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		public void Write(Stream source, Stream destination)
		{
			this.source = source;
			this.destination = destination;

			segmentId = 0;

			parallelProcessor.Process(() => DoBlockByBlockWriting(), configuration.ThreadsCount);

			while (!parallelProcessor.ProcessingFinished()) ;
		}

		private void DoBlockByBlockWriting()
		{
			// Process while there is still remaining data in the stream
			while (true)
			{
				// Generate segment
				Segment segment = null;

				lock (source)
				{
					byte[] buffer = new byte[configuration.SegmentSize];

					int readResult = source.Read(buffer, 0, buffer.Length);

					if (readResult > 0)
					{
						// Adjust buffer size to the stream length in case if we almost reached the end of it
						if (readResult < configuration.SegmentSize) buffer = buffer.Take(readResult).ToArray();

						segment = dataSegmenter.GetSegment(++segmentId, buffer);
					}
					else break;
				}

				// Compress Segment
				lock (segmentStreamWriter)
				{
					segmentStreamWriter.Write(destination, segment);
				}
			}
		}
	}
}