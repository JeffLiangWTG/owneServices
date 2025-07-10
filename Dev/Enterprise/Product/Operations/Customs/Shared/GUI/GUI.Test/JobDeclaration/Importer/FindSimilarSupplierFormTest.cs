using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.DataTransfer;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(FindSimilarSupplierForm))]
	sealed class FindSimilarSupplierFormTest : ZFormBasherTest
	{
		public void TestCaptions()
		{
			var factory = new BusinessObjectFactory();
			var finder = new OrganisationFinder(new UnknownOrganisationCodeEventArgs(), factory);
			using (var form = new FindSimilarSupplierForm(finder))
			{
				AssertEquals("Caption should be", "Select Supplier", form.FormHeading);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var factory = new BusinessObjectFactory();
			var finder = new OrganisationFinder(new UnknownOrganisationCodeEventArgs(), factory);
			return new FindSimilarSupplierForm(finder);
		}
	}
}
