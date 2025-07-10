using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;
using static Enterprise.Customs.ZA.Business.JobDeclaration;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(ZADeclarationDocManagerInfo))]
	sealed class JobDeclarationDocManagerInfoTest : DeclarationDocManagerInfoTest
	{
		public void TestRelatedObjectExcludeInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PreviousEntryNumber = "JHB201707051234567";

			var previousDeclaration = Factory.New<JobDeclaration>();
			var entry = previousDeclaration.CustomsEntryHeaders.AddNew();
			var entrynum = Factory.New<CusEntryNumber>();
			entrynum.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			entrynum.CE_EntryNum = "JHB201707051234567";
			entrynum.CE_ParentID = previousDeclaration.PK;
			entrynum.CE_ParentTable = previousDeclaration.TableName;
			Factory.Save();

			var list = declaration.DocManagerInfo.RelatedObjects;
			AssertEquals(1, list.Length);
			Assert(list.Contains(invoice));
			Assert(!list.Contains(previousDeclaration));
		}

		public override BusinessObject GetEmptyParentBusinessObject() => Factory.New(typeof(JobDeclaration));

		protected override BaseJobDeclaration GetJobDeclaration() => new RelatedDeclaration(Factory).GetRelatedImportDeclaration();
	}
}
