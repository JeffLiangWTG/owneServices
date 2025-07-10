using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ProductPGATestForm<ExportProductAMSEPAUserControl>))]
	sealed class ExportProductAMSEPAUserControlTest : ZProductPGAFormBasherAbstractTest<ExportProductAMSEPAUserControl>
	{
		protected override string BindMember => "PivotsForBinding";

		protected override BusinessObject GetPGABusinessObject(CusClassPartPivot pivot)
		{
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			return pivot;
		}
	}
}
