using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transit.Business
{
	public class TWHDCNJobMatcher : TWHJobMatcher
	{
		public TWHDCNJobMatcher(BusinessObjectFactory factory, WhsWarehouse trw, string referenceNumber, ReferenceNumberTypes referenceNumberType)
		{
			Factory = factory;
			TransitWarehouse = trw;
			ReferenceNumber = referenceNumber;
			ReferenceNumberType = referenceNumberType;
		}

		protected override TWHJobMatcherResult InternalProcess()
		{
			var dcn = FindDCN(TransitWarehouse, ReferenceNumberType, ReferenceNumber);
			if (dcn != null)
			{
				var packageStates = TWHJobValidationHelper.GetValidPackageStates(dcn.PackageStates);
				var packageStateTotals = TWHJobValidationHelper.GetTotalsOfPackageStates(packageStates);

				return new TWHJobMatcherResult()
				{
					ReferenceNumber = ReferenceNumber,
					ReferenceNumberType = ReferenceNumberType,
					PackageStates = packageStates,
					GrossWeightValue = packageStateTotals.totalWeightInKG,
					GrossWeightUnit = Core.Constants.Weight.Kilograms,
					GrossVolumeValue = packageStateTotals.totalVolumeInM3,
					GrossVolumeUnit = Core.Constants.Volume.CubicMetres,
					QuantityValue = packageStateTotals.totalQuantity
				};
			}
			else
			{
				return new TWHJobMatcherResult()
				{
					ReferenceNumber = ReferenceNumber,
					ReferenceNumberType = ReferenceNumberType,
					ErrorCode = ValidationErrorCode.JNF
				};
			}
		}

		protected override bool IsReferenceTypeMatch() =>
			ReferenceNumberType == ReferenceNumberTypes.DispatchConsignment
			|| ReferenceNumberType == ReferenceNumberTypes.ForwardingShipmentNumber
			|| ReferenceNumberType == ReferenceNumberTypes.HouseBill
			|| ReferenceNumberType == ReferenceNumberTypes.Unknown;

		WhsItemDispatchConsignment FindDCN(WhsWarehouse trw, ReferenceNumberTypes referenceNumberType, string reference)
		{
			switch (referenceNumberType)
			{
				case ReferenceNumberTypes.DispatchConsignment:
					return FindDCNViaDispatchConsignment(trw, reference);
				case ReferenceNumberTypes.ForwardingShipmentNumber:
					return FindDCNViaForwardingShipmentNumber(trw, reference);
				case ReferenceNumberTypes.HouseBill:
					return FindDCNViaHouseBill(trw, reference);
				case ReferenceNumberTypes.Unknown:
					var dcn = FindDCNViaDispatchConsignment(trw, reference);
					if (dcn != null)
					{
						ReferenceNumberType = ReferenceNumberTypes.DispatchConsignment;
						return dcn;
					}

					dcn = FindDCNViaForwardingShipmentNumber(trw, reference);
					if (dcn != null)
					{
						ReferenceNumberType = ReferenceNumberTypes.ForwardingShipmentNumber;
						return dcn;
					}

					dcn = FindDCNViaHouseBill(trw, reference);
					if (dcn != null)
					{
						ReferenceNumberType = ReferenceNumberTypes.HouseBill;
						return dcn;
					}

					break;
			}

			return null;
		}

		WhsItemDispatchConsignment FindDCNViaDispatchConsignment(WhsWarehouse trw, string reference)
		{
			var query = new ZDBOnlyQuery(typeof(WhsItemDispatchConsignment));
			query.AddToFilter(WhsItemDispatchConsignmentSchema.WDC_CompleteTime, null);
			query.AddToFilter(WhsItemDispatchConsignmentSchema.WDC_WW_Warehouse, trw.PK);
			query.AddToFilter(WhsItemDispatchConsignmentSchema.WDC_JobID, reference);
			query.OrderBy = WhsItemDispatchConsignmentSchema.Constants.WDC_SystemCreateTimeUtc + OrderByClause.Descending;

			return Factory.LoadTop1<WhsItemDispatchConsignment>(query);
		}

		WhsItemDispatchConsignment FindDCNViaForwardingShipmentNumber(WhsWarehouse trw, string reference)
		{
			var query = new ZDBOnlyQuery(typeof(WhsItemDispatchConsignment));

			var forwardingShipmentNumberQuery = new ZDBOnlySubQuery(typeof(ICusEntryNumber), CusEntryNumSchema.CE_ParentID);
			forwardingShipmentNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber);
			forwardingShipmentNumberQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.AdditionalReferenceNumber);
			forwardingShipmentNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, reference);

			query.AddToFilter(WhsItemDispatchConsignmentSchema.WDC_CompleteTime, null);
			query.AddToFilter(WhsItemDispatchConsignmentSchema.WDC_WW_Warehouse, trw.PK);
			query.AddSubQuery(forwardingShipmentNumberQuery, JoinCondition.And);
			query.OrderBy = WhsItemDispatchConsignmentSchema.Constants.WDC_SystemCreateTimeUtc + OrderByClause.Descending;

			return Factory.LoadTop1<WhsItemDispatchConsignment>(query);
		}

		WhsItemDispatchConsignment FindDCNViaHouseBill(WhsWarehouse trw, string reference)
		{
			var query = new ZDBOnlyQuery(typeof(WhsItemDispatchConsignment));

			query.AddToFilter(WhsItemDispatchConsignmentSchema.WDC_CompleteTime, null);
			query.AddToFilter(WhsItemDispatchConsignmentSchema.WDC_WW_Warehouse, trw.PK);
			query.AddToFilter(WhsItemDispatchConsignmentSchema.WDC_HouseBillNumber, reference);
			query.OrderBy = WhsItemDispatchConsignmentSchema.Constants.WDC_SystemCreateTimeUtc + OrderByClause.Descending;

			return Factory.LoadTop1<WhsItemDispatchConsignment>(query);
		}
	}
}
