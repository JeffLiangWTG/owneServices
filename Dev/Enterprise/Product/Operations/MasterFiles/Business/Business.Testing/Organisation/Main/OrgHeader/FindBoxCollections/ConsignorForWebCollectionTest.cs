using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ConsignorForWebCollection))]
	sealed class ConsignorForWebCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new ConsignorForWebCollection(Factory, orgDefaults);
		}

		public void TestSetFilterBusinessObjectDefaults()
		{
			Assert(ConsignorsForWeb.FilterBusinessObjectDefaults.ContainsDefaultFor("OH_IsConsignor"));
		}

		#region Implementation

		ConsignorForWebCollection ConsignorsForWeb;

		protected override void SetUp()
		{
			base.SetUp();
			ConsignorsForWeb = new ConsignorForWebCollection(Factory);
		}

		#endregion
	}
}
