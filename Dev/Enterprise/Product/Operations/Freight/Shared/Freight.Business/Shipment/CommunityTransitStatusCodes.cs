using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.Common.EU;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public class CommunityTransitStatusCodes : ICommunityTransitStatusCodes
	{
		public CommunityTransitStatusCodes(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		ICodeDescriptionPairList ICommunityTransitStatusCodes.GetList() => GetCommunityTransitStatusCodes();

		public CodeDescriptionPairList GetList() => GetCommunityTransitStatusCodes();

		CodeDescriptionPairList GetCommunityTransitStatusCodes()
		{
			var result = new UntranslatableCodeDescriptionPairList((NoResString)"Country-Specific Customs values");
			result.AddRangeOverwriteIfExists(GetImportCommunityTransitStatusIDList());

			if (!GlbCompany.CurrentCompany.Country.Code.ToString().Equals(Core.Constants.CountryCodes.Switzerland) &&
				!GlbCompany.CurrentCompany.Country.Code.ToString().Equals(Core.Constants.CountryCodes.UnitedKingdom) &&
				Factory.CommonTransitButNotEUCountries().Contains(GlbCompany.CurrentCompany.Country.Code.ToString()))
			{
				result.AddRangeOverwriteIfExists(GetExportCommonTransitStatusIDList());
			}
			else
			{
				result.AddRangeOverwriteIfExists(GetExportCommunityTransitStatusIDList());
			}

			result.Sort();
			return result;
		}

		protected virtual CodeDescriptionPairList GetExportCommonTransitStatusIDList()
		{
			return Factory.GetCachedValue("GetExportCommonTransitStatusIDList", () =>
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddRange(GetExportCommunityTransitStatusIDList());
				result.RemoveCode(ExportCommunityTransitStatusList.Codes.C);
				return result;
			});
		}

		protected virtual CodeDescriptionPairList GetExportCommunityTransitStatusIDList() => Factory.GetCachedValue<ExportCommunityTransitStatusList>();

		protected virtual CodeDescriptionPairList GetImportCommunityTransitStatusIDList() => Factory.GetCachedValue<ImportCommunityTransitStatusList>();

		protected BusinessObjectFactory Factory { get; }
	}
}
