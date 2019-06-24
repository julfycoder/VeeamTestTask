using System;

namespace Cmprsr.Parallelization
{
	/// <summary>
	/// Represents operations for processing actions in parallel thread.
	/// </summary>
	public interface IParallelProcessor
	{
		/// <summary>
		/// Processes specified action in a parallel thread.
		/// </summary>
		/// <param name="action"></param>
		/// <param name="threadsCount"></param>
		void Process(Action action, int threadsCount);

		/// <summary>
		/// Returns true if current operation is completed.
		/// </summary>
		/// <returns></returns>
		bool ProcessingFinished();
	}
}
