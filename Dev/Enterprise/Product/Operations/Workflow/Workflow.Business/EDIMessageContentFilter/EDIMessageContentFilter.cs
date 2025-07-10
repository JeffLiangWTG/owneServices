using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business
{
	[DescriptionProperty(EDIMessageContentFilterSchema.Constants.ECF_Name)]
	[CodeProperty(EDIMessageContentFilterSchema.Constants.ECF_Name)]
	public class EDIMessageContentFilter : AutoEDIMessageContentFilter, IEDIMessageContentFilter
	{
		public EDIMessageContentFilter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			using (SuspendSettingHasChanges())
			{
				ECF_FilterType = EDIMessageContentFilterTypes.Codes.Exclude;
				using (UniversalShipment.SuspendSettingHasChanges())
				using (UniversalEvent.SuspendSettingHasChanges())
				using (UniversalTransaction.SuspendSettingHasChanges())
				{
					UniversalTransaction.FilterType = UniversalShipment.FilterType = UniversalEvent.FilterType = ECF_FilterType;
				}
			}
		}

		public override void OnLoaded()
		{
			base.OnLoaded();

			// A defect in the introduction of XUT filters has resulted in some customer DBs having empty filter types.
			// These should be replaced with the default value on load.
			UniversalEvent.MakeEmptyFilterTypeDefaultValue();
			UniversalShipment.MakeEmptyFilterTypeDefaultValue();
			UniversalTransaction.MakeEmptyFilterTypeDefaultValue();
		}

		protected override ZString HumanReadableNameCore => ECF_Name;

		#endregion

		#region Xml Boilerplate

		protected override IEnumerable<XmlColumnSpecification> XmlSerialisedColumns => xmlColumnSpecifications;
		readonly static ImmutableArray<XmlColumnSpecification> xmlColumnSpecifications = ImmutableArray.Create(new XmlColumnSpecification(EDIMessageContentFilterSchema.ECF_FilterContent));

		#endregion

		#region Properties

		[List("Lookups.FilterTypes")]
		public override ZString ECF_FilterType
		{
			get => base.ECF_FilterType;
			set => base.ECF_FilterType = value;
		}

		[MaxLength(200)]
		[XmlColumnProperty]
		public ZString Description
		{
			get => GetXmlColumnPropertyValue<ZString>(DescriptionInfo);
			set => SetXmlColumnPropertyValue(DescriptionInfo, value);
		}

		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(nameof(Description));

		#endregion

		#region Collections

		[XmlColumnProperty]
		public EDIMessageContentFilterSpec UniversalEvent
		{
			get
			{
				if (universalEvent == null)
				{
					universalEvent = new EDIMessageContentFilterSpec(Factory, EDIMessageContentFilterLineSchemas.Codes.UniversalEvent);
					RegisterEditableChildObject(universalEvent);
				}
				return universalEvent;
			}
		}
		EDIMessageContentFilterSpec universalEvent;

		[XmlColumnProperty]
		public EDIMessageContentFilterUniversalShipmentSpec UniversalShipment
		{
			get
			{
				if (universalShipment == null)
				{
					universalShipment = new EDIMessageContentFilterUniversalShipmentSpec(Factory, EDIMessageContentFilterLineSchemas.Codes.UniversalShipment);
					RegisterEditableChildObject(universalShipment);
				}
				return universalShipment;
			}
		}
		EDIMessageContentFilterUniversalShipmentSpec universalShipment;

		[XmlColumnProperty]
		public EDIMessageContentFilterSpec UniversalTransaction
		{
			get
			{
				if (universalTransaction == null)
				{
					universalTransaction = new EDIMessageContentFilterSpec(Factory, EDIMessageContentFilterLineSchemas.Codes.UniversalTransaction);
					RegisterEditableChildObject(universalTransaction);
				}
				return universalTransaction;
			}
		}
		EDIMessageContentFilterSpec universalTransaction;

		[XmlColumnProperty]
		public EDIMessageContentFilterSharedAdditionalConfiguration Config
		{
			get
			{
				if (config == null)
				{
					config = new EDIMessageContentFilterSharedAdditionalConfiguration(Factory);
					RegisterEditableChildObject(config);
				}
				return config;
			}
		}
		EDIMessageContentFilterSharedAdditionalConfiguration config;

		#endregion

		#region Utils

		public string GetUniversalShipmentPrimaryDataSource()
		{
			return UniversalShipment.AdditionalConfiguration.PrimaryDataSource;
		}

		public static EDIMessageContentFilter Load(IFactory factory, ZString purposeCode)
		{
			var purpose = EDIMessagePurpose.Load(factory, purposeCode);
			return Load(factory, purpose);
		}

		public static EDIMessageContentFilter Load(IFactory factory, IEDIMessagePurpose purpose)
		{
			return purpose != null ? factory.Load<EDIMessageContentFilter>(purpose.EMP_ECF_Filter) : null;
		}

		#endregion
	}
}
