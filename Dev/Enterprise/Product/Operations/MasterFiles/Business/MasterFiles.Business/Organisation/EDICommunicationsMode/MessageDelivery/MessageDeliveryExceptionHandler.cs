using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class MessageDeliveryExceptionHandler
	{
		public static IDeliveryResult DoWithExceptionHandling(DeliveryContext context, IErrorNotifier<IEDICommunicationsMode> errorNotifier, Func<string> getName, IEDICommunicationsMode mode, IEnumerable<IDeliveryStreamWrapper> streams, Action deliveryMethod)
		{
			Argument.NotNull(context, nameof(context));
			Argument.NotNull(mode, nameof(mode));
			Argument.NotNull(deliveryMethod, nameof(deliveryMethod));

			try
			{
				deliveryMethod();
				return DeliveryResult.Success;
			}
			catch (Exception exception) when (!exception.IsCriticalException())
			{
				if (context.Notifications == null && errorNotifier == null)
				{
					ErrorReporter.ReportOnce("", "Exception happened in " + getName() + " on delivery, and there is no notifiers available to notify users." + GetModeDetails(mode), exception);
				}

				if (context.Notifications != null)
				{
					context.Notifications.AddMessageError((NoResString)"Error delivering message: " + exception.Message + (NoResString)"." + GetModeDetails(mode) + (NoResString)"\r\n\r\nException detail:\r\n" + exception.ToString());
				}

				if (errorNotifier != null)
				{
					var environmentStackTrace = System.Environment.StackTrace;

					foreach (var stream in streams)
					{
						errorNotifier.Notify(context.Factory, exception, environmentStackTrace, mode, stream.Content, null);
					}
				}

				return DeliveryResult.Error(exception);
			}
		}

		public static string GetModeDetails(IEDICommunicationsMode mode)
		{
			string modeDetails = string.Empty;
			if (mode != null)
			{
				modeDetails = string.Format(
					CultureInfo.InvariantCulture,
					(NoResString)"\r\nDelivery mode details:\r\nTransport: {0}\r\nFile format: {1}\r\nPurpose: {2}\r\nDestination: {3}\r\nOrganization: {4}\r\nFile name: {5}",
					mode.EK_CommunicationsTransport,
					mode.EK_FileFormat,
					mode.EK_MessagePurpose,
					mode.EK_Destination,
					mode.Organisation != null ? (string)mode.Organisation.OH_Code : string.Empty,
					mode.EK_Filename);
			}
			return modeDetails;
		}
	}
}
