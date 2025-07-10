using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class CusInBondMoveHeaderValidation : Customs.Business.CusInBondMoveHeaderValidation
	{
		public CusInBondMoveHeaderValidation(CusInBondMoveHeader parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateInBondNumber();
			}
		}

		public void ValidateInBondNumber()
		{
			ValidateCalculatedProperty(Parent.InBondNumberInfo);
		}

		protected virtual void CheckInBondNumber()
		{
			InBondNumberAvailabilityChecker.Check(Parent.InBondNumberInfo, Parent.HeaderBranch, Parent.Header?.IsPostDepartureMessageOnly ?? false);
		}

		protected new CusInBondMoveHeader Parent
		{
			get { return (CusInBondMoveHeader)base.Parent; }
		}
	}
}
