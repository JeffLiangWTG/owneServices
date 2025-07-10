using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class RefContainerMapProvider : IRefContainerMapProvider
	{
		public UsageRequirement IsUsageNeeded => UsageRequirement.MayRequire;

		public ICodeDescriptionPairList GetUsageList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Enterprise.Customs.US.Business.RefContainerMapProvider.UsageList", () =>
			{
				return new USContainerUsageList();
			});
		}

		public ICodeDescriptionPairList GetCodeList(BusinessObjectFactory factory, ZString usage)
		{
			return factory.GetCachedValue($"Enterprise.Customs.US.Business.RefContainerMapProvider.{(usage == USContainerUsageList.Codes.AMS ? USContainerUsageList.Codes.AMS : ZString.Empty)}CodeList", () =>
			{
				var result = new UntranslatableCodeDescriptionPairList("US specific descriptions"); // Untranslatable reason
				result.AddRange(ObjectFactory.New<Integration.Customs.US.eManifest.IEquipmentTypesProvider>().GetEquipmentTypes());
				result.AddRangeOverwriteIfExists(new USContainerCodeList());
				if (usage != USContainerUsageList.Codes.AMS)
				{
					result.RemoveCode(USContainerCodeList.Codes._25T2);
					result.RemoveCode(USContainerCodeList.Codes._22T0);
					result.RemoveCode(USContainerCodeList.Codes._22T5);
					result.RemoveCode(USContainerCodeList.Codes._22T7);
					result.RemoveCode(USContainerCodeList.Codes._22T8);
					result.RemoveCode(USContainerCodeList.Codes._42T0);
				}
				result.SortByDescription();
				return result;
			});
		}

		public ZString GetDefaultCustomsCode(BusinessObjectFactory factory, ZString containerType) => ZString.Empty;
	}
}
