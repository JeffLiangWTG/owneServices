using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Workflow.Business
{
	public class EDIMessageContentFilterAdditionalConfiguration : NonPersistentBusinessObject
	{
		public EDIMessageContentFilterAdditionalConfiguration(EDIMessageContentFilterSpec parent)
			: base(parent.Factory)
		{
			Parent = parent;
		}

		public EDIMessageContentFilterSpec Parent { get; }

		[XmlColumnProperty]
		[List("Lookups.PrimaryDataSource")]
		[ResourceStringData("EDIMessageContentFilterSchema.AdditionalConfiguration.PrimaryDataSource", Caption = "Primary Data Source", FullDescription = "Controls Universal Shipment Primary Data Source.")]
		public ZString PrimaryDataSource
		{
			get => GetXmlColumnPropertyValue<ZString>(PrimaryDataSourceInfo);
			set => SetXmlColumnPropertyValue(PrimaryDataSourceInfo, value, Validation.ValidateFilterContent);
		}

		public ZPropertyInfo PrimaryDataSourceInfo => GetZPropertyInfo(nameof(PrimaryDataSource));

		public EDIMessageContentFilterAdditionalConfigurationLookups Lookups => new EDIMessageContentFilterAdditionalConfigurationLookups(this);

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public EDIMessageContentFilterAdditionalConfigurationValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual EDIMessageContentFilterAdditionalConfigurationValidation GetNewValidation()
		{
			return new EDIMessageContentFilterAdditionalConfigurationValidation(this);
		}
	}
}
