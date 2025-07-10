using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Tracking.Module
{
	public class USCForeignPortFilterStripBusinessObject : FilterStripBusinessObject
	{
		public USCForeignPortFilterStripBusinessObject()
		{
			this.QueryObjectType = typeof(ZZRefCusCodeListCombined);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddTextFilter("Code", ZZRefCusCodeListCombinedSchema.ZZD_Code).MultilingualDescription = ResString.GetMultilingualString("94fa4d5d-a9e8-43be-896f-f41926f65005", "Code");
			result.AddTextFilter("Name", ZZRefCusCodeListCombinedSchema.ZZD_Description).MultilingualDescription = ResString.GetMultilingualString("b7be6954-d89b-43af-9ff9-fddd948d9a17", "Name");
			result.AddTextFilter("Valid For Type", GetValidForTypeQuery, TypeList).MultilingualDescription = ResString.GetMultilingualString("3ed70142-0811-48cf-ba73-da6869a54bbb", "Valid For Type");
			return result;
		}

		CodeDescriptionPairList TypeList
		{
			get
			{
				if (typeList == null)
				{
					typeList = new CodeDescriptionPairList();
					typeList.AddPair(ForeignPortTypeList.Codes.AES, ForeignPortTypeList.Descriptions.AES);
					typeList.AddPair("COM", ForeignPortTypeList.Descriptions.Common);
					typeList.AddPair(ForeignPortTypeList.Codes.InBond, ForeignPortTypeList.Descriptions.InBond);
				}
				return typeList;
			}
		}
		CodeDescriptionPairList typeList;

		ZQuery GetValidForTypeQuery(ZString value)
		{
			var foreignPortTypes = new List<ZString>() { ForeignPortTypeList.Codes.Common };
			if (value != ForeignPortTypeList.Codes.Common)
			{
				foreignPortTypes.Add(value);
			}
			return ZZRefCusCodeListCombined.Loader.GetFilter(
				Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today,
				new RefCusCodeListAttributeFilter[]
				{
					new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.PortValidType, SQLComparisonOperator.Equal, foreignPortTypes.ToArray())
				},
				false);
		}
	}
}
