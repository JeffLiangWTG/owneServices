using Enterprise.Customs.US.AMS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.GUI.Testing
{
	[TestedType(typeof(ImportFromSailingForm))]
	class ImportFromSailingFormTest : ZFormBasherTest
	{
		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			return new ImportFromSailingForm(new BillImportActionCollection(Factory.New<CusInBondHeader>().Bills));
		}
	}
}
