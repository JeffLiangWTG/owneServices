using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class NonPersistentCusContainerCollectionTest<T> : NonPersistentBusinessObjectCollectionTestCase<T> where T : NonPersistentCusContainerCollection
	{
		public void TestFindByContainer()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseCusContainer container1 = declaration.CusContainers.AddNew();
			BaseCusContainer container2 = declaration.CusContainers.AddNew();

			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			AssertEquals("FindByContainer", container1, invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container1).Container);
			AssertEquals("FindByContainer", container2, invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container2).Container);
		}

		public void TestFindByContainerNumber()
		{
			BaseCusContainer cusContainer1 = Factory.New<BaseCusContainer>();
			BaseCusContainer cusContainer2 = Factory.New<BaseCusContainer>();
			BaseJobComInvoiceLine invoiceLine = Factory.New<BaseJobComInvoiceLine>();

			NonPersistentCusContainerCollection nPCusContainers = new NonPersistentCusContainerCollection(invoiceLine);

			cusContainer1.CO_ContainerNumber = "UUU2222222";
			NonPersistentCusContainer containerInPivot1 = nPCusContainers.AddNew(cusContainer1);

			cusContainer2.CO_ContainerNumber = "UUU1111111";
			NonPersistentCusContainer containerInPivot2 = nPCusContainers.AddNew(cusContainer2);

			AssertEquals("We should have two objects here", 2, nPCusContainers.Count);

			NonPersistentCusContainer foundContainer = nPCusContainers.FindByContainerNumber(cusContainer2.CO_ContainerNumber);
			AssertNotNull("Found container", foundContainer);

			NonPersistentCusContainer notFoundContainer = nPCusContainers.FindByContainerNumber("Test");
			AssertNull("Not Found container", notFoundContainer);
		}

		public void TestAllowNew()
		{
			BaseCusContainer cusContainer1 = Factory.New<BaseCusContainer>();
			BaseCusContainer cusContainer2 = Factory.New<BaseCusContainer>();
			BaseJobComInvoiceLine invoiceLine = Factory.New<BaseJobComInvoiceLine>();

			NonPersistentCusContainerCollection nPCusContainers = new NonPersistentCusContainerCollection(invoiceLine);
			AssertEquals("NP CusContainers does now allow new", false, nPCusContainers.AllowNew);
		}

		public void TestNonPersistentCusContainerCollection1()
		{
			BaseCusContainer cusContainer1 = Factory.New<BaseCusContainer>();
			BaseCusContainer cusContainer2 = Factory.New<BaseCusContainer>();
			BaseJobComInvoiceLine invoiceLine = Factory.New<BaseJobComInvoiceLine>();

			NonPersistentCusContainerCollection nPCusContainers = new NonPersistentCusContainerCollection(invoiceLine);

			cusContainer1.CO_ContainerNumber = "UUU2222222";
			NonPersistentCusContainer containerInPivot1 = nPCusContainers.AddNew(cusContainer1);

			cusContainer2.CO_ContainerNumber = "UUU1111111";
			NonPersistentCusContainer containerInPivot2 = nPCusContainers.AddNew(cusContainer2);

			AssertEquals("We should have two objects here", 2, nPCusContainers.Count);
			nPCusContainers.SortByContainerNumber();

			AssertEquals("First Container Number after sorted", "UUU1111111", nPCusContainers[0].ContainerNumber);
			AssertEquals("Second Container Number after sorted", "UUU2222222", nPCusContainers[1].ContainerNumber);

			nPCusContainers.RemoveAndDelete(containerInPivot1);
			AssertEquals("We should have 1 object left", 1, nPCusContainers.Count);
		}

		public void TestFindByLinkedContainer()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			BaseCusContainer cusContainer1 = Factory.New<BaseCusContainer>();
			cusContainer1.CO_ContainerNumber = "UUU2222222";
			BaseCusContainer cusContainer2 = Factory.New<BaseCusContainer>();
			cusContainer2.CO_ContainerNumber = "UUU1111111";
			declaration.CusContainers.Add(cusContainer1);
			declaration.CusContainers.Add(cusContainer2);
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_InvoiceNumber = "1";
			BaseJobComInvoiceLine invoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();
			AssertEquals("Two containers for invoice line available", 2, invoiceLine.ContainersForInvoiceLinesForBindingOnly.Count);
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = ZBool.True;

			Factory.Save();

			AssertEquals("Linked Container not found", 1, invoiceLine.ContainersForInvoiceLinesForBindingOnly.AllLinkedContainerNumbers.Length);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BaseJobComInvoiceLine invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			return new NonPersistentCusContainer(invoiceLine);
		}

		public void TestAutoAllocateContainerToInvoiceLineDefaultBehaviour()
		{
			CustomsDataRegistry.Instance.AutoAllocateContainerToInvoiceLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseCusContainer container = declaration.CusContainers.AddNew();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			ZQuery query = new ZQuery(CusContainerInvoiceLinePivotSchema.C2_CO, container.PK);
			query.AddToFilter(CusContainerInvoiceLinePivotSchema.C2_JI, invoiceLine.PK);
			AssertNotNull("Pivot should exist without touching collection", Factory.LoadTop1<CusContainerInvoiceLinePivot>(query));
		}

		public void TestAutoAllocateContainerToInvoiceLineWithouLoading()
		{
			CustomsDataRegistry.Instance.AutoAllocateContainerToInvoiceLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var cusContainerCollection = invoiceLine.ContainersForInvoiceLinesForBindingOnly;
			for (int i = 0; i < 3; i++)
			{
				declaration.CusContainers.AddNew();
				AssertEquals("Container count of a declaration should be equal to container pivots of an invoice line", declaration.CusContainers.Count, cusContainerCollection.Count);
			}
		}

		public void TestAutoDeleteContainerFromDeletedInvoiceLine()
		{
			CustomsDataRegistry.Instance.AutoAllocateContainerToInvoiceLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var cusContainerCollection = invoiceLine.ContainersForInvoiceLinesForBindingOnly;
			declaration.CusContainers.AddNew();
			AssertEquals("Container count of a declaration should be equal to container pivots of an invoice line", 1, cusContainerCollection.Count);

			invoiceLine.Delete();
			AssertEquals("Deleted invoice line does not update its containers", 1, cusContainerCollection.Count);

			declaration.CusContainers.AddNew();
			Assert("No error should be thrown over container editing after invoice line is deleted", true);
			AssertEquals("Deleted invoice line does not update its containers", 1, cusContainerCollection.Count);
		}

		public void TestAutoAllocateContainerToInvoiceLineWhenTurnedOff()
		{
			CustomsDataRegistry.Instance.AutoAllocateContainerToInvoiceLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseCusContainer container = declaration.CusContainers.AddNew();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			AssertEquals("ContainersForInvoiceLines[0].IsForInvoiceLine", false, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
		}

		public void TestAutoAllocateContainerToInvoiceLineWhenMultipleContainers()
		{
			CustomsDataRegistry.Instance.AutoAllocateContainerToInvoiceLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.CusContainers.AddNew();
			declaration.CusContainers.AddNew();
			declaration.CusContainers.AddNew();

			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			AssertEquals("ContainersForInvoiceLines[0].IsForInvoiceLine", false, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
			AssertEquals("ContainersForInvoiceLines[1].IsForInvoiceLine", false, invoiceLine.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine);
			AssertEquals("ContainersForInvoiceLines[2].IsForInvoiceLine", false, invoiceLine.ContainersForInvoiceLinesForBindingOnly[2].IsForInvoiceLine);
		}

		public void TestAutoAllocateContainerToInvoiceLineWhenOneContainer()
		{
			CustomsDataRegistry.Instance.AutoAllocateContainerToInvoiceLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseCusContainer container = declaration.CusContainers.AddNew();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.BreakBulk;
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("Shouldn't be allocated when container mode is not containerised", false, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);

			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.Containerised;
			invoiceLine = invoice.JobComInvoiceLines.AddNew();

			AssertEquals("Shouldn be allocated when container mode is containerised", true, invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
		}
	}
}
