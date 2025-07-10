using System;
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
	[TestedType(typeof(BaseJobComInvoiceLine))]
	class BaseJobComInvoiceLineClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
		{
			var invLineRef = ClusterKeyEntityToTest.InvoiceLineRefs.AddNew();
			var invLineTax = Factory.New<JobComInvoiceLineTax>();
			invLineTax.JLT_JI = ClusterKeyEntityToTest.PK;
			invLineTax.JLT_Type = "X";
			Factory.Save();

			var quarantineLinePk = InsertQuarantineLine(ClusterKeyEntityToTest);
			var quarantineLine = (IClusterKeyWorker)Factory.Load<Integration.Customs.AU.IQuarantineExdocLine>(quarantineLinePk);

			return new IClusterKeyWorker[] { invLineRef, invLineTax, quarantineLine };
		}

		Guid InsertQuarantineLine(BaseJobComInvoiceLine invLine)
		{
			var quarantineLinePk = Guid.NewGuid();

			var sql = $@"
				INSERT {QuarantineExDocLineSchema.Constants.TableName}
				(
					{QuarantineExDocLineSchema.Constants.PK},
					{QuarantineExDocLineSchema.Constants.QL_JI},
					{QuarantineExDocLineSchema.Constants.QL_ClusterKey}
				)
				VALUES
				(
					'{quarantineLinePk}',
					'{invLine.PK}',
					{invLine.JI_ClusterKey}
				)";

			TestConnection.ExecuteNonQuery(sql);
			return quarantineLinePk;
		}

		protected override IEnumerable<SchemaGuidColumn> ExemptedCandidateChildrenRepresentedByActualParentClusterKeyFk => actualParentClusterKeyFksOfCandidateChildren;

		readonly SchemaGuidColumn[] actualParentClusterKeyFksOfCandidateChildren = new SchemaGuidColumn[]
		{
			CusPackableItemSchema.CUI_CUL,
			CusUnderbondDecSchema.BU_CL,
			CusContainerInvoiceLinePivotSchema.C2_CO,
			CusHouseContPackInvoiceLinePivotSchema.CHC_JE,
		};

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var invoice = (BaseJobComInvoiceHeader)NewParentObject();
			return invoice.InvoiceLines.AddNew();
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			return dec.Invoices.AddNew();
		}

		new BaseJobComInvoiceLine ClusterKeyEntityToTest => (BaseJobComInvoiceLine)base.ClusterKeyEntityToTest;
	}
}
