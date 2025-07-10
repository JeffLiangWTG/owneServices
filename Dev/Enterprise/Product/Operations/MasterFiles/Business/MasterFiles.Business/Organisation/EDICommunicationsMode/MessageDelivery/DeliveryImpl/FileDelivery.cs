using System;
using System.IO;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.MessageDelivery
{
	public class FileDelivery : Delivery
	{
		public override IDeliveryResult Deliver(DeliveryContext context, IEDICommunicationsMode mode, IDeliveryStreamWrapper stream, Func<Messaging.Integration.IEDIMessage> getMessageFunc = null)
		{
			if (stream.Content.Position == 1)
			{
				ErrorReporter.ReportOnce("Stream position ought to zero, but somehow it is one.");
			}

			try
			{
				stream.Content.WriteToFile(GetFileName(mode));
			}
			catch (Exception exception)
			{
				if (exception.IsCriticalException())
				{ throw; }

				if (mode.EK_LastFailed == ZDateTime.Empty || (ZDateTime.Now - mode.EK_LastFailed).Hours > 2)
				{
					new EmailNotifier().Notify(context.Factory, exception, mode, stream.Content, null);
					return DeliveryResult.Error(exception);
				}
			}

			return DeliveryResult.Success;
		}

		ZString GetFileName(IEDICommunicationsMode mode)
		{
			return Path.Combine(mode.EK_Destination, mode.EK_Filename);
		}
	}
}
