using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(GlobalClientRateData),
	Enterprise.Core.Constants.DocManagerCodes.GlobalClientRate)]

namespace Enterprise.Rating.Business
{
	public class GlobalClientRateData : ClientRateData
	{
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("2da43503-1b90-4c5f-af8a-a3677be7dd69", "Global Client Rate"); } }
		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new RatingHeaderEDocsViaUniversalXmlSupport(this, RatingConstants.RatingHeaderTypes.ClientRate, true);
	}
}
