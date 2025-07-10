using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Rating.Module;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Rating.Testing.Module
{
	sealed class UrsNamedAccountFilterStripControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoadControl()
		{
			using (var form = new ZForm())
			{
				var filterControl = new UrsNamedAccountFilterStripControl(collection, filterBizObj);
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
			}
		}

		public void TestInitializeColumns()
		{
			using (var form = new ZForm())
			using (var filterControl = new UrsNamedAccountFilterStripControl(collection, filterBizObj))
			{
				form.Controls.Add(filterControl);
				form.Show();
				AssertNotNull(filterControl.FilteredGrid.Columns["ONA_ForeignName"]);
				AssertNotNull(filterControl.FilteredGrid.Columns["ONA_OH_Organization"]);
				AssertNotNull(filterControl.FilteredGrid.Columns["Organization+OH_FullName"]);
				AssertNotNull(filterControl.FilteredGrid.Columns["ONA_OH_Carrier"]);
				AssertNotNull(filterControl.FilteredGrid.Columns["Carrier+OH_FullName"]);
			}
		}

		OrgCarrierNamedAccountCollection collection;
		UrsNamedAccountFilterBusinessObject filterBizObj;

		protected override void SetUp()
		{
			base.SetUp();
			collection = new OrgCarrierNamedAccountCollection(Factory);
			filterBizObj = new UrsNamedAccountFilterBusinessObject();
		}
	}
}
