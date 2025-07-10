using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(OMCHeader))]
	internal class USDeclarationOMCTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<OMCHeader>
	{
		public void TestCloneInNewFactory()
		{
			var originalBO = Factory.New<OMCHeader>();
			originalBO.AquacultureFacilities.AddNew();

			var newBO = (OMCHeader)originalBO.Clone();

			AssertEquals(1, newBO.AquacultureFacilities.Count);

			var fac = new BusinessObjectFactory();
			var newFacClone = (OMCHeader)originalBO.Clone(new BusinessObjectCloneArgs(fac, System.Array.Empty<string>(), typeof(OMCHeader), false));
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.AquacultureFacilities[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.AquacultureFacilities[0].Factory.GetHashCode());
		}

		protected override IEnumerable<OMCHeader> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (OMCHeader)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT1";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			var header = invoiceLine.OMCHeaders.AddNew();
			header.US_LineNo = 1;
			return header;
		}
	}
}
