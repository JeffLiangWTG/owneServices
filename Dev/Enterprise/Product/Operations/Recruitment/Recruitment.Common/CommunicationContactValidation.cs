using System;
using CargoWise.EntityFramework;

namespace Enterprise.Recruitment.Common
{
	public class CommunicationContactValidation : ZValidation
	{
		public CommunicationContactValidation(CommunicationContact parent) : base(parent)
		{ }

		CommunicationContact Contact => (CommunicationContact)ParentFilter;

		public override Type AutoValidationType => typeof(CommunicationContact);

		public override void ValidateAll()
		{
			ValidatePosition();
			ValidateStaff();
			ValidateEmail();
		}

		public void ValidatePosition() => ValidateCalculatedProperty(Contact.PositionInfo);

		public void ValidateStaff() => ValidateCalculatedProperty(Contact.StaffInfo);

		public void ValidateEmail() => ValidateCalculatedProperty(Contact.EmailInfo);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method is used via reflection (see GetValidationMethod() in ZValidation.cs)")]
		void CheckPosition()
		{
			if (Contact.ReadOnly)
			{
				return;
			}

			if (string.IsNullOrEmpty(Contact.Position))
			{
				Contact.PositionInfo.AddError(Res.GetString("ee909153-1323-45dd-b84a-10309a6036da", "You must select a position"));
			}
			else if (string.IsNullOrEmpty(Contact.Positions.GetCodeFromDescription(Contact.Position)))
			{
				Contact.PositionInfo.AddError(Res.GetString("66fd5100-c6dd-465d-a803-bd575d9c15e5", "Invalid position"));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method is used via reflection (see GetValidationMethod() in ZValidation.cs)")]
		void CheckStaff()
		{
			if (Contact.ReadOnly || Contact.StaffInfo.ReadOnly)
			{
				return;
			}

			if (string.IsNullOrEmpty(Contact.Staff))
			{
				Contact.StaffInfo.AddError(Res.GetString("159b5cdb-e167-412c-b9de-6e9a56c9bd89", "You must select a staff member"));
			}
			else if (Contact.AsGlbStaff == null)
			{
				Contact.StaffInfo.AddError(Res.GetString("e4818991-db02-4ca9-b257-985fdd32bfd9", "Staff member does not exist"));
			}
			else if (string.IsNullOrEmpty(Contact.AsGlbStaff.GS_EmailAddress))
			{
				Contact.StaffInfo.AddError(Res.GetString("595caa2c-8f22-4740-8bdc-5711ca7cef94", "Staff member does not have an email address"));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method is used via reflection (see GetValidationMethod() in ZValidation.cs)")]
		void CheckEmail()
		{
			if (Contact.ReadOnly || Contact.EmailInfo.ReadOnly)
			{
				return;
			}

			if (string.IsNullOrEmpty(Contact.Email))
			{
				Contact.EmailInfo.AddError(Res.GetString("552028bb-cca3-40b7-b688-262bd68aa127", "You must enter an email address"));
			}

			EmailAddressValidation.ValidateEmailAddress(Contact.EmailInfo);
		}
	}
}
