using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(DocumentCusContainerForm))]
	sealed class DocumentCusContainerFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var doc = new DocumentCusContainerCollectionHeader(new DocumentCusContainerCollection(Factory, declaration.CusContainers));
			return new DocumentCusContainerForm(doc);
		}
	}
}
