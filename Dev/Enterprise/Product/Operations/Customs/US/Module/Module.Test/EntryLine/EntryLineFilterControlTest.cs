using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class EntryLineFilterControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoadControl()
		{
			using (var form = new ZForm())
			{
				var filterControl = new EntryLineFilterControl(collection, filterBizObj);
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
			}
		}

		public void TestInitializeColumns()
		{
			using (var form = new ZForm())
			using (var filterControl = new EntryLineFilterControl(collection, filterBizObj))
			{
				form.Controls.Add(filterControl);
				form.Show();
				AssertNull(filterControl.FilteredGrid.Columns["Declaration+JE_DateOfFirstArrival"]);
				AssertNull(filterControl.FilteredGrid.Columns["Header+DeclarationDate"]);
				AssertNotNull(filterControl.FilteredGrid.Columns["Declaration+US_EntryDate"]);
				AssertNotNull(filterControl.FilteredGrid.Columns["Declaration+US_SchDEntry"]);
				AssertNotNull(filterControl.FilteredGrid.Columns["Declaration+US_SchDArrival"]);
				AssertNotNull(filterControl.FilteredGrid.Columns["Declaration+IOROrgPK"]);
			}
		}

		GlobalCusEntryLineCollection collection;
		EntryLineFilterBusinessObject filterBizObj;
		protected override void SetUp()
		{
			base.SetUp();
			collection = new GlobalCusEntryLineCollection(Factory);
			filterBizObj = new EntryLineFilterBusinessObject(collection);
		}
	}
}
