using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PSCReasonCusCodeData))]
	sealed class PSCReasonCusCodeDataTest : Customs.Business.Testing.CusCodeDataTest<PSCReasonCusCodeData>
	{
		public void TestLoader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var headerPSCReason = Factory.New<PSCReasonCusCodeData>();
			headerPSCReason.CY_ParentID = entry.PK;
			headerPSCReason.CY_ParentTableCode = CusEntryHeaderSchema.Constants.Prefix;
			headerPSCReason.CY_Data = "Some reasons";
			var linePSCReason = Factory.New<PSCReasonCusCodeData>();
			linePSCReason.CY_ParentID = entryLine.PK;
			linePSCReason.CY_ParentTableCode = CusEntryLineSchema.Constants.Prefix;
			linePSCReason.CY_Data = "Some reasons";
			Factory.Save();
			var loadedHeaderPSCReason = new PSCReasonCusCodeData.Loader(Factory).Load(entry);
			AssertEquals("Same header PSC reason saved", headerPSCReason, loadedHeaderPSCReason);
			var loadedLinePSCReason = new PSCReasonCusCodeData.Loader(Factory).Load(entryLine);
			AssertEquals("Same header PSC reason saved", linePSCReason, loadedLinePSCReason);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var headerPSCReason = Factory.New<PSCReasonCusCodeData>();
			headerPSCReason.CY_ParentID = entry.PK;
			headerPSCReason.CY_ParentTableCode = CusEntryHeaderSchema.Constants.Prefix;
			headerPSCReason.CY_Data = "";
			Factory.Save();
			var loadedHeaderPSCReason = new PSCReasonCusCodeData.Loader(Factory).Load(entry);
			AssertNull("It is not saved because CY_Data is empty", loadedHeaderPSCReason);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var headerPSCReason = Factory.New<PSCReasonCusCodeData>();
			headerPSCReason.Parent = entry;
			headerPSCReason.CY_Data = "as";
			return headerPSCReason;
		}

		protected override IEnumerable<PSCReasonCusCodeData> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var headerPSCReason = factory.New<PSCReasonCusCodeData>();
			headerPSCReason.Parent = entry;
			headerPSCReason.CY_Data = "as";
			yield return headerPSCReason;
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var linePSCReason = factory.New<PSCReasonCusCodeData>();
			linePSCReason.Parent = entryLine;
			linePSCReason.CY_Data = "as";
			yield return linePSCReason;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = base.GetNewBusinessObjectForDeleteTest(factory) as PSCReasonCusCodeData;
			result.CY_Data = "D";
			return result;
		}
	}
}
