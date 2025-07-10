using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Registry.Business
{
	public class ComplianceNumberSequenceConfigurationValidation
	{
		public ComplianceNumberSequenceConfigurationValidation(ComplianceNumberSequenceConfiguration parent)
		{
			Parent = parent;
		}

		readonly ComplianceNumberSequenceConfiguration Parent;

		public void ValidateCode()
		{
			Parent.CodeInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(Parent.CodeInfo);

			if (!Parent.CodeInfo.HasErrors() && Parent.Code == AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Code)
			{
				Parent.CodeInfo.AddError(Res.GetString("295e79e4-6970-4e2d-8e22-49b36e4e5c22", "The specify Code cannot be 'DEF' as this is reserved for the default format (Series Prefix + Sequence Number)"));
			}

			if (!Parent.CodeInfo.HasErrors())
			{
				if (Parent.ParentCollection.Cast<ComplianceNumberSequenceConfiguration>().Any(x => x.PK != Parent.PK && x.Code == Parent.Code))
				{
					Parent.CodeInfo.AddError(Res.GetString("433c537d-2df1-4fce-8129-4d21e1b0d43f", "This code already exists."));
				}
			}
		}

		public void ValidateDescription()
		{
			Parent.DescriptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(Parent.DescriptionInfo);
		}
	}
}
