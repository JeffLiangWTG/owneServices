using System;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ParDepComponent;

namespace OcmPoc.Components.Paralleliser
{
	class ProcessManager : IDisposable
	{
		EventWaitHandle addProcessor;
		EventWaitHandle removeProcessor;
		EventWaitHandle removeAllProcessors;

		ConcurrentQueue<int> bufferSizeSamples;
		Indexer indexer;

		CancellationTokenSource cts;
		CancellationToken token;

		TimeSpan samplePeriod;
		TimeSpan decisionPeriod;

		double previousMeanBufferSize;

		Task managementTask;

		public ProcessManager(Indexer indexer)
		{
			this.indexer = indexer;

			addProcessor = new EventWaitHandle(true, EventResetMode.AutoReset, "ParDep:AddProcessor");
			removeProcessor = new EventWaitHandle(false, EventResetMode.AutoReset, "ParDep:RemoveProcessor");
			removeAllProcessors = new EventWaitHandle(false, EventResetMode.AutoReset, "ParDep:RemoveAllProcessors");

			bufferSizeSamples = new ConcurrentQueue<int>();

			cts = new CancellationTokenSource();
			token = cts.Token;

			samplePeriod = TimeSpan.FromMilliseconds(200);
			decisionPeriod = TimeSpan.FromSeconds(5);

			managementTask = Task.Run(() => ManageProcessors());
		}

		void ManageProcessors()
		{
			using (new Timer(SampleBufferSize, null, samplePeriod, samplePeriod))
			using (new Timer(MakeDecision, null, decisionPeriod, decisionPeriod))
			{
				token.WaitHandle.WaitOne();
			}
		}

		void SampleBufferSize(object _)
		{
			bufferSizeSamples.Enqueue(indexer.BufferSize);
		}

		void MakeDecision(object _)
		{
			var meanBufferSize = bufferSizeSamples.Average();
			bufferSizeSamples.Clear();
			var bufferSizeDelta = meanBufferSize - previousMeanBufferSize;

			Display.WriteLine(ConsoleColor.DarkBlue, $"Mean buffer size = {meanBufferSize} (delta {bufferSizeDelta})");

			if (meanBufferSize > 25 && bufferSizeDelta > 3)
			{
				addProcessor.Set();
				Display.WriteLine(ConsoleColor.DarkBlue, "Requested additional processor");
			}
			else if (meanBufferSize < 15)
			{
				removeProcessor.Set();
				Display.WriteLine(ConsoleColor.DarkBlue, "Requested removal of a processor");
			}

			previousMeanBufferSize = meanBufferSize;
		}

		#region IDisposable Support
		bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					removeAllProcessors.Set();
					Thread.Sleep(200);

					cts.Cancel();

					managementTask.GetAwaiter().GetResult();

					cts.Dispose();

					addProcessor.Dispose();
					removeProcessor.Dispose();
					removeAllProcessors.Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}