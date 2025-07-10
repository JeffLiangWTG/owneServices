using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(DocumentCommonConsolForm))]
	sealed class DocumentCommonConsolFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			CommonConsol commonConsol = Factory.New<CommonConsol>();
			DocumentCommonConsol doc = new DocumentCommonConsol(commonConsol, Core.Constants.DataContext.LoadListDocument);
			return new DocumentCommonConsolForm(doc);
		}
	}
}
