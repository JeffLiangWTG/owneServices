using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTaxPairList;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaPackedItemTaxCollection : ASYCUDA.Business.AsycudaPackedItemTaxCollection<AsycudaPackedItemTax, AsycudaPackedItem>
	{
		public AsycudaPackedItemTaxCollection(AsycudaPackedItem master) : base(master)
		{
			MaxCountValidationEnable(maxAllowed);
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(AsycudaPackedItemTax);
		}

		public AsycudaPackedItemTax FirstTobaccoTax => this.Cast<AsycudaPackedItemTax>().FirstOrDefault(tax => tax.AET_ChargeType == ChargeTypeOtherList.Codes.TT);

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			var tax = (AsycudaPackedItemTax)elementToDelete;
			if (tax.IsTobaccoTax && !this.Cast<AsycudaPackedItemTax>().Any(t => t.IsTobaccoTax && t.PK != tax.PK))
			{
				RemoveHealthWelfareSurcharge();
			}
			base.RemoveAndDelete(elementToDelete);
		}

		public void CreateHealthWelfareSurchargeIfNeed()
		{
			var healthWelfareSurcharge = this.Cast<AsycudaPackedItemTax>().FirstOrDefault(tax => tax.IsHealthWelfareSurcharge);
			if (healthWelfareSurcharge == null)
			{
				healthWelfareSurcharge = this.AddNew();
				healthWelfareSurcharge.AET_ChargeType = ChargeTypeOtherList.Codes.HWS;
			}
			healthWelfareSurcharge.SetRateAndMethodOfCalculation();
		}

		public void RemoveHealthWelfareSurcharge()
		{
			this.Cast<AsycudaPackedItemTax>().Where(tax => tax.IsHealthWelfareSurcharge).ToArray().ForEach(tax => tax.Delete());
		}

		protected override bool AllowNewCore => base.AllowNewCore && Count < maxAllowed;

		const int maxAllowed = 9;
	}
}
