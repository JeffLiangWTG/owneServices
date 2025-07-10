using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.US.LVS.DataTransfer.Universal
{
	public class USLVClearanceDataContextManager : ShipmentDataContextManager<CusUSLVClearance>, IEventDataContextManagerWithTriggeringLog
	{
		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			var subShipment = universalShipment?.SubShipmentCollection?.FirstOrDefault();
			if (subShipment != null && (subShipment.ShipmentType.GetNullableCodeAsUpperCase()).Equals(ShipmentTypes.HighVolumeLowValue))
			{
				return new eTailUSLVClearanceDataObjectReader(universalShipment, subShipment, logger, factory);
			}

			return new USLVClearanceDataReader(universalShipment, logger, factory);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new USLVClearanceDataWriter(writeManager);
		}

		public override bool ManagesShipments
		{
			get { return true; }
		}

		public override bool ManagesEvents
		{
			get { return true; }
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				var houseBillNumber = string.Empty;
				var triggeringLog = ((IEventDataContextManagerWithTriggeringLog)this).TriggeringLogForUseInPopulatingEventContext;

				if (triggeringLog != null)
				{
					triggeringLog.Parameters.TryGetValue(EventReferenceParameters.Codes.ReferenceNumber, out houseBillNumber);
					if (!string.IsNullOrEmpty(houseBillNumber))
					{
						var matchedConsignment = ParentBO.CusUSLVConsignments.OfType<CusUSLVConsignment>().FirstOrDefault(c => c.ULB_HouseBill == houseBillNumber);
						if (matchedConsignment != null)
						{
							result.AddIfNotEmpty(UniversalEvent.ContextTypes.MBOLNumber, ParentBO.ULH_MasterBill);
							result.AddIfNotEmpty(UniversalEvent.ContextTypes.MBOLOriginUNLOCO, ParentBO.ULH_RL_NKPortOfLoading);
							result.AddIfNotEmpty(UniversalEvent.ContextTypes.MBOLDestinationUNLOCO, ParentBO.ULH_RL_NKPortOfDischarge);
							result.AddIfNotEmpty(UniversalEvent.ContextTypes.MBOLPortOfLoadingScheduleK, ParentBO.ULH_PortOfLoading);
							result.AddIfNotEmpty(UniversalEvent.ContextTypes.MBOLPortOfDischargeScheduleD, ParentBO.ULH_PortOfDischarge);
							result.AddIfNotEmpty(UniversalEvent.ContextTypes.MBOLPortOfEntryScheduleD, ParentBO.ULH_PortOfEntry);
							result.AddIfNotEmpty(UniversalEvent.ContextTypes.USLowValueEntriesMatchingKey, ParentBO.ULH_MatchingKey);
							result.AddIfNotEmpty(UniversalEvent.ContextTypes.USLowValueEntriesUseCode, ParentBO.ULH_UseCode);
							result.AddIfNotEmpty(UniversalEvent.ContextTypes.CustomsReleaseDate, matchedConsignment.CE_IssueDate);
							result.AddIfNotEmpty(UniversalEvent.ContextTypes.HBOLNumber, matchedConsignment.ULB_HouseBill);

							triggeringLog.Parameters.TryGetValue(EventReferenceParameters.Codes.New, out string complianceStatus);
							result.AddIfNotEmpty(UniversalEvent.ContextTypes.ComplianceStatus, (ZString)complianceStatus);
						}
					}
				}
			}
			return result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return null;
		}

		public override ZString DataContextKey
		{
			get { return ParentBO.ULH_JobNumber; }
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.USCustomsLowValueEntriesClearance; }
		}

		public override string DefaultOutputDirectory
		{
			get { return string.Empty; }
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var result = new ZQuery(CusUSLVClearanceSchema.ULH_JobNumber, matchingValues.Key);
			return result;
		}

		#region IEventDataContextManagerWithTriggeringLog Members

		BaseStmALog IEventDataContextManagerWithTriggeringLog.TriggeringLogForUseInPopulatingEventContext { get; set; }

		#endregion
	}
}
