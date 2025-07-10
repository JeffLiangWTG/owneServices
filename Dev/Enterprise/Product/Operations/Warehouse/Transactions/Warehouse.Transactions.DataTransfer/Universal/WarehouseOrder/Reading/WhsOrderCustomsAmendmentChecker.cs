using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsOrderCustomsAmendmentChecker : IWhsOrderCustomsAmendmentChecker
	{
		bool IWhsOrderCustomsAmendmentChecker.CanDoAnAmendment(WhsOrder order, IEnumerable<ZGuid> orderLinePksWithNoEntryNum, bool holdServiceCodeSetInContext)
		{
			var result = true;
			if (order.IsInDatabase && (order.Pick?.IsFinalised ?? false))
			{
				var noClearedEntryNumbers = NoEntryNumbersCleared(order, orderLinePksWithNoEntryNum);
				var orderHasOnlyAllowedChanges =
					order.HasChanges
					&& !order.ZPropertyInfoHash.Cast<ZPropertyInfo>().Any(p => p.IsPersistent && p.HasChanges)
					&& NoNewLines(order)
					&& noClearedEntryNumbers;
				if (orderHasOnlyAllowedChanges)
				{
					var propertiesAllowedToChange = new[]
					{
						WhsBondedWarehouseAttribute.Schema.WB_EntryKey,
						WhsBondedWarehouseAttribute.Schema.WB_EntryLineNo,
						WhsBondedWarehouseAttribute.Schema.WB_RN_NKCountryOfOrigin,
						WhsBondedWarehouseAttribute.Schema.WB_RN_NKCountryOfDestination,
						WhsBondedWarehouseAttribute.Schema.WB_CustomsQty,
						WhsBondedWarehouseAttribute.Schema.WB_CustomsUnitOfQty,
						WhsBondedWarehouseAttribute.Schema.WB_ValueForDuty,
						WhsBondedWarehouseAttribute.Schema.WB_AddInfo,
						WhsBondedWarehouseAttribute.Schema.WB_Tariff,
						WhsBondedWarehouseAttribute.Schema.WB_InwardStyle,
						WhsBondedWarehouseAttribute.Schema.WB_InwardProcedure,
						WhsBondedWarehouseAttribute.Schema.WB_Remarks,
					};

					foreach (var orderLine in order.Lines.Cast<WhsOrderLine>().Where(ol => ol.HasChanges).ToArray())
					{
						result = !orderLine.ZPropertyInfoHash.Cast<ZPropertyInfo>().Any(p => p.IsPersistent && p.HasChanges);
						if (result)
						{
							var lineCustomsData = orderLine.CustomsData;
							var props = lineCustomsData.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent && p.HasChanges && !propertiesAllowedToChange.Contains(p.Name));
							var hasChangesToNotAllowedProperties = props.Any();
							result = !hasChangesToNotAllowedProperties
									&& (orderLine.CustomsClearingInProgress != holdServiceCodeSetInContext)
									&& orderLinePksWithNoEntryNum.Contains(orderLine.PK);
						}

						if (!result)
						{
							break; // Unallowed change has been detected. No need to keep checking any other line.
						}
					}
				}
				else
				{
					result = !order.HasChanges && noClearedEntryNumbers;
				}
			}

			return result;
		}

		bool NoEntryNumbersCleared(WhsOrder order, IEnumerable<ZGuid> orderLinePksWithNoEntryNum) => !order.Lines.Any(l => !orderLinePksWithNoEntryNum.Contains(l.PK) && l.CustomsData.WB_EntryKey.IsEmpty);

		// Only check for new lines as deleting picked order lines is prohibited.
		bool NoNewLines(WhsOrder order) => order.Lines.Count == new BusinessObjectFactory().Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WD, order.PK)).Length;
	}

	public interface IWhsOrderCustomsAmendmentChecker
	{
		bool CanDoAnAmendment(WhsOrder order, IEnumerable<ZGuid> orderLinePksWithNoEntryNum, bool holdServiceCodeSetInContext);
	}
}
