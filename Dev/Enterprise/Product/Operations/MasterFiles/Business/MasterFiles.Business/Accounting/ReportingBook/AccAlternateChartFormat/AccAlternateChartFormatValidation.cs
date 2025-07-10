using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business
{
	public class AccAlternateChartFormatValidation : AutoAccAlternateChartFormatValidation
	{
		public AccAlternateChartFormatValidation(AutoAccAlternateChartFormat parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckAlternateGLAccountCreated();
		}

		void CheckAlternateGLAccountCreated()
		{
			Parent.ClearRowNotifications();
			if (Parent.HasChanges && Parent.Factory.Exists(typeof(AccAlternateGLAccount), new ZQuery(AccAlternateGLAccountSchema.AGA_AAC_AlternateChart, Parent.ANF_AAC_AlternateChart)))
			{
				Parent.AddRowError(Res.GetString("55FA9404-2B4D-4135-9C9F-4271476D0B78", "Alternate GL Account is created for this chart, you cannot change Account Format."));
			}
		}

		protected override void CheckANF_Tier()
		{
			var tierErrorMessage = Res.GetString("EC30E45C-ED63-4D48-B48F-B13DD05F4DB9", "Tier numbers must be consecutive, starting from 1, maximum number allowed is 20.");

			if (Parent.ANF_Tier.IsEmpty)
			{
				Parent.ANF_TierInfo.AddError(Res.GetString("B35F7C4E-F96F-47C8-A9A7-E1560026C399", "The Alternate Chart of Accounts should have at least one Tier."));
			}

			if (!Parent.ANF_TierInfo.HasErrors() && (Parent.ANF_Tier > 20 || Parent.ANF_Tier < 1))
			{
				Parent.ANF_TierInfo.AddError(tierErrorMessage);
			}

			if (!Parent.ANF_TierInfo.HasErrors())
			{
				var chartFormats = ((IBusinessObjectInternals)Parent).ParentCollections.FirstOrDefault();
				if (chartFormats.Count != 0)
				{
					var tierSets = chartFormats.Cast<AccAlternateChartFormat>().Select(format => format.ANF_Tier).ToHashSet();
					if (tierSets.Max() != chartFormats.Count || tierSets.Min() != 1 || tierSets.Count != chartFormats.Count || !tierSets.All(n => tierSets.Contains(n - 1) || n == 1))
					{
						Parent.ANF_TierInfo.AddError(tierErrorMessage);
					}
				}
			}
		}

		protected override void CheckANF_Description()
		{
			MandatoryValidation.CheckEntered(Parent.ANF_DescriptionInfo);
		}

		protected override void CheckANF_Format()
		{
			var formatErrorMessage = Res.GetString("86C2F3DF-C407-4F49-BAFF-D7952B9CFF7F", "The Total length of Format in all Tier should not exceed 20 characters.");
			var info = Parent.ANF_FormatInfo;
			MandatoryValidation.CheckEntered(info);
			if (!info.HasErrors() && Parent.ANF_Format.Length > 20)
			{
				info.AddError(formatErrorMessage);
			}

			if (!info.HasErrors() && !Parent.ANF_Format.ToString().All(character => character == AlternateGLAccountFormatType.AlphabatFormat || character == AlternateGLAccountFormatType.NumberFormat))
			{
				info.AddError(Res.GetString("4FB5DF60-A7CD-40D5-B620-152D4056BA69", "Format invalid. Please enter 9 to represent number, enter X to represent letter."));
			}

			if (!info.HasErrors())
			{
				var chartFormats = ((IBusinessObjectInternals)Parent).ParentCollections.FirstOrDefault();
				var formatLength = chartFormats.Cast<AccAlternateChartFormat>().Select(format => format.ANF_Format.Length).Sum();

				if (formatLength > 20)
				{
					info.AddError(formatErrorMessage);
				}
			}
		}
		protected override void CheckANF_Separator()
		{
			var info = Parent.ANF_SeparatorInfo;
			if (Parent.ANF_Separator != "-" && Parent.ANF_Separator != "." && Parent.ANF_Separator != "")
			{
				info.AddError(Res.GetString("7559E663-4CA4-4442-8F24-C2AB3159FEF1", "Separator invalid. Please enter . or -."));
			}
		}
	}
}
