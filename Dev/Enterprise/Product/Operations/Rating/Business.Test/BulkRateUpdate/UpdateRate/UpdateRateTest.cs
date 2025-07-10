using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(UpdateRate))]
	internal sealed class UpdateRateTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var org = Helper.NewOrgHeader();
			var updateRate = new UpdateRate(Factory);

			updateRate.ClientPK = org.PK;
			AssertEquals("TESTORG1", updateRate.Client.OH_Code);
			AssertEquals("Test Client #1", updateRate.FullName);
			AssertEquals(true, updateRate.IncludeInUpdate);

			org.CompanyData.OB_ARAutoUpdateRates = false;
			updateRate.ClientPK = org.PK;
			AssertEquals("Do not overwrite if org not changed", true, updateRate.IncludeInUpdate);

			updateRate.ClientPK = ZGuid.Empty;
			updateRate.ClientPK = org.PK;
			AssertEquals("Overwrite if org *has* changed", false, updateRate.IncludeInUpdate);
		}

		public void TestGetClientRate()
		{
			var org1 = Helper.NewOrgHeader();
			var org2 = Helper.NewOrgHeader();

			var rate1 = Helper.NewClientRate(org1);

			var updateRate = new UpdateRate(Factory);
			AssertNull(updateRate.ClientRate);

			updateRate.ClientPK = org1.PK;
			AssertEquals(rate1, updateRate.ClientRate);

			updateRate.ClientPK = org2.PK;
			AssertNull(updateRate.ClientRate);
		}

		#region Implementation

		TestHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new TestHelper(Factory);
				}

				return fHelper;
			}
		}

		TestHelper fHelper;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UpdateRate(Factory);
		}

		#endregion
	}
}
