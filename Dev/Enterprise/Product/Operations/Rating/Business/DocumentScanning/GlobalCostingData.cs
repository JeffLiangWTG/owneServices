using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(GlobalCostingData),
	Enterprise.Core.Constants.DocManagerCodes.GlobalCosting)]

namespace Enterprise.Rating.Business
{
	public class GlobalCostingData : CostingData
	{
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("609d4e3b-a2eb-4595-9f15-0a853d73fc93", "Global Costing"); } }
		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new RatingHeaderEDocsViaUniversalXmlSupport(this, RatingConstants.RatingHeaderTypes.Costing, true);
	}
}
