using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Test
{
	[TestedType(typeof(ConsolidatedDeclarationController))]
	sealed class ConsolidatedDeclarationControllerTest : Customs.Module.Testing.ConsolidatedDeclarationControllerTest
	{
		public override void TestEditForm()
		{
			using (var myForm = Controller.ShowEditForm(GetBusinessObjectThatIsInTheDatabase()) as ConsolidatedDeclarationForm)
			{
				AssertNotEquals("New form should be of type ConsolidatedDeclarationForm", null, myForm);
			}
		}

		protected override string CountryCode => "NZ";

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			Factory.Save();
			return consolidatedDeclaration;
		}
	}
}
