using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class PSCLotValidation : USFDALotAddInfoValidation
	{
		public PSCLotValidation(USLotAddInfo parent) : base(parent)
		{
		}

		bool IsREF
		{
			get
			{
				var lot = Lot;
				var header = lot != null ? lot.CPSCHeader : null;
				return header != null && header.IsREF;
			}
		}

		Lot Lot => Parent?.Parent as Lot;

		protected override void CheckUS_LotNumber()
		{
			if (!Parent.US_LotNumberType.IsEmpty && !IsREF)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_LotNumberInfo);
			}
			ValidateUS_LotNumberType();
		}

		protected override void CheckUS_LotNumberType()
		{
			base.CheckUS_LotNumberType();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_LotNumberTypeInfo, Lot.AddInfoLookups.LotNumberTypeCodeList);

			if (!Parent.US_LotNumber.IsEmpty && !IsREF)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_LotNumberTypeInfo);
			}
			ValidateUS_LotNumber();
		}
	}
}
