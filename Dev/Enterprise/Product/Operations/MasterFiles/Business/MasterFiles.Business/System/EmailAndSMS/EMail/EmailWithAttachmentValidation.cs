using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public class EmailWithAttachmentValidation : ZValidation
	{
		public EmailWithAttachmentValidation(EmailWithAttachment parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected readonly EmailWithAttachment Parent;

		public override Type AutoValidationType => typeof(EmailWithAttachment);

		#region Properties

		#region FromDisplayName

		public void ValidateFromDisplayName()
		{
			ValidateCalculatedProperty(Parent.FromDisplayNameInfo);
		}

		protected void CheckFromDisplayName()
		{
			if (Parent.ShouldValidateFrom)
			{
				ListValidation.ErrorIfInvalidCode(Parent.FromDisplayNameInfo);
			}
		}

		#endregion

		#region FromEmailAddress

		public void ValidateFromEmailAddress()
		{
			ValidateCalculatedProperty(Parent.FromEmailAddressInfo);
		}

		protected void CheckFromEmailAddress()
		{
			MandatoryValidation.CheckEntered(Parent.FromEmailAddressInfo);

			if (Parent.ShouldValidateFrom)
			{
				ListValidation.ErrorIfInvalidCode(Parent.FromEmailAddressInfo);
			}

			if (!Parent.FromEmailAddressInfo.HasErrors())
			{
				ValidateEmailAddress(Parent.FromEmailAddressInfo, Parent.FromEmailAddress);
			}
		}

		#endregion

		#region ToEmailAddress

		public void ValidateToEmailAddress()
		{
			ValidateCalculatedProperty(Parent.ToEmailAddressInfo);
		}

		protected void CheckToEmailAddress()
		{
			MandatoryValidation.CheckEntered(Parent.ToEmailAddressInfo);
			ValidateEmailAddress(Parent.ToEmailAddressInfo, Parent.Recipients);
		}

		#endregion

		#region Cc

		public void ValidateCc()
		{
			ValidateCalculatedProperty(Parent.CcInfo);
		}

		protected void CheckCc()
		{
			ValidateEmailAddress(Parent.CcInfo, Parent.CcRecipients);
		}

		#endregion

		#region Attachment

		public void ValidateAttachment()
		{
			ValidateCalculatedProperty(Parent.AttachmentInfo);
		}

		protected void CheckAttachment()
		{
			ListValidation.ErrorIfInvalidCode(Parent.AttachmentInfo);

			long totalSize = 0;

			for (int i = 0; i < Parent.AttachmentList.Count; i++)
			{
				string currentAttachment = Parent.GetFileNameFromAttachmentListDescription(Parent.AttachmentList[i].Description);

				if (File.Exists(currentAttachment))
				{
					FileInfo fileInfo = new FileInfo(currentAttachment);
					totalSize += fileInfo.Length;
				}
				else
				{
					Parent.AttachmentInfo.AddError(Res.GetString("c2018888-ae87-4dfd-ae67-dcbd6d2a315b", "The file {0} does not exist.", currentAttachment));
				}
			}

			var actualTotalSizeInKB = totalSize / 1024;
			if (actualTotalSizeInKB > MaxTotalAttachmentSizeInKB)
			{
				Parent.AttachmentInfo.AddError(Res.GetString("47ce2b9a-14f2-4fba-b5ce-d5e80eaa7277", "The total attachment size is currently {0}KB but the maximum is {1}KB. Please reduce the size or number of attachments.", actualTotalSizeInKB, MaxTotalAttachmentSizeInKB));
			}
		}

		internal long MaxTotalAttachmentSizeInKB => (long)MaxTotalAttachmentSizeInMB * 1024;

		int MaxTotalAttachmentSizeInMB => SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.Value;

		#endregion

		#region Priority

		public void ValidatePriority()
		{
			ValidateCalculatedProperty(Parent.PriorityInfo);
		}

		protected void CheckPriority()
		{
			MandatoryValidation.CheckEntered(Parent.PriorityInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PriorityInfo);
		}

		#endregion

		#region Body

		public void ValidateBody()
		{
			ValidateCalculatedProperty(Parent.BodyInfo);
		}

		protected virtual void CheckBody()
		{
		}

		#endregion

		#endregion

		#region Email Address Validation

		protected void ValidateEmailAddress(ZPropertyInfo propertyInfo, params string[] emailAddresses)
		{
			foreach (string emailAddress in emailAddresses)
			{
				EmailAddressValidation.ValidateEmailAddress(propertyInfo, emailAddress);
			}
		}

		EmailAddressForSendValidation EmailAddressValidation => emailAddressValidation ?? (emailAddressValidation = new EmailAddressForSendValidation(Parent.Factory ?? new BusinessObjectFactory()));
		EmailAddressForSendValidation emailAddressValidation;

		#endregion

		#region Validate All

		public override void ValidateAll()
		{
			ValidateFromDisplayName();
			ValidateAttachment();
			ValidateCc();
			ValidateFromEmailAddress();
			ValidatePriority();
			ValidateToEmailAddress();
		}

		#endregion
	}
}
