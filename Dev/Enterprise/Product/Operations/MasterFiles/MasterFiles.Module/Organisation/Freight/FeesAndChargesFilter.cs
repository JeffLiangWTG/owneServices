using System;
using System.Linq;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Module
{
	public class FeesAndChargesFilter : ModuleTextBaseFilter
	{
		public FeesAndChargesFilter(ZString description, GetFeesAndChargesQueryDelegate queryDelegate)
			: base(description, queryDelegate)
		{ }

		#region Lists

		public CodeDescriptionPairList ServiceTypeList
		{
			get
			{
				if (serviceTypeList == null)
				{
					serviceTypeList = new CodeDescriptionPairList();

					foreach (FeeChargeType type in OrganisationsDataRegistry.Instance.RateFeeChargeLevels.Value.FeeChargeTypes)
					{
						serviceTypeList.AddPair(type.Code, (ZString)type.Description);
					}
				}
				return serviceTypeList;
			}
		}
		CodeDescriptionPairList serviceTypeList;

		public CodeDescriptionPairList ServiceLevelList
		{
			get
			{
				serviceLevelList = new CodeDescriptionPairList();
				var chargeType = OrganisationsDataRegistry.Instance.RateFeeChargeLevels.Value.FeeChargeTypes.Cast<FeeChargeType>().FirstOrDefault(c => c.Code == ServiceType);

				if (chargeType != null)
				{
					foreach (FeeChargeLevel level in chargeType.FeeChargeLevels)
					{
						serviceLevelList.AddPair(level.Code, level.Description);
					}
				}

				return serviceLevelList;
			}
		}
		CodeDescriptionPairList serviceLevelList;

		#endregion

		#region ServiceType

		[List("ServiceTypeList")]
		public ZString ServiceType
		{
			get { return serviceType; }
			set
			{
				if (serviceType != value)
				{
					serviceType = value;
					ServiceLevel = string.Empty;
				}
			}
		}
		ZString serviceType;

		#endregion

		#region ServiceLevel

		[List("ServiceLevelList")]
		public ZString ServiceLevel { get; set; }

		#endregion

		#region Delegate

		public delegate ZQuery GetFeesAndChargesQueryDelegate(ZString serviceType, ZString serviceLevel);

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { ServiceType, ServiceLevel }; }
		}

		#endregion

		#region GetNewCommonModuleFilter, CopyPersistantValuesFromFilter

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			throw new NotSupportedException();
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var filter = (FeesAndChargesFilter)filterToCopyFrom;
			ServiceType = filter.ServiceType;
			ServiceLevel = filter.ServiceLevel;
		}

		#endregion

		#region Clear / IsEmpty / Defaults

		protected override void ClearCore()
		{
			ServiceType = string.Empty;
			ServiceLevel = string.Empty;
		}

		protected override bool IsEmptyCore => ServiceType.IsEmpty && ServiceLevel.IsEmpty;

		public override bool IsExpensiveQuery => false;

		protected override FilterCategory DefaultCategory => FilterCategories.Other;

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString("ServiceType", ServiceType);
			writer.WriteElementString("ServiceLevel", ServiceLevel);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			if (reader.Name == "ServiceType")
			{
				ServiceType = reader.ReadElementString("ServiceType");
			}

			if (reader.Name == "ServiceLevel")
			{
				ServiceLevel = reader.ReadElementString("ServiceLevel");
			}
		}

		#endregion
	}
}
