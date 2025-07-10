using System.Windows.Forms;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(AdditionalTranCircumstancesForm))]
class AdditionalTranCircumstancesFormTest : ZFormBasherTest
{
	protected override Form GetFormToBashCore()
	{
		var dec = Factory.New<JobDeclaration>();
		var header = dec.Invoices.AddNew();
		var collection = new TranCircumstanceCollection(header);

		return new AdditionalTranCircumstancesForm(collection);
	}
}
