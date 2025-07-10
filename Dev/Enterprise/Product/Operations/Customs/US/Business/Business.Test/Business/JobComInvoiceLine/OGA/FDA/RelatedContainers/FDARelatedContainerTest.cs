using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FDARelatedContainer))]
	public class FDARelatedContainerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetContainerInvoiceLinePivotAndGenPivot()
		{
			FDARelatedContainer relatedContainer = SetRelatedContainer("20");
			AssertEquals(false, relatedContainer.IsForFDALine);
			AssertEquals("MAEUVVVVV", relatedContainer.ContainerNumber);
			AssertNull(relatedContainer.Pivot);

			relatedContainer.IsForFDALine = true;
			AssertNotNull(relatedContainer.Pivot);

			relatedContainer.SetContainerInvoiceLinePivot(null);
			AssertEquals(ZString.Empty, relatedContainer.ContainerNumber);

			var containerInvoiceLinePivot = Factory.New<CusContainerInvoiceLinePivot>();
			containerInvoiceLinePivot.C2_CO = ZGuid.Empty;
			relatedContainer.SetContainerInvoiceLinePivot(containerInvoiceLinePivot);
			AssertEquals(ZString.Empty, relatedContainer.ContainerNumber);
		}

		public void TestNotAddedToFactoryCache()
		{
			var relatedContainer = SetRelatedContainer("40");
			AssertEquals("Data should not be cached in the factory", 0, Factory.GetBizOsForPK(relatedContainer.PK.ToGuid()).Length);
		}

		public void TestIsForFDALine()
		{
			FDARelatedContainer relatedContainer = SetRelatedContainer("20");

			FDA.ContainersForInvoiceLine.Add(relatedContainer);
			relatedContainer.IsForFDALine = true;
			AssertEquals(1, FDA.ContainersForFDALine.Count);

			relatedContainer.IsForFDALine = false;
			AssertEquals(0, FDA.ContainersForFDALine.Count);
		}

		public void TestValidateIsForFDALine()
		{
			CusContainer container = Factory.New<CusContainer>();
			container.CO_ContainerNumber = "MAEUVVVVV";

			CusContainerInvoiceLinePivot containerInvoiceLinePivot = Factory.New<CusContainerInvoiceLinePivot>();
			containerInvoiceLinePivot.C2_CO = container.PK;

			FDARelatedContainer relatedContainer = new FDARelatedContainer(FDA);
			relatedContainer.SetContainerInvoiceLinePivot(containerInvoiceLinePivot);
			FDA.ContainersForInvoiceLine.Add(relatedContainer);
			FDARelatedContainersGenPivot pivot1 = FDA.ContainersForFDALine.AddNew();
			pivot1.Relation2Object = containerInvoiceLinePivot;
			FDARelatedContainersGenPivot pivot2 = FDA.ContainersForFDALine.AddNew();
			pivot2.Relation2Object = containerInvoiceLinePivot;
			relatedContainer.IsForFDALine = true;
			AssertHasErrorContaining(relatedContainer.IsForFDALineInfo, "A possible fix for this is to untick and re-tick the related container.");
			AssertEquals(pivot1, relatedContainer.Pivot);

			relatedContainer.IsForFDALine = false;
			AssertNoErrorContaining(relatedContainer.IsForFDALineInfo, "A possible fix for this is to untick and re-tick the related container.");
			AssertEquals(pivot2, relatedContainer.Pivot);
			AssertEquals(true, pivot1.IsDeleted);

			relatedContainer.IsForFDALine = true;
			AssertNoErrorContaining(relatedContainer.IsForFDALineInfo, "A possible fix for this is to untick and re-tick the related container.");
			AssertEquals(pivot2, relatedContainer.Pivot);

			relatedContainer.IsForFDALine = false;
			AssertNoErrorContaining(relatedContainer.IsForFDALineInfo, "A possible fix for this is to untick and re-tick the related container.");
			AssertNull(relatedContainer.Pivot);
			AssertEquals(true, pivot2.IsDeleted);
		}

		public void TestContainerEquipmentID()
		{
			FDARelatedContainer relatedContainer = SetRelatedContainer("40");
			IUSContainer iUSContainer = relatedContainer;
			AssertEquals("MAEUVVVVV", iUSContainer.ContainerEquipmentID);
			AssertEquals("40", iUSContainer.USContainerCode);
		}

		public void TestContainerUSContainerCode()
		{
			FDARelatedContainer relatedContainer = SetRelatedContainer(USContainerCodeList.Codes.RR);
			IUSContainer iUSContainer = relatedContainer;
			AssertEquals("MAEUVVVVV", iUSContainer.ContainerEquipmentID);
			AssertEquals(USContainerCodeList.Codes.RR, iUSContainer.USContainerCode);
		}

		public void TestContainerIsRailCar()
		{
			FDARelatedContainer relatedContainer1 = SetRelatedContainer("40");
			IUSContainer iUSContainer = relatedContainer1;
			AssertEquals(false, iUSContainer.IsRailCar);

			FDARelatedContainer relatedContainer2 = SetRelatedContainer(USContainerCodeList.Codes.RR);
			iUSContainer = relatedContainer2;
			AssertEquals(true, iUSContainer.IsRailCar);
		}

		FDARelatedContainer SetRelatedContainer(string uSContainerCode)
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "MAEUVVVVV";

			RefContainer refContainer = Factory.New<RefContainer>();
			refContainer.SetCountrySpecificContainerCode(uSContainerCode, Enterprise.Core.Constants.CountryCodes.UnitedStates);
			container.CO_RC = refContainer.PK;

			CusContainerInvoiceLinePivot containerInvoiceLinePivot = Factory.New<CusContainerInvoiceLinePivot>();
			containerInvoiceLinePivot.C2_CO = container.PK;

			FDARelatedContainer relatedContainer = new FDARelatedContainer(FDA);
			relatedContainer.SetContainerInvoiceLinePivot(containerInvoiceLinePivot);

			return relatedContainer;
		}

		FDA FDA
		{
			get
			{
				if (fda == null)
				{
					fda = InvoiceLine.FDAs.AddNew();
				}
				return fda;
			}
		}
		FDA fda;

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

		protected override BusinessObject GetNewBusinessObject()
		{
			return new FDARelatedContainer(Factory.New<FDA>());
		}
	}
}
