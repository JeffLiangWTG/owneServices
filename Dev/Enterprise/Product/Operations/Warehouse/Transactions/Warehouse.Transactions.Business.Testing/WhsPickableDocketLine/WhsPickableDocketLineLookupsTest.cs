using System;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsPickableDocketLineLookupsTest<TPickableDocket, TPickableDocketLine> : WhsDocketLineLookupsTest<TPickableDocket, TPickableDocketLine>
		where TPickableDocket : WhsPickableDocket
		where TPickableDocketLine : WhsPickableDocketLine
	{
		#region TestPickGroups

		public void TestPickGroups()
		{
			var collection = new PickGroupCollection();
			var pickGroup = collection.AddNew();
			pickGroup.Description = (NoResString)"Desc";

			using (WarehouseDataRegistry.Instance.PickGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var pickableDocketLine = GetNewDocketLine();
				AssertEquals(typeof(PickGroupCollection), pickableDocketLine.Lookups.PickGroups.GetType());
				AssertEquals(1, pickableDocketLine.Lookups.PickGroups.Count);

				var pickGroupFromLookups = (PickGroup)pickableDocketLine.Lookups.PickGroups[0];
				AssertEquals(new ZShort(1), pickGroupFromLookups.PickSequence);
				AssertEquals("Desc", pickGroupFromLookups.Description);
			}
		}

		#endregion
	}
}
