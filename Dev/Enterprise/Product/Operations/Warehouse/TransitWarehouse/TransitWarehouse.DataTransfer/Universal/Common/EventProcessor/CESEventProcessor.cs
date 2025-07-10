using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using static Enterprise.Integration.Customs;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class CESEventProcessor
	{
		public CESEventProcessor(UniversalEvent eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory)
		{
			this.eventDataObject = Argument.NotNull(eventDataObject, "eventDataObject");
			this.logger = Argument.NotNull(logger, "logger");
			this.factory = Argument.NotNull(factory, "factory");
		}

		public const string CustomsReasonEventParameterCode = "RES";
		public const string CustomsClearedCode = "CLR";

		#region Process

		public void Process(WhsItemReceiveConsignment consignment)
		{
			PopulateCustomsReleaseNumber(consignment);
		}

		#endregion

		#region PopulateCustomsReleaseNumber

		void PopulateCustomsReleaseNumber(WhsItemReceiveConsignment consignment)
		{
			var customsStatusCode = EventParameters.GetEventParameter(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, eventDataObject.EventParameters, eventDataObject.EventReference);
			var customsCleared = Res.GetString("184448ba-6665-4f95-bc17-deef1b6e5055", "Customs Cleared");

			if (customsStatusCode.Equals(CustomsClearedCode))
			{
				var customsReleaseReference = consignment.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
				if (customsReleaseReference != null)
				{
					customsReleaseReference.CE_EntryNum = customsCleared;
				}
				else
				{
					AddCustomsReleaseNumber(consignment, customsCleared);
				}
				logger.Log(LogType.Information, ResString.GetMultilingualString("b72c89d6-a576-4c57-9760-1595f17092d7", "Customs Release Number '{0}' has been added to Receive Consignment '{1}'. It is now Cleared for Release.", customsCleared, consignment.WRC_ConsignmentID));
			}
		}

		void AddCustomsReleaseNumber(WhsItemReceiveConsignment consignment, ZString entryNum)
		{
			var customsReleaseNumber = (ICusEntryNumber)consignment.CustomsReferenceNumbers.AddNew();
			customsReleaseNumber.CE_EntryType = TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber;
			customsReleaseNumber.CE_EntryNum = entryNum;
		}

		#endregion

		protected readonly UniversalEvent eventDataObject;
		protected readonly IXmlImportLogger logger;
		protected readonly BusinessObjectFactory factory;
	}
}
