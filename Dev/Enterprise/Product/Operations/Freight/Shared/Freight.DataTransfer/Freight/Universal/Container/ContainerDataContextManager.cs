using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ContainerDataContextManager : ShipmentDataContextManager<CommonContainer>
	{
		#region DataContext

		public override DataContextType DataContextType
		{
			get { return DataContextType.ForwardingContainer; }
		}

		public override ZString DataContextKey
		{
			get
			{
				return ParentBO.JC_ContainerJobID;
			}
		}

		public bool ShouldAddParentContextValues { get; set; } = true;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ZQuery(JobContainerSchema.JC_ContainerJobID, matchingValues.Key);
		}

		#endregion

		#region Shipment Management

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return null;
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new ContainerTopLevelDataObjectWriter(writeManager);
		}

		#endregion

		#region Event Management

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			List<KeyValuePair<TypeWithDescription, IZType>> result = null;
			if (ParentBO != null)
			{
				var references = new ContainerReferences(ParentBO, ShouldAddParentContextValues);
				result = references.GetEventContextValues();
			}

			return result == null || !result.Any() ? null : result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ContainerEventParentFinder(factory, this, logger);
		}

		protected override void OnUniversalEventAddedCore(IXmlSessionTracker logger, UniversalDataBuss.DataObjects.Universal.Event eventAdded)
		{
			OnUniversalEventAddedCore_UpdateContainerReferenceNumber(eventAdded);

			base.OnUniversalEventAddedCore(logger, eventAdded);
		}

		void OnUniversalEventAddedCore_UpdateContainerReferenceNumber(UniversalDataBuss.DataObjects.Universal.Event eventAdded)
		{
			if (IsContainerRelease(eventAdded))
			{
				HandleContainerRelease(eventAdded);
			}

			if (IsFRPortLDEorLPDNotification(eventAdded))
			{
				HandleFRPortLDEorLPDNotification(eventAdded);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const variable")]
		bool IsContainerRelease(UniversalDataBuss.DataObjects.Universal.Event eventAdded)
		{
			const string documentDepartment = "Terminal";
			const string documentType = "Container Release";
			const string documentFacility = "CTO";

			var eventDepartment = eventAdded.EventParameters?.Department.GetValueOrDefault() ?? ZString.Empty;
			var eventType = eventAdded.EventParameters?.Type.GetValueOrDefault() ?? ZString.Empty;
			var eventFacility = eventAdded.EventParameters?.Facility.GetValueOrDefault() ?? ZString.Empty;
			var eventLocation = eventAdded.EventParameters?.Location.GetValueOrDefault() ?? ZString.Empty;
			var consolArrivalCTOAddress = ParentBO.Consol?.ArrivalCTOAddress;
			var consolArrivalCTOAddressUnloco = consolArrivalCTOAddress?.Header?.UNLOCO?.Code ?? consolArrivalCTOAddress?.HeaderClosestPort?.Code ?? ZString.Empty;

			return string.Equals(eventDepartment, documentDepartment, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(eventType, documentType, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(eventFacility, documentFacility, StringComparison.OrdinalIgnoreCase)
				&& eventLocation == consolArrivalCTOAddressUnloco;
		}

		void HandleContainerRelease(UniversalDataBuss.DataObjects.Universal.Event eventAdded)
		{
			var eventType = eventAdded.EventType.GetValueOrDefault();
			var eventReferenceNumber = eventAdded.EventParameters.ReferenceNumber.GetValueOrDefault();

			if (eventType == Events.AuthorisedCode
				&& !eventReferenceNumber.IsEmpty)
			{
				ParentBO.JC_ContainerImportDORelease = eventReferenceNumber;
			}
			else if (eventType == Events.AuthorisationWithdrawnCode)
			{
				ParentBO.JC_ContainerImportDORelease = ZString.Empty;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const variable")]
		bool IsFRPortLDEorLPDNotification(UniversalDataBuss.DataObjects.Universal.Event eventAdded)
		{
			const string documentDepartment = "Terminal";
			const string ldeDocumentType = Core.Constants.EventReferenceParameterTypes.LDENotification;
			const string lpdDocumentType = Core.Constants.EventReferenceParameterTypes.LPDNotification;

			var eventDepartment = eventAdded.EventParameters?.Department.GetValueOrDefault() ?? ZString.Empty;
			var eventType = eventAdded.EventParameters?.Type.GetValueOrDefault() ?? ZString.Empty;
			var isLDEOrLPDDocument = string.Equals(eventType, ldeDocumentType, StringComparison.OrdinalIgnoreCase) || string.Equals(eventType, lpdDocumentType, StringComparison.OrdinalIgnoreCase);

			return string.Equals(eventDepartment, documentDepartment, StringComparison.OrdinalIgnoreCase) && isLDEOrLPDDocument;
		}

		void HandleFRPortLDEorLPDNotification(UniversalDataBuss.DataObjects.Universal.Event eventAdded)
		{
			var eventType = eventAdded.EventType.GetValueOrDefault();
			var eventParameterType = eventAdded.EventParameters?.Type.GetValueOrDefault() ?? ZString.Empty;
			var customsReferenceNumber = eventAdded.EventParameters?.CustomsReferenceNumber.GetValueOrDefault() ?? ZString.Empty;

			if (string.Equals(eventType, Events.StatusUpdatedCode, StringComparison.OrdinalIgnoreCase) && !customsReferenceNumber.IsEmpty)
			{
				switch (eventParameterType)
				{
					case Core.Constants.EventReferenceParameterTypes.LDENotification:
						ParentBO.JC_ExportDepotCustomsReference = customsReferenceNumber;
						break;
					case Core.Constants.EventReferenceParameterTypes.LPDNotification:
						ParentBO.JC_ImportDepotCustomsReference = customsReferenceNumber;
						break;
				}
			}
		}

		#endregion

		#region Implementation

		public override string DefaultOutputDirectory
		{
			get { return SystemDataRegistry.Instance.ShipmentExportDirectory.Value; }
		}

		public override bool ManagesShipments
		{
			get { return true; }
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		#endregion
	}
}
