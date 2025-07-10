namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	sealed class AdditionalGenCustomColumnDefinitionValidation : AutoGenCustomColumnDefinitionValidation
	{
		public AdditionalGenCustomColumnDefinitionValidation(GenCustomColumnDefinition parent)
			: base(parent)
		{
		}

		public string WarningToAdd { get; set; }

		protected override void CheckXC_Name()
		{
			base.CheckXC_Name();

			if (!string.IsNullOrWhiteSpace(WarningToAdd))
			{
				Parent.XC_NameInfo.AddWarning(WarningToAdd);
			}
		}
	}
}
