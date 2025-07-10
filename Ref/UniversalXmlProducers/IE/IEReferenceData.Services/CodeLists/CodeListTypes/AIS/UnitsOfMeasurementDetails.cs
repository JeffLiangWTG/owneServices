using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS
{
	public class UnitsOfMeasurementDetails : RevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf, IHaveAsyncAdditionalFilter
	{
		public UnitsOfMeasurementDetails()
		{
			GetEuList = Task.Run(GetEuListTask);
		}
		Task<List<RefCusCodeList>> GetEuList;

		public ApplicationType ApplicationType => ApplicationType.AIS;

		public string Code => Constants.CommonCodeTypes.UnitsOfMeasurement;

		public string NameInFile => "CL349 - Measurement Unit";

		public string TableTitleInFile => "Code Name / description ";

		public string CodeFormattingRegularExpression => "123|X|[A-Z]{3,4}";

		protected virtual Task<List<RefCusCodeList>> GetEuListTask() => RefDataLoader.GetRefDataAsync<RefCusCodeList>(
			RefDataLoader.RefDataActionNames.RefCusCodeListUpdate,
			(nameof(RefCusCodeList.ZZD_ZZZ_NKDataGrouping), RefDataLoader.RefDataComparisonOperator.Equal, "EUN"),
			(nameof(RefCusCodeList.ZZD_ZZK_NKCodeType), RefDataLoader.RefDataComparisonOperator.Equal, Constants.CommonCodeTypes.UnitsOfMeasurement)
		);

		HashSet<string> euListCache;
		async Task<HashSet<string>> EuList() => euListCache ?? (euListCache = (await GetEuList).Select(r => r.ZZD_Code).Distinct().ToHashSet());

		public async Task<bool> Filter(IRevenueCodeDescriptionPair codeDescriptionPair) => !(await EuList()).Contains(codeDescriptionPair.Code);
	}
}
