using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USScientificDataAddInfo))]
	public class USScientificDataAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		JobComInvoiceHeader Invoice
		{
			get { return invoice ?? (invoice = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoice;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Invoice.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		PGA LaceyActLine
		{
			get { return laceyActLine ?? (laceyActLine = InvoiceLine.LaceyActLines.AddNew()); }
		}
		PGA laceyActLine;

		ConstituentElement ConstituentElement
		{
			get { return constituentElement ?? (constituentElement = LaceyActLine.PG04ConstituentElements.AddNew()); }
		}
		ConstituentElement constituentElement;

		ScientificData ScientificData
		{
			get { return scientificData ?? (scientificData = ConstituentElement.ScientificDataCollection.AddNew()); }
		}
		ScientificData scientificData;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new USScientificDataAddInfo(ScientificData.B7_AddInfoDataInfo);
		}
	}
}
