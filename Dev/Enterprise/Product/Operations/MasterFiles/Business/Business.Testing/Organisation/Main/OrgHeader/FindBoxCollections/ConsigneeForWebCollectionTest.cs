using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ConsigneeForWebCollection))]
	sealed class ConsigneeForWebCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new ConsigneeForWebCollection(Factory, orgDefaults);
		}

		public void TestSetFilterBusinessObjectDefaults()
		{
			Assert(ConsigneesForWeb.FilterBusinessObjectDefaults.ContainsDefaultFor("OH_IsConsignee"));
		}

		#region Implementation

		ConsigneeForWebCollection ConsigneesForWeb;

		protected override void SetUp()
		{
			base.SetUp();
			ConsigneesForWeb = new ConsigneeForWebCollection(Factory);
		}

		#endregion
	}
}
