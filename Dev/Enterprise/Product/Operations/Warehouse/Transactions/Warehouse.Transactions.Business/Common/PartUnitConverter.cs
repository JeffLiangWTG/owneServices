using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class PartUnitConverter_DonotUseThisClassItCausesAStackOverflow
	{
		public const int DefaultDecimals = 6;

		// No Rounding
		public ZDecimal Convert(OrgSupplierPart part, ZDecimal units, ZString fromUQ, ZString toUQ)
		{
			return units * ConversionFactor(part, fromUQ, toUQ);
		}

		// Auto Rounding
		public ZDecimal ConvertRounded(OrgSupplierPart part, ZDecimal units, ZString fromUQ, ZString toUQ)
		{
			return ConvertRounded(part, units, fromUQ, toUQ, PartUnitConverter_DonotUseThisClassItCausesAStackOverflow.DefaultDecimals);
		}

		// User specified Rounding
		public ZDecimal ConvertRounded(OrgSupplierPart part, ZDecimal units, ZString fromUQ, ZString toUQ, int decimals)
		{
			return ZArchitecture.Core.Utilities.Round(Convert(part, units, fromUQ, toUQ), decimals);
		}

		public bool HasPalletDefinition(OrgSupplierPart part)
		{
			bool result = false;

			if (part != null)
			{
				result = (ConversionFactor(part, part.OP_StockKeepingUnit, "PLT") > 0m);
			}

			return result;
		}

		#region Implementation

		ZDecimal ConversionFactor(OrgSupplierPart part, ZString fromUQ, ZString toUQ)
		{
			toUQ = toUQ.ToUpper().Trim();
			fromUQ = fromUQ.ToUpper().Trim();

			ZDecimal result = 0m;

			if (fromUQ == toUQ)
			{
				result = 1m;
			}
			else
			{
				if (part != null)
				{
					result = FindFactorByExactMatch(part, fromUQ, toUQ);
					if (result == 0m)
					{
						result = FindFactorByTree(part, fromUQ, toUQ);
					}
				}
			}

			return result; //(Result != 0m ? Result : new ZDecimal(1m));
		}

		ZDecimal FindFactorByExactMatch(OrgSupplierPart part, ZString fromUQ, ZString toUQ)
		{
			ZDecimal result = 0m;

			foreach (OrgPartUnit unit in part.PartUnits)
			{
				if (unit.OF_PackType == fromUQ && unit.OF_ParentPackType == toUQ)
				{
					result = 1m / unit.OF_QuantityInParent;
					break;
				}
				else if (unit.OF_PackType == toUQ && unit.OF_ParentPackType == fromUQ)
				{
					result = unit.OF_QuantityInParent;
					break;
				}
			}

			return result;
		}

		ZDecimal FindFactorByTree(OrgSupplierPart part, ZString fromUQ, ZString toUQ)
		{
			ZDecimal result = SearchUpTreeRecursive(part, fromUQ, toUQ);

			if (result == 0m)
			{
				result = SearchDownTreeRecursive(part, fromUQ, toUQ);
			}
			else
			{
				result = 1.0m / result;
			}

			return result;
		}

		ZDecimal SearchUpTreeRecursive(OrgSupplierPart part, ZString fromUQ, ZString toUQ)
		{
			ZDecimal result = 0m;

			foreach (OrgPartUnit unit in part.PartUnits)
			{
				if (unit.OF_PackType == fromUQ)
				{
					if (unit.OF_ParentPackType == toUQ)
					{
						result = QtyInParent(unit);
						break;
					}
					else
					{
						ZDecimal factor = SearchUpTreeRecursive(part, unit.OF_ParentPackType, toUQ);
						if (factor > 0m)
						{
							result = QtyInParent(unit) * factor;
						}
						break;
					}
				}
			}
			return result;
		}

		ZDecimal SearchDownTreeRecursive(OrgSupplierPart part, ZString fromUQ, ZString toUQ)
		{
			ZDecimal result = 0m;

			foreach (OrgPartUnit unit in part.PartUnits)
			{
				if (unit.OF_ParentPackType == fromUQ)
				{
					if (unit.OF_PackType == toUQ)
					{
						result = QtyInParent(unit);
						break;
					}
					else
					{
						ZDecimal factor = SearchDownTreeRecursive(part, unit.OF_PackType, toUQ);
						if (factor > 0m)
						{
							result = QtyInParent(unit) * factor;
						}
						break;
					}
				}
			}
			return result;
		}

		ZDecimal QtyInParent(OrgPartUnit partUnit)
		{
			return (partUnit.OF_QuantityInParent == 0m ? new ZDecimal(1m) : partUnit.OF_QuantityInParent);
		}

		#endregion
	}
}
