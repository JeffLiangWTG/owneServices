#define CODE_ANALYSIS

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondCargoDescCustomLabelsProvider : ICustomLabelsProvider
	{
		public CusInBondCargoDescCustomLabelsProvider(ICustomLabelsConfigOrgProvider configOrgProvider)
		{
			fConfigOrgProvider = configOrgProvider;
		}

		public ICustomLabelsConfigOrgProvider ConfigOrgProvider
		{
			get { return fConfigOrgProvider; }
		}

		public CustomLabelInfoList GetCustomFields(OrgHeader configOrg, BusinessObjectFactory factory)
		{
			if (configOrg != null)
			{
				CustomLabelInfoList result;
				var dictionary = configOrg.Factory.GetCachedValue("USInBondCommodity_CustomLabelsProvider", () => new Dictionary<ZGuid, CustomLabelInfoList>());
				if (!dictionary.TryGetValue(configOrg.PK, out result))
				{
					result = GetCustomFieldsCore(configOrg, factory);
					dictionary.Add(configOrg.PK, result);
				}
				return result;
			}
			return customFieldsCached ?? (customFieldsCached = GetCustomFieldsCore(configOrg, factory));
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		CustomLabelInfoList GetCustomFieldsCore(OrgHeader configOrg, BusinessObjectFactory factory)
		{
			var customFields = new CustomLabelInfoList(typeof(CusInBondCargoDesc), configOrg, ResString.GetMultilingualString("C4ADEE03-03D8-4C12-BEA9-B87F0FD14D99", "importer on the header"), factory);
			if (ConfigOrgProvider != null && ConfigOrgProvider.ConfigOrg != null && ConfigOrgProvider.ConfigOrg.MiscServ != null)
			{
				customFields.Add(new PartCustomLabelInfo(ConfigOrgProvider.ConfigOrg.MiscServ.OM_IMPartAttrib1Name, ConfigOrgProvider.ConfigOrg.MiscServ.OM_IMPartAttrib1Type, CusInBondCargoDesc.Schema.BY_PartAttrib1, typeof(ZString), (NoResString)"Part Attribute 1", ConfigOrgProvider.ConfigOrg, factory));
				customFields.Add(new PartCustomLabelInfo(ConfigOrgProvider.ConfigOrg.MiscServ.OM_IMPartAttrib2Name, ConfigOrgProvider.ConfigOrg.MiscServ.OM_IMPartAttrib2Type, CusInBondCargoDesc.Schema.BY_PartAttrib2, typeof(ZString), (NoResString)"Part Attribute 2", ConfigOrgProvider.ConfigOrg, factory));
				customFields.Add(new PartCustomLabelInfo(ConfigOrgProvider.ConfigOrg.MiscServ.OM_IMPartAttrib3Name, ConfigOrgProvider.ConfigOrg.MiscServ.OM_IMPartAttrib3Type, CusInBondCargoDesc.Schema.BY_PartAttrib3, typeof(ZString), (NoResString)"Part Attribute 3", ConfigOrgProvider.ConfigOrg, factory));

				var caption = ConfigOrgProvider.ConfigOrg.MiscServ.OM_IMUseSerialNumber
					? Res.GetString("e7f27994-22f7-4252-bd13-88ffea5c9655", "Serial Number")
					: "";
				customFields.Add(new PartCustomLabelInfo(caption, "", CusInBondCargoDesc.Schema.BY_SerialNumber, typeof(ZString), (NoResString)"Serial Number", ConfigOrgProvider.ConfigOrg, factory));
			}
			return customFields;
		}

		protected ICustomLabelsConfigOrgProvider fConfigOrgProvider;
		CustomLabelInfoList customFieldsCached;
	}
}
