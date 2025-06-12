using System;
using System.IO;
using System.Threading.Tasks;

namespace OcmPoc.Tests.Scenario.Helpers
{
	class FileSystemReceiver : IReceiver
	{
		readonly string path;
		readonly int count;

		FileSystemWatcher watcher;

		public FileSystemReceiver(string path, int count)
		{
			this.path = path;
			this.count = count;
		}

		public IReceiver Initialise()
		{
			foreach (string file in Directory.EnumerateFiles(path))
			{
				File.Delete(file);
			}

			watcher = new FileSystemWatcher(path);

			return this;
		}

		public async Task WaitForMessages(TimeSpan timeout)
		{ 
			int received = 0;
			var completionSource = new TaskCompletionSource<object>();

			watcher.Created += CheckCompletion;
			watcher.EnableRaisingEvents = true;
			
			await Task.WhenAny(
				completionSource.Task,
				Task.Delay(timeout));

			watcher.EnableRaisingEvents = false;
			watcher.Created -= CheckCompletion;
			if (!completionSource.Task.IsCompleted) { completionSource.SetCanceled(); }

			void CheckCompletion(object sender, FileSystemEventArgs e)
			{
				if (++received >= count)
				{
					completionSource.SetResult(null);
				}
			}
		}
	}
}
