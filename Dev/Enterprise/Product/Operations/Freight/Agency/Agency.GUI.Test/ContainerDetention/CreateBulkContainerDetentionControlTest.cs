using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class CreateBulkContainerDetentionControlTest : TestCaseWithFactory
	{
		public void TestDoubleClick()
		{
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			ContainerMovement movement = stock.Movements.AddNew();
			ContainerDetention detention = Factory.New<ContainerDetention>();
			detention.NC_OH_Client = client.PK;
			detention.NC_OH_Principal = principal.PK;
			Factory.Save();
			BulkDetentionHeader header = new BulkDetentionHeader(Factory);
			BulkDetentionChild child = header.Children.AddNew();
			child.DetentionPK = detention.PK;
			using (ZForm form = new ZForm(header))
			{
				CreateBulkContainerDetentionControl control = new CreateBulkContainerDetentionControl();
				control.Dock = DockStyle.Fill;
				form.Size = new Size(600, 600);
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				ZGrid childGrid = (ZGrid)typeof(CreateBulkContainerDetentionControl).InvokeMember("childGrid", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic, null, control, Array.Empty<object>());
				DoubleClick(childGrid, 0, 1);
				Application.DoEvents();
				AssertNotNull(control.LastUsedControllerForTest);
				AssertNotNull(control.LastUsedControllerForTest.LastShownForm);
				using (control.LastUsedControllerForTest.LastShownForm)
				{
					AssertEquals(detention.PK, ((BusinessObject)((ZForm)control.LastUsedControllerForTest.LastShownForm).BusinessEntity).PK);
				}
			}
		}

		public void TestCountryFilter_Visible()
		{
			AgencyRegistry.Instance.AllowInterCountryDetentionJobCreation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BulkDetentionHeader header = new BulkDetentionHeader(Factory);
			using (ZForm form = new ZForm(header))
			{
				CreateBulkContainerDetentionControl control = new CreateBulkContainerDetentionControl();
				control.Dock = DockStyle.Fill;
				form.Size = new Size(600, 600);
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				ZCodeFindBox countryCode = (ZCodeFindBox)typeof(CreateBulkContainerDetentionControl).InvokeMember("countryCode", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic, null, control, Array.Empty<object>());
				AssertEquals("should be visible", true, countryCode.Visible);
			}
		}

		public void TestCountryFilter_NotVisible()
		{
			AgencyRegistry.Instance.AllowInterCountryDetentionJobCreation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BulkDetentionHeader header = new BulkDetentionHeader(Factory);
			using (ZForm form = new ZForm(header))
			{
				CreateBulkContainerDetentionControl control = new CreateBulkContainerDetentionControl();
				control.Dock = DockStyle.Fill;
				form.Size = new Size(600, 600);
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				ZCodeFindBox countryCode = (ZCodeFindBox)typeof(CreateBulkContainerDetentionControl).InvokeMember("countryCode", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic, null, control, Array.Empty<object>());
				AssertEquals("should be visible", false, countryCode.Visible);
			}
		}

		#region Implementation
		static void DoubleClick(ZGrid grid, int row, int column)
		{
			Rectangle rectangle = grid.GetCellBounds(row, column);
			var args = new MouseEventArgs(MouseButtons.Left, 2, (rectangle.Left + rectangle.Right) >> 1, (rectangle.Top + rectangle.Bottom) >> 1, 0);
			grid.PerformMouseDownForTest(args, row: row);
		}

		#endregion
	}
}
