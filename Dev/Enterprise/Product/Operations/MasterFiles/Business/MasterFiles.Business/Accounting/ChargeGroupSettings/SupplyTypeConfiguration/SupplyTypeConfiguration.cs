using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class SupplyTypeConfiguration : ChargeGroupSetting, ISupplyTypeSelector
	{
		public SupplyTypeConfiguration() : base()
		{
		}

		public SupplyTypeConfiguration(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		#region Schema

		public new abstract class Schema : ChargeGroupSetting.Schema
		{
			public const string Incoterm = "Incoterm";
			public const string LineDepartmentPK = "LineDepartmentPK";
			public const string SupplyType = "SupplyType";
		}

		#endregion

		public override IJobConfigurationSelector[] ParentCollectionForValidation
		{
			get
			{
				var parentCollection = GetParentCollection(this, typeof(SupplyTypeConfigurationCollection));
				return parentCollection != null ? parentCollection.Cast<ISupplyTypeSelector>().ToArray() : System.Array.Empty<ISupplyTypeSelector>();
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new SupplyTypeConfiguration(fallbackLevel);
		}

		#region Validation

		public new SupplyTypeConfigurationValidation Validation
		{
			get { return (SupplyTypeConfigurationValidation)base.Validation; }
		}

		protected override JobConfigurationSelectorValidation GetNewValidation()
		{
			return new SupplyTypeConfigurationValidation(this);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateIncoterm();
			ValidateLineDepartmentPK();
			ValidateSupplyType();
		}

		#endregion

		#region Properties

		[MaxLength(3)]
		[List("JobTypeList")]
		public override ZString JobType
		{
			get { return base.JobType; }
			set
			{
				base.JobType = value;
				UpdateInfoTermIfShouldBeReadonly();
			}
		}

		void UpdateInfoTermIfShouldBeReadonly()
		{
			if (IncoTerm_ReadOnly)
			{
				Incoterm = AccountingMasterFilesConstants.INCOTermCodes.All;
			}
		}

		#region Incoterm

		[List("IncotermList")]
		[ReadOnlyMember(nameof(IncoTerm_ReadOnly))]
		public ZString Incoterm
		{
			get { return fIncoterm; }
			set
			{
				SetNonPersistentPropertyValue(IncotermInfo, ref fIncoterm, value);
			}
		}
		bool IncoTerm_ReadOnly => JobType == JobInvoicingConsumerTypes.ForwardingConsolCode;

		public ZPropertyInfo IncotermInfo
		{
			get { return GetZPropertyInfo(Schema.Incoterm); }
		}

		public CodeDescriptionPairList IncotermList
		{
			get { return SupplyTypeConfigurationLookups.IncotermList; }
		}

		ZString fIncoterm;

		public void ValidateIncoterm()
		{
			IncotermInfo.ClearAllNotifications();

			Validation.ValidateIncoterm();
		}

		#endregion

		#region Line Department

		[RelatedBusinessObject("LineDepartment")]
		[List("DepartmentList")]
		public ZGuid LineDepartmentPK
		{
			get { return fLineDepartmentPK; }
			set
			{
				SetNonPersistentPropertyValue(LineDepartmentPKInfo, ref fLineDepartmentPK, value);
			}
		}

		public ZPropertyInfo LineDepartmentPKInfo
		{
			get { return GetZPropertyInfo(Schema.LineDepartmentPK); }
		}

		public GlbDepartment LineDepartment
		{
			get { return (GlbDepartment)(new ReadOnlyBusinessObjectFactory().Load(typeof(GlbDepartment), LineDepartmentPK)); }
		}

		public GlbDepartmentCollection DepartmentList
		{
			get { return SupplyTypeConfigurationLookups.DepartmentList; }
		}

		ZGuid fLineDepartmentPK;

		public void ValidateLineDepartmentPK()
		{
			LineDepartmentPKInfo.ClearAllNotifications();

			Validation.ValidateLineDepartmentPK();
		}

		#endregion

		#region Supply Type

		[List("SupplyTypeList")]
		public ZString SupplyType
		{
			get { return fSupplyType; }
			set
			{
				SetNonPersistentPropertyValue(SupplyTypeInfo, ref fSupplyType, value);
			}
		}

		public ZPropertyInfo SupplyTypeInfo
		{
			get { return GetZPropertyInfo(Schema.SupplyType); }
		}

		public CodeDescriptionPairList SupplyTypeList
		{
			get { return SupplyTypeConfigurationLookups.SupplyTypeList; }
		}

		ZString fSupplyType;

		public void ValidateSupplyType()
		{
			SupplyTypeInfo.ClearAllNotifications();

			Validation.ValidateSupplyType();
		}

		#endregion

		#endregion

		#region SupplyTypeConfigurationLookups

		public SupplyTypeConfigurationLookups SupplyTypeConfigurationLookups
		{
			get { return (SupplyTypeConfigurationLookups)ChargeGroupSettingLookups; }
		}

		protected override JobConfigurationSelectorLookups GetNewLookups()
		{
			return new SupplyTypeConfigurationLookups(this);
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Incoterm, Incoterm);
			writer.WriteElementString(Schema.LineDepartmentPK, LineDepartmentPK.ToString());
			writer.WriteElementString(Schema.SupplyType, SupplyType);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);

			Incoterm = reader.ReadElementString(Schema.Incoterm);
			LineDepartmentPK = new ZGuid(reader.ReadElementString(Schema.LineDepartmentPK));
			SupplyType = reader.ReadElementString(Schema.SupplyType);
		}

		#endregion
	}
}
