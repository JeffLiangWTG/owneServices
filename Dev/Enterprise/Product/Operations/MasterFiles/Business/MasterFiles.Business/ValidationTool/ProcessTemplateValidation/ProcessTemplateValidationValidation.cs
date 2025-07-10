using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTemplateValidationValidation : AutoProcessTemplateValidationValidation
	{
		public ProcessTemplateValidationValidation(AutoProcessTemplateValidation parent) : base(parent)
		{
		}

		new ProcessTemplateValidation Parent => (ProcessTemplateValidation)base.Parent;

		protected override void CheckP0V_Condition1()
		{
			base.CheckP0V_Condition1();
			ListValidation.ErrorIfInvalidCode(Parent.P0V_Condition1Info);
		}

		protected override void CheckP0V_Condition2()
		{
			base.CheckP0V_Condition2();
			ListValidation.ErrorIfInvalidCode(Parent.P0V_Condition2Info);
		}

		protected override void CheckP0V_Severity()
		{
			base.CheckP0V_Severity();
			ListValidation.ErrorIfInvalidCode(Parent.P0V_SeverityInfo);
		}

		protected override void CheckP0V_Description()
		{
			base.CheckP0V_Description();
			MandatoryValidation.CheckEntered(Parent.P0V_DescriptionInfo);

			var processTaskTemplate = Parent.WorkflowTemplate;
			if (processTaskTemplate != null)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.P0V_DescriptionInfo, processTaskTemplate.ProcessTemplateValidations, Res.GetString("8ad011e5-560f-430e-be6f-a54723f02fc8", "Validation rule description must be unique for each workflow template"));
			}
		}

		protected override void CheckP0V_ContextType()
		{
			ListValidation.ErrorIfInvalidCodeOrEmpty(Parent.P0V_ContextTypeInfo, Parent.Lookups.ContextTypeList);

			base.CheckP0V_ContextType();
		}

		protected override void CheckP0V_RQT_RequestTypeOnFailure()
		{
			if (!Parent.P0V_RQT_RequestTypeOnFailure.IsEmpty)
			{
				ListValidation.ErrorIfInvalidPK(Parent.P0V_RQT_RequestTypeOnFailureInfo, Parent.Lookups.RequestTypesOnFailure);
			}

			base.CheckP0V_RQT_RequestTypeOnFailure();
		}

		protected override void CheckP0V_FieldToDisplayValidation()
		{
			base.CheckP0V_FieldToDisplayValidation();

			if (Parent.HasValidationAction(ProcessTemplateValidationActionSourceList.Codes.FieldSpecificValidation))
			{
				MandatoryValidation.CheckEntered(Parent.P0V_FieldToDisplayValidationInfo);

				var fieldToDisplayValidationStaticInfo = Parent.FieldToDisplayValidationStaticInfo;
				if (fieldToDisplayValidationStaticInfo is null || !fieldToDisplayValidationStaticInfo.IsZPropertyInfo)
				{
					Parent.P0V_FieldToDisplayValidationInfo.AddWarning(Res.GetString("a8c87883-71a0-4fa5-ac8b-d6886fe3ccdb", "Validation messages must be displayed on a pre-existing field."));
				}
			}
		}
	}
}
