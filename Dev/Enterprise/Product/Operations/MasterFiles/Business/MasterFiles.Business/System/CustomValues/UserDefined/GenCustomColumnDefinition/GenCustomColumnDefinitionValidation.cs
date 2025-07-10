//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGenCustomColumnDefinitionValidation
//
//    This class should be used for overriding validation in AutoGenCustomColumnDefinitionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public class GenCustomColumnDefinitionValidation : AutoGenCustomColumnDefinitionValidation
	{
		public GenCustomColumnDefinitionValidation(AutoGenCustomColumnDefinition parent)
			: base(parent)
		{
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
		protected override void CheckXC_Name()
		{
			MandatoryValidation.CheckEntered(Parent.XC_NameInfo);
			TranslatableDataFieldAttribute.Validate(Parent.XC_NameInfo);

			if (Parent.XC_Type == AddOnColumnDataType.Codes.ComboBox)
			{
				var maxLength = Custom_Field_Name_Max_Length - (AddOnColumnDataType.PartIdentifier.Length + 1);
				if (Parent.XC_Name.Length > maxLength)
				{
					Parent.XC_NameInfo.AddError(Res.GetString("816988d8-c279-4be1-80cf-1ca179b58e8c", "Enter a {0} no longer than {1} characters.", Parent.XC_NameInfo.HumanReadableName, maxLength));
				}
			}
			else if (Parent.XC_Name.Length > Custom_Field_Name_Max_Length)
			{
				Parent.XC_NameInfo.AddError(Res.GetString("816988d8-c279-4be1-80cf-1ca179b58e8c", "Enter a {0} no longer than {1} characters.", Parent.XC_NameInfo.HumanReadableName, Custom_Field_Name_Max_Length));
			}

			if (Parent.XC_Name.Contains(AddOnColumnDataType.PartIdentifier + "1", StringComparison.Ordinal) || Parent.XC_Name.Contains(AddOnColumnDataType.PartIdentifier + "2", StringComparison.Ordinal))
			{
				Parent.XC_NameInfo.AddError(Res.GetString("7b45043d-a36a-4734-967d-521b6162ae23", "{0} cannot contain {1} followed by a number.", Parent.XC_NameInfo.HumanReadableName, AddOnColumnDataType.PartIdentifier));
			}

			if (new Regex(@",\s*(\d+)$").IsMatch(Parent.XC_Name))
			{
				Parent.XC_NameInfo.AddError(Res.GetString("2e6a0758-ece9-4225-93b5-42060636866d", "{0} cannot end with ',' followed by a number.", Parent.XC_NameInfo.HumanReadableName, AddOnColumnDataType.PartIdentifier));
			}

			if (Parent.XC_ParentTableCode == ProcessTaskTemplateSchema.Constants.Prefix)
			{
				ProcessTaskTemplate processTaskTemplate = Parent.Factory.Load<ProcessTaskTemplate>(Parent.XC_ParentID);
				if (processTaskTemplate != null)
				{
					if (processTaskTemplate.GenCustomColumnDefinitions.Any(
						otherDefinition => otherDefinition != Parent && !otherDefinition.IsDeleted && !otherDefinition.XC_Name.IsEmpty && otherDefinition.XC_Name.EqualsIgnoringCase(Parent.XC_Name)))
					{
						Parent.XC_NameInfo.AddError(Res.GetString("d0b1baba-72dd-4491-9be4-9c172a4ddc91", "The {0} has been duplicated and must be unique.", Parent.XC_NameInfo.HumanReadableName));
					}

					WarningIfThereIsOtherColumnWithSameNameAndDifferentType(processTaskTemplate);

					if (processTaskTemplate.WorkflowDescriptor != null && processTaskTemplate.WorkflowDescriptor.WorkflowProviderType.GetProperties().Any(property => property.Name == Parent.XC_Name))
					{
						Parent.XC_NameInfo.AddWarning(Res.GetString("1A0780A0-DC6E-466E-9E94-EE4A2E1288EC", "There is a system property with same name '{0}' on related business object.", Parent.XC_Name));
					}
				}
			}
		}

		void WarningIfThereIsOtherColumnWithSameNameAndDifferentType(ProcessTaskTemplate processTaskTemplate)
		{
			if (!Parent.XC_Name.IsEmpty && !Parent.XC_Type.IsEmpty && !processTaskTemplate.P0_ProcessType.IsEmpty)
			{
				ZDBOnlySubQuery columnsSubQuery = new ZDBOnlySubQuery(typeof(GenCustomColumnDefinition), GenCustomColumnDefinitionSchema.XC_ParentID);
				columnsSubQuery.AddToFilter(GenCustomColumnDefinitionSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				columnsSubQuery.AddToFilter(GenCustomColumnDefinitionSchema.XC_Name, SQLComparisonOperator.Equal, Parent.XC_Name);
				columnsSubQuery.AddToFilter(GenCustomColumnDefinitionSchema.XC_Type, SQLComparisonOperator.NotEqual, Parent.XC_Type);

				ZDBOnlyQuery templatesQuery = new ZDBOnlyQuery(typeof(ProcessTaskTemplate));
				templatesQuery.AddToFilter(ProcessTaskTemplateSchema.P0_ProcessType, processTaskTemplate.P0_ProcessType);
				templatesQuery.AddToFilter(ProcessTaskTemplateSchema.P0_IsActive, true);
				templatesQuery.AddSubQuery(columnsSubQuery, JoinCondition.And);

				if (Parent.Factory.LoadTop1<ProcessTaskTemplate>(templatesQuery) != null)
				{
					Parent.XC_NameInfo.AddWarning(Res.GetString("079f215d-f25f-4b70-81b8-4718fbb83f51",
@"There is a Custom Field definition in other Workflow Template with same Name, but different Type.
This may cause confusions in a case when such Custom Fields of several Workflow Templates are grouped together, e.g. in a module filter grid."));
				}
			}
		}

		protected override void CheckXC_Type()
		{
			MandatoryValidation.CheckEntered(Parent.XC_TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.XC_TypeInfo);
		}

		protected override void CheckXC_XR()
		{
			if (Parent.XC_XR.IsValid && Parent.Lookups.Types.ContainsCode(Parent.XC_Type))
			{
				Type type = AddOnColumnDataType.GetTypeFromCode(Parent.XC_Type);
				var rulesWhichCannotBeApplied = ((ICustomColumnDefinition)Parent).GetRules().Where(r => r.IsEnabled && !r.CanBeApplied(type));
				if (rulesWhichCannotBeApplied.Any())
				{
					Parent.XC_XRInfo.AddError(Res.GetString("3f518e1f-1171-45fa-801a-0fe8296b2915", "The '{0}' rule cannot be applied to the {1} field.",
						Parent.CustomAddOnRule.XR_Code, Parent.Lookups.Types.GetDescriptionFromCode(Parent.XC_Type)));
				}
			}
		}

		protected override void CheckXC_DisplaySequence()
		{
			MandatoryValidation.CheckNotNegative(Parent.XC_DisplaySequenceInfo);
		}

		public const int Custom_Field_Name_Max_Length = 32;
	}
}
