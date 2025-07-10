using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class BoleroInvitationDetailsValidation : ZValidation
	{
		public BoleroInvitationDetailsValidation(BoleroInvitationDetails boleroInvitationDetails)
			: base(boleroInvitationDetails)
		{
			Parent = boleroInvitationDetails;
		}

		public BoleroInvitationDetails Parent { get; }

		public override Type AutoValidationType => typeof(BoleroInvitationDetailsValidation);

		public override void ValidateAll()
		{
			ValidateSelectedContactPK();
			ValidateSelectedContactEmail();
			ValidateYourName();
			ValidateYourEmail();
		}

		public void ValidateSelectedContactPK()
		{
			ValidateCalculatedProperty(Parent.SelectedContactPKInfo);
		}

		protected void CheckSelectedContactPK()
		{
			MandatoryValidation.CheckEntered(Parent.SelectedContactPKInfo);
			ListValidation.ErrorIfInvalidPK(Parent.SelectedContactPKInfo);
		}

		public void ValidateSelectedContactEmail()
		{
			ValidateCalculatedProperty(Parent.SelectedContactEmailInfo);
		}

		protected void CheckSelectedContactEmail()
		{
			MandatoryValidation.CheckEntered(Parent.SelectedContactEmailInfo);
		}

		public void ValidateYourName()
		{
			ValidateCalculatedProperty(Parent.YourNameInfo);
		}

		protected void CheckYourName()
		{
			MandatoryValidation.CheckEntered(Parent.YourNameInfo);
		}

		public void ValidateYourEmail()
		{
			ValidateCalculatedProperty(Parent.YourEmailInfo);
		}

		protected void CheckYourEmail()
		{
			MandatoryValidation.CheckEntered(Parent.YourEmailInfo);
		}
	}
}
