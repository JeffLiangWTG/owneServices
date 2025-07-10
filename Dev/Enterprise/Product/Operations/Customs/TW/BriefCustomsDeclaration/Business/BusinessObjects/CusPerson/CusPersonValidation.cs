using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class CusPersonValidation : ASYCUDA.Business.CusPersonValidation
	{
		public CusPersonValidation(CusPerson parent) : base(parent)
		{
		}

		new CusPerson Parent => (CusPerson)base.Parent;

		protected override void CheckCPN_PER_Person()
		{
			base.CheckCPN_PER_Person();
			if (Parent.Person != null)
			{
				var targetInfo = Parent.CPN_PER_PersonInfo;
				CheckPersonDescription(targetInfo);
				CheckPersonPreferredLanguage(targetInfo);
			}
		}

		void CheckPersonDescription(ZPropertyInfo targetInfo)
		{
			if (Parent.PersonDescription.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("875AF12B-C7C4-409A-A6E0-2850E67CCDB4", "The entered Onboard does not have an Identification Number nor a Passport Number. Press F3 to edit the entered person."));
			}
		}

		void CheckPersonPreferredLanguage(ZPropertyInfo targetInfo)
		{
			if (Parent.Person.PER_PreferredLanguage.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("34A0C32D-C678-4D3B-8EB3-DA871A4D721A", "Please enter a Preferred Language"));
			}
		}

		protected override void CheckCPN_PER_PersonIsNotEmpty()
		{
		}

		protected override bool IdentificationNumberMandatoryCondition => Parent.PersonPassport.IsEmpty;

		protected override bool PersonPassportMandatoryCondition => Parent.PersonIdentificationNumber.IsEmpty;

		protected override bool PersonPassportExpiryMandatoryCondition => Parent.PersonIdentificationNumber.IsEmpty;
	}
}
