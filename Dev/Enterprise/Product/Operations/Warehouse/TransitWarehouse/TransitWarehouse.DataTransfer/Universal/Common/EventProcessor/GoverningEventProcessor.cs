using System;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using static Enterprise.Integration.Customs;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public abstract class GoverningEventProcessor
	{
		protected GoverningEventProcessor(UniversalEvent eventDataObject, IXmlImportLogger logger, UniversalObjectFactory factory) : this(eventDataObject, logger)
		{
			universalObjectFactory = Argument.NotNull(factory, nameof(factory));
		}

		protected GoverningEventProcessor(UniversalEvent eventDataObject, IXmlImportLogger logger)
		{
			this.eventDataObject = Argument.NotNull(eventDataObject, nameof(eventDataObject));
			this.logger = Argument.NotNull(logger, nameof(logger));
		}

		public abstract void Process(WhsItemReceiveConsignment consignment);
		public abstract void Process(WhsItemDispatchConsignment consignment);
		public abstract void Process(PkgPackage package);

		protected void AddCustomsReferenceIfNotExist(ICustomsReferenceCollection customsReferences, string eventParameterCode, string entryType)
		{
			var entryNum = GetEventParameteByCode(eventParameterCode);
			if (entryNum != "")
			{
				var entryNumber = customsReferences.First(entryType, entryNum);
				if (entryNumber == null)
				{
					entryNumber = (ICusEntryNumber)customsReferences.AddNew();
					entryNumber.CE_EntryNum = entryNum;
					entryNumber.CE_EntryType = entryType;
				}

				ZInt.TryParse(GetEventParameteByCode(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.OuterPackQuantity), out var outerPackageQuantity);
				ZInt.TryParse(GetEventParameteByCode(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.InnerPackQuantity), out var innerPackageQuantity);

				var cusEntryNum = entryNumber as BusinessObject;
				if (cusEntryNum != null)
				{
					var helper = new WhsTransitAdditionalReferencesHelper(logger, universalObjectFactory);
					helper.UpdateOrCreateAddOnValue(cusEntryNum, "TWOuterPackQty", "INT", outerPackageQuantity.ToString());
					helper.UpdateOrCreateAddOnValue(cusEntryNum, "TWInnerPackQty", "INT", innerPackageQuantity.ToString());
				}
			}
		}

		protected ICusEntryNumber CreateOrUpdatePortReference(IPortReferenceCollection portReferences, string entryType, string entryNum, string status)
		{
			var portReference = portReferences.First(entryType, entryNum);
			if (portReference == null)
			{
				portReference = (ICusEntryNumber)portReferences.AddNew();
				portReference.CE_EntryNum = entryNum;
				portReference.CE_EntryType = entryType;
			}
			portReference.CE_EntryStatus = status;

			return portReference;
		}

		protected ZString GetEventParameteByCode(string code) => EventParameters.GetEventParameter(code, eventDataObject.EventParameters, eventDataObject.EventReference).GetValueOrDefault();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Matching to xml data")]
		protected string ExpectedPortReferenceType => eventDataObject.DataContext?.DocumentaryOverride?.DocumentName.GetValueOrDefault().Contains("CRESA", StringComparison.OrdinalIgnoreCase) ?? false
			? TransitWarehousePortReferenceTypes.Codes.PortExport
			: TransitWarehousePortReferenceTypes.Codes.PortAuthority;

		protected readonly UniversalEvent eventDataObject;
		protected readonly IXmlImportLogger logger;
		protected readonly UniversalObjectFactory universalObjectFactory;
	}
}
