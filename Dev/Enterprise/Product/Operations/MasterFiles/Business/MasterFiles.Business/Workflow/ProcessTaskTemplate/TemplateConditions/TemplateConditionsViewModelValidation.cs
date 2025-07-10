using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class TemplateConditionsViewModelValidation : ZValidation
	{
		public TemplateConditionsViewModelValidation(TemplateConditionsViewModel parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly TemplateConditionsViewModel parent;

		public override Type AutoValidationType
		{
			get { return typeof(TemplateConditionsViewModelValidation); }
		}

		public override void ValidateAll()
		{
			ValidateTemplateCondition1();
			ValidateTemplateCondition2();
			ValidateTemplateCondition2Value();
		}

		public void ValidateTemplateCondition1()
		{
			ValidateCalculatedProperty(parent.TemplateCondition1Info);
		}

		protected void CheckTemplateCondition1()
		{
			if (parent.Template != null)
			{
				ListValidation.ErrorIfInvalidCode(parent.TemplateCondition1Info);
			}
		}

		public void ValidateTemplateCondition2()
		{
			ValidateCalculatedProperty(parent.TemplateCondition2Info);
		}

		protected void CheckTemplateCondition2()
		{
			if (parent.Template != null)
			{
				ListValidation.ErrorIfInvalidCode(parent.TemplateCondition2Info);
			}
		}

		public void ValidateTemplateCondition2Value()
		{
			ValidateCalculatedProperty(parent.TemplateCondition2ValueInfo);
		}

		protected void CheckTemplateCondition2Value()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(parent.TemplateCondition2ValueInfo); // Because ProcessTasks doesn't support this. Can be removed if this is changed in future.

			if (parent.Template != null)
			{
				switch (parent.Condition2ValueStyle)
				{
					case TemplateConditionValueStyle.Text:
					case TemplateConditionValueStyle.UDFMacro:
					case TemplateConditionValueStyle.MCRMacro:
						MandatoryValidation.CheckEntered(parent.TemplateCondition2ValueInfo);
						break;

					case TemplateConditionValueStyle.DropDown:
						MandatoryValidation.CheckEntered(parent.TemplateCondition2ValueInfo);
						ListValidation.ErrorIfInvalidCode(parent.TemplateCondition2ValueInfo);
						break;
				}
			}
		}

		public void ValidateOriginCountryCode()
		{
			ValidateCalculatedProperty(parent.OriginCountryCodeInfo);
		}

		protected void CheckOriginCountryCode()
		{
			ListValidation.ErrorIfInvalidCode(parent.OriginCountryCodeInfo);
		}

		public void ValidateDestinationCountryCode()
		{
			ValidateCalculatedProperty(parent.DestinationCountryCodeInfo);
		}

		protected void CheckDestinationCountryCode()
		{
			ListValidation.ErrorIfInvalidCode(parent.DestinationCountryCodeInfo);
		}
	}
}
