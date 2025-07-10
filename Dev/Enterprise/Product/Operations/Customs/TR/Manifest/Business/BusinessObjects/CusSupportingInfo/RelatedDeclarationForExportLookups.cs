using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class RelatedDeclarationForExportLookups : CusSupportingInfoLookups
	{
		public RelatedDeclarationForExportLookups(AutoCusSupportingInfo parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList ProcedureList => new ProcedureList();

		public override CodeDescriptionPairList SubTypeList => new SubTypeList();
	}
}
