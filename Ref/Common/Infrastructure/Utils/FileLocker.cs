using System;
using System.IO;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.Common.Utils;

public static class FileLocker
{
	public static async Task RunWithLockAsync(string lockFilePath, Func<Task> action)
	{
		try
		{
			await using var lockStream = File.Open(lockFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
			await action();
		}
		catch (IOException ex)
		{
			Console.WriteLine($"failed to execute action with lock due to {ex.Message}");
		}
	}
}
