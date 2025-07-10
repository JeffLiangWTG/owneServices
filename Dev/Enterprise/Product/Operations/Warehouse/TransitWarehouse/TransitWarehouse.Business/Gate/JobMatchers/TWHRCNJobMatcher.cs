using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transit.Business
{
	public class TWHRCNJobMatcher : TWHJobMatcher
	{
		public TWHRCNJobMatcher(BusinessObjectFactory factory, WhsWarehouse trw, string referenceNumber, ReferenceNumberTypes referenceNumberType)
		{
			Factory = factory;
			TransitWarehouse = trw;
			ReferenceNumber = referenceNumber;
			ReferenceNumberType = referenceNumberType;
		}

		protected override TWHJobMatcherResult InternalProcess()
		{
			var rcn = FindRCN(TransitWarehouse, ReferenceNumberType, ReferenceNumber);
			if (rcn != null)
			{
				var packageStates = rcn.PackageStates.Where(wps => wps.WPS_Status == TransitWarehouseStatuses.Codes.Booked);
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
			ReferenceNumberType == ReferenceNumberTypes.ForwardingShipmentNumber
			|| ReferenceNumberType == ReferenceNumberTypes.ReceiveConsignment
			|| ReferenceNumberType == ReferenceNumberTypes.HouseBill
			|| ReferenceNumberType == ReferenceNumberTypes.Unknown;

		WhsItemReceiveConsignment FindRCN(WhsWarehouse trw, ReferenceNumberTypes referenceNumberType, string reference)
		{
			switch (referenceNumberType)
			{
				case ReferenceNumberTypes.ReceiveConsignment:
					return FindRCNViaReceiveConsignment(trw, reference);
				case ReferenceNumberTypes.ForwardingShipmentNumber:
					return FindRCNViaForwardingShipmentNumber(trw, reference);
				case ReferenceNumberTypes.HouseBill:
					return FindRCNViaHouseBill(trw, reference);
				case ReferenceNumberTypes.Unknown:
					var rcn = FindRCNViaReceiveConsignment(trw, reference);
					if (rcn != null)
					{
						ReferenceNumberType = ReferenceNumberTypes.ReceiveConsignment;
						return rcn;
					}

					rcn = FindRCNViaForwardingShipmentNumber(trw, reference);
					if (rcn != null)
					{
						ReferenceNumberType = ReferenceNumberTypes.ForwardingShipmentNumber;
						return rcn;
					}

					rcn = FindRCNViaHouseBill(trw, reference);
					if (rcn != null)
					{
						ReferenceNumberType = ReferenceNumberTypes.HouseBill;
						return rcn;
					}

					break;
			}

			return null;
		}

		WhsItemReceiveConsignment FindRCNViaReceiveConsignment(WhsWarehouse trw, string reference)
		{
			var query = new ZDBOnlyQuery(typeof(WhsItemReceiveConsignment));

			query.AddToFilter(WhsItemReceiveConsignmentSchema.WRC_CompleteTime, null);
			query.AddToFilter(WhsItemReceiveConsignmentSchema.WRC_WW_IntendedWarehouse, trw.PK);
			query.AddToFilter(WhsItemReceiveConsignmentSchema.WRC_JobID, reference);
			query.OrderBy = WhsItemReceiveConsignmentSchema.Constants.WRC_SystemCreateTimeUtc + OrderByClause.Descending;

			return Factory.LoadTop1<WhsItemReceiveConsignment>(query);
		}

		WhsItemReceiveConsignment FindRCNViaForwardingShipmentNumber(WhsWarehouse trw, string reference)
		{
			var query = new ZDBOnlyQuery(typeof(WhsItemReceiveConsignment));

			var forwardingShipmentNumberQuery = new ZDBOnlySubQuery(typeof(ICusEntryNumber), CusEntryNumSchema.CE_ParentID);
			forwardingShipmentNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber);
			forwardingShipmentNumberQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.AdditionalReferenceNumber);
			forwardingShipmentNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, reference);

			query.AddToFilter(WhsItemReceiveConsignmentSchema.WRC_WW_IntendedWarehouse, trw.PK);
			query.AddToFilter(WhsItemReceiveConsignmentSchema.WRC_CompleteTime, null);
			query.AddSubQuery(forwardingShipmentNumberQuery, JoinCondition.And);
			query.OrderBy = WhsItemReceiveConsignmentSchema.Constants.WRC_SystemCreateTimeUtc + OrderByClause.Descending;

			return Factory.LoadTop1<WhsItemReceiveConsignment>(query);
		}

		WhsItemReceiveConsignment FindRCNViaHouseBill(WhsWarehouse trw, string reference)
		{
			var query = new ZDBOnlyQuery(typeof(WhsItemReceiveConsignment));

			query.AddToFilter(WhsItemReceiveConsignmentSchema.WRC_CompleteTime, null);
			query.AddToFilter(WhsItemReceiveConsignmentSchema.WRC_WW_IntendedWarehouse, trw.PK);
			query.AddToFilter(WhsItemReceiveConsignmentSchema.WRC_HouseBillNumber, reference);
			query.OrderBy = WhsItemReceiveConsignmentSchema.Constants.WRC_SystemCreateTimeUtc + OrderByClause.Descending;

			return Factory.LoadTop1<WhsItemReceiveConsignment>(query);
		}
	}
}
