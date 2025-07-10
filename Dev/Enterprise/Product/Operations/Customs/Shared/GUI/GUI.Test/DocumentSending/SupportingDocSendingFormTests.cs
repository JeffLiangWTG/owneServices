using System.Windows.Forms;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(SupportingDocSendingForm))]
	sealed class SupportingDocSendingFormTests : MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			return new SupportingDocSendingForm(new JobDeclarationSupportingDocSendingObjectParent(declaration));
		}
	}
}
