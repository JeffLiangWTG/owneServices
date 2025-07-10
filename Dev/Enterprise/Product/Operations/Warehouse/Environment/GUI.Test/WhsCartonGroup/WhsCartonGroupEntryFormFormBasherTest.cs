using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.GUI.Testing
{
	[TestedType(typeof(WhsCartonGroupEntryForm))]
	class WhsCartonGroupEntryFormFormBasherTest : ZFormBasherTest
	{
		#region TestAttachAndDetach_CartonSizes

		public void TestAttachAndDetach_CartonSizes()
		{
			var cartonGroup = Helper.CreateWhsCartonGroup("G1", "Group");
			var cartonSize = Helper.CreateWhsCartonSize("S");
			Factory.Save();

			using (var form = new WhsCartonGroupEntryForm(cartonGroup))
			{
				form.Show();

				var moduleButtonGrid = GuiTestHelper.FindControl<ZModuleButtonGrid>(form.Controls, "CartonSizeModuleButtonGrid");
				var toolStrip = GuiTestHelper.FindControl<ZToolStrip>(moduleButtonGrid.Controls, "toolStrip");
				var attachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).FirstOrDefault();
				var detachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Detach, true).FirstOrDefault();
				var editButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Edit, true).FirstOrDefault();
				AssertEquals("Grid should *not* be readonly.", false, moduleButtonGrid.InnerGrid.ReadOnly);
				AssertEquals("Grid should *not* allow adding rows.", false, moduleButtonGrid.InnerGrid.List.AllowNew);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				attachButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertNotNull(moduleButtonGrid.LastShownAttachPopupForTesting);
				AssertEquals(ModuleIDs.WhsCartonSize, moduleButtonGrid.LastShownAttachPopupForTesting.CurrentModule.ModuleID);

				editButton.PerformClick();
				AssertType<EmbeddedModulePopup>(ZFormModaliser.LastFormShownForTest);
				AssertEquals("Carton Size", ((EmbeddedModulePopup)ZFormModaliser.LastFormShownForTest).Text);

				cartonGroup.CartonSizes.Add(cartonSize);
				AssertEquals("Precondition", 1, cartonGroup.CartonSizes.Count);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				detachButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals(0, cartonGroup.CartonSizes.Count);
			}
		}

		#endregion

		#region TestAttachAndDetach_Organisations

		public void TestAttachAndDetach_Organisations()
		{
			var org = Helper.CreateClient("1");
			var cartonGroup = Helper.CreateWhsCartonGroup("G1", "Group");
			Factory.Save();

			using (var form = new WhsCartonGroupEntryForm(cartonGroup))
			{
				form.Show();

				var moduleButtonGrid = GuiTestHelper.FindControl<ZModuleButtonGrid>(form.Controls, "CartonGroupParentGrid");
				var toolStrip = GuiTestHelper.FindControl<ZToolStrip>(moduleButtonGrid.Controls, "toolStrip");
				var attachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).FirstOrDefault();
				var detachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Detach, true).FirstOrDefault();
				var editButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Edit, true).FirstOrDefault();
				AssertEquals("Grid should be readonly.", true, moduleButtonGrid.InnerGrid.ReadOnly);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				attachButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertNotNull(moduleButtonGrid.LastShownAttachPopupForTesting);
				AssertEquals(ModuleIDs.Organisation, moduleButtonGrid.LastShownAttachPopupForTesting.CurrentModule.ModuleID);

				editButton.PerformClick();
				AssertType<EmbeddedModulePopup>(ZFormModaliser.LastFormShownForTest);
				AssertEquals("Organization", ((EmbeddedModulePopup)ZFormModaliser.LastFormShownForTest).Text);

				cartonGroup.ParentOrgMiscServs.Add(org.MiscServ);
				AssertEquals("Precondition", 1, cartonGroup.ParentOrgMiscServs.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				attachButton.PerformClick();
				((ZFilterStripCommonControl)((EmbeddedModulePopup)ZFormModaliser.LastFormShownForTest).Module_ForTest.EmbeddedControl).FirePerformSearch();
				AssertHasRowError(moduleButtonGrid.FindBoxList.FindByPK(org.PK),
					"This record has already been selected. Please ensure you select only records that have not already been used.");

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				detachButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals(0, cartonGroup.CartonSizes.Count);
			}
		}

		#endregion

		#region TestDbHits

		public void TestDbHits()
		{
			var cartonGroup = Helper.CreateWhsCartonGroup("G1", "Group");
			for (int i = 0; i < 100; i++)
			{
				cartonGroup.CartonSizes.Add(Helper.CreateWhsCartonSize(i.ToString()));
			}

			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var cartonGroup_InSecondFactory = factory2.Load<WhsCartonGroup>(cartonGroup.PK);
			using (var form = new WhsCartonGroupEntryForm(cartonGroup_InSecondFactory))
			{
				form.Show();
			}

			var expectedDBHits = new Dictionary<string, int>
			{
				{ WhsCartonGroupSchema.Constants.TableName, 1 },
				{ WhsCartonGroupSizeLinkSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 }
			};

			AssertDbHits(expectedDBHits, factory2);
		}

		#endregion

		#region Implementation

		WhsTestHelperFunctionsEnv Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory)); }
		}

		WhsTestHelperFunctionsEnv helper;

		protected override Form GetFormToBashCore()
		{
			return new WhsCartonGroupEntryForm(Factory.New<WhsCartonGroup>());
		}

		#endregion
	}
}
