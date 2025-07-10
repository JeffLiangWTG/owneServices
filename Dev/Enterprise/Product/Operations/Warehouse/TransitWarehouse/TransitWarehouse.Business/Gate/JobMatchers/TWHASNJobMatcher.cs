using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transit.Business
{
	public class TWHASNJobMatcher : TWHJobMatcher
	{
		public TWHASNJobMatcher(BusinessObjectFactory factory, WhsWarehouse trw, string referenceNumber, ReferenceNumberTypes referenceNumberType)
		{
			Factory = factory;
			TransitWarehouse = trw;
			ReferenceNumber = referenceNumber;
			ReferenceNumberType = referenceNumberType;
		}

		protected override TWHJobMatcherResult InternalProcess()
		{
			var asn = FindASN(TransitWarehouse, ReferenceNumberType, ReferenceNumber);
			if (asn != null)
			{
				var packageStates = asn.PackageStates.Where(wps => wps.WPS_Status == TransitWarehouseStatuses.Codes.Booked);
				var packageStateTotals = TWHJobValidationHelper.GetTotalsOfPackageStates(packageStates);

				var errorMessage = TWHJobValidationHelper.ValidateDGPackageNotExceedCore(packageStates, TransitWarehouse);
				if (errorMessage != null)
				{
					return new TWHJobMatcherResult()
					{
						ReferenceNumber = ReferenceNumber,
						ReferenceNumberType = ReferenceNumberType,
						ErrorCode = ValidationErrorCode.VDG,
						ErrorMessage = errorMessage
					};
				}

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
			ReferenceNumberType == ReferenceNumberTypes.AdvancedShippingNotice
			|| ReferenceNumberType == ReferenceNumberTypes.ForwardingConsolNumber
			|| ReferenceNumberType == ReferenceNumberTypes.MasterBill
			|| ReferenceNumberType == ReferenceNumberTypes.Unknown;

		WhsItemReceiveASN FindASN(WhsWarehouse trw, ReferenceNumberTypes referenceNumberType, string reference)
		{
			switch (referenceNumberType)
			{
				case ReferenceNumberTypes.AdvancedShippingNotice:
					return FindASNViaAdvancedShippingNotice(trw, reference);
				case ReferenceNumberTypes.ForwardingConsolNumber:
					return FindASNViaForwardingConsolNumber(trw, reference);
				case ReferenceNumberTypes.MasterBill:
					return FindASNViaMasterBill(trw, reference);
				case ReferenceNumberTypes.Unknown:
					var asn = FindASNViaAdvancedShippingNotice(trw, reference);
					if (asn != null)
					{
						ReferenceNumberType = ReferenceNumberTypes.AdvancedShippingNotice;
						return asn;
					}

					asn = FindASNViaForwardingConsolNumber(trw, reference);
					if (asn != null)
					{
						ReferenceNumberType = ReferenceNumberTypes.ForwardingConsolNumber;
						return asn;
					}

					asn = FindASNViaMasterBill(trw, reference);
					if (asn != null)
					{
						ReferenceNumberType = ReferenceNumberTypes.MasterBill;
						return asn;
					}

					break;
			}

			return null;
		}

		WhsItemReceiveASN FindASNViaAdvancedShippingNotice(WhsWarehouse trw, string reference)
		{
			var query = new ZDBOnlyQuery(typeof(WhsItemReceiveASN));

			query.AddToFilter(WhsItemReceiveASNSchema.WRP_WW_IntendedWarehouse, trw.PK);
			query.AddToFilter(WhsItemReceiveASNSchema.WRP_CompleteTime, null);
			query.AddToFilter(WhsItemReceiveASNSchema.WRP_ReferenceNumber, reference);
			query.OrderBy = WhsItemReceiveASNSchema.Constants.WRP_SystemCreateTimeUtc + OrderByClause.Descending;

			return Factory.LoadTop1<WhsItemReceiveASN>(query);
		}

		WhsItemReceiveASN FindASNViaForwardingConsolNumber(WhsWarehouse trw, string reference)
		{
			var query = new ZDBOnlyQuery(typeof(WhsItemReceiveASN));

			var forwardingConsolNumberQuery = new ZDBOnlySubQuery(typeof(ICusEntryNumber), CusEntryNumSchema.CE_ParentID);
			forwardingConsolNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			forwardingConsolNumberQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.AdditionalReferenceNumber);
			forwardingConsolNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, reference);

			query.AddToFilter(WhsItemReceiveASNSchema.WRP_WW_IntendedWarehouse, trw.PK);
			query.AddToFilter(WhsItemReceiveASNSchema.WRP_CompleteTime, null);
			query.AddSubQuery(forwardingConsolNumberQuery, JoinCondition.And);
			query.OrderBy = WhsItemReceiveASNSchema.Constants.WRP_SystemCreateTimeUtc + OrderByClause.Descending;

			return Factory.LoadTop1<WhsItemReceiveASN>(query);
		}

		WhsItemReceiveASN FindASNViaMasterBill(WhsWarehouse trw, string reference)
		{
			var query = new ZDBOnlyQuery(typeof(WhsItemReceiveASN));

			var masterbillQuery = new ZDBOnlySubQuery(typeof(ICusEntryNumber), CusEntryNumSchema.CE_ParentID);
			masterbillQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, AdditionalReferenceTypes.Codes.MasterBill);
			masterbillQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, reference);

			query.AddToFilter(WhsItemReceiveASNSchema.WRP_WW_IntendedWarehouse, trw.PK);
			query.AddToFilter(WhsItemReceiveASNSchema.WRP_CompleteTime, null);
			query.AddSubQuery(masterbillQuery, JoinCondition.And);
			query.OrderBy = WhsItemReceiveASNSchema.Constants.WRP_SystemCreateTimeUtc + OrderByClause.Descending;

			return Factory.LoadTop1<WhsItemReceiveASN>(query);
		}
	}
}
