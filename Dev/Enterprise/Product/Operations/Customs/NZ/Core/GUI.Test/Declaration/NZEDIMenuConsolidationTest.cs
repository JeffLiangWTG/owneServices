using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Data.Utils;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.Customs.NZ.GUI.Declaration;
using Enterprise.Customs.NZ.GUI.Declaration.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Test.Declaration
{
	sealed class NZEDIMenuConsolidationTest : Customs.GUI.Testing.EDIMenuConsolidationTest<NZEDIMenuConsolidationTest.ConsolidationTestMenu, JobDeclaration>
	{
		public void TestSubmitMenuItem()
		{
			var declaration = TestHelper.CreateSendableImportDeclaration(Factory);
			declaration.JE_VoyageFlightNo = "XXX999";
			Factory.Save();

			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var menu = new NZEDIMenu_ForTest())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals("Precondition: Declaration not merged", 0, declaration.CusEntryHeader.MergedLines.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var consolidatedEntryMenuItem = menu.MenuItems.FindByName("consolidatedEntryMenuItem");
				var queueForConsolidationMenuItem = consolidatedEntryMenuItem.MenuItems.FindByName("queueForConsolidationMenuItem");
				menu.SubmitJobMenuItem.PerformClick();
				AssertContains("Reports validation Message Error", "Flight number is not in the list of valid Flights supported by NZ Customs", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Declaration not queued", ZString.Empty, declaration.JE_ConsolidationStatus);
				AssertEquals("Declaration is merged", 1, declaration.CusEntryHeader.MergedLines.Count);

				Factory.Save();
				menu.RefreshMenu();
				AssertEquals("Menu is still enabled", true, queueForConsolidationMenuItem.Enabled);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.SubmitJobMenuItem.PerformClick();
				AssertContains("Reports success", "Original Entry Message Generated and Ready to be sent by Service Tasks.", UnitTestUserNotification.Instance.LastMessage.Text);
				Factory.Save();

				menu.RefreshMenu();
				AssertEquals("Menu is now disabled", false, queueForConsolidationMenuItem.Enabled);
				AssertEquals(false, declaration.HasChanges);
			}
		}

		public void TestResetToOriginalMenuItem_Enable()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCA;
			declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			using (var menu = new NZEDIMenu_ForTest())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals("Menu should be inactive for dec with status 'Ready For Consolidation'", false, menu.ResetToOriginalMenuItem.Enabled);
			}

			declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.AppliedToConsolidation;
			using (var menu = new NZEDIMenu_ForTest())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals("Menu should be inactive for dec with status 'Applied To Consolidation'", false, menu.ResetToOriginalMenuItem.Enabled);
			}

			var consolidatedDeclaration = Factory.New<ConsolidatedDeclaration>();
			var cusReconDeclarationCollection = new Customs.Business.ConsolidatedJobDeclarationCollection<JobDeclaration>(consolidatedDeclaration);
			declaration.ActiveEntryHeaders.AddNew();
			declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			cusReconDeclarationCollection.Add(declaration);
			declaration.JE_EntryStatus = ZString.Empty;
			using (var menu = new NZEDIMenu_ForTest())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals("Menu should be inactive for consolidated dec", false, menu.ResetToOriginalMenuItem.Enabled);
			}
		}

		protected override ConsolidationTestMenu GetEdiMenu => new ConsolidationTestMenu();
		protected override JobDeclaration GetDeclaration => TestHelper.CreateSendableImportDeclaration(Factory);
		protected override Func<ConsolidationTestMenu, IEnumerable<MenuItem>> QueuedForConsolidationDisablesMenuItems => menu => new[] { menu.SubmitJobMenuItem, menu.CancelJobMenuItem };
		protected override Func<ConsolidationTestMenu, IEnumerable<MenuItem>> QueuedForConsolidationPromptsOnSubmitMenuItems => menu => new[] { menu.SubmitJobMenuItem, menu.TSWSubmitWithDetails, menu.TSWQueueForManifesting };
		protected override Func<JobDeclaration, IList> GetMergedLinesFunc => jobDeclaration => jobDeclaration.CusEntryHeader.MergedLines;
		protected override Action<JobDeclaration> MakeDeclarationMessageErrorAction => declaration => declaration.JE_VoyageFlightNo = "XXX999";
		protected override string MessageError => "Flight number is not in the list of valid Flights supported by NZ Customs";
		protected override Action<ConsolidationTestMenu> SubmitDeclarationClick => menu => menu.SubmitJobMenuItem.PerformClick();
		protected override Action<ConsolidationTestMenu, bool> SetRefuseLockForTesting => (menu, value) => menu.ConsolidatedEntryMenuProvider.RefuseLockForTesting = value;
		protected override bool SupportsRemoveFromConsolidation => true;

		public sealed class ConsolidationTestMenu : NZEDIMenu
		{
			public MenuItem TSWSubmitWithDetails => base.tSWSubmitWithDetails;
			public MenuItem TSWQueueForManifesting => base.tSWQueueForManifesting;
			public MenuItem SubmitJobMenuItem => base.submitJobMenuItem;
			public MenuItem CancelJobMenuItem => base.cancelJobMenuItem;
			public MenuItem ResetToOriginalMenuItem => base.resetToOriginalMenuItem;
			protected override ConsolidatedEntryMenuProvider GetConsolidatedEntryMenuProvider() => new ConsolidationMenuProviderForTest(Form);
			public new ConsolidationMenuProviderForTest ConsolidatedEntryMenuProvider => (ConsolidationMenuProviderForTest)base.ConsolidatedEntryMenuProvider;
		}

		public sealed class ConsolidationMenuProviderForTest : NZConsolidatedEntryMenuProvider
		{
			public ConsolidationMenuProviderForTest(ZForm parentForm) : base(parentForm)
			{
			}

			public bool RefuseLockForTesting { get; set; }

			protected override SqlApplicationLock LockDeclarationForConsolidation() => RefuseLockForTesting ? null : base.LockDeclarationForConsolidation();
		}
	}
}
