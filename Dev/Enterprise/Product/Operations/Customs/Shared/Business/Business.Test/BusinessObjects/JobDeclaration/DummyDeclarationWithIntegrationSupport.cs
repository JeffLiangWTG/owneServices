using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	public class DummyDeclarationWithIntegrationSupport : BaseJobDeclaration
	{
		public DummyDeclarationWithIntegrationSupport(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			OrgHeader importer = OrgHeader.New(factory);
			importer.FillWithValidTestData();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			JE_OH_Importer = importer.PK;
		}

		public BaseJobComInvoiceLine CreateBondedLine()
		{
			var mockLine = Factory.NewMoq<BaseJobComInvoiceLine>();
			mockLine.Protected().Setup<bool>("SupportsBondedWarehousingCore").Returns(() =>
			{
				return SupportsBondedWarehousing;
			});

			var line = mockLine.Object;
			FilteredInvoiceLines.Add(line);
			var header = JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header.JobComInvoiceLines.Add(line);
			line.JI_JZ = header.PK;
			line.SetDeclarationForTesting(this);
			return line;
		}

		public bool SupportsBondedWarehousingCoreExposed = true;
		protected internal override bool SupportsBondedWarehousingCore
		{
			get { return SupportsBondedWarehousingCoreExposed; }
		}

		protected override JobDeclarationIAccIntegrationDataProvider GetJobDeclarationIAccIntegrationDataProvider()
		{
			return new AccountingIntegrationTest.TestJobDeclarationIAccIntegrationDataProvider(this);
		}

		public bool SupportMultipleWarehouseEntryCoreExposed;
		protected internal override bool SupportMultipleWarehouseEntryCore
		{
			get { return SupportMultipleWarehouseEntryCoreExposed; }
		}

		protected internal override bool IsInvoiceQuantityRequiredForBondedWarehouse
		{
			get { return true; }
		}

		protected internal override bool IsBondedWhsQuantityRequiredForBondedWarehouse
		{
			get { return false; }
		}
	}
}
