using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(EPaymentReasonsRegistryItemEditor))]
	class EPaymentReasonRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new EPaymentReasonsRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((EPaymentReasonsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(EPaymentReasonsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new EPaymentReasonRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
		}

		protected override object[] GetValidRegistryValues()
		{
			var reasonCollection1 = new EPaymentReasonCollection();
			var reason1InCollection1 = reasonCollection1.AddNew();
			reason1InCollection1.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			reason1InCollection1.ReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;
			reason1InCollection1.ReasonDescription = EPaymentReasonCodes.OFXReasonCodes.CodesList.GetDescriptionFromCode(EPaymentReasonCodes.OFXReasonCodes.AccountingServices);
			var reason2InCollection1 = reasonCollection1.AddNew();
			reason2InCollection1.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			reason2InCollection1.ReasonCode = EPaymentReasonCodes.OFXReasonCodes.EmployeePaymentSalaryWages;
			reason2InCollection1.ReasonDescription = EPaymentReasonCodes.OFXReasonCodes.CodesList.GetDescriptionFromCode(EPaymentReasonCodes.OFXReasonCodes.EmployeePaymentSalaryWages);

			var reasonCollection2 = new EPaymentReasonCollection();
			var reason1InCollection2 = reasonCollection2.AddNew();
			reason1InCollection2.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			reason1InCollection2.ReasonCode = EPaymentReasonCodes.OFXReasonCodes.HardwareConsultancyImplementation;
			reason1InCollection2.ReasonDescription = EPaymentReasonCodes.OFXReasonCodes.CodesList.GetDescriptionFromCode(EPaymentReasonCodes.OFXReasonCodes.HardwareConsultancyImplementation);
			var reason2InCollection2 = reasonCollection2.AddNew();
			reason2InCollection2.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			reason2InCollection2.ReasonCode = EPaymentReasonCodes.OFXReasonCodes.GoodsPaymentPurchase;
			reason2InCollection2.ReasonDescription = EPaymentReasonCodes.OFXReasonCodes.CodesList.GetDescriptionFromCode(EPaymentReasonCodes.OFXReasonCodes.GoodsPaymentPurchase);

			return new[] { reasonCollection1, reasonCollection2 };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		new class RegistryFormForTest : RegistryItemEditorTestCase.RegistryFormForTest
		{
			public RegistryFormForTest(IRegistryItem registryItem)
				: base(registryItem)
			{
			}

			public override void DisplayRegistryItem()
			{
				RegistriesTreeView.SelectedNode = RegistriesTreeView.Nodes[0];
				FallbackTreeView.SelectedNode = FallbackTreeView.Nodes[0].Nodes[0];
			}
		}

		protected override RegistryItemEditorTestCase.RegistryFormForTest GetNewRegistryFormForTest(IRegistryItem registryItem)
		{
			return new RegistryFormForTest(registryItem);
		}
	}
}
