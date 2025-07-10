using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using IStowPlanMessage = Enterprise.Integration.Customs.US.USAMS.IStowPlanMessage;

namespace Enterprise.Freight.Business
{
	public static class StowPlanLicenceLoggingExtensions
	{
		public static void LogSTWLicense(this IEDIMessageCollectionProvider provider)
		{
			var latestSTWResponse = provider.Messages.OfType<IStowPlanMessage>()
				.Where(x => ((EDIMessage)x).EM_ReceiveTransmit == Enterprise.Messaging.Business.EDIMessage.Direction.Receive)
				.OrderByDescending(x => ((EDIMessage)x).EM_SystemCreateTimeUtc).FirstOrDefault();
			if (latestSTWResponse != null)
			{
				var logger = ObjectFactory.Get<ILicenceConsumptionLogCreator>();
				for (int i = 0; i < latestSTWResponse.CountAcceptedContainersWhichPreviouslyNotAccepted(); i++)
				{
					logger.CreateLog(Env.Licence.StowPlanReporting, true);
				}
			}
		}

		public static bool HasUserDefinedValueChanged(this BusinessObject bizObj, ZString propertyName)
		{
			var property = bizObj.GetUserDefinedProperty(propertyName, null);
			return (!property.IsInDatabase && !property.XV_Data.IsEmpty)
				|| (property.IsInDatabase && !property.XV_DataInfo.OriginalValue.Equals(property.XV_Data));
		}
	}
}
