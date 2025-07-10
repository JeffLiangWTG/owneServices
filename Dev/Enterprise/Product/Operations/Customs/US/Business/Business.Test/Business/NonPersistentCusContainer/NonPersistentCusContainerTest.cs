using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(NonPersistentCusContainer))]
	public class NonPersistentCusContainerTest : Customs.Business.Testing.NonPersistentCusContainerTest
	{
		public void TestIsForInvoiceLineChangedIsFDAContainer()
		{
			container1.CO_ContainerNumber = "MAII89045";
			container2.CO_ContainerNumber = "AEER5006230";

			FDA fda = invoiceLine.FDAs.AddNew();
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			Assert(fda.ContainersForInvoiceLine.FindByContainerNumber("MAII89045") != null);
			Assert(fda.ContainersForFDALine.Find(x => x.Container.CO_ContainerNumber == "MAII89045") != null);

			invoiceLine.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = true;
			Assert(fda.ContainersForInvoiceLine.FindByContainerNumber("AEER5006230") != null);
			Assert(fda.ContainersForFDALine.Find(x => x.Container.CO_ContainerNumber == "AEER5006230") != null);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			NonPersistentCusContainer result = new NonPersistentCusContainer(invoiceLine);
			result.Container = container1;
			result.IsForInvoiceLine = true;
			return result;
		}

		JobDeclaration declaration;
		CusContainer container1;
		CusContainer container2;
		JobComInvoiceLine invoiceLine;

		protected override void SetUp()
		{
			base.SetUp();
			CustomsDataRegistry.Instance.DefaultCurrencyToLocalCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableINB = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.JE_ContainerMode = Constants.ContainerModes.NonContainerised;

			Bill bill = declaration.Bills.AddNew();
			bill.US_AMSCarrierIndicator = YesNoDefaultList.Codes.No;

			container1 = declaration.CusContainers.AddNew();
			container2 = declaration.CusContainers.AddNew();

			declaration.Invoices.AddNew();
			invoiceLine = declaration.InvoiceLines.AddNew();
		}
	}
}
