using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public abstract class ChargeGroupSetting : RegistryBusinessObjectTemplate, IJobConfigurationSelector
	{
		protected ChargeGroupSetting() : base()
		{
		}

		protected ChargeGroupSetting(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		#region Schema

		public abstract class Schema
		{
			public const string JobType = "JobType";
			public const string DirectionCode = "DirectionCode";
			public const string Mode = "Mode";
		}

		#endregion

		IJobConfigurationSelector[] IJobConfigurationSelector.ParentCollectionForValidation => ParentCollectionForValidation;

		public abstract IJobConfigurationSelector[] ParentCollectionForValidation { get; }

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateJobType();
			ValidateDirectionCode();
			ValidateMode();
		}

		public JobConfigurationSelectorValidation Validation => GetNewValidation();

		protected abstract JobConfigurationSelectorValidation GetNewValidation();

		#endregion

		#region Properties

		#region JobType

		[MaxLength(3)]
		[List("JobTypeList")]
		public virtual ZString JobType
		{
			get { return fJobType; }
			set
			{
				CheckMaximumLength(JobTypeInfo, value);
				SetNonPersistentPropertyValue(JobTypeInfo, ref fJobType, value);
				if (!IsValidationSuspended)
				{
					ValidateJobType();
				}

				JobConfigurationSelectorReadOnlyHelper.UpdateFieldIfShouldBeReadOnly(DirectionCodeInfo, ref directionCodeToRestore);
				JobConfigurationSelectorReadOnlyHelper.UpdateFieldIfShouldBeReadOnly(ModeInfo, ref modeToRestore);
			}
		}

		public virtual ZPropertyInfo JobTypeInfo => GetZPropertyInfo(Schema.JobType);

		public virtual CodeDescriptionPairList JobTypeList => ChargeGroupSettingLookups.JobTypeList;

		public void ValidateJobType()
		{
			JobTypeInfo.ClearAllNotifications();

			Validation.ValidateJobType();
		}

		ZString fJobType;

		#endregion

		#region Direction

		[MaxLength(3)]
		[List("DirectionList")]
		public virtual ZString DirectionCode
		{
			get { return fDirectionCode; }
			set
			{
				CheckMaximumLength(DirectionCodeInfo, value);
				SetNonPersistentPropertyValue(DirectionCodeInfo, ref fDirectionCode, value);
				if (!IsValidationSuspended)
				{
					ValidateDirectionCode();
				}
			}
		}

		public virtual ZPropertyInfo DirectionCodeInfo => GetZPropertyInfo(Schema.DirectionCode);

		public virtual bool DirectionCode_ReadOnly => ReadOnlyHelper.DirectionCode_ReadOnly;

		public virtual CodeDescriptionPairList DirectionList => ChargeGroupSettingLookups.DirectionList;

		public void ValidateDirectionCode()
		{
			DirectionCodeInfo.ClearAllNotifications();

			Validation.ValidateDirectionCode();
		}

		ZString fDirectionCode;
		ZString directionCodeToRestore;

		#endregion

		#region Mode

		[MaxLength(3)]
		[List("ModeList")]
		public virtual ZString Mode
		{
			get { return fMode; }
			set
			{
				CheckMaximumLength(ModeInfo, value);
				SetNonPersistentPropertyValue(ModeInfo, ref fMode, value);
				if (!IsValidationSuspended)
				{
					ValidateMode();
				}
			}
		}

		public virtual ZPropertyInfo ModeInfo => GetZPropertyInfo(Schema.Mode);

		public virtual bool Mode_ReadOnly => ReadOnlyHelper.Mode_ReadOnly;

		public virtual CodeDescriptionPairList ModeList => ChargeGroupSettingLookups.ModeList;

		public void ValidateMode()
		{
			ModeInfo.ClearAllNotifications();

			Validation.ValidateMode();
		}

		ZString fMode;
		ZString modeToRestore;

		#endregion

		#endregion

		#region RevenueRecognitionLookups

		JobConfigurationSelectorLookups IJobConfigurationSelector.Lookups => ChargeGroupSettingLookups;

		public JobConfigurationSelectorLookups ChargeGroupSettingLookups => fLookups ??= GetNewLookups();
		JobConfigurationSelectorLookups fLookups;

		protected virtual JobConfigurationSelectorLookups GetNewLookups() => new JobConfigurationSelectorLookups(this);

		#endregion

		#region Read Only Helper

		public JobConfigurationSelectorReadOnly ReadOnlyHelper => fReadOnlyHelper ??= GetNewReadOnlyHelper();
		JobConfigurationSelectorReadOnly fReadOnlyHelper;

		protected virtual JobConfigurationSelectorReadOnly GetNewReadOnlyHelper() => new JobConfigurationSelectorReadOnly(this);

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.JobType, JobType);
			writer.WriteElementString(Schema.DirectionCode, DirectionCode);
			writer.WriteElementString(Schema.Mode, Mode);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected override void ReadElements(XmlReaderWrapper reader)
		{
			JobType = reader.ReadElementString(Schema.JobType);

			string oldDirectionName = (NoResString)"Direction";
			if (reader.Reader.Name == oldDirectionName)
			{
				var oldFormatValue = reader.ReadElementString(oldDirectionName);
				switch (oldFormatValue)
				{
					case "All":
						DirectionCode = Constants.FreightShipmentDirection.Code.All;
						break;
					case "Import":
						DirectionCode = Constants.FreightShipmentDirection.Code.Import;
						break;
					case "Export":
						DirectionCode = Constants.FreightShipmentDirection.Code.Export;
						break;
					case "Domestic":
						DirectionCode = Constants.FreightShipmentDirection.Code.Domestic;
						break;
					case "Other":
						DirectionCode = Constants.FreightShipmentDirection.Code.Other;
						break;
				}
			}
			else
			{
				DirectionCode = reader.ReadElementString(Schema.DirectionCode);
			}

			Mode = reader.ReadElementString(Schema.Mode);
		}

		#endregion
	}
}
