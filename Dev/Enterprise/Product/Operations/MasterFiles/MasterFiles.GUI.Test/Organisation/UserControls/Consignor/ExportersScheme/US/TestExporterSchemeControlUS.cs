using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class TestExporterSchemeControlUS : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestControlIsShown()
		{
			using (ZForm testForm = new ZForm())
			{
				using (ExporterSchemeControlUS testControl = new ExporterSchemeControlUS())
				{
					testForm.Controls.Add(testControl);
					testForm.Show();
				}
			}
		}
	}
}
