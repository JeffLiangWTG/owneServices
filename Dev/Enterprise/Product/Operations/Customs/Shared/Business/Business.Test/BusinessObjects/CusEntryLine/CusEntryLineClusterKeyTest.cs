using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusEntryLine))]
	class CusEntryLineClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
		{
			var entryLine = (CusEntryLine)ClusterKeyEntityToTest;
			var lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeAmount = 1;
			var invoice = entryLine.Declaration.Invoices.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();
			var underbondDec = Factory.New<AdditionalInvoiceLineEntryLineLink>();
			underbondDec.BU_CL = entryLine.PK;
			underbondDec.BU_JI = invLine.PK;
			return new IClusterKeyWorker[] { lineFee, underbondDec };
		}

		protected override IEnumerable<SchemaGuidColumn> ExemptedCandidateChildrenRepresentedByActualParentClusterKeyFk => actualParentClusterKeyFksOfCandidateChildren;

		readonly SchemaGuidColumn[] actualParentClusterKeyFksOfCandidateChildren = new SchemaGuidColumn[]
		{
			JobComInvoiceLineSchema.JI_JZ,
			QuarantineColsDirectionSchema.QCD_QCH_ColsHeader,
		};

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var entryHeader = (CusEntryHeader)NewParentObject();
			return entryHeader.AllEntryLines.AddNew();
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_JE = dec.PK;
			return entryHeader;
		}
	}
}
