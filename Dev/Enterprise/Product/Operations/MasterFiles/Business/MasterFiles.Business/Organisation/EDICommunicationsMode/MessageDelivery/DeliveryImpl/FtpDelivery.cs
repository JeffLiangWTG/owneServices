using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.MessageDelivery
{
	public class FtpDelivery : Delivery
	{
		const int Tries = 10;
		const int Pause = 5;

		public override IDeliveryResult Deliver(DeliveryContext context, IEDICommunicationsMode mode, IDeliveryStreamWrapper stream, Func<Messaging.Integration.IEDIMessage> getMessageFunc = null)
		{
			var remoteFilename = GetRemoteFilename(mode);
			try
			{
				stream.PopulateStream(context.Factory);

				if (stream.Content.Position == 1)
				{
					ErrorReporter.ReportOnce("Stream position ought to zero, but somehow it is one.");
				}

				var streamPosition = stream.Content.Position;
				var ftpProcessor = ObjectFactory.Get<IFtpProcessor>();
				var credentials = new NetworkCredential(mode.EK_LoginName, mode.EK_Password);
				ftpProcessor.ServerName = GetTargetUri(mode);
				ftpProcessor.Username = credentials.UserName;
				ftpProcessor.Password = credentials.Password;
				ftpProcessor.ReadTimeout = TimeSpan.FromMinutes(RawDataRegistry.Instance.FTPReadTimeout.Value);
				ftpProcessor.ConnectTimeout = TimeSpan.FromMinutes(RawDataRegistry.Instance.FTPConnectionTimeout.Value);
				ftpProcessor.UploadStreamSeveralAttempts(stream.Content, remoteFilename, Tries, Pause);
				// TODO: Might be able to remove
				stream.Content.Position = streamPosition;
			}
			catch (Exception exception) when (!exception.IsCriticalException())
			{
				new EmailNotifier().Notify(context.Factory, exception, mode, stream.Content, null);
				return DeliveryResult.Error(exception);
			}

			return DeliveryResult.Success;
		}

		static string GetTargetUri(IEDICommunicationsMode mode)
		{
			var ftpProtocolId = Uri.UriSchemeFtp + Uri.SchemeDelimiter;

			if (!mode.EK_Destination.StartsWith(ftpProtocolId, StringComparison.Ordinal))
			{
				throw new NotSupportedException("Only FTP protocols are supported. URI must start with ftp://");
			}

			var uri = new Uri(mode.EK_Destination);
			if (mode.EK_PortNumber != 0 && mode.EK_PortNumber != 21)
			{
				uri = new Uri(ftpProtocolId + uri.Host + ":" + mode.EK_PortNumber + "/" + uri.AbsolutePath);
			}

			return uri.ToString();
		}

		[SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Path names are exempt... for now")]
		string GetRemoteFilename(IEDICommunicationsMode mode)
		{
			var desiredFileName = mode.EK_Filename;
			if (desiredFileName.IsEmpty)
			{
				desiredFileName = (NoResString)"CargoWise One Uploaded file.txt";
			}
			return desiredFileName;
		}
	}
}
