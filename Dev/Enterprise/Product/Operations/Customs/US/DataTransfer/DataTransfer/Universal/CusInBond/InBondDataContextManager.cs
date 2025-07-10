using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	public abstract class InBondDataContextManager<THeader, TBill, TMoveHeader, TMoveDetail, TContainer, TCommodity> : ShipmentDataContextManager<THeader>
		where THeader : Customs.Business.CusInBondHeader
		where TBill : Customs.Business.CusInBondBill
		where TMoveHeader : CusInBondMoveHeader
		where TMoveDetail : CusInBondMoveDetail
		where TContainer : Customs.Business.CusInBondContainer
		where TCommodity : Customs.Business.CusInBondCargoDesc
	{
		public override ZString DataContextKey
		{
			get { return ParentBO.BH_JobReference; }
		}

		protected sealed override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			if (ParentDataContextTypes.Any(p => (universalShipment.GetMatchingDataTarget(p) != null)))
			{
				return null; // Should let the parent context manager process this universal shipment and hence stopping the system from processing this twice.
			}
			else
			{
				return GetShipmentDataObjectReaderCore(universalShipment, logger, factory, null);
			}
		}

		protected abstract List<DataContextType> ParentDataContextTypes { get; }

		protected abstract CusInBondHeaderDataObjectReader<THeader, TBill, TMoveHeader, TMoveDetail, TContainer, TCommodity> GetShipmentDataObjectReaderCore(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory, ICusInBondParent parentBO);

		public override bool ManagesShipments
		{
			get { return true; }
		}

		public override bool ManagesEvents
		{
			get { return false; }
		}

		public override string DefaultOutputDirectory
		{
			get { return ""; }
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		protected override bool TryGetMatchingDataTarget(UniversalShipment universalShipment, out IDataTargetDataObject dataTarget)
		{
			dataTarget = null;
			return IsSupportedTransportMode(universalShipment.TransportMode.GetCodeAsUpperCase()) && base.TryGetMatchingDataTarget(universalShipment, out dataTarget);
		}

		protected virtual ZBool IsSupportedTransportMode(ZString transportCode)
		{
			return false;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return null;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return null;
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var query = new ZQuery(CusInBondHeaderSchema.BH_JobReference, matchingValues.Key);
			query.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCode);
			return query;
		}

		protected abstract ZString CusInBondApplicationCode { get; }
	}
}
