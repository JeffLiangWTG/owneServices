using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using System.Linq;
	using CargoWise.EntityFramework.Testing;

	public class CusContainerInvoiceLinePivotValidationTest : TestCaseWithFactory
	{
		public void TestValidateIsForInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "OOCU0000001";
			container1.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			invoiceLine.ContainersPivot.AddPivotFor(container1);

			var pivot1 = (NonPersistentCusContainer)invoiceLine.ContainersForInvoiceLinesForBindingOnly.First();
			AssertNoMessageError("One Container on line", pivot1.IsForInvoiceLineInfo, CusContainerInvoiceLinePivotValidation.InvoiceLineCannotHaveMultipleWriteoffContainers);

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "OOCU0000002";
			container2.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			invoiceLine.ContainersPivot.AddPivotFor(container2);

			pivot1 = (NonPersistentCusContainer)invoiceLine.ContainersForInvoiceLinesForBindingOnly.First();
			AssertHasMessageError("Two Containers on line", pivot1.IsForInvoiceLineInfo, CusContainerInvoiceLinePivotValidation.InvoiceLineCannotHaveMultipleWriteoffContainers);

			pivot1.IsForInvoiceLine = false;
			pivot1 = (NonPersistentCusContainer)invoiceLine.ContainersForInvoiceLinesForBindingOnly.First();
			AssertNoMessageError("One Container on line", pivot1.IsForInvoiceLineInfo, CusContainerInvoiceLinePivotValidation.InvoiceLineCannotHaveMultipleWriteoffContainers);
		}
	}
}
