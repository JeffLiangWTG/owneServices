using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.DataTransfer;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public abstract class WhsDocketExportMenuItemTest<TDocket, TValueObject> : TestCaseWithFactory
			where TDocket : WhsDocket
			where TValueObject : IValueObject
	{
		#region TestOnClick

		public void TestOnClick()
		{
			bool onClickHasBeenPerformed = false;
			MenuItem.Click += delegate
			{ onClickHasBeenPerformed = true; };
			MenuItem.PerformClick();
			AssertEquals("On Click performed", true, onClickHasBeenPerformed);
		}

		#endregion

		#region TestExportDirector

		public void TestExportDirector()
		{
			AssertEquals(GetExpectedExportDirectorType(), ((WhsDocketExportMenuItem<TDocket, TValueObject>)MenuItem).ExportDirector.GetType());
		}

		#endregion

		#region Implementation

		protected virtual ZString GetExpectedNotFinalizedMessage()
		{
			return Docket.HumanReadableName + " is not FINALIZED. Export action aborted.";
		}

		protected TDocket Docket
		{
			get { return docket ?? (docket = GetNewDocket()); }
		}

		protected WhsValueObjectDataAdapter<TDocket, TValueObject> Adapter
		{
			get { return adapter ?? (adapter = GetNewAdapter()); }
		}

		protected MenuItem MenuItem
		{
			get { return menuItem ?? (menuItem = GetNewMenuItem()); }
		}

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		protected OrgHeader Client
		{
			get { return client ?? (client = Helper.CreateClient()); }
		}

		protected WhsWarehouse Warehouse
		{
			get { return warehouse ?? (warehouse = Helper.CreateWarehouse("TST", "ROW", 1, 1)); }
		}

		protected abstract Type GetExpectedExportDirectorType();
		protected abstract TDocket GetNewDocket();
		protected abstract WhsValueObjectDataAdapter<TDocket, TValueObject> GetNewAdapter();
		protected abstract MenuItem GetNewMenuItem();

		TDocket docket;
		WhsValueObjectDataAdapter<TDocket, TValueObject> adapter;
		MenuItem menuItem;
		WhsTestHelperFunctions helper;
		OrgHeader client;
		WhsWarehouse warehouse;

		#endregion
	}
}
