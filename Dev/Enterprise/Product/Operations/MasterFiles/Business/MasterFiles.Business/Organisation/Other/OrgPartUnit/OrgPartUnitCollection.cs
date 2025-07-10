using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Has a product's packing details like 100 BOX in 1 CTN > 200 BOT in 1 BOX > 125 ML in 1 BOT
	/// </summary>
	public class OrgPartUnitCollection : DependentBusinessObjectCollection<OrgPartUnit, OrgSupplierPart>
	{
		public static class ConvertIndicator
		{
			public const int Done = 1;
			public const int NotDone = -1;
		}

		public OrgPartUnitCollection(OrgSupplierPart parentSupplierPart, BusinessObjectFactory factory) : base(parentSupplierPart, factory)
		{
			fParentOrgSupplierPart = parentSupplierPart;
		}

		public OrgSupplierPart Part
		{
			get { return Master; }
		}

		public void UpdateConversionToStockKeepingUnit(ZPropertyInfo childUQInfo, ZDecimal quantityInParent)
		{
			if (fParentOrgSupplierPart != null)
			{
				OrgPartUnit partUnit = null;
				if (fParentOrgSupplierPart.IsInDatabase)
				{
					BusinessObjectFactory dBFactory = new BusinessObjectFactory();
					OrgSupplierPart dBProduct = (OrgSupplierPart)dBFactory.Load(typeof(OrgSupplierPart), fParentOrgSupplierPart.PK);
					partUnit = GetUnitConversion(dBProduct.OP_StockKeepingUnit, (ZString)dBProduct[childUQInfo.Name]);
				}

				var childUQValue = (ZString)fParentOrgSupplierPart[childUQInfo.Name];
				if (quantityInParent.IsDefault && partUnit != null)
				{
					partUnit.Delete();
				}
				else if (!quantityInParent.IsDefault && partUnit == null)
				{
					var existingConversion = GetUnitConversion(fParentOrgSupplierPart.OP_StockKeepingUnit, childUQValue);
					partUnit = existingConversion ?? AddNew();
				}

				if (partUnit != null && !partUnit.IsDeleted)
				{
					partUnit.OF_PackType = childUQValue;
					partUnit.OF_QuantityInParent = quantityInParent;
					partUnit.OF_ParentPackType = fParentOrgSupplierPart.OP_StockKeepingUnit;
					if (partUnit.OF_PackType.EqualsIgnoringCase(partUnit.OF_ParentPackType))
					{
						partUnit.Delete();
					}
				}
			}
		}

		public string GetConversionErrors(ZString orderUQ, ZString customsUQ)
		{
			string result = "";

			if (!fParentOrgSupplierPart.UnitConverter.Convertible(orderUQ, customsUQ))
			{
				result = Res.GetString("f2cc8356-2615-4615-b8b4-9906e3acc1c8", @"You set classification details here. If you specify a unit conversion from Stock Keeping UQ '{0}' to Classification UQ '{1}', then the system will convert to Customs Qty for this part.", orderUQ, customsUQ);
			}
			else if (fParentOrgSupplierPart.UnitConverter.HasMoreThanOneConversionFactor(orderUQ, customsUQ))
			{
				result = Res.GetString("b449ead2-6271-49c8-be42-e6f42b132dab", "There is more than one factor between UQ '{0}' and Classification UQ '{1}'.", orderUQ, customsUQ);
			}
			return result;
		}

		public ZString GetParentPackageByPackage(ZString package)
		{
			foreach (OrgPartUnit part in this)
			{
				if (part.OF_PackType.EqualsIgnoringCase(package))
				{
					return part.OF_ParentPackType;
				}
			}
			return ZString.Empty;
		}

		readonly OrgSupplierPart fParentOrgSupplierPart;

		public OrgPartUnit GetUnitConversion(ZString parentUQ, ZString childUQ)
		{
			OrgPartUnit result = null;
			foreach (OrgPartUnit partUnit in this)
			{
				if (partUnit.OF_ParentPackType.EqualsIgnoringCase(parentUQ) && partUnit.OF_PackType.EqualsIgnoringCase(childUQ))
				{
					result = partUnit;
					break;
				}
			}
			return result;
		}
	}
}
