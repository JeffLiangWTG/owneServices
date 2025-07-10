using CargoWise.EntityFramework;
namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondMoveLineItemValidation : Customs.Business.CusInBondMoveLineItemValidation
	{
		public CusInBondMoveLineItemValidation(CusInBondMoveLineItem parent)
			: base(parent)
		{
		}

		protected SendingMessageValidationHelper Helper => new SendingMessageValidationHelper(Header, Parent);

		public override void ValidateAll()
		{
			var helper = Helper;
			if (!helper.IsArrivalValidationMode && !helper.IsExportationValidationMode)
			{
				base.ValidateAll();
			}
		}

		protected new CusInBondMoveLineItem Parent
		{
			get { return (CusInBondMoveLineItem)base.Parent; }
		}

		protected override void CheckBI_WeightUnit()
		{
			base.CheckBI_WeightUnit();
			ListValidation.WarnIfInvalidCode(Parent.BI_WeightUnitInfo);
		}

		protected CusInBondHeader Header => Parent.Header;
	}
}
