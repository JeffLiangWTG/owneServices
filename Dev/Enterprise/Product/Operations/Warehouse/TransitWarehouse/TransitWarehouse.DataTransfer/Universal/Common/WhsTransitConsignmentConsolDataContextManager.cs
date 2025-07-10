using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public abstract class WhsTransitConsignmentConsolDataContextManager<T> : ShipmentDataContextManager<T>
		where T : NonPersistentBusinessObject
	{
		#region DefaultOutputDirectory

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		#endregion

		#region DataContextKey

		public override ZString DataContextKey
		{
			get { return ((IJobNumber)ParentBO).JobNumber; }
		}

		#endregion

		#region EventContextValues

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		#endregion

		#region EventParentFinder

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return null;
		}

		#endregion

		#region GetDataContextKeyMatchingQuery

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory,
			IXmlImportLogger logger)
		{
			return ZQuery.NoResultQuery;
		}

		#endregion

		#region GetShipmentDataObjectWriter

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return null;
		}

		#endregion

		#region RecipientRoleTargettedToThisModule

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			if (recipientRoles != null)
			{
				foreach (var roleType in SupportedRoleTypes)
				{
					if (recipientRoles.Any(o => o.Code == roleType && o.ServiceCode.HasValue && SupportedServiceTypes.Contains(o.ServiceCode.Value)))
					{
						return dataSources.IsSupportedJob();
					}
				}
			}

			return false;
		}

		protected IEnumerable<RecipientRoleType> SupportedRoleTypes => new[] { RecipientRoleType.DTW, RecipientRoleType.ATW };
		protected abstract IEnumerable<ServiceCodeType> SupportedServiceTypes { get; }

		#endregion

		#region ManagesShipments 

		public override bool ManagesShipments { get { return true; } }

		#endregion

		#region ManagesEvents

		public override bool ManagesEvents { get { return false; } }

		#endregion
	}
}
