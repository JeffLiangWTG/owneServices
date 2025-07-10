using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transit.Business
{
	public class TWHDLLJobMatcher : TWHJobMatcher
	{
		public TWHDLLJobMatcher(BusinessObjectFactory factory, WhsWarehouse trw, string referenceNumber, ReferenceNumberTypes referenceNumberType)
		{
			Factory = factory;
			TransitWarehouse = trw;
			ReferenceNumber = referenceNumber;
			ReferenceNumberType = referenceNumberType;
		}

		protected override TWHJobMatcherResult InternalProcess()
		{
			var dll = FindDLL(TransitWarehouse, ReferenceNumberType, ReferenceNumber);
			if (dll != null)
			{
				var packageStates = TWHJobValidationHelper.GetValidPackageStates(dll.PackageStates);
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
			ReferenceNumberType == ReferenceNumberTypes.DispatchLoadList
			|| ReferenceNumberType == ReferenceNumberTypes.ForwardingConsolNumber
			|| ReferenceNumberType == ReferenceNumberTypes.MasterBill
			|| ReferenceNumberType == ReferenceNumberTypes.Unknown;

		WhsItemDispatchLoadList FindDLL(WhsWarehouse trw, ReferenceNumberTypes referenceNumberType, string reference)
		{
			switch (referenceNumberType)
			{
				case ReferenceNumberTypes.DispatchLoadList:
					return FindDLLViaDispatchLoadList(trw, reference);
				case ReferenceNumberTypes.ForwardingConsolNumber:
					return FindDLLViaForwardingConsolNumber(trw, reference);
				case ReferenceNumberTypes.MasterBill:
					return FindDLLViaMasterBill(trw, reference);
				case ReferenceNumberTypes.Unknown:
					var dll = FindDLLViaDispatchLoadList(trw, reference);
					if (dll != null)
					{
						ReferenceNumberType = ReferenceNumberTypes.DispatchLoadList;
						return dll;
					}

					dll = FindDLLViaForwardingConsolNumber(trw, reference);
					if (dll != null)
					{
						ReferenceNumberType = ReferenceNumberTypes.ForwardingConsolNumber;
						return dll;
					}

					dll = FindDLLViaMasterBill(trw, reference);
					if (dll != null)
					{
						ReferenceNumberType = ReferenceNumberTypes.MasterBill;
						return dll;
					}

					break;
			}

			return null;
		}

		WhsItemDispatchLoadList FindDLLViaDispatchLoadList(WhsWarehouse trw, string reference)
		{
			var query = new ZDBOnlyQuery(typeof(WhsItemDispatchLoadList));
			query.AddToFilter(WhsItemDispatchLoadListSchema.WDL_WW_Warehouse, trw.PK);
			query.AddToFilter(WhsItemDispatchLoadListSchema.WDL_CompleteTime, null);
			query.AddToFilter(WhsItemDispatchLoadListSchema.WDL_IsActive, true);
			query.AddToFilter(WhsItemDispatchLoadListSchema.WDL_JobID, reference);
			query.OrderBy = WhsItemDispatchLoadListSchema.Constants.WDL_SystemCreateTimeUtc + OrderByClause.Descending;

			return Factory.LoadTop1<WhsItemDispatchLoadList>(query);
		}

		WhsItemDispatchLoadList FindDLLViaForwardingConsolNumber(WhsWarehouse trw, string reference)
		{
			var query = new ZDBOnlyQuery(typeof(WhsItemDispatchLoadList));

			var forwardingConsolNumberQuery = new ZDBOnlySubQuery(typeof(ICusEntryNumber), CusEntryNumSchema.CE_ParentID);
			forwardingConsolNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			forwardingConsolNumberQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.AdditionalReferenceNumber);
			forwardingConsolNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, reference);

			query.AddToFilter(WhsItemDispatchLoadListSchema.WDL_WW_Warehouse, trw.PK);
			query.AddToFilter(WhsItemDispatchLoadListSchema.WDL_CompleteTime, null);
			query.AddToFilter(WhsItemDispatchLoadListSchema.WDL_IsActive, true);
			query.AddSubQuery(forwardingConsolNumberQuery, JoinCondition.And);
			query.OrderBy = WhsItemDispatchLoadListSchema.Constants.WDL_SystemCreateTimeUtc + OrderByClause.Descending;

			return Factory.LoadTop1<WhsItemDispatchLoadList>(query);
		}

		WhsItemDispatchLoadList FindDLLViaMasterBill(WhsWarehouse trw, string reference)
		{
			var query = new ZDBOnlyQuery(typeof(WhsItemDispatchLoadList));

			var masterbillQuery = new ZDBOnlySubQuery(typeof(ICusEntryNumber), CusEntryNumSchema.CE_ParentID);
			masterbillQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, AdditionalReferenceTypes.Codes.MasterBill);
			masterbillQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, reference);

			query.AddToFilter(WhsItemDispatchLoadListSchema.WDL_WW_Warehouse, trw.PK);
			query.AddToFilter(WhsItemDispatchLoadListSchema.WDL_CompleteTime, null);
			query.AddToFilter(WhsItemDispatchLoadListSchema.WDL_IsActive, true);
			query.AddSubQuery(masterbillQuery, JoinCondition.And);
			query.OrderBy = WhsItemDispatchLoadListSchema.Constants.WDL_SystemCreateTimeUtc + OrderByClause.Descending;

			return Factory.LoadTop1<WhsItemDispatchLoadList>(query);
		}
	}
}
