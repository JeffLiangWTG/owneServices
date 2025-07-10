using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Workflow.Business
{
	public class EDIMessageContentFilterSharedAdditionalConfiguration : NonPersistentBusinessObject
	{
		public EDIMessageContentFilterSharedAdditionalConfiguration(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			using (SuspendSettingHasChanges())
			{
				ExcludeEmptyElements = false;
			}
		}

		[XmlColumnProperty]
		public ZBool ExcludeEmptyElements
		{
			get => GetXmlColumnPropertyValue<ZBool>(ExcludeEmptyElementsInfo);
			set => SetXmlColumnPropertyValue(ExcludeEmptyElementsInfo, value);
		}

		public ZPropertyInfo ExcludeEmptyElementsInfo => GetZPropertyInfo(nameof(ExcludeEmptyElements));

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public EDIMessageContentFilterSharedAdditionalConfigurationValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual EDIMessageContentFilterSharedAdditionalConfigurationValidation GetNewValidation()
		{
			return new EDIMessageContentFilterSharedAdditionalConfigurationValidation(this);
		}

		#endregion

	}
}
