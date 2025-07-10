using System.Net;
using System;
using System.IO;
using CargoWise.RefDbRepo.PLReferenceData.Services.Taric4.Interfaces;
using System.Linq;

namespace CargoWise.RefDbRepo.PLReferenceData.Services.Taric4;

sealed class FtpClient : IFtpClient
{
	readonly IFtpRequestFactory factory;
	readonly Uri ftpUri;

	public FtpClient(string ftpAddress, IFtpRequestFactory seleniumFactory = null)
	{
		ftpUri = new Uri(ftpAddress);
		if (!ftpUri.Scheme.Equals("ftp", StringComparison.OrdinalIgnoreCase))
		{
			throw new ArgumentException($"Invalid FTP address: {ftpAddress}");
		}
		factory = seleniumFactory ?? new InternalFtpRequestFactory();
	}

	public void CreateDirectory(string directoryName)
	{
		var uri = BuildUri(directoryName);
		using var _ = factory.CreateAndRun(uri, WebRequestMethods.Ftp.MakeDirectory);
	}

	public string[] GetDirectoryContent(string directoryName)
	{
		var uri = BuildUri(directoryName);
		using var response = factory.CreateAndRun(uri, WebRequestMethods.Ftp.ListDirectory);
		using var responseStream = response.GetResponseStream();
		using var reader = new StreamReader(responseStream);
		var fileNames = reader.ReadToEnd();
		return fileNames.Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries);
	}

	public void DownloadFile(string fileName, string directoryName, string localDownloadDirectory)
	{
		var uri = BuildUri(directoryName, fileName);
		var localPath = Path.Combine(localDownloadDirectory, fileName);
		using var response = factory.CreateAndRun(uri, WebRequestMethods.Ftp.DownloadFile);
		using var downloadStream = response.GetResponseStream();
		using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 8192, useAsync: false);
		downloadStream.CopyTo(fileStream);
	}

	public void DeleteFile(string fileName, string directoryName)
	{
		var uri = BuildUri(directoryName, fileName);
		try
		{
			using var _ = factory.CreateAndRun(uri, WebRequestMethods.Ftp.DeleteFile);
		}
		catch (Exception ex)
		{
			var errorMessage = $"Failed to remove {uri} - {ex.Message}";
			Console.Error.WriteLine(errorMessage);
		}
	}

	Uri BuildUri(params string[] segments) =>
		new(ftpUri, string.Join("/", segments.Where(x => !string.IsNullOrEmpty(x)).Select(x => x.Trim('/'))));

	class InternalFtpRequestFactory : IFtpRequestFactory
	{
		public IFtpWebResponse CreateAndRun(Uri ftpUri, string ftpMethod)
		{
#pragma warning disable SYSLIB0014 // Type or member is obsolete
			var request = WebRequest.Create(ftpUri) as FtpWebRequest;
#pragma warning restore SYSLIB0014 // Type or member is obsolete
			request.Method = ftpMethod;

			return new InternalFtpWebResponseWrapper((FtpWebResponse)request.GetResponse());
		}
	}

	class InternalFtpWebResponseWrapper(FtpWebResponse ftpWebResponse) : IFtpWebResponse
	{
		readonly FtpWebResponse response = ftpWebResponse;

		public void Dispose() => response.Dispose();

		public Stream GetResponseStream() => response.GetResponseStream();
	}
}

