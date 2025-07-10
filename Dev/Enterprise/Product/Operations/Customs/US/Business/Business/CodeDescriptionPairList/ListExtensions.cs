using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public static class Extensions
	{
		public static CodeDescriptionPairList GetCachedUSStateList(this BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("USStateList", delegate
			{ return new OrgCodeLists().State_List(factory, Core.Constants.CountryCodes.UnitedStates); });
		}

		public static CodeDescriptionPairList GetCachedUSStateAndDistrictList(this BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("USStateListFor5106",
				delegate
				{
					var result = new OrgCodeLists().State_List(factory, Core.Constants.CountryCodes.UnitedStates);

					result.RemoveCode("UM");

					return result;
				});
		}

		public static CodeDescriptionPairList GetCachedUSStateListForVehicles(this BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("USStatesForVehiclesList",
				delegate
				{
					var statesForVehiclesList = new CodeDescriptionPairList(factory.GetCachedUSStateList());

					statesForVehiclesList.AddPair("US", "Use for diplomatic vehicle");
					statesForVehiclesList.Sort();

					return statesForVehiclesList;
				}
			);
		}
	}
}
