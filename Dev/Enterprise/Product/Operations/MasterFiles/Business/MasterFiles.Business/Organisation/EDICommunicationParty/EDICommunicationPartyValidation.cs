using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class EDICommunicationPartyValidation : AutoEDICommunicationPartyValidation
	{
		public EDICommunicationPartyValidation(AutoEDICommunicationParty parent)
			: base(parent)
		{
		}

		protected override void CheckECP_OC_TechnicalContact()
		{
			MandatoryValidation.CheckEntered(Parent.ECP_OC_TechnicalContactInfo);
			if (!Parent.ECP_OC_TechnicalContactInfo.HasErrors())
			{
				if (Parent.TechnicalContact == null)
				{
					Parent.ECP_OC_TechnicalContactInfo.AddError(Res.GetString("EA003040-1DB2-4F9A-843B-5DC2A033250F", "Technical contact must exist."));
				}
				if (Parent.TechnicalContact != null && Parent.TechnicalContact.OC_Email.IsEmpty)
				{
					Parent.ECP_OC_TechnicalContactInfo.AddError(Res.GetString("0C784E11-6CDB-4C30-AB4C-860C09592D45", "Technical contact must have an email address."));
				}
			}
		}

		protected override void CheckECP_Name()
		{
			MandatoryValidation.CheckEntered(Parent.ECP_NameInfo);
		}

		protected override void CheckECP_ApplicationCode()
		{
			MandatoryValidation.CheckEntered(Parent.ECP_ApplicationCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ECP_ApplicationCodeInfo, Parent.Lookups.ApplicationCodeList);
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
		}

		public new EDICommunicationParty Parent => (EDICommunicationParty)base.Parent;
	}
}
