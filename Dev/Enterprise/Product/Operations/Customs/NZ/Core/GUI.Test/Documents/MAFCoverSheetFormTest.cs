using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet;
using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Documents.Testing
{
	[TestedType(typeof(MAFCoverSheetForm))]
	public class MAFCoverSheetFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var coverSheetBO = new NZDocsMAFCoverSheet(TestDataBuilder.GetMAFMessaging(declaration));
			coverSheetBO.D0_AccountHolder = "LALALA";
			var commodityBO = coverSheetBO.Commodities.AddNew();
			commodityBO.D1_CommodityOrSpecies = "LOLOLO";
			var containerBO = coverSheetBO.Containers.AddNew();
			containerBO.D2_ContainerNumber = "LELELE";
			coverSheetBO.HasChanges = false;
			return new MAFCoverSheetForm(coverSheetBO);
		}

		protected override bool ShouldTestFormIsFullyTranslatable => false;
	}
}
