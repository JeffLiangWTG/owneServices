using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.US.InBond.Business.Universal
{
	public sealed class WarehouseInBondDataContextManager : ShipmentDataContextManager<CusInBondMoveHeader>
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.WarehouseInBond; }
		}

		public override ZString DataContextKey
		{
			get { return ParentBO.InBondNumberAndJobReference.Left(35); }
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new WarehouseInBondDataObjectReader(universalShipment, logger, factory);
		}

		public override bool ManagesShipments
		{
			get { return true; }
		}

		public override bool ManagesEvents
		{
			get { return true; }
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new WarehouseInBondDataObjectWriter(writeManager);
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

		bool IsSupportedTransportMode(ZString transportCode)
		{
			switch (transportCode)
			{
				case Enterprise.Customs.US.Business.TransportTypeList.Codes.Air:
				case Enterprise.Customs.US.Business.TransportTypeList.Codes.Rail:
				case Enterprise.Customs.US.Business.TransportTypeList.Codes.Truck:
				case Enterprise.Customs.US.Business.TransportTypeList.Codes.Sea:
				case Enterprise.Customs.US.Business.TransportTypeList.Codes.FixedTransportInstallations:
					return true;
				default:
					return false;
			}
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				var headerBO = ParentBO.Header;
				if (headerBO != null)
				{
					new InBondDataEventContextReader(ParentBO.Header).AddEventContextValues(result);
				}
				new WarehouseInBondDataEventContextReader(ParentBO).AddEventContextValues(result);
			}

			return result.Count == 0 ? null : result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new WarehouseInBondDataEventParentFinder(factory, this, logger);
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			ZQuery result = null;
			var elements = matchingValues.Key.Split('-');
			if (elements.Length == 2)
			{
				var inBondNumber = elements[0];
				var jobReference = elements[1];
				if (!inBondNumber.IsEmpty && !jobReference.IsEmpty)
				{
					var moveHeaderQuery = new ZDBOnlyQuery(typeof(CusInBondMoveHeader));
					var inBondNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
					inBondNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, Enterprise.Customs.US.Business.CusEntryHeaderMessageTypeList.Codes.InBond);
					inBondNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, inBondNumber);
					moveHeaderQuery.AddSubQuery(inBondNumberQuery, JoinCondition.And);
					var headerQuery = new ZDBOnlySubQuery(typeof(CusInBondHeader), CusInBondHeaderSchema.PK);
					headerQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.InBond);
					headerQuery.AddToFilter(CusInBondHeaderSchema.BH_JobReference, SQLComparisonOperator.StartsWith, jobReference);
					moveHeaderQuery.AddSubQuery(CusInBondMoveHeaderSchema.BM_BH, headerQuery, JoinCondition.And);
					moveHeaderQuery.OrderBy = CusInBondMoveHeaderSchema.BM_SystemCreateTimeUtc.Name;
					result = moveHeaderQuery;
				}
			}
			return result;
		}
	}
}
