using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCustomLabelsValidation : AutoOrgCustomLabelsValidation
	{
		public OrgCustomLabelsValidation(AutoOrgCustomLabels parent) : base(parent)
		{
		}

		protected override void CheckOT_Caption()
		{
			base.CheckOT_Caption();

			if (Parent.OT_Type == OrgConstants.CustomLabelType.Form)
			{
				MandatoryValidation.CheckEntered(Parent.OT_CaptionInfo);
			}

			if (!SupportsNonWesternEuropeanCharacters(Parent))
			{
				if (Parent.OT_Caption.Length >= 1 && Parent.OT_Caption.Length < 3)
				{
					Parent.OT_CaptionInfo.AddError(Res.GetString("fa484bbf-7d80-4bfc-a2ac-635be85bbce9", "The Caption must be more than two characters."));
				}

				EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.OT_CaptionInfo);
			}
		}

		bool SupportsNonWesternEuropeanCharacters(AutoOrgCustomLabels label)
		{
			var listOfSupportedCustomFields = new ZString[]
			{
				Constants.CustomLabels.OrderLine.CustomAttribute1,
				Constants.CustomLabels.OrderLine.CustomAttribute2,
				Constants.CustomLabels.OrderLine.CustomAttribute3,
				Constants.CustomLabels.OrderLine.CustomAttribute4,
				Constants.CustomLabels.OrderLine.CustomAttribute5,
				Constants.CustomLabels.OrderLine.CustomAttribute6,
				Constants.CustomLabels.OrderLine.CustomTextBlob1,
			};

			return listOfSupportedCustomFields.Contains(label.OT_FieldName);
		}

		protected override void CheckOT_FieldName()
		{
			base.CheckOT_FieldName();

			ListValidation.ErrorIfInvalidCode(Parent.OT_FieldNameInfo);
			MandatoryValidation.CheckEntered(Parent.OT_FieldNameInfo);

			if (!Parent.OT_FieldNameInfo.HasNotifications() && Parent.Header != null)
			{
				foreach (OrgCustomLabels label in Parent.Header.CustomFormLabels)
				{
					if (label != Parent && label.OT_FieldName == Parent.OT_FieldName)
					{
						Parent.OT_FieldNameInfo.AddError(Res.GetString("c90497f7-9954-4453-af99-81a44a56204b", "This field has already been entered."));
						break;
					}
				}
			}
		}
	}
}
