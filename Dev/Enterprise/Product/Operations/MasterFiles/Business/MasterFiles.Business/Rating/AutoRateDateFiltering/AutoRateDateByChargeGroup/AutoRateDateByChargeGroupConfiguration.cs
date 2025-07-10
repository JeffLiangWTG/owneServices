using System.Collections.Generic;
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

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class AutoRateDateByChargeGroupConfiguration : RegistryBusinessObjectTemplate, IAutoRateDateByChargeGroupConfiguration
	{
		public abstract class Schema
		{
			public const string FilterType = "FilterType";
		}

		public AutoRateDateByChargeGroupConfiguration()
		{
		}

		public AutoRateDateByChargeGroupConfiguration(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AutoRateDateByChargeGroupConfiguration(fallbackLevel, null);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var configClone = (AutoRateDateByChargeGroupConfiguration)clone;
			if (configClone != null && autoRateDateByChargeGroups != null)
			{
				configClone.autoRateDateByChargeGroups = (AutoRateDateByChargeGroupCollection)autoRateDateByChargeGroups.Clone(configClone.CurrentFallbackLevel, configClone.Factory);
				configClone.autoRateDateByChargeGroups.CurrentFallbackLevel = autoRateDateByChargeGroups.CurrentFallbackLevel;
				configClone.RegisterEditableChildObject(configClone.autoRateDateByChargeGroups);
			}
		}

		#region FilterType

		[MaxLength(3)]
		[List("FilterTypeList")]
		public ZString FilterType
		{
			get { return filterType; }
			set
			{
				SetNonPersistentPropertyValue(FilterTypeInfo, ref filterType, value);
				AutoRateDateByChargeGroups.RefreshBinding();
			}
		}

		public ZPropertyInfo FilterTypeInfo
		{
			get { return GetZPropertyInfo(Schema.FilterType); }
		}

		ZString filterType;

		public CodeDescriptionPairList FilterTypeList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.DateFilterType); }
		}

		#endregion

		#region AutoRateDateByChargeGroups

		public AutoRateDateByChargeGroupCollection AutoRateDateByChargeGroups
		{
			get
			{
				if (autoRateDateByChargeGroups == null)
				{
					autoRateDateByChargeGroups = new AutoRateDateByChargeGroupCollection();

					var chargeCodeGroups = new ChargeCodeGroupList();
					foreach (var group in chargeCodeGroups.Cast<CodeDescriptionPair>().Where(x => !NonApplicableChargeGroups.Contains(x.Code)))
					{
						var defaultValue = autoRateDateByChargeGroups.AddNew();
						defaultValue.ChargeGroup = group.Code;
						defaultValue.ChargeGroupDescription = group.MultilingualDescription;
					}

					RegisterEditableChildObject(autoRateDateByChargeGroups);
				}
				return autoRateDateByChargeGroups;
			}
		}

		AutoRateDateByChargeGroupCollection autoRateDateByChargeGroups;

		HashSet<string> NonApplicableChargeGroups
		{
			get
			{
				if (nonApplicableChargeGroups == null)
				{
					nonApplicableChargeGroups = new HashSet<string>();
					nonApplicableChargeGroups.Add(ChargeCodeGroupList.Codes.CustomsDuty);
					nonApplicableChargeGroups.Add(ChargeCodeGroupList.Codes.NonJobRelated);
					nonApplicableChargeGroups.Add(ChargeCodeGroupList.Codes.NotGrouped);
				}

				return nonApplicableChargeGroups;
			}
		}

		HashSet<string> nonApplicableChargeGroups;

		#endregion

		#region Xml Serialization

		ZXmlSerializer ChargeGroupSetupSerialiser
		{
			get { return chargeGroupSetupSerialiser ?? (chargeGroupSetupSerialiser = ZXmlSerializer.New(typeof(AutoRateDateByChargeGroupCollection))); }
		}

		ZXmlSerializer chargeGroupSetupSerialiser;

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.FilterType, FilterType);
			ChargeGroupSetupSerialiser.Serialize(writer, AutoRateDateByChargeGroups);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			FilterType = reader.ReadElementString(Schema.FilterType);
			autoRateDateByChargeGroups = (AutoRateDateByChargeGroupCollection)ChargeGroupSetupSerialiser.Deserialize(reader);
			RegisterEditableChildObject(autoRateDateByChargeGroups);
		}

		#endregion

		IEnumerable<IAutoRateDate> IAutoRateDateByChargeGroupConfiguration.GetAutoRateDates(string chargeGroup)
		{
			return AutoRateDateByChargeGroups
				.Cast<AutoRateDateByChargeGroup>()
				.FirstOrDefault(autoRateDateByChargeGroup => autoRateDateByChargeGroup.ChargeGroup == chargeGroup)
				?.ChargeGroupSettings.Cast<IAutoRateDate>();
		}
	}
}
