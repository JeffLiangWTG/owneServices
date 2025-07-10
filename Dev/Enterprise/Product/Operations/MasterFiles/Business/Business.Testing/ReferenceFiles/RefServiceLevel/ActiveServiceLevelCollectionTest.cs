using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ActiveServiceLevelCollection))]
	sealed class ActiveServiceLevelCollectionTest : ActiveBusinessObjectCollectionTestCase<ActiveServiceLevelCollection>
	{
		#region TestDefaultFilters

		public void TestDefaultFilters()
		{
			//string RS_ShowInactive = "RS_ShowInactive";
			string rS_ShowInactive = "Active Status" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property";

			ActiveServiceLevelCollection collection = new ActiveServiceLevelCollection(Factory);

			AssertEquals("should specify a default for RS_ShowInactive", true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor(rS_ShowInactive));
			//AssertEquals("default for RS_ShowInactive should be false", false, Collection.FilterBusinessObjectDefaults[RS_ShowInactive].Value);
			AssertEquals("default for RS_ShowInactive should be false", "Active", collection.FilterBusinessObjectDefaults[rS_ShowInactive].Value);
		}

		#endregion

		#region TestAdditionalFilter

		public void TestAdditionalFilter()
		{
			RefServiceLevel activeServiceLevel = Factory.New<RefServiceLevel>();
			activeServiceLevel.RS_IsActive = true;

			RefServiceLevel inactiveServiceLevel = Factory.New<RefServiceLevel>();
			inactiveServiceLevel.RS_IsActive = false;

			ActiveServiceLevelCollection collection = new ActiveServiceLevelCollection(Factory);

			AssertEquals("Should contain active service levels", true, collection.Contains(activeServiceLevel));
			AssertEquals("Should not contain inactive service levels", false, collection.Contains(inactiveServiceLevel));

			string error = "This service level is inactive";

			AssertNotContains("Should not add the error for active shipments", error, ((IActiveBusinessObjectCollection)collection).GetAllNotificationsWhenAdditionalFilterNotMet(activeServiceLevel));
			AssertContains("Should add the error for inactive shipments", error, ((IActiveBusinessObjectCollection)collection).GetAllNotificationsWhenAdditionalFilterNotMet(inactiveServiceLevel));
		}

		#endregion
	}
}
