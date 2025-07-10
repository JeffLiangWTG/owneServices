using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class ConsolDocumentSupporterGuiQueryProviderTest : SharedGuiQueryProviderTest
	{
		public void TestRegister()
		{
			ICommonConsolDocumentSupporterQueryProvider provider = Factory.GetValue<ICommonConsolDocumentSupporterQueryProvider>();
			AssertNull("prerequisite", provider);

			ConsolDocumentSupporterGuiQueryProvider.Register(Factory);

			provider = Factory.GetValue<ICommonConsolDocumentSupporterQueryProvider>();

			AssertNotNull(provider);
			Assert(provider is ConsolDocumentSupporterGuiQueryProvider);
		}

		public void TestGetConsolToPrint()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			DocumentCommonConsol docConsol = new DocumentCommonConsol(consol, Core.Constants.DataContext.LoadListDocument);

			ICommonConsolDocumentSupporterQueryProvider queryProvider = new ConsolDocumentSupporterGuiQueryProvider();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
			AssertNull(queryProvider.GetConsolToPrint(docConsol));

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			AssertEquals(docConsol, queryProvider.GetConsolToPrint(docConsol));
		}
	}
}
