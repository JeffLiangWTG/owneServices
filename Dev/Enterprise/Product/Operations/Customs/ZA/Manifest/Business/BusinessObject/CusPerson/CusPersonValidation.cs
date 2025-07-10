using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class CusPersonValidation : ASYCUDA.Business.CusPersonValidation
	{
		public CusPersonValidation(CusPerson parent)
			: base(parent)
		{ }

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateOccupationInZA();
			ValidateReasonForMovementInZA();
			ValidateTravellerTypeInZA();
			ValidateTravelDocumentTypeInZA();
		}

		public new CusPerson Parent => (CusPerson)base.Parent;

		protected override void CheckCPN_PER_Person()
		{
			base.CheckCPN_PER_Person();
			var isRoad = Parent.Header?.IsRoad ?? false;
			if (Parent.Person != null && Parent.PersonPassportPlaceOfIssue.IsEmpty && isRoad)
			{
				Parent.CPN_PER_PersonInfo.AddMessageError("Please enter the Passport Country/Region Of Issue against the person");
			}
		}

		protected override void CheckIdentificationNumber()
		{
			if (Parent.PersonNationality == Core.Constants.CountryCodes.SouthAfrica && Parent.PersonIdentificationNumber.IsEmpty)
			{
				Parent.CPN_PER_PersonInfo.AddMessageError("Valid ZA Identification Number is mandatory when the Person's Nationality is ZA");
			}
			else if (Parent.PersonNationality != Core.Constants.CountryCodes.SouthAfrica && !Parent.PersonIdentificationNumber.IsEmpty)
			{
				Parent.CPN_PER_PersonInfo.AddMessageError("Identification Number must not be provided when the Person's Nationality is not ZA");
			}
		}

		public void ValidateOccupationInZA()
		{
			ValidateCalculatedProperty(Parent.OccupationInZAInfo);
		}

		protected virtual void CheckOccupationInZA()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.OccupationInZAInfo);
		}

		public void ValidateTravellerTypeInZA()
		{
			ValidateCalculatedProperty(Parent.TravellerTypeInZAInfo);
		}

		protected virtual void CheckTravellerTypeInZA()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.TravellerTypeInZAInfo);
		}

		public void ValidateReasonForMovementInZA()
		{
			ValidateCalculatedProperty(Parent.ReasonForMovementInZAInfo);
		}

		protected virtual void CheckReasonForMovementInZA()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ReasonForMovementInZAInfo);
		}

		public void ValidateTravelDocumentTypeInZA()
		{
			ValidateCalculatedProperty(Parent.TravelDocumentTypeInZAInfo);
		}

		protected virtual void CheckTravelDocumentTypeInZA()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.TravelDocumentTypeInZAInfo);
		}
	}
}
