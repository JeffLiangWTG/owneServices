using System;
using System.Linq;
using CargoWise.Blazor.Client.Integration.Files.Requests;
using CargoWise.Blazor.Client.Integration.Files.Responses;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Windows.UI;
using Enterprise.Integration.RemoteDesktopServices;

namespace WinzorFramework.RemoteClientServices
{
	public class CargoWiseClientFile : IRemoteFile
	{
		public CargoWiseClientFile()
		{
		}

		public CargoWiseClientFile(string fileName, byte[] fileData, bool readOnly) : this(fileName, fileData, readOnly, false)
		{
		}

		public CargoWiseClientFile(string fileName, byte[] fileData, bool readOnly, bool sendingExecutionResult)
		{
			Id = Guid.NewGuid();
			FileName = fileName;
			OriginalFileData = fileData;
			this.readOnly = readOnly;
			sendExecutionResult = sendingExecutionResult;
		}

		public string FileName { get; }

		public ReadOnlyMemory<byte> OriginalFileData { get; }

		public Guid Id { get; }

		public bool RemoteFilesSupported => true;

		public event EventHandler FileChanged;

		public event EventHandler<int> ProcessExited;

		public void Dispose()
		{
			if (Id != Guid.Empty)
			{
				try
				{
					CargoWiseClientInvoker.Invoke(async cws => await cws.RemoteFileService.DisposeAsync(new DisposeFileRequest(Id)));
				}
				catch (InvalidOperationException)
				{
					// When the user closes all windows during dispose, WinzorDispatcher wont be available.
					// We should ignore this exception.
				}
			}
		}

		public byte[] FetchFileData()
		{
			return CargoWiseClientInvoker
				.Invoke(async cws => await cws.RemoteFileService.FetchDataAsync(new FetchFileDataRequest(Id)))
				?.FileData;
		}

		public bool GetDoesExistStatus()
		{
			return CargoWiseClientInvoker.Invoke(async cws => await cws.RemoteFileService.DoesExistsAsync(new FileExistsRequest(Id)))
				?.FileExists ?? false;
		}

		public bool GetIsOpenStatus()
		{
			return CargoWiseClientInvoker.Invoke(async cws => await cws.RemoteFileService.IsOpenAsync(new IsFileOpenRequest(Id)))
				?.IsFileOpen ?? false;
		}

		public bool Open()
		{
			if (ZApplication.GetOpenForms().LastOrDefault(f => !f.IsClosing) == null)
			{
				return false;
			}

			var result = CargoWiseClientInvoker.Invoke(async cws => await cws?.RemoteFileService?.OpenAsync(
				new OpenFileRequest(
					Id,
					FileName,
					OriginalFileData.ToArray(),
					readOnly),
				OnChanged,
				sendExecutionResult ? OnProcessExit : default(Action<ProcessExitResponse>)));

			return result == RequestSentResult.MessageSent;
		}

		void OnProcessExit(ProcessExitResponse processExitResult)
		{
			ProcessExited?.Invoke(this, processExitResult.ExitCode);
		}

		void OnChanged(FileChangedResponse fileChangedResult)
		{
			FileChanged?.Invoke(this, EventArgs.Empty);
		}

		readonly bool readOnly;
		readonly bool sendExecutionResult;
	}
}
