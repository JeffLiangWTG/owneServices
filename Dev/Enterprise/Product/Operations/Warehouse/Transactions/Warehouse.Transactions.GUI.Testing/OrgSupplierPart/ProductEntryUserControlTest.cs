using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class ProductEntryUserControlTest : WhsGuiTestCaseWithFactory
	{
		public void TestUserControl()
		{
			using (var control = new ProductEntryUserControl())
			{
				AssertEquals("ProductEntryUserControl", control.Name);
			}
		}

		public void TestImportPickFacesLinesBasedOnClientCode()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var osp = Factory.New<OrgSupplierPart>();

			using (var form = new OrgSupplierPartForm(osp))
			{
				form.Show();
				Application.DoEvents();

				var bottomTabControl = (ZTabControl)form.Find(c => c.Name == "BottomTabControl").First();

				var relatedOrgsTabPage = (ZTabPage)form.Find(c => c.Name == "RelatedOrgsTabPage").First();
				bottomTabControl.SelectedTab = relatedOrgsTabPage;
				Application.DoEvents();
				var gridPartRelations = (ZGrid)form.Find(c => c.Name == "GridPartRelations").First();
				var oh = Factory.New<OrgHeader>();
				oh.OH_Code = "OOOOOO";
				oh.OH_FullName = "OH_COMPANYNAME";
				var ww = Factory.New<WhsWarehouse>();
				ww.WW_WarehouseCode = "WWW";
				ww.WW_WarehouseName = "WW_WAREHOUSENAME";

				var cs = (ZGuidFindBoxColumnStyle)gridPartRelations.Columns[0].ColumnStyle;
				gridPartRelations.List.AddNew();
				gridPartRelations[0, 0] = oh.PK;
				Application.DoEvents();

				var warehouseTabPage = (ZTabPage)form.Find(c => c.Name == "WarehouseTabPage").First();
				bottomTabControl.SelectedTab = warehouseTabPage;
				Application.DoEvents();

				var zgrid1 = (ZGrid)form.Find(c => c.Name == "zGrid1").First();
				zgrid1.List.AddNew();
				zgrid1[0, 0] = oh.PK;
				Application.DoEvents();

				// test for import columns "Client" and "Warehouse", mapping as "Code"
				var cm = zgrid1.ContextMenu;
				var menuItem = zgrid1.ContextMenu.MenuItems.FindByText("&Import Data...");
				menuItem.PerformClick();
				Application.DoEvents();

				var forms = Application.OpenForms;
				var iwForm = (ZForm)forms.Cast<Form>().Single(f => f.Text == "Data Import Wizard");
				var wizard = (ImportWizard)iwForm.BusinessEntity;
				string filename = EnvProxy.Instance.GetTempFileName();
				using (var fs = File.Create(filename))
				{
					using (var sw = new StreamWriter(fs))
					{
						sw.WriteLine("OOOOOO,WWW,C3,C4,C5,C6");
					}
				}
				wizard.FileName = filename;
				Application.DoEvents();
				var btnNext = (ZButton)iwForm.Find(c => c.Name == "NextButton").First();
				btnNext.PerformClick();
				Application.DoEvents();

				var toGrid = (ZGrid)iwForm.Find(c => c.Name == "ToGrid").First();
				var mapping = wizard.Mapping[0];
				mapping.AddFileColumnIndex(0);
				mapping = wizard.Mapping[1];
				mapping.AddFileColumnIndex(1);
				mapping.RefreshBinding();
				Application.DoEvents();

				btnNext.PerformClick();
				Application.DoEvents();
				var previewGrid = (ZGrid)iwForm.Find(c => c.Name == "PreviewGrid").First();
				var companyGuid = (ZGuid)previewGrid[0, 0];
				AssertEquals("the guid value in the target cell must be valid and be equal to the org's pk", companyGuid, oh.PK);

				var btnCancel = (ZButton)iwForm.Find(c => c.Name == "CancelButtonX").First();
				btnCancel.PerformClick();
				Application.DoEvents();

				// test for import columns "Client" and "Warehouse", mapping as "Description"
				menuItem.PerformClick();
				Application.DoEvents();
				forms = Application.OpenForms;
				iwForm = (ZForm)forms.Cast<Form>().Single(f => f.Text == "Data Import Wizard");
				wizard = (ImportWizard)iwForm.BusinessEntity;
				var filename2 = EnvProxy.Instance.GetTempFileName();
				using (var fs = File.Create(filename2))
				{
					using (var sw = new StreamWriter(fs))
					{
						sw.WriteLine("OH_COMPANYNAME,WW_WAREHOUSENAME,C3,C4,C5,C6");
					}
				}
				wizard.FileName = filename2;
				Application.DoEvents();
				btnNext = (ZButton)iwForm.Find(c => c.Name == "NextButton").First();
				btnNext.PerformClick();
				Application.DoEvents();

				toGrid = (ZGrid)iwForm.Find(c => c.Name == "ToGrid").First();
				mapping = wizard.Mapping[0];
				mapping.AddFileColumnIndex(0);
				mapping.MapAs = "Description";
				mapping = wizard.Mapping[1];
				mapping.AddFileColumnIndex(1);
				mapping.MapAs = "Description";
				mapping.RefreshBinding();
				Application.DoEvents();

				btnNext.PerformClick();
				Application.DoEvents();
				previewGrid = (ZGrid)iwForm.Find(c => c.Name == "PreviewGrid").First();
				companyGuid = (ZGuid)previewGrid[0, 0];
				AssertEquals("the guid value in the target cell must be valid and be equal to the org's pk", companyGuid, oh.PK);

				btnCancel = (ZButton)iwForm.Find(c => c.Name == "CancelButtonX").First();
				btnCancel.PerformClick();
				Application.DoEvents();

				File.Delete(filename);
				File.Delete(filename2);
			}
		}

		public void TestSetupPutawayRules()
		{
			using (var form = new ZForm())
			using (var productEntryUserControl = new ProductEntryUserControl())
			{
				form.Controls.Add(productEntryUserControl);
				form.Show();

				var label = productEntryUserControl.FindSingle<ZLinkLabel>("ProductionRulesEngineLinkLabel");
				AssertNotNull("Link Label should exist.", label);
				Assert("Link Label should be visible", label.Visible);

				label.OnLinkClicked_Exposed(new LinkLabelLinkClickedEventArgs(null));
				Assert("Notification type is correct.", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Notification message is correct.", @"This Production Rules Engine portal cannot be opened in a browser as GLOW has not been configured for this client.
Registry: GLOW/Services/GLOW Portals Root URL", UnitTestUserNotification.Instance.LastMessage.Text);

				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

				var staff = Helper.CreateGlbStaff("ABC", "ABC");
				Factory.Save();

				using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					label.OnLinkClicked_Exposed(new LinkLabelLinkClickedEventArgs(null));
					var launchedUrl = WebUrlLauncher.LastUrlLaunched;
					var uri = new Uri(launchedUrl, UriKind.Absolute);

					AssertEquals("Launched uri is correct.", "https", uri.Scheme);
					AssertEquals("Launched uri is correct.", "address", uri.Host);
					AssertEquals("Launched uri is correct.", "/goto/ProductWarehousePutaway", uri.AbsolutePath);
				}
			}
		}

		public void TestReplenishmentMultipleColumnsAreVisible()
		{
			var staff = Helper.CreateGlbStaff("ABC", "ABC");
			Factory.Save();
			AssertEquals("Precondition.", true, Env.CurrentUser.IsSupportUser);
			TestReplenishmentMultipleFieldIsVisible();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertEquals("Precondition.", false, Env.CurrentUser.IsSupportUser);
				TestReplenishmentMultipleFieldIsVisible();
			}
		}

		void TestReplenishmentMultipleFieldIsVisible()
		{
			using (var control = new ProductEntryUserControl())
			{
				var replenishmentMultipleCalEdit = control.Controls.Find("zCalcEdit4", true);
				var zGrid = control.Controls.Find("zGrid2", true);
				AssertNotNull(replenishmentMultipleCalEdit);
				AssertEquals(1, replenishmentMultipleCalEdit.Length);
				AssertEquals("Replenishment Multiple ZCalEdit should not be hidden", true, replenishmentMultipleCalEdit[0].Visible);

				AssertNotNull(zGrid);
				AssertEquals(1, zGrid.Length);

				var replenishmentMultipleColumn = ((ZGrid)zGrid[0]).ColumnStyles.OfType<ZGridColumnInfo>().FirstOrDefault(col => col.ColumnName == "W3_ReplenishmentMultiple");
				AssertNotNull(replenishmentMultipleColumn);
				AssertEquals("Replenishment Multiple Column should not be hidden", true, replenishmentMultipleColumn.IsVisible);
			}
		}

		public void TestPutawayGroupColumnIsVisible()
		{
			using (var control = new ProductEntryUserControl())
			{
				var zGrid = control.Controls.Find("zGrid2", true);
				AssertNotNull("Precondition", zGrid);

				var putawayGroupColumn = ((ZGrid)zGrid[0]).ColumnStyles.OfType<ZGridColumnInfo>().FirstOrDefault(col => col.ColumnName == "W3_WPG_PutawayGroup");
				AssertNotNull("There is a putaway group column.", putawayGroupColumn);
				AssertEquals("PutawayGroup Column should not be hidden.", true, putawayGroupColumn.IsVisible);
			}
		}

		public void TestInwardsProcessingStagingLocationBOMVisibility_InwardProcessingSupported()
		{
			TestInwardsProcessingStagingLocationBOMVisibility(supportsInwardProcessing: true);
		}

		public void TestInwardsProcessingStagingLocationBOMVisibility_InwardProcessingNotSupported()
		{
			TestInwardsProcessingStagingLocationBOMVisibility(supportsInwardProcessing: false);
		}

		void TestInwardsProcessingStagingLocationBOMVisibility(bool supportsInwardProcessing)
		{
			var mock = new Mock<Enterprise.Integration.Customs.ISupportedForProcessing>();
			mock.Setup(m => m.IsSupportedForProcessing()).Returns(supportsInwardProcessing);

			using (ObjectFactory.Substitute(mock.Object))
			{
				using (var control = new ProductEntryUserControl())
				{
					var zGrid = control.Controls.Find("zGrid2", true);
					AssertNotNull("Precondition", zGrid);

					var ipLocationColumn = ((ZGrid)zGrid[0]).ColumnStyles.OfType<ZGridColumnInfo>().FirstOrDefault(col => col.ColumnName == nameof(WhsProductParamsByWhsAndClient.W3_WL_InwardsProcessingStagingLocationBOM));
					AssertNotNull("There is an inwards processing location column.", ipLocationColumn);
					AssertEquals("PutawayGroup Column availability.", !supportsInwardProcessing, ipLocationColumn.IsUnavailable);
				}
			}
		}

		#region TestLocationAutoComplete

		public void TestLocationAutoCompleteDisabled()
		{
			using (var control = new ProductEntryUserControl())
			{
				var grid = (ZGrid)control.Controls.Find("zGrid1", true)[0];
				var style = (ZCodeFindBoxColumnStyleInfo)grid.GetColumnStyle("LocationString");
				Assert("Location style should have auto complete disabled", style.AutoCompleteDisabled);
			}
		}

		#endregion

		#region TestProductEntryUserControlIsINotifications

		public void TestProductEntryUserControlIsINotifications()
		{
			using (var productEntryUserControl = new ProductEntryUserControl())
			{
				Assert("ProductEntryUserControl implements INotifications.", productEntryUserControl is INotifications);
			}
		}

		public void TestProductEntryUserControlIsINotifications_Error()
		{
			using (var productEntryUserControl = new ProductEntryUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var notify = (INotifications)productEntryUserControl;
				notify.AddError("This is error.");

				Assert("Notification type is correct.", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Notification message is correct.", "This is error.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestProductEntryUserControlIsINotifications_Info()
		{
			using (var productEntryUserControl = new ProductEntryUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var notify = (INotifications)productEntryUserControl;
				notify.AddInformation("This is info.");

				Assert("Notification type is correct.", UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("Notification message is correct.", "This is info.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestProductEntryUserControlIsINotifications_Warning()
		{
			using (var productEntryUserControl = new ProductEntryUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var notify = (INotifications)productEntryUserControl;
				notify.AddWarning("This is warning.");

				Assert("Notification type is correct.", UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals("Notification message is correct.", "This is warning.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion
	}
}
