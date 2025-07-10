using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestsSubclassesOf(typeof(EntryInstructionDetailsUserControl))]
abstract class EntryInstructionDetailsUserControlTest<T> : TestCaseWithFactory
	where T : EntryInstructionDetailsUserControl
{
	public void TestDetailsUserControlType()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryInstructions.AddNew();
		using (var form = new ZForm(declaration))
		{
			using (var control = GetNewEntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var detailsUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("DetailsUserControl");
				AssertEquals("DetailsUserControl.UserControlType", ExpectedEntryInstructionDetailBasicUserControlTest(), detailsUserControl.UserControlType);
			}
		}
	}

	public void TestGridUserControlType()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryInstructions.AddNew();
		using (var form = new ZForm(declaration))
		{
			using (var control = GetNewEntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var entryInstructionGridUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("EntryInstructionGridUserControl");
				AssertEquals("EntryInstructionGridUserControl.UserControlType", typeof(EntryInstructionGridUserControl), entryInstructionGridUserControl.UserControlType);
			}
		}
	}

	public void TestSupplyChainActorTabPageVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryInstructions.AddNew();
		using (var form = new ZForm(declaration))
		using (var control = GetNewEntryInstructionDetailsUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				control.SetTabPagesVisibility();
				AssertEquals("SupplyChainActorTabPage Export", true, control.FindSingle<ZTabPage>("SupplyChainActorTabPage").TabVisible);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				control.SetTabPagesVisibility();
				AssertEquals("SupplyChainActorTabPage Import", true, control.FindSingleOrDefault<ZTabPage>("SupplyChainActorTabPage").TabVisible);
			});
		}
	}

	public void TestGetSupportingDocumentsUserControlType() => TestTabPageUserControlType("SupportingDocumentsTabPage", "SupportingDocumentsUserControl", ExpectedSupportingDocumentsUserControlType());

	public void TestGetAdditionalInfosUserControlType() => TestTabPageUserControlType("AdditionalInfoTabPage", "AdditionalInfoUserControl", ExpectedAdditionalInfosUserControlType(), GetExpectedAdditionalInfoTabPageCaption());

	public void TestGetPreviousDocumentsUserControlType() => TestTabPageUserControlType("PreviousDocumentsTabPage", "PreviousDocumentsUserControl", ExpectedPreviousDocumentsUserControlType());

	public void TestTabPageUserControlType(ZString tabPageName, ZString userControlName, Type expectedType, string expectedCaption = null)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageType;
		declaration.CustomsEntryInstructions.AddNew();
		using (var form = new ZForm(declaration))
		{
			using (var control = GetNewEntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					var tabControl = control.FindSingle<ZTabControl>("EntryInstructionTabControl");
					var tabPage = control.FindSingle<ZTabPage>(tabPageName);
					AssertEquals($"{tabPageName} should be visible", true, tabPage.TabVisible);
					if (expectedCaption != null)
					{
						AssertEquals($"{tabPageName} Caption", expectedCaption, tabPage.CaptionResourceString.Caption);
					}

					tabControl.SelectTab(tabPage);
					AssertEquals($"{userControlName}", expectedType, control.FindSingle<ZDynamicControlCreationUserControl>(userControlName).UserControlType);
				});
			}
		}
	}

	protected abstract ZString MessageType { get; }

	protected abstract Type ExpectedSupportingDocumentsUserControlType();

	protected abstract Type ExpectedAdditionalInfosUserControlType();

	protected abstract Type ExpectedPreviousDocumentsUserControlType();

	protected abstract Type ExpectedEntryInstructionDetailBasicUserControlTest();

	protected abstract T GetNewEntryInstructionDetailsUserControl();

	protected abstract string GetExpectedAdditionalInfoTabPageCaption();
}
