using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsReceiveCustomsAmendmentChecker : IWhsReceiveCustomsAmendmentChecker
	{
		bool IWhsReceiveCustomsAmendmentChecker.CanDoAnAmendment(WhsReceive receive, bool allowNoLineChangesIfStockWithdrawn)
		{
			var result = true;
			if (receive.IsInDatabase)
			{
				result = HasOnlyAllowedHeaderChanges(receive) && !HasDeletedCommittedDocketLines(receive);
				if (result && ((IWhsReceiveCustomsAmendmentChecker)this).IsStockWithdrawn(receive))
				{
					if (allowNoLineChangesIfStockWithdrawn)
					{
						result = receive.Lines.All(HasNoLineChanges);
					}
					else
					{
						result = receive.Lines.All(HasOnlyAllowedLineChanges);
					}
				}
			}

			return result;
		}

		bool IWhsReceiveCustomsAmendmentChecker.IsStockWithdrawn(WhsReceive receive)
		{
			var newFactory = new BusinessObjectFactory();
			var receiveInAnotherFactory = newFactory.Load<WhsReceive>(receive.PK);
			return receiveInAnotherFactory.Lines.Any(l => l.WE_StockOnHand != l.WE_TransactionQuantity);
		}

		static bool HasOnlyAllowedHeaderChanges(WhsReceive receive)
		{
			return !CriticalDocketFields(receive).Any(property => property.HasChanges)
				&& !SupplierHasBeenChanged(receive);
		}

		static bool SupplierHasBeenChanged(WhsReceive receive)
		{
			var newFactory = new BusinessObjectFactory();
			var receiveInAnotherFactory = newFactory.Load<WhsReceive>(receive.PK);
			return receive.Supplier?.PK != receiveInAnotherFactory?.Supplier?.PK;
		}

		static IEnumerable<ZPropertyInfo> CriticalDocketFields(WhsDocket docket)
		{
			yield return docket.WD_WW_WhsInfo;
			yield return docket.WD_OH_ClientInfo;
			yield return docket.WD_ExternalReferenceInfo;
		}

		static bool HasDeletedCommittedDocketLines(WhsReceive whsReceive)
		{
			var newFactory = new BusinessObjectFactory();
			var query = new ZDBOnlyQuery(typeof(WhsReceiveLine));
			query.AddToFilter(WhsDocketLineSchema.WE_WD, whsReceive.PK);
			var subQueryPickLine = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_InventoryLine);
			query.AddSubQuery(subQueryPickLine, JoinCondition.And);

			var committedLinesInAnotherFactory = newFactory.Load<WhsReceiveLine>(query);
			var currentReceiveLinePKs = whsReceive.Lines.Select(line => line.PK);
			var newFactoryCommittedReceiveLinePKs = committedLinesInAnotherFactory.Select(line => line.PK);
			// if there are committed receive lines in DB which are not in current BizO -> some of them were deteled.
			return newFactoryCommittedReceiveLinePKs.Except(currentReceiveLinePKs).Any();
		}

		static bool HasOnlyAllowedLineChanges(WhsDocketLine docketLine)
		{
			return !docketLine.IsInDatabase ||
				(!CriticalDocketLineFields(docketLine).Any(property => property.HasChanges)
				&& UnitsQuanityChangedCorrectly(docketLine));
		}

		static bool HasNoLineChanges(WhsDocketLine docketLine)
		{
			return docketLine.IsInDatabase && !docketLine.ZPropertyInfoHash.Cast<ZPropertyInfo>().Any(p => p.IsPersistent && p.HasChanges);
		}

		static bool UnitsQuanityChangedCorrectly(WhsDocketLine docketLine)
		{
			var result = true;
			if (docketLine.WE_TransactionQuantityInfo.HasChanges || docketLine.WE_StockOnHandInfo.HasChanges)
			{
				var deltaWE_TransactionQuantity = docketLine.WE_TransactionQuantity - (ZDecimal)docketLine.WE_TransactionQuantityInfo.OriginalValue;
				var deltaWE_StockOnHand = docketLine.WE_StockOnHand - (ZDecimal)docketLine.WE_StockOnHandInfo.OriginalValue;
				if (deltaWE_TransactionQuantity != deltaWE_StockOnHand)
				{
					result = false;
				}
				// We can't reduce received amount below what is committed
				else if (docketLine.AvailableToPickQuantity < 0)
				{
					result = false;
				}
			}
			return result;
		}

		static IEnumerable<ZPropertyInfo> CriticalDocketLineFields(WhsDocketLine docketLine)
		{
			yield return docketLine.WE_OPInfo;
			yield return docketLine.WE_BondedEntryKeyInfo;
			yield return docketLine.WE_F3_NKPackTypeInfo;
			if (docketLine.WE_StockOnHand != docketLine.WE_TransactionQuantity) // if picked
			{
				yield return docketLine.WE_PartAttrib1Info;
				yield return docketLine.WE_PartAttrib2Info;
				yield return docketLine.WE_PartAttrib3Info;
				yield return docketLine.WE_SerialNumberInfo;
			}
			yield return docketLine.WE_ExpiryDateInfo;
			yield return docketLine.WE_PackingDateInfo;
			yield return docketLine.WE_PackageGroupIdInfo;
			yield return docketLine.WE_PerPackageQtyInfo;
		}
	}

	public interface IWhsReceiveCustomsAmendmentChecker
	{
		bool CanDoAnAmendment(WhsReceive whsReceive, bool allowNoLineChangesIfStockWithdrawn);
		bool IsStockWithdrawn(WhsReceive receive);
	}
}
