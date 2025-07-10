using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public abstract class CIN750NotificationBuilderTest<TBuilder, TSource, TDocDataObject> : TestCaseWithFactory where TBuilder : CIN750NotificationBuilder<TSource, TDocDataObject> where TSource : EnterpriseBusinessObject, IConsignment where TDocDataObject : CIN750Notification
	{
		public void TestGeneralValidations()
		{
			var notification = GetGeneralNotification();

			AssertNoErrors(notification.RefCodeInfo);
			AssertNoErrors(notification.MovementTimeInfo);
			AssertNoErrors(notification.DeclaredInWarehouseCINInfo);
			notification.RefCode = ZString.Empty;
			AssertHasMessageError(notification.RefCodeInfo, "Ref Code is required.");

			notification.MovementTime = ZDateTime.Empty;
			AssertHasMessageError(notification.MovementTimeInfo, "Movement Time is required.");

			notification.DeclaredInWarehouseCIN = ZString.Empty;
			AssertHasMessageError(notification.DeclaredInWarehouseCINInfo, "Declared In Warehouse CIN is required.");
		}

		public void TestRemoveHypen()
		{
			TestRemoveHypenCore();
		}

		protected abstract void TestRemoveHypenCore();

		#region Implementation

		protected WhsItemDispatchConsignment CreateGeneralDCN()
		{
			var dcn = Helper.CreateDispatchConsignment("DCN1", TestWarehouse.PK);

			return dcn;
		}

		protected WhsWarehouse TestWarehouse
		{
			get
			{
				if (warehouse == null)
				{
					warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
					warehouse.WarehouseAddress.Address1 = "WH1Address";
				}

				return warehouse;
			}
		}

		WhsWarehouse warehouse;

		protected WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		#endregion

		protected abstract TDocDataObject GetGeneralNotification();
	}
}
