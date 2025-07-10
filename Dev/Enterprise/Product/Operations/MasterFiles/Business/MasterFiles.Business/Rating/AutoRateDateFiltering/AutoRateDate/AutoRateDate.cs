using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class AutoRateDate : ChargeGroupSetting, IAutoRateDate
	{
		#region Schema

		public new abstract class Schema : ChargeGroupSetting.Schema
		{
			public const string DateType = "DateType";
			public const string Location = "Location";
			public const string IsFallbackDisabled = "IsFallbackDisabled";
			public const string RateType = "RateType";
			public const string ContainerMode = "ContainerMode";
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new AutoRateDate();

		public override IJobConfigurationSelector[] ParentCollectionForValidation
		{
			get
			{
				var parentCollection = GetParentCollection(this, typeof(AutoRateDateCollection));
				return parentCollection != null
					? parentCollection.Cast<IJobConfigurationSelector>().ToArray()
					: System.Array.Empty<IJobConfigurationSelector>();
			}
		}

		protected override JobConfigurationSelectorReadOnly GetNewReadOnlyHelper() => new AutoRateDateReadOnly(this);

		public new AutoRateDateReadOnly ReadOnlyHelper => (AutoRateDateReadOnly)base.ReadOnlyHelper;

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateDateType();
		}

		public new AutoRateDateValidation Validation => (AutoRateDateValidation)base.Validation;

		protected override JobConfigurationSelectorValidation GetNewValidation() => new AutoRateDateValidation(this);

		#endregion

		#region Properties

		public override ZString JobType
		{
			get => base.JobType;
			set
			{
				base.JobType = value;
				AutoRateDateReadOnlyHelper.UpdateFieldIfShouldBeReadOnly(Location_ReadOnly, LocationInfo);
				JobConfigurationSelectorReadOnlyHelper.UpdateFieldIfShouldBeReadOnly(ContainerModeInfo, ref containerModeToRestore);
			}
		}

		public override ZString DirectionCode
		{
			get => base.DirectionCode;
			set
			{
				base.DirectionCode = value;
				AutoRateDateReadOnlyHelper.UpdateFieldIfShouldBeReadOnly(Location_ReadOnly, LocationInfo);
			}
		}

		public override ZString Mode
		{
			get => base.Mode;
			set
			{
				base.Mode = value;
				JobConfigurationSelectorReadOnlyHelper.UpdateFieldIfShouldBeReadOnly(ContainerModeInfo, ref containerModeToRestore);
			}
		}

		#region DateType

		[MaxLength(3)]
		[List("Lookups.DateTypeList")]
		public ZString DateType
		{
			get => dateType;
			set
			{
				SetNonPersistentPropertyValue(DateTypeInfo, ref dateType, value);
				if (!IsValidationSuspended)
				{
					ValidateDateType();
				}

				if (value != JobDateTypes.Codes.HouseBillIssueDate)
				{
					IsFallbackDisabled = false;
				}
				if (value == JobDateTypes.Codes.GateOutDate)
				{
					IsFallbackDisabled = true;
				}
			}
		}

		public virtual ZPropertyInfo DateTypeInfo => GetZPropertyInfo(Schema.DateType);

		public void ValidateDateType()
		{
			DateTypeInfo.ClearAllNotifications();
			Validation.ValidateDateType();
		}

		ZString dateType;

		#endregion

		#region Location

		[List("Lookups.AutoRatingLocationCollection")]
		public ZString Location
		{
			get => location;
			set
			{
				SetNonPersistentPropertyValue(LocationInfo, ref location, value);
				if (!IsValidationSuspended)
				{
					ValidateLocation();
				}
			}
		}
		ZString location;

		public virtual ZPropertyInfo LocationInfo => GetZPropertyInfo(Schema.Location);

		public void ValidateLocation()
		{
			LocationInfo.ClearAllNotifications();
			Validation.ValidateLocation();
		}

		public virtual bool Location_ReadOnly => ReadOnlyHelper.Location_ReadOnly;

		#endregion

		#region IsFallbackDisabled

		public ZBool IsFallbackDisabled
		{
			get => isFallbackDisabled;
			set
			{
				SetNonPersistentPropertyValue(IsFallbackDisabledInfo, ref isFallbackDisabled, value);
			}
		}

		ZBool isFallbackDisabled;

		public virtual ZPropertyInfo IsFallbackDisabledInfo => GetZPropertyInfo(nameof(IsFallbackDisabled));

		protected bool IsFallbackDisabled_ReadOnly => DateType != JobDateTypes.Codes.HouseBillIssueDate && DateType != JobDateTypes.Codes.GateOutDate;

		#endregion

		#region RateType

		[MaxLength(3)]
		[List("Lookups.RateTypeList")]
		public ZString RateType
		{
			get => rateType;
			set
			{
				CheckMaximumLength(RateTypeInfo, value);
				SetNonPersistentPropertyValue(RateTypeInfo, ref rateType, value);
				if (!IsValidationSuspended)
				{
					ValidateRateType();
				}
			}
		}
		ZString rateType;

		public virtual ZPropertyInfo RateTypeInfo => GetZPropertyInfo(Schema.RateType);

		public void ValidateRateType()
		{
			RateTypeInfo.ClearAllNotifications();
			Validation.ValidateRateType();
		}

		#endregion

		#region ContainerMode

		[MaxLength(3)]
		[List("Lookups.ContainerModeList")]
		public ZString ContainerMode
		{
			get => containerMode;
			set
			{
				CheckMaximumLength(ContainerModeInfo, value);
				SetNonPersistentPropertyValue(ContainerModeInfo, ref containerMode, value);
				if (!IsValidationSuspended)
				{
					ValidateContainerMode();
				}
			}
		}
		ZString containerMode;
		ZString containerModeToRestore;

		public virtual ZPropertyInfo ContainerModeInfo => GetZPropertyInfo(Schema.ContainerMode);

		public void ValidateContainerMode()
		{
			ContainerModeInfo.ClearAllNotifications();
			Validation.ValidateContainerMode();
		}

		public virtual bool ContainerMode_ReadOnly => ReadOnlyHelper.ContainerMode_ReadOnly;

		#endregion

		#endregion

		public AutoRateDateLookups Lookups => lookups ??= new(this);
		AutoRateDateLookups lookups;

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.DateType, DateType);
			writer.WriteElementString(Schema.Location, Location);
			writer.WriteElementString(Schema.IsFallbackDisabled, IsFallbackDisabled.ToString());
			writer.WriteElementString(Schema.RateType, RateType);
			writer.WriteElementString(Schema.ContainerMode, ContainerMode);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			DateType = reader.ReadElementString(Schema.DateType);
			Location = reader.ReadElementString(Schema.Location);
			IsFallbackDisabled = reader.ReadElementStringAsZBool(Schema.IsFallbackDisabled);
			RateType = reader.ReadElementString(Schema.RateType);
			ContainerMode = reader.ReadElementString(Schema.ContainerMode);
		}

		#endregion
	}
}
