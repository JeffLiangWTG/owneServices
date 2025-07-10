using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class RateTariffDiscountCollection : DependentBusinessObjectCollection<RateTariffDiscount, CompanyTariff>
	{
		public RateTariffDiscountCollection(CompanyTariff tariff, params string[] tariffTypes)
			: base(tariff)
		{
			this.TariffTypes = tariffTypes;
		}

		#region Load

		public override void Load()
		{
			base.Load();

			var typesToCreate = new List<string>(TariffTypes);
			var discountsToRemove = new List<RateTariffDiscount>();
			foreach (RateTariffDiscount discount in this)
			{
				if (typesToCreate.Contains(discount.TD_TariffType))
				{
					typesToCreate.Remove(discount.TD_TariffType);
				}
				else
				{
					discountsToRemove.Add(discount);
				}
			}

			foreach (var typeToCreate in typesToCreate)
			{
				var newDiscount = AddNew();
				newDiscount.TD_TariffType = typeToCreate;
			}

			foreach (var discount in discountsToRemove)
			{
				RemoveAndDelete(discount);
			}
		}

		#endregion

		#region Get / Set Discount / Service Level

		public ZDecimal GetDiscount(string tariffType)
		{
			var discount = Find(tariffType);
			return discount != null ? discount.TD_DiscountPercent : (ZDecimal)0m;
		}

		public ZDecimal GetDiscount(string tariffType, string serviceLevel)
		{
			var discount = Find(tariffType);
			return (discount != null && (discount.TD_RS_NKServiceLevel.IsEmpty || discount.TD_RS_NKServiceLevel == serviceLevel)) ? discount.TD_DiscountPercent : (ZDecimal)0m;
		}

		public void SetDiscount(string tariffType, ZDecimal value)
		{
			var discount = Find(tariffType);
			if (discount != null)
			{
				discount.TD_DiscountPercent = value;
			}
		}

		public ZString GetServiceLevel(string tariffType)
		{
			var discount = Find(tariffType);
			return discount != null ? discount.TD_RS_NKServiceLevel : ZString.Empty;
		}

		public void SetServiceLevel(string tariffType, ZString value)
		{
			var discount = Find(tariffType);
			if (discount != null)
			{
				discount.TD_RS_NKServiceLevel = value;
			}
		}

		public RateTariffDiscount Find(string tariffType)
		{
			foreach (RateTariffDiscount discount in this)
			{
				if (discount.TD_TariffType == tariffType)
				{
					return discount;
				}
			}

			return null;
		}

		readonly string[] TariffTypes;

		#endregion
	}
}



