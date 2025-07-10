using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTaxPairList;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaTaxCollection : ASYCUDA.Business.AsycudaTaxCollection<AsycudaTax, AsycudaBill>
	{
		public AsycudaTaxCollection(AsycudaBill master) : base(master)
		{
			MaxCountValidationEnable(maxAllowed);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var tax = (AsycudaTax)child;
			tax.AET_MethodOfPayment = MethodOfPaymentList.Codes.DutyLevied;
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(AsycudaTax);
		}

		protected override bool AllowNewCore => base.AllowNewCore && Count < maxAllowed;

		const int maxAllowed = 9;
	}
}
