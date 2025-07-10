using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(NonPersistentCusContainer))]
	public class NonPersistentCusContainerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestOwnerCountry()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var container = declaration.CusContainers.AddNew();
			container.CO_RN_NKOwnerCountry = Core.Constants.CountryCodes.Turkey;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var containerForTest = new NonPersistentCusContainer(invoiceLine);
			AssertEquals("OwnerCountry should be empty when Container is null", ZString.Empty, containerForTest.OwnerCountry);

			containerForTest.Container = container;
			AssertEquals("OwnerCountry should be CO_RN_NKOwnerCountry of Container", Core.Constants.CountryCodes.Turkey, containerForTest.OwnerCountry);
		}

		public void TestCustomsValue()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
				declaration.AutoCreateChargesBasedOnIncoTerm = false;

				BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
				invoice.JZ_InvoiceAmount = 10000m;
				invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

				BaseJobComInvHeaderCharge oFT = invoice.Charges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight, 1000m);
				oFT.J7_IsIncludedInITOT = true;

				BaseJobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 10000m;

				BaseCusContainer container1 = declaration.CusContainers.AddNew();
				BaseCusContainer container2 = declaration.CusContainers.AddNew();

				invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
				invoiceLine.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = true;

				invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].SplitValue = 4000m;
				invoiceLine.ContainersForInvoiceLinesForBindingOnly[1].SplitValue = 6000m;
				declaration.ResumeApportionment();
				AssertEquals("CustomsValue for pivot1", 3600m, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].CustomsValue);
				AssertEquals("CustomsValue for pivot2", 5400m, invoiceLine.ContainersForInvoiceLinesForBindingOnly[1].CustomsValue);
			}
		}

		public void TestNotAddedToFactoryCache()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "Test";
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var nPCusContainer = new NonPersistentCusContainer(invoiceLine);
			AssertEquals("Data should not be cached in the factory", 0, Factory.GetBizOsForPK(nPCusContainer.PK.ToGuid()).Length);
		}

		public void TestContainerNumberException()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "Test";
			BaseJobComInvoiceLine invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			NonPersistentCusContainer nPCusContainer = new NonPersistentCusContainer(invoiceLine);
			nPCusContainer.Container = container;
			nPCusContainer.IsForInvoiceLine = ZBool.True;
			AssertEquals("Container Number", "TEST", nPCusContainer.ContainerNumber);

			declaration.CusContainers.RemoveAndDelete(container);
			AssertEquals("Container Number", ZString.Empty, nPCusContainer.ContainerNumber);
		}

		public void TestUpdatePivotProxyPropertiesReadOnly()
		{
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BaseJobDeclaration declaration = factory2.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.Bills.AddNew();
			BaseCusContainer container = declaration.CusContainers.AddNew();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			AssertEquals("PreCondition:IsForInvoiceLine (default from registry)", true, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = false;
			AssertEquals("PreCondition:IsForInvoiceLine (for this test)", false, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);

			AssertEquals("GrossWeightInKGInfo", true, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].GrossWeightInKGInfo.ReadOnly);
			AssertEquals("NetWeightInKGInfo", true, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].NetWeightInKGInfo.ReadOnly);
			AssertEquals("SplitValueInfo", true, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].SplitValueInfo.ReadOnly);
			AssertEquals("PackQtyInfo", true, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].PackQtyInfo.ReadOnly);

			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			AssertEquals("GrossWeightInKGInfo", false, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].GrossWeightInKGInfo.ReadOnly);
			AssertEquals("NetWeightInKGInfo", false, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].NetWeightInKGInfo.ReadOnly);
			AssertEquals("SplitValueInfo", false, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].SplitValueInfo.ReadOnly);
			AssertEquals("PackQtyInfo", false, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].PackQtyInfo.ReadOnly);

			factory2.Save();

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			BaseJobComInvoiceLine invoiceLineLoaded = factory3.Load<BaseJobComInvoiceLine>(invoiceLine.PK);
			AssertEquals("Pivot is persisted", true, invoiceLineLoaded.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
			AssertEquals("GrossWeightInKGInfo", false, invoiceLineLoaded.ContainersForInvoiceLinesForBindingOnly[0].GrossWeightInKGInfo.ReadOnly);
			AssertEquals("NetWeightInKGInfo", false, invoiceLineLoaded.ContainersForInvoiceLinesForBindingOnly[0].NetWeightInKGInfo.ReadOnly);
			AssertEquals("SplitValueInfo", false, invoiceLineLoaded.ContainersForInvoiceLinesForBindingOnly[0].SplitValueInfo.ReadOnly);
			AssertEquals("PackQtyInfo", false, invoiceLineLoaded.ContainersForInvoiceLinesForBindingOnly[0].PackQtyInfo.ReadOnly);
		}

		public void TestPivotProxyProperties()
		{
			var declaration = GetNewDeclaration();
			var container = declaration.CusContainers.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			AssertEquals("GrossWeightInKG", 0m, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].GrossWeightInKG);
			AssertEquals("NetWeightInKG", 0m, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].NetWeightInKG);
			AssertEquals("SplitValue", 0m, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].SplitValue);
			AssertEquals("SplitValueCurrency", ZGuid.Empty, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].SplitValueCurrency);
			AssertEquals("PackQty", ZInt.Zero, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].PackQty);

			invoice.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			AssertEquals("GrossWeightInKG", 0m, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].GrossWeightInKG);
			AssertEquals("NetWeightInKG", 0m, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].NetWeightInKG);
			AssertEquals("SplitValue", 0m, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].SplitValue);
			AssertEquals("SplitValueCurrency", GlbCompany.CurrentCompany.Country.LocalCurrency.PK, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].SplitValueCurrency);
			AssertEquals("PackQty", ZInt.Zero, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].PackQty);

			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].GrossWeightInKG = 10m;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].NetWeightInKG = 20m;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].SplitValue = 30m;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].PackQty = 11;

			AssertEquals("GrossWeightInKG", 10m, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].GrossWeightInKG);
			AssertEquals("NetWeightInKG", 20m, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].NetWeightInKG);
			AssertEquals("SplitValue", 30m, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].SplitValue);
			AssertEquals("PackQty", 11, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].PackQty);
		}
		public void TestAvailableProperties()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseCusContainer cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "UUUU1234567";
			BaseJobComInvoiceLine invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			NonPersistentCusContainer nPCusContainer = new NonPersistentCusContainer(invoiceLine);
			nPCusContainer.Container = cusContainer;
			nPCusContainer.IsForInvoiceLine = ZBool.True;
			AssertEquals("UUUU1234567", nPCusContainer.ContainerNumber);
		}

		public void TestDeveloperExceptionWhenAValueIsAssignedAndNoPivotExists()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseCusContainer container = declaration.CusContainers.AddNew();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			//CustomsDataRegistry.Instance.AutoAllocateContainerToInvoiceLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			// defaulting IsForInvoiceLine to true, now means we do have a pivot.. so, um, is this test still valid?  have amended logic a little to just test current behaviour - drh - WI00013478 
			// AssertNull("container does not have a pivot yet", invoiceLine.ContainersForInvoiceLines[0].Pivot);

			AssertDeveloperExceptionWhenNoPivotExistsAndSetterIsTriggered(invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].GrossWeightInKGInfo);
			AssertDeveloperExceptionWhenNoPivotExistsAndSetterIsTriggered(invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].NetWeightInKGInfo);
			AssertDeveloperExceptionWhenNoPivotExistsAndSetterIsTriggered(invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].SplitValueInfo);
			AssertDeveloperExceptionWhenNoPivotExistsAndSetterIsTriggered(invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].PackQtyInfo);
		}

		void AssertDeveloperExceptionWhenNoPivotExistsAndSetterIsTriggered(ZPropertyInfo propertyOfPivotContainer)
		{
			try
			{
				ErrorReporter.Clear();
				propertyOfPivotContainer.Value = propertyOfPivotContainer.OriginalValue;
			}
			finally
			{
				//AssertEquals(NonPersistentCusContainer.NoPivotAndCantAssignValue, ErrorReporter.LastMessageReported);
				AssertEquals(ZString.Empty, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		#region Test

		protected override BusinessObject GetNewBusinessObject()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseCusContainer container = declaration.CusContainers.AddNew();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			NonPersistentCusContainer result = new NonPersistentCusContainer(invoiceLine);
			result.Container = container;
			result.IsForInvoiceLine = true;
			return result;
		}

		protected virtual BaseJobDeclaration GetNewDeclaration()
		{
			return BaseJobDeclaration.New(Factory);
		}
		#endregion
	}
}
