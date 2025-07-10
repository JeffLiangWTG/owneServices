using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class AccComplianceSequenceForSplitValidation : AccComplianceSequenceValidation
	{
		public AccComplianceSequenceForSplitValidation(AccComplianceSequenceForSplit parent) : base(parent)
		{
		}

		protected override void CheckXD_StartNumber()
		{
			base.CheckXD_StartNumber();

			if (!Parent.XD_StartNumberInfo.HasErrors())
			{
				if (Parent.IsNewBook)
				{
					if (Parent.XD_StartNumber <= Parent.SplitController.ExistSequence.XD_EndNumber)
					{
						Parent.XD_StartNumberInfo.AddError(Res.GetString("EF964F8C-D028-453F-9E5A-F33E491810B8", "Start number of new book must be bigger than end number of exist book."));
					}
					if (Parent.XD_StartNumber < Parent.SplitController.ExistSequence.XD_NextNumber)
					{
						Parent.XD_StartNumberInfo.AddError(Res.GetString("85E7F395-F5CF-40F1-AE42-BAE0FAACDA42", "Start number of new book must not be smaller than next number of exist book."));
					}
				}
			}
		}

		protected override void CheckXD_EndNumber()
		{
			base.CheckXD_EndNumber();

			if (!Parent.XD_EndNumberInfo.HasErrors())
			{
				if (Parent.IsNewBook)
				{
					if (Parent.XD_EndNumber > Parent.OriginalEndNumber)
					{
						Parent.XD_EndNumberInfo.AddError(Res.GetString("5360CC53-7915-4D05-BC63-435336D04D39", "End number of new book must not be bigger than original end number."));
					}
				}
				else
				{
					if (Parent.XD_EndNumber >= Parent.OriginalEndNumber)
					{
						Parent.XD_EndNumberInfo.AddError(Res.GetString("E60F36C8-64DD-4715-BFF0-74C465553FBA", "End number of exist book must be smaller than original end number."));
					}
				}
			}
		}

		protected override void CheckXD_StartDate()
		{
			base.CheckXD_StartDate();

			if (!Parent.XD_StartDateInfo.HasErrors() &&
				Parent.IsNewBook &&
				Parent.SplitController.ExistSequence.XD_ExpiryDate.IsValid &&
				Parent.XD_StartDate <= Parent.SplitController.ExistSequence.XD_ExpiryDate.Date)
			{
				Parent.XD_StartDateInfo.AddError(Res.GetString("E241FCE2-F4B0-4BEB-9A13-E1834A9F97A8", "Start date of new book must be bigger than end date of exist book."));
			}
		}

		protected override void CheckXD_ExpiryDate()
		{
			base.CheckXD_ExpiryDate();

			if (!Parent.XD_ExpiryDateInfo.HasErrors() && Parent.OriginalExpiryDate.IsValid)
			{
				if (Parent.IsNewBook)
				{
					if (Parent.XD_ExpiryDate > Parent.OriginalExpiryDate)
					{
						Parent.XD_ExpiryDateInfo.AddError(Res.GetString("026F6A39-3B9D-4D1C-B3ED-AECCCC7B57D6", "Expiry date of new book must not be bigger than original expiry date."));
					}
				}
				else
				{
					if (Parent.XD_ExpiryDate >= Parent.OriginalExpiryDate)
					{
						Parent.XD_ExpiryDateInfo.AddError(Res.GetString("78EF5ACA-DF53-4555-B9A8-1A6585667D54", "Expiry date of exist book must be smaller than original expiry date."));
					}
				}
			}
		}

		protected override ZGuid GetExcludeValidationPK()
		{
			return Parent.SplitController?.ExistSequence?.PK ?? ZGuid.Empty;
		}

		protected new AccComplianceSequenceForSplit Parent
		{
			get
			{
				return (AccComplianceSequenceForSplit)base.Parent;
			}
		}
	}
}
