using System.Collections.Generic;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(MiscOptionsLayouts))]
	sealed class MiscOptionsLayoutsTest : LayoutsAbstractTest
	{
		public void TestSetVisibility_ManifestToOpenUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MiscOptionsTabPage;

				var manifestToOpenUserControl = form.FindSingleOrDefault<ManifestToOpenUserControl>(c => c.Name == "ManifestToOpenUserControl");
				AssertEquals("Should be invisible for non-import declaration.", false, manifestToOpenUserControl.Visible);

				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MiscOptionsTabPage;
				manifestToOpenUserControl = form.FindSingleOrDefault<ManifestToOpenUserControl>(c => c.Name == "ManifestToOpenUserControl");
				AssertEquals("Should be invisible if no invoice lines.", false, manifestToOpenUserControl.Visible);

				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoce = declaration.Invoices.AddNew();
				var invoiceLine = invoce.InvoiceLines.AddNew();
				invoiceLine.JI_Procedure = "5800";
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MiscOptionsTabPage;
				manifestToOpenUserControl = form.FindSingleOrDefault<ManifestToOpenUserControl>(c => c.Name == "ManifestToOpenUserControl");
				AssertEquals("Should be visible when import and procedure ends with 00.", true, manifestToOpenUserControl.Visible);

				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLine.JI_Procedure = "5391";
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MiscOptionsTabPage;
				manifestToOpenUserControl = form.FindSingleOrDefault<ManifestToOpenUserControl>(c => c.Name == "ManifestToOpenUserControl");
				AssertEquals("Should be invisible if procedure doesn't end with 00.", false, manifestToOpenUserControl.Visible);
			}
		}

		public void TestVisibility_GuaranteeControls()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			AssertEquals("Visible when is Import", true, Layout.IsVisible(MiscOptionsControlBag.Instance.GuaranteeInfoOptionsSeparatorUserControl, declaration));
			AssertEquals("Visible when is Import", true, Layout.IsVisible(MiscOptionsControlBag.Instance.GuaranteeGuidFindBox, declaration));
			AssertEquals("Visible when is Import", true, Layout.IsVisible(MiscOptionsControlBag.Instance.BondTypeDropEdit, declaration));
			AssertEquals("Visible when is Import", true, Layout.IsVisible(MiscOptionsControlBag.Instance.ReferenceNumberTextBox, declaration));
			AssertEquals("Visible when is Import", true, Layout.IsVisible(MiscOptionsControlBag.Instance.GuaranteeDescriptionTextBox, declaration));
			AssertEquals("Visible when is Import", true, Layout.IsVisible(MiscOptionsControlBag.Instance.DedicatedAmountCalcEdit, declaration));
			AssertEquals("Visible when is Import", true, Layout.IsVisible(MiscOptionsControlBag.Instance.RatioCalcEdit, declaration));
			AssertEquals("Visible when is Import", true, Layout.IsVisible(MiscOptionsControlBag.Instance.AmountCalcEdit, declaration));

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Invisible when is not Import", false, Layout.IsVisible(MiscOptionsControlBag.Instance.GuaranteeInfoOptionsSeparatorUserControl, declaration));
			AssertEquals("Invisible when is not Import", false, Layout.IsVisible(MiscOptionsControlBag.Instance.GuaranteeGuidFindBox, declaration));
			AssertEquals("Invisible when is not Import", false, Layout.IsVisible(MiscOptionsControlBag.Instance.BondTypeDropEdit, declaration));
			AssertEquals("Invisible when is not Import", false, Layout.IsVisible(MiscOptionsControlBag.Instance.ReferenceNumberTextBox, declaration));
			AssertEquals("Invisible when is not Import", false, Layout.IsVisible(MiscOptionsControlBag.Instance.GuaranteeDescriptionTextBox, declaration));
			AssertEquals("Invisible when is not Import", false, Layout.IsVisible(MiscOptionsControlBag.Instance.DedicatedAmountCalcEdit, declaration));
			AssertEquals("Invisible when is not Import", false, Layout.IsVisible(MiscOptionsControlBag.Instance.RatioCalcEdit, declaration));
			AssertEquals("Invisible when is not Import", false, Layout.IsVisible(MiscOptionsControlBag.Instance.AmountCalcEdit, declaration));
		}

		public void TestVisibility_ExporterUnionControls()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Visible when is Export", true, Layout.IsVisible(MiscOptionsControlBag.Instance.ExportersUnionInfoUserControl, declaration));

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Invisible when is Import", false, Layout.IsVisible(MiscOptionsControlBag.Instance.ExportersUnionInfoUserControl, declaration));
		}

		protected override int ControlBagCount => 3;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonMiscOptionsControlBag.Instance.MiscellaneousOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (CommonMiscOptionsControlBag.Instance.BranchGuidFindBox, ControlWidthClass.Auto);
				yield return (CommonMiscOptionsControlBag.Instance.BrokerCodeFindBox, ControlWidthClass.Auto);
				yield return (CommonMiscOptionsControlBag.Instance.MergeByDropEdit, ControlWidthClass.Auto);
				yield return (CommonMiscOptionsControlBag.Instance.PaidByDropEdit, ControlWidthClass.Auto);
				yield return (MiscOptionsControlBag.Instance.ManifestToOpenUserControl, ControlWidthClass.LongNoCaption);
				yield return (MiscOptionsControlBag.Instance.ExportersUnionInfoUserControl, ControlWidthClass.LongNoCaption);
				yield return (MiscOptionsControlBag.Instance.GuaranteeInfoOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (MiscOptionsControlBag.Instance.GuaranteeGuidFindBox, ControlWidthClass.Long);
				yield return (MiscOptionsControlBag.Instance.BondTypeDropEdit, ControlWidthClass.Long);
				yield return (MiscOptionsControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (MiscOptionsControlBag.Instance.GuaranteeDescriptionTextBox, ControlWidthClass.Long);
				yield return (MiscOptionsControlBag.Instance.DedicatedAmountCalcEdit, ControlWidthClass.Medium);
				yield return (MiscOptionsControlBag.Instance.RatioCalcEdit, ControlWidthClass.Medium);
				yield return (MiscOptionsControlBag.Instance.AmountCalcEdit, ControlWidthClass.Medium);
				yield return (MiscOptionsControlBag.Instance.TotalAmountsSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (MiscOptionsControlBag.Instance.InvoiceCountCalcEdit, ControlWidthClass.Long);
				yield return (MiscOptionsControlBag.Instance.TotalInvoiceAmountLocalCurrencyControl, ControlWidthClass.Long);
				yield return (MiscOptionsControlBag.Instance.TotalFreeOnBoardLocalCurrencyControl, ControlWidthClass.Long);
				yield return (MiscOptionsControlBag.Instance.TotalFreightLocalCurrencyControl, ControlWidthClass.Long);
				yield return (MiscOptionsControlBag.Instance.TotalInsuranceLocalCurrencyControl, ControlWidthClass.Long);
				yield return (MiscOptionsControlBag.Instance.TotalOverseasLocalCurrencyControl, ControlWidthClass.Long);
				yield return (MiscOptionsControlBag.Instance.LocalTotalChargesLocalCurrencyControl, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (EU.GUI.MiscOptionsControlBag.Instance.RelatedDeclarationsUserControl, ControlWidthClass.LongNoCaption);
				yield return (MiscOptionsControlBag.Instance.SupportingInformationUserControl, ControlWidthClass.LongControl);
			}
		}

		public PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new MiscOptionsLayouts()).Layout);

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new MiscOptionsLayoutBuilder<JobDeclaration>();

		PanelLayout layout;
	}
}
