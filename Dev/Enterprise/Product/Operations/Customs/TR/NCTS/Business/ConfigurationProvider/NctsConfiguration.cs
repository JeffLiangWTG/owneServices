using CargoWise.Types;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public sealed class NctsConfiguration : EU.NCTS.Business.NctsConfiguration
	{
		protected override ZBool UseAdditionalDeclarationTypeCore => true;
	}
}
