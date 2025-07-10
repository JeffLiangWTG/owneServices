using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ConsolTemplateValidation : AutoConsolTemplateValidation
	{
		public ConsolTemplateValidation(AutoConsolTemplate parent)
			: base(parent)
		{
		}

		protected override void CheckConsolTemplateName()
		{
			base.CheckConsolTemplateName();

			if (Parent.ConsolTemplateName.IsEmpty)
			{
				return;
			}

			var templateRecord = Parent.LoadConsolTemplateByName(Parent.ConsolTemplateName);

			CheckLoadedTemplate(templateRecord, Parent.ConsolTemplateNameInfo);
		}

		protected override void CheckConsolTemplateReferenceId()
		{
			base.CheckConsolTemplateReferenceId();

			MandatoryValidation.CheckEntered(Parent.ConsolTemplateReferenceIdInfo);

			var templateRecord = Parent.LoadConsolTemplateByReferenceId(Parent.ConsolTemplateReferenceId);

			CheckLoadedTemplate(templateRecord, Parent.ConsolTemplateReferenceIdInfo);
		}

		protected override void CheckConsolsPerFlight()
		{
			base.CheckConsolsPerFlight();

			MandatoryValidation.CheckEntered(Parent.ConsolsPerFlightInfo);
			MandatoryValidation.CheckNotNegative(Parent.ConsolsPerFlightInfo);
		}

		#region Implementation

		public new ConsolTemplate Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ConsolTemplate)base.Parent; }
		}

		#endregion

		static void CheckLoadedTemplate(StmTemplateRecord templateRecord, ZPropertyInfo propertyInfo)
		{
			if (templateRecord == null)
			{
				propertyInfo.AddError(ResString.GetMultilingualString("a557dffd-3c4b-46a3-98f2-bde9a2ffed57", "Consolidation template doesn't exist."));
			}
			else if (!templateRecord.STR_IsActive)
			{
				propertyInfo.AddError(ResString.GetMultilingualString("4a498cf5-ab51-4138-99f7-acd584367920", "Consolidation template should be active."));
			}
		}
	}
}
