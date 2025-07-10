using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.UniversalDataBuss.Integration;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.GUI.Testing
{
	public class AmendmentSnapshotManagementMenuItemsCreatorTest : TestCaseWithFactory
	{
		public void TestCreateMenuItem()
		{
			var ediMenu = new EDIMenu();
			var creator = new InstantiableAmendmentSnapshotManagementMenuItemsCreatorForTesting(ediMenu);
			var menuItem = creator.CreateMenuItem();

			AssertEquals("Revert to last cleared data", menuItem.Text);
			Assert("Should be invisible by default.", !menuItem.Visible);
		}

		public void TestRefreshMenuItem()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var entryMock = GetEntryHeader("01", "01 Desc~", "11223344");
			ActionAndAssert("Invisible as no entryHeaders and security right being false by default.",
				null, parentMenuItemVisible: false, null);
			ActionAndAssert("Invisible as one entryHeader but no AmendmentSnapshotManager implemented and security right being false by default.",
				(Mock<InstantiableAmendmentSnapshotManagementMenuItemsCreatorForTesting> creator) => declaration.ActiveEntryHeaders.Add(entryMock.Object), parentMenuItemVisible: false, null);
			ActionAndAssert("Invisible as one entryHeader but no AmendmentSnapshotManager implemented, security right configured to true though.",
				(Mock<InstantiableAmendmentSnapshotManagementMenuItemsCreatorForTesting> creator) => Env.Security.EnableRevertToLastClearedData.IsAllowed = true, parentMenuItemVisible: false, null);
			ActionAndAssert("InVisible as one entryHeader with AmendmentSnapshotManager implemented and security right configured to true, but the entryHeader cannot be reverted.",
				(Mock<InstantiableAmendmentSnapshotManagementMenuItemsCreatorForTesting> creator) => entryMock.Protected().Setup<AmendmentSnapshotManager>("GetNewAmendmentSnapshotManagerCore").Returns(new InstantiableAmendmentSnapshotManagerForTesting(entryMock.Object, ZString.Empty)), parentMenuItemVisible: false, null);

			ActionAndAssert("Sub-MenuItems added when ShouldRevertToLastClearedMenuItemForTypicalEntryHeaderCore is true.",
				(Mock<InstantiableAmendmentSnapshotManagementMenuItemsCreatorForTesting> creator) => creator.Protected().Setup<bool>("ShouldRevertToLastClearedMenuItemForTypicalEntryHeaderCore", ItExpr.IsAny<CusEntryHeader>()).Returns(true), parentMenuItemVisible: true, new[] { "01 - 01 Desc~ - 11223344" });

			void ActionAndAssert(string message, Action<Mock<InstantiableAmendmentSnapshotManagementMenuItemsCreatorForTesting>> modifyDeclaration, bool parentMenuItemVisible, string[] childrenMenuItemsText)
			{
				var ediMenu = new EDIMenu();
				var creator = new Mock<InstantiableAmendmentSnapshotManagementMenuItemsCreatorForTesting>(ediMenu);
				var menuItem = creator.Object.CreateMenuItem();

				modifyDeclaration?.Invoke(creator);
				creator.Object.RefreshMenuItem(declaration);
				CombineAssertions(message, () =>
				{
					AssertEquals($"RevertToBAEMenuItem should be {(parentMenuItemVisible ? string.Empty : "in")}visible.", parentMenuItemVisible, menuItem.Visible);
					if (childrenMenuItemsText is not null)
					{
						var subMenuItems = menuItem.MenuItems.Cast<MenuItem>();
						AssertSequencesEqual(childrenMenuItemsText, subMenuItems.Select(m => m.Text).OrderBy(t => t));
						Assert(subMenuItems.All(x => string.Join(",", x.MenuItems.Cast<MenuItem>().Select(m => m.Text)).Equals("Revert to last cleared data (override if conflict),Revert to last cleared data (skip if conflict)")));
					}
					else
					{
						AssertEquals(0, menuItem.MenuItems.Count);
					}
				});
			}

			Mock<CusEntryHeader> GetEntryHeader(string ceiStyle, string ceiDesc, string entryNumber)
			{
				var cei = Factory.New<CusEntryInstruction>();
				cei.CEI_Style = ceiStyle;
				cei.CEI_Description = ceiDesc;

				var entry = Factory.NewMoq<CusEntryHeader>();
				entry.Object.CH_CEI_Instruction = cei.PK;
				entry.Setup(x => x.EntryNumber).Returns(entryNumber);
				return entry;
			}
		}

		public class InstantiableAmendmentSnapshotManagementMenuItemsCreatorForTesting : AmendmentSnapshotManagementMenuItemsCreator
		{
			public InstantiableAmendmentSnapshotManagementMenuItemsCreatorForTesting(EDIMenu menu) : base(menu)
			{
			}

			protected override bool ShouldRevertToLastClearedMenuItemForTypicalEntryHeaderCore(CusEntryHeader entryHeader) => false;
		}

		sealed class InstantiableAmendmentSnapshotManagerForTesting : AmendmentSnapshotManager
		{
			public InstantiableAmendmentSnapshotManagerForTesting(CusEntryHeader entryHeader, ZString messageType) : base(entryHeader, messageType)
			{
			}

			protected override ZInt GetVersionNumberCore()
			{
				throw new NotImplementedException();
			}

			protected override void RestoreEntryHeaderCore(SnapshotRevertingStrategy strategy, IXmlImportLogger logger)
			{
				throw new NotImplementedException();
			}

			protected override Stream TakeSnapshotCore()
			{
				throw new NotImplementedException();
			}
		}
	}
}
