using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal
{
	public class CYDYardUnitStateDataContextManager : EventDataContextManager<CYDYardUnitState>, IEventDataContextManagerWithTriggeringLog
	{
		public override DataContextType DataContextType => DataContextType.CYDYardUnitState;

		public override ZString DataContextKey => ParentBO.YUS_UnitID;

		public override string DefaultOutputDirectory => null;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var query = new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, matchingValues.Key);
			query.OrderBy = CYDYardUnitStateSchema.YUS_SystemCreateTimeUtc.Name;

			return query;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				result.AddIfNotEmpty(Event.ContextTypes.ContainerNumber, ParentBO.YUS_UnitID);
				result.AddIfNotEmpty(Event.ContextTypes.ContainerISOCode, ParentBO.Container.RC_ISOType);
				result.AddIfNotEmpty(Event.ContextTypes.ContainerGrossWeight, ParentBO.Container.RC_GrossWeight);
				result.AddIfNotEmpty(Event.ContextTypes.ContainerWeightUnit, new ZString(Constants.Weight.Kilograms));
				result.AddIfNotEmpty(Event.ContextTypes.PortUNLOCO, ParentBO.CurrentYard.RelatedCompanyBranch.HomePort.Code);
				result.AddIfNotEmpty(Event.ContextTypes.WarehouseCode, ParentBO.CurrentYard.WW_WarehouseCode);
				result.AddIfNotEmpty(Event.ContextTypes.ClientCode, ParentBO.ReceiveAdvice.Client.Organisation.OH_Code);

				var triggeringLog = ((IEventDataContextManagerWithTriggeringLog)this).TriggeringLogForUseInPopulatingEventContext;
				if (triggeringLog is not null)
				{
					var eventCode = triggeringLog.SL_SE_NKEvent;
					if (eventCode == EventCodes.GateOut)
					{
						result.AddIfNotEmpty(Event.ContextTypes.ContainerReleaseNumber,
							ParentBO.ReleaseAdviceLine.ReleaseAdvice.YRE_ReleaseNumber);
						result.AddIfNotEmpty(Event.ContextTypes.GateOutTime,
							ParentBO.DispatchTransportationUnit.YTU_GateOutTime);
						result.AddIfNotEmpty(Event.ContextTypes.IsEmptyContainer,
							ParentBO.ReleaseAdviceLine.UnitLineItem.YLI_IsEmpty);
						result.AddIfNotEmpty(Event.ContextTypes.TransportReference,
							ParentBO.DispatchTransportationUnit.YTU_TransportationReference);
					}
					else if (eventCode == EventCodes.GateIn)
					{
						result.AddIfNotEmpty(Event.ContextTypes.ContainerAcceptanceNumber,
							ParentBO.ReceiveAdviceLine.ReceiveAdvice.YRA_AcceptanceNumber);
						result.AddIfNotEmpty(Event.ContextTypes.GateInTime,
							ParentBO.ReceiveTransportationUnit.YTU_GateInTime);
						result.AddIfNotEmpty(Event.ContextTypes.IsEmptyContainer,
							ParentBO.ReceiveAdviceLine.UnitLineItem.YLI_IsEmpty);
						result.AddIfNotEmpty(Event.ContextTypes.TransportReference,
							ParentBO.ReceiveTransportationUnit.YTU_TransportationReference);
					}
				}
			}

			return result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) => new CYDYardUnitStateEventParentFinder(this, factory, logger);

		#region IEventDataContextManagerWithTriggeringLog Members

		public BaseStmALog TriggeringLogForUseInPopulatingEventContext
		{
			get;
			set;
		}

		#endregion
	}
}
