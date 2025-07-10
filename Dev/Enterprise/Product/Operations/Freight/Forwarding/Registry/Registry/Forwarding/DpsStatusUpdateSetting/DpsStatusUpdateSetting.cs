using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Freight.Forwarding.Registry
{
	[XmlSerializerAssembly("Enterprise.Freight.Forwarding.Registry.XmlSerializers")]
	public class DpsStatusUpdateSetting : RegistryBusinessObjectTemplate
	{
		public DpsStatusUpdateSetting()
		{
		}

		public DpsStatusUpdateSetting(PhaseSecurityRegistryItem phaseSecurityRegistryItem)
		{
			PhaseRegistryItem = phaseSecurityRegistryItem;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = new DpsStatusUpdateSetting(PhaseRegistryItem);
			using (clone.GetValidationSuspender())
			{
				clone.jobUpdateSettings = (JobPhaseSettingCollection)JobUpdateSettings.Clone(fallbackLevel, factory);
				clone.Option = Option;
			}
			return clone;
		}

		public PhaseSecurityRegistryItem PhaseRegistryItem { get; internal set; }

		#region Schema
		public abstract class Schema
		{
			public const string Option = "Option";
			public const int Option_MaxLength = 3;
		}
		#endregion

		#region Option
		[List("OptionList")]
		[MaxLength(Schema.Option_MaxLength)]
		public ZString Option
		{
			get => option;
			set
			{
				if (option != value)
				{
					SetNonPersistentPropertyValue(OptionInfo, ref option, value);
					if (!IsValidationSuspended)
					{
						ValidateOption();
					}
				}
			}
		}
		ZString option = DpsStatusUpdateOptions.Codes.DAB;

		public CodeDescriptionPairList OptionList => optionList ?? (optionList = new DpsStatusUpdateOptions());
		CodeDescriptionPairList optionList;

		public ZPropertyInfo OptionInfo => GetZPropertyInfo(Schema.Option);
		#endregion

		#region Xml Serialisation
		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Option = reader.ReadElementString(Schema.Option);
			JobUpdateSettings = (JobPhaseSettingCollection)JobPhaseSettingCollectionSerialiser.Deserialize(reader);
		}

		ZXmlSerializer JobPhaseSettingCollectionSerialiser
		{
			get => serialiser ?? (serialiser = ZXmlSerializer.New(typeof(JobPhaseSettingCollection)));
		}
		ZXmlSerializer serialiser;

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Option, Option);
			JobPhaseSettingCollectionSerialiser.Serialize(writer, jobUpdateSettings);
		}
		#endregion

		#region JobUpdateSettings

		[BusinessObjectTestExclude]
		public JobPhaseSettingCollection JobUpdateSettings
		{
			get
			{
				if (jobUpdateSettings == null)
				{
					jobUpdateSettings = new JobPhaseSettingCollection();
				}

				return SynchronizeUpdateSettings(jobUpdateSettings);
			}
			set
			{
				jobUpdateSettings = value;
				RegisterEditableChildObject(jobUpdateSettings);
			}
		}

		JobPhaseSettingCollection SynchronizeUpdateSettings(JobPhaseSettingCollection updateSettings)
		{
			JobPhaseSettingCollection result;
			if (PhaseRegistryItem == null || HasNoDifferencesWithPhaseRegistryValue(updateSettings))
			{
				result = updateSettings;
			}
			else
			{
				result = new JobPhaseSettingCollection();
				var phases = PhaseRegistryItem.Value.Phases;
				foreach (Phase phase in phases)
				{
					var updateSetting = updateSettings.FirstOrDefault(u => ((JobPhaseSetting)u).Code == phase.Code) as JobPhaseSetting;
					result.Add(new JobPhaseSetting(phase.Code, phase.Description, updateSetting?.ShouldUpdate ?? false));
				}

				JobUpdateSettings = result;
			}

			return result;
		}

		bool HasNoDifferencesWithPhaseRegistryValue(JobPhaseSettingCollection updateSettings)
		{
			var phases = PhaseRegistryItem.Value.Phases;
			return updateSettings.Count == phases.Count &&
				phases.Cast<Phase>().All(p => updateSettings.Cast<JobPhaseSetting>().Any(j => (j.Code == p.Code && j.Description == p.Description)));
		}

		JobPhaseSettingCollection jobUpdateSettings;
		#endregion

		#region Validation

		public void ValidateOption()
		{
			OptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(OptionInfo);
			ListValidation.ErrorIfInvalidCode(OptionInfo, OptionList);
			if (!OptionInfo.Value.Equals(DpsStatusUpdateOptions.Codes.DAB))
			{
				CheckPhases();
			}
		}

		void CheckPhases()
		{
			CheckPhasesConfigured();
			CheckAtLeastOnePhaseIsConfigured();
		}

		void CheckAtLeastOnePhaseIsConfigured()
		{
			if (PhaseRegistryItem != null &&
				Option != DpsStatusUpdateOptions.Codes.DAB &&
				!jobUpdateSettings.Cast<JobPhaseSetting>().Any(s => s.ShouldUpdate))
			{
				var emptyPhasesMessage = Res.GetString("136412b5-8a49-426f-98a6-8cc50d1434d0", "Please select at least one Phase to update the DPS status in {0} mode.", Option);
				OptionInfo.AddError(emptyPhasesMessage);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Baseline")]
		void CheckPhasesConfigured()
		{
			if (PhaseRegistryItem != null)
			{
				var phases = PhaseRegistryItem.Value.Phases;
				if (phases.Count <= 0)
				{
					var emptyPhasesMessage = Res.GetString("915cddee-3baf-428f-b737-8bff57a7bca3", "Please define Phases under {0} to select option {1}", ((IMultilingualRegistryItem)PhaseRegistryItem).LocationMultilingual, Option);
					OptionInfo.AddError(emptyPhasesMessage);
				}
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateOption();
		}

		#endregion
	}

	[XmlSerializerAssembly("Enterprise.Freight.Forwarding.Registry.XmlSerializers")]
	public class JobPhaseSetting : RegistryBusinessObject
	{
		public JobPhaseSetting()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public JobPhaseSetting(ZString phaseCode, MultilingualString description, ZBool needToUpdate)
		{
			Code = phaseCode;
			Description = description;
			ShouldUpdate = needToUpdate;
		}

		public new abstract class Schema
		{
			public const string ShouldUpdate = "ShouldUpdate";
			public const string Code = "Code";
		}

		ZBool shouldUpdate;
		public ZBool ShouldUpdate
		{
			get { return shouldUpdate; }
			set
			{
				SetNonPersistentPropertyValue(ShouldUpdateInfo, ref shouldUpdate, value);
			}
		}

		public ZPropertyInfo ShouldUpdateInfo => GetZPropertyInfo(Schema.ShouldUpdate);

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Code = reader.ReadElementString(Schema.Code);
			ShouldUpdate = new ZBool(reader.ReadElementString(Schema.ShouldUpdate));
		}

		protected override void WriteElements(XmlWriter writer)
		{
			writer.WriteElementString(Schema.Code, Code.ToString());
			writer.WriteElementString(Schema.ShouldUpdate, ShouldUpdate.ToString());
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new JobPhaseSetting(Code, Description, ShouldUpdate);
		}

		protected override int MaxDescriptionLength
		{
			get { return 256; }
		}
	}

	[XmlSerializerAssembly("Enterprise.Freight.Forwarding.Registry.XmlSerializers")]
	[XmlRoot("JobUpdateSettings")]
	public class JobPhaseSettingCollection : RegistryBusinessObjectCollectionTemplate
	{
		public JobPhaseSettingCollection()
		{
		}

		protected override bool AllowNewCore => false;

		public new JobPhaseSetting AddNew()
		{
			return (JobPhaseSetting)base.AddNew();
		}

		public new JobPhaseSetting this[int i]
		{
			get { return (JobPhaseSetting)base[i]; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new JobPhaseSetting();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new JobPhaseSettingCollection();
		}
	}
}
