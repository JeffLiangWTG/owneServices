using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FDARelatedContainersGenPivot))]
	public class FDARelatedContainersGenPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestContainerEquipmentID()
		{
			AddContainerForInvoiceLine("MAEUJJJJ", "20");
			var fdaContainersForInvoiceLine = FDA.ContainersForInvoiceLine;
			AssertEquals(1, fdaContainersForInvoiceLine.Count);
			fdaContainersForInvoiceLine[0].IsForFDALine = true;

			AssertEquals(1, FDA.ContainersForFDALine.Count);
			AssertEquals("MAEUJJJJ", ((IUSContainer)FDA.ContainersForFDALine[0]).ContainerEquipmentID);
		}

		public void TestContainerUSContainerCode()
		{
			AddContainerForInvoiceLine("GWR001975", USContainerCodeList.Codes.RR);
			var fdaContainersForInvoiceLine = FDA.ContainersForInvoiceLine;
			AssertEquals(1, fdaContainersForInvoiceLine.Count);
			fdaContainersForInvoiceLine[0].IsForFDALine = true;

			AssertEquals(1, FDA.ContainersForFDALine.Count);
			AssertEquals("GWR001975", ((IUSContainer)FDA.ContainersForFDALine[0]).ContainerEquipmentID);
			AssertEquals(USContainerCodeList.Codes.RR, ((IUSContainer)FDA.ContainersForFDALine[0]).USContainerCode);
		}

		public void TestIsRailCar()
		{
			AddContainerForInvoiceLine("MAEUJJJJ", "20");
			var fdaContainersForInvoiceLine = FDA.ContainersForInvoiceLine;
			fdaContainersForInvoiceLine[0].IsForFDALine = true;
			AssertEquals(false, ((IUSContainer)FDA.ContainersForFDALine[0]).IsRailCar);

			AddContainerForInvoiceLine("GWR001975", USContainerCodeList.Codes.RR);
			fdaContainersForInvoiceLine = FDA.ContainersForInvoiceLine;
			fdaContainersForInvoiceLine[1].IsForFDALine = true;
			AssertEquals(true, ((IUSContainer)FDA.ContainersForFDALine[1]).IsRailCar);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return FDARelatedContainersGenPivot;
		}

		FDARelatedContainersGenPivot FDARelatedContainersGenPivot
		{
			get { return fdaRelatedContainersGenPivot ?? (fdaRelatedContainersGenPivot = Factory.New<FDARelatedContainersGenPivot>()); }
		}
		FDARelatedContainersGenPivot fdaRelatedContainersGenPivot;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					JobDeclaration declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableCRL = true;
					JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
					invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				}
				return invoiceLine;
			}
		}
		JobComInvoiceLine invoiceLine;

		FDA FDA
		{
			get { return fda ?? (fda = InvoiceLine.FDAs.AddNew()); }
		}
		FDA fda;

		void AddContainerForInvoiceLine(ZString containerNumber, string usContainerCode)
		{
			CusContainer container = InvoiceLine.Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = containerNumber;
			RefContainer refContainer = Factory.New<RefContainer>();
			refContainer.SetCountrySpecificContainerCode(usContainerCode, Enterprise.Core.Constants.CountryCodes.UnitedStates);
			container.CO_RC = refContainer.PK;
			InvoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(containerNumber).IsForInvoiceLine = true;
		}
	}
}
