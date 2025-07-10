using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Workflow.Business
{
	public class EDIMessageContentFilterSpec : NonPersistentBusinessObject
	{
		public EDIMessageContentFilterSpec(BusinessObjectFactory factory, ZString schema)
			: base(factory)
		{
			Schema = schema;
		}

		public ZString Schema { get; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			using (SuspendSettingHasChanges())
			{
				FilterType = EDIMessageContentFilterTypes.Codes.Exclude;
			}
		}

		internal void MakeEmptyFilterTypeDefaultValue()
		{
			if (FilterType.IsEmpty)
			{
				using (SuspendSettingHasChanges())
				{
					FilterType = EDIMessageContentFilterTypes.Codes.Exclude;
				}
			}
		}

		[XmlColumnProperty(DefaultValue = EDIMessageContentFilterTypes.Codes.Exclude)]
		[List("Lookups.Schemas")]
		[ResourceStringData("EDIMessageContentFilterSchema.FilterType", Caption = "Filter Type", FullDescription = "Controls how lines affect the messages this filter is applied to.")]
		public ZString FilterType
		{
			get => GetXmlColumnPropertyValue<ZString>(FilterTypeInfo);
			set => SetXmlColumnPropertyValue(FilterTypeInfo, value, Validation.ValidateFilterType);
		}

		public ZPropertyInfo FilterTypeInfo => GetZPropertyInfo(nameof(FilterType));

		[XmlColumnProperty]
		public EDIMessageContentFilterLineCollection Lines
		{
			get
			{
				if (lines == null)
				{
					lines = new EDIMessageContentFilterLineCollection(this);
					RegisterEditableChildObject(lines);
				}
				return lines;
			}
		}
		EDIMessageContentFilterLineCollection lines;

		[XmlColumnProperty]
		public EDIMessageContentFilterDocumentCollection Documents
		{
			get
			{
				if (documents == null)
				{
					documents = new EDIMessageContentFilterDocumentCollection(this);
					RegisterEditableChildObject(documents);
				}
				return documents;
			}
		}
		EDIMessageContentFilterDocumentCollection documents;

		public static bool SupportsDocuments(string code) => code == EDIMessageContentFilterLineSchemas.Codes.UniversalShipment;

		public static bool SupportsAdditionalConfiguration(string code) => code == EDIMessageContentFilterLineSchemas.Codes.UniversalShipment;

		public bool IsExclude => FilterType == EDIMessageContentFilterTypes.Codes.Exclude;

		#region Lookups

		public EDIMessageContentFilterSpecLookups Lookups => new EDIMessageContentFilterSpecLookups(this);

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public EDIMessageContentFilterSpecValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual EDIMessageContentFilterSpecValidation GetNewValidation()
		{
			return new EDIMessageContentFilterSpecValidation(this);
		}

		#endregion

	}
}
