using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	class CommonCartageTestCase : TestCase
	{
		[UseSnapshotProtection]
		public void TestOnSavingSetJJ_ConsignmentID_NoDuplicateRef()
		{
			var factory = new BusinessObjectFactory();
			var cartage = factory.New<CommonCartage>();
			var consignmentID = ZString.Empty;
			var dbConnection = ((CargoWise.Data.IDbConnected)factory).Connection;
			try
			{
				dbConnection.BeginTransaction();
				cartage.OnSaving();
				consignmentID = cartage.JJ_ConsignmentID;
			}
			finally
			{
				dbConnection.RollbackTransaction();
			}

			var newFactory = new BusinessObjectFactory();
			var cartage2 = newFactory.New<CommonCartage>();
			newFactory.Save();
			AssertEquals("The consignmentID which cartage get first is set to cartage2", consignmentID, cartage2.JJ_ConsignmentID);
			AssertEquals("cartage.JJ_ConsignmentID is still consignmentID under the same transaction before successfully saved", consignmentID, cartage.JJ_ConsignmentID);
			factory.Save();
			AssertNotEquals("Saved with no duplicate error and cartage get another consignmentID", consignmentID, cartage.JJ_ConsignmentID);
		}
	}
}
