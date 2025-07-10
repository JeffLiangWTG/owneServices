using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing.Organisation.Main.OrgCompanyData
{
	public class InvoiceTermsListWithShortDescriptionTest : TestCaseWithFactory
	{
		public void TestInvoiceTermsListWithShortDescriptionWithInvoiceTermsExists()
		{
			var termsList = new InvoiceTermsListWithShortDescription();

			var terms = typeof(InvoiceTermsList).GetProperties(BindingFlags.Static | BindingFlags.Public)
				.Where(x => x.PropertyType.Equals(typeof(CodeDescriptionPair)))
				.Select(x => (CodeDescriptionPair)x.GetValue(null));

			foreach (var term in terms)
			{
				Assert($"termsList should have term {term}. ", termsList.GetMultilingualDescriptionFromCode(term.Code) != null);
			}
		}
	}
}
