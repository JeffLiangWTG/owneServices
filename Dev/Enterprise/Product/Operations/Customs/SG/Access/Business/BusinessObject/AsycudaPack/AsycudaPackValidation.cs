using CargoWise.Types;

namespace Enterprise.Customs.SG.Access.Business
{
	public class AsycudaPackValidation : ASYCUDA.Business.AsycudaPackValidation
	{
		public AsycudaPackValidation(AsycudaPack parent)
			: base(parent)
		{
		}

		protected new AsycudaPack Parent => base.Parent as AsycudaPack;

		protected override void CheckLinePriceCurrency()
		{
		}

		void CheckTradeNetPermitNumber()
		{
			var parent = Parent;
			parent.RemoveRowMessageError(ValidationConstants.Bill.SGShouldHaveTradeNetPermitNumberWithLinkedIBGAccount);

			var bill = parent.Bill;
			if (bill != null && bill.IsIBGAccountLinked && !parent.HasValidTradeNetPermitNumber)
			{
				parent.AddRowMessageError(ValidationConstants.Bill.SGShouldHaveTradeNetPermitNumberWithLinkedIBGAccount);
			}
		}

		protected override ZBool NeedsToCheckAPA_PackQty => false;

		protected override ZBool NeedsToCheckAPA_PackUQ => false;

		protected override bool IsWeightRequired() => false;
		protected override bool IsVolumeRequired() => false;

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckTradeNetPermitNumber();
		}
	}
}
