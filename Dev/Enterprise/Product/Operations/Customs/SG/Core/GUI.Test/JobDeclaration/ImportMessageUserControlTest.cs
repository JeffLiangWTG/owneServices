using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	sealed class ImportMessageUserControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestImportMessageUserControl()
		{
			var control = new ImportMessageUserControl();
			control.Dispose();
		}
	}
}
