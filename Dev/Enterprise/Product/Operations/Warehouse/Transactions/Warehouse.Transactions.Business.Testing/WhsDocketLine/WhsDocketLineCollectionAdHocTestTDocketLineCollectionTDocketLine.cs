using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsDocketLineCollectionAdHocTest<TDocketLineCollection, TDocketLine> : ActiveBusinessObjectCollectionTestCase<TDocketLineCollection>
			where TDocketLineCollection : WhsDocketLineCollectionAdHoc<TDocketLine>
			where TDocketLine : WhsDocketLine
	{
		#region TestAllowNew

		public void TestAllowNew()
		{
			AssertEquals(false, ((IBindingList)GetCollectionToTest()).AllowNew);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Docket.Lines.AddNew();
		}

		#region Docket

		WhsDocket Docket
		{
			get { return docket ?? (docket = GetNewDocket()); }
		}

		WhsDocket docket;

		protected abstract WhsDocket GetNewDocket();

		#endregion

		#endregion
	}
}
