using System;

namespace Cmprsr.Parallelization
{
	/// <summary>
	/// Represents operations for processing actions in parallel thread.
	/// </summary>
	public class ParallelProcessor : IParallelProcessor
	{
		private readonly IAsyncProcessor asyncProcessor;

		private int threadsCount;

		public ParallelProcessor(IAsyncProcessor asyncProcessor)
		{
			this.asyncProcessor = asyncProcessor;
			this.asyncProcessor.ProcessingEnded += AsyncProcessor_ProcessingEnded;
		}

		/// <summary>
		/// Processes specified action in a parallel thread.
		/// </summary>
		/// <param name="action"></param>
		/// <param name="threadsCount"></param>
		public void Process(Action function, int threadsCount)
		{
			this.threadsCount = threadsCount;

			for (int i = 0; i < threadsCount; i++)
			{
				asyncProcessor.Process(() => function());
			}
		}

		/// <summary>
		/// Returns true if current operation is completed.
		/// </summary>
		/// <returns></returns>
		public bool ProcessingFinished()
		{
			return threadsCount <= 0;
		}

		private void AsyncProcessor_ProcessingEnded(object sender, EventArgs e)
		{
			threadsCount--;
		}
	}
}
