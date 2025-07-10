using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class UpdateRateValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateClientPK()
		{
			var collection = new UpdateRateCollection(Factory);
			var updateRate = collection.AddNew();

			updateRate.ClientPK = ZGuid.Invalid;
			AssertHasError(updateRate.ClientPKInfo, "Enter a valid Code.");

			updateRate.ClientPK = ZGuid.NewZGuid();
			AssertHasError(updateRate.ClientPKInfo, "Enter a valid Code.");

			var org1 = Helper.NewOrgHeader();
			updateRate.ClientPK = org1.PK;
			AssertHasError(updateRate.ClientPKInfo, "This client has no client rate in the system. You cannot send an update to them.");

			Helper.NewClientRate(org1);
			updateRate.ClientPK = org1.PK;
			AssertNoErrors(updateRate.ClientPKInfo);

			var updateRate2 = collection.AddNew();
			updateRate2.ClientPK = org1.PK;
			AssertHasError(updateRate2.ClientPKInfo, "You cannot have the same organization listed more than once.");

			var org2 = Helper.NewOrgHeader();
			Helper.NewClientRate(org2);
			updateRate2.ClientPK = org2.PK;
			AssertNoErrors(updateRate2.ClientPKInfo);
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

		#endregion
	}
}
