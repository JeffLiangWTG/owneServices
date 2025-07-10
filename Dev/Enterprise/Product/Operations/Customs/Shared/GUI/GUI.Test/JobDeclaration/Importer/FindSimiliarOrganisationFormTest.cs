using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(FindSimilarOrganisationForm))]
	sealed class FindSimilarOrganisationFormTest : ZFormBasherTest
	{
		public void InitialiseForm()
		{
			var factory = new BusinessObjectFactory();
			var finder = new OrganisationFinder(new UnknownOrganisationCodeEventArgs(), factory);
			using (var form = new FindSimilarOrganisationForm(finder))
			{
				form.Show();
				AssertEquals(0, finder.UnknownOrg.SimilarOrgMatches.Count);
				AssertEquals(true, form.NoRecordsLabel.Visible);
				form.Close();
			}

			var arg = new UnknownOrganisationCodeEventArgs();
			arg.Code = "000";
			arg.Country = "AU";
			arg.City = "Brisbane";
			arg.Name = "Top 1";
			arg.Street = "Street 1";
			finder = new OrganisationFinder(arg, factory);
			finder.UnknownOrg.SimilarOrgMatches.AddNew();
			using (var form = new FindSimilarOrganisationForm(finder))
			{
				form.Show();
				AssertEquals(1, finder.UnknownOrg.SimilarOrgMatches.Count);
				AssertEquals(false, form.NoRecordsLabel.Visible);
				form.Close();
			}
		}

		public void TestCaptions()
		{
			var factory = new BusinessObjectFactory();
			var finder = new OrganisationFinder(new UnknownOrganisationCodeEventArgs(), factory);
			using (var form = new FindSimilarOrganisationForm(finder))
			{
				AssertEquals("Caption should be", "Select Organization", form.FormHeading);
			}
		}

		public void TestSkipSupplier()
		{
			var factory = new BusinessObjectFactory();
			var arg = new UnknownOrganisationCodeEventArgs();
			arg.Code = "000";
			arg.Country = "AU";
			arg.City = "Brisbane";
			arg.Name = "Top 1";
			arg.Street = "Street 1";
			var finder = new OrganisationFinder(arg, factory);
			finder.UnknownOrg.SimilarOrgMatches.AddNew();
			using (var form = new FindSimilarOrganisationForm(finder))
			{
				form.Show();
				form.SkipAllButton.Focus();
				form.SkipAllButton.PerformClick();
				AssertEquals(ZString.Empty, form.OrganisationCode);
				AssertEquals(ZBool.True, form.SkipAll);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var factory = new BusinessObjectFactory();
			var finder = new OrganisationFinder(new UnknownOrganisationCodeEventArgs(), factory);
			return new FindSimilarOrganisationForm(finder);
		}
	}
}
