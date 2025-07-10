using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using static Enterprise.Core.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public abstract class WhsTransitConsignmentDataContextManager<T> : ShipmentDataContextManager<T>
		where T : BusinessObject, IConsignment
	{
		#region Context

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		#endregion

		#region Events

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var contextValues = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				if (ParentBO.TransportMode == TransportModes.Air)
				{
					contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.HAWBNumber, ParentBO.HouseBillNumber);
					contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.MAWBNumber, ParentBO.MasterBillNumber);
				}
				else
				{
					contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.HBOLNumber, ParentBO.HouseBillNumber);
					contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.MBOLNumber, ParentBO.MasterBillNumber);
				}
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.CFSReference, ParentBO.JobID);
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.DepotCode, ParentBO.Warehouse.WarehouseAddress.OA_Code);
			}

			return contextValues;
		}

		#endregion

		protected override IEnumerable<IUniversalJobLink> GetEventDataTargetCore(RecipientRoleType recipientRoleType, IOrgHeader recipientOrganisation)
		{
			DataContextType? dataContextType;
			if (TryGetDataContextTypeForRecipientRole(recipientRoleType, out dataContextType))
			{
				var links = UniversalJobLinkHelper.GetMatchingJobLinks(ParentBO, dataContextType.Value, recipientOrganisation);
				return links.Any() ? links : base.GetEventDataTargetCore(recipientRoleType, recipientOrganisation);
			}
			return base.GetEventDataTargetCore(recipientRoleType, recipientOrganisation);
		}

		bool TryGetDataContextTypeForRecipientRole(RecipientRoleType recipientRoleType, out DataContextType? contextType)
		{
			switch (recipientRoleType)
			{
				case RecipientRoleType.FOR:
					contextType = DataContextType.ForwardingShipment;
					break;
				case RecipientRoleType.TPC:
					contextType = DataContextType.LandTransportConsignment;
					break;
				default:
					contextType = null;
					break;
			}
			return contextType != null;
		}

		#region Shipments

		public override bool ManagesShipments
		{
			get { return true; }
		}

		#endregion

		#region Recipient Roles

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			if (recipientRoles != null)
			{
				foreach (var roleType in SupportedRoleTypes)
				{
					if (recipientRoles.Any(o => o.Code == roleType && o.ServiceCode.HasValue && SupportedServiceTypes.Contains(o.ServiceCode.Value)))
					{
						return !dataSources.IsSupportedJob();
					}
				}
			}

			return false;
		}

		protected IEnumerable<RecipientRoleType> SupportedRoleTypes => new[] { RecipientRoleType.DTW, RecipientRoleType.ATW };
		protected abstract IEnumerable<ServiceCodeType> SupportedServiceTypes { get; }

		#endregion
	}
}
