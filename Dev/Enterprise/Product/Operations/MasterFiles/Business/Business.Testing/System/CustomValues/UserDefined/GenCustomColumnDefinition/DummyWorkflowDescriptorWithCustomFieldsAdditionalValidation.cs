using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	sealed class DummyWorkflowDescriptorWithCustomFieldsAdditionalValidation : DummyWorkflowDescriptor
	{
		public override string Code
		{
			get { return "XXX"; }
		}

		public AdditionalGenCustomColumnDefinitionValidation AdditionalValidation
		{
			get;
			set;
		}

		protected internal override AutoGenCustomColumnDefinitionValidation GetAdditionalCustomColumnDefinitionValidation(GenCustomColumnDefinition customColumnDefinition)
		{
			return AdditionalValidation;
		}
	}
}
