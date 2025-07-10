using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class GlbCompanySignatureCredential : GlbExternalPassword
	{
		public GlbCompanySignatureCredential(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : GlbExternalPassword.Schema
		{
			public const int CurrentDecryptedPasswordMaxLengthTR = 20;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_GC = GlbCompany.CurrentCompany.PK;
			GP_GS = ZGuid.Empty;
			GP_UserID = ZString.Empty;
			GP_CurrentPassword = ZString.Empty;
			GP_PasswordType = PasswordTypesList.Codes.TRU;
			GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
		}

		[MaxLength(Schema.CurrentDecryptedPasswordMaxLengthTR)]
		[ResourceStringData("825a03d5-831f-46b7-b653-634670038ef6", Caption = "Credential Password", ShortCaption = "Password", MediumCaption = "Password")]
		public override ZString CurrentDecryptedPassword
		{
			get => base.CurrentDecryptedPassword;
			set => base.CurrentDecryptedPassword = value;
		}

		[ResourceStringData("df3ccabf-51a1-4483-be80-36916194d36a", Caption = "Credential Status", ShortCaption = "Status", MediumCaption = "Status")]
		public override ZString GP_PasswordStatus
		{
			get => base.GP_PasswordStatus;
			set => base.GP_PasswordStatus = value;
		}

		[ResourceStringData("ddac0802-14ca-432f-bf58-6b736a2b4e92", Caption = "Credential User ID", ShortCaption = "User ID", MediumCaption = "User ID")]
		public override ZString GP_UserID
		{
			get => base.GP_UserID;
			set => base.GP_UserID = value;
		}

		protected override GlbExternalPasswordValidation GetNewValidation()
		{
			return new GlbCompanySignatureCredentialValidation(this);
		}

		public override void Delete()
		{
			var oldValue = GP_UserIDInfo.OriginalValue;
			string reference = (NoResString)"Signature Credential " + oldValue + (NoResString)" is deleted, " + System.Environment.MachineName + (NoResString)", " + Utilities.GetLocalIPAddress() + (NoResString)", " + System.Environment.UserName;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Company?.Logs.AddNew(Events.DeletedARecordInTheSystem, reference);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			base.Delete();
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsInDatabase)
			{
				string reference = (NoResString)"Signature Credential " + GP_UserID + (NoResString)" is added, " + System.Environment.MachineName + (NoResString)", " + Utilities.GetLocalIPAddress() + (NoResString)", " + System.Environment.UserName;
				Company?.Logs.AddNew(Events.ItemAdded, reference);
			}
			else if (HasChanges)
			{
				var oldValue = GP_UserIDInfo.OriginalValue;
				string reference = (NoResString)"Signature Credential is edited, " + oldValue + (NoResString)" -> " + GP_UserID + (NoResString)", " + System.Environment.MachineName + (NoResString)", " + Utilities.GetLocalIPAddress() + (NoResString)", " + System.Environment.UserName;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Company?.Logs.AddNew(Events.EditedARecord, reference);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}
	}
}
