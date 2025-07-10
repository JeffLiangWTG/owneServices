using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CommonJobComInvoiceHeader))]
	sealed class CommonJobComInvoiceHeaderWorkerClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		public void TestSetClusterKeyOnExistingObject()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var invLine = Factory.New<BaseJobComInvoiceLine>();
			invLine.JI_JZ = invoice.PK;
			var invLineRef = Factory.New<JobComInvLineRefs>();
			invLineRef.JG_JI = invLine.PK;

			CombineAssertions("[PRE-CONDITION] Initial Values", () =>
			{
				AssertEquals("ClusterKey", 0, invoice.JZ_ClusterKey);
				AssertEquals("Child ClusterKey", 0, invLine.JI_ClusterKey);
				AssertEquals("Grand-Child ClusterKey", 0, invLineRef.JG_ClusterKey);
			});

			Factory.Save();

			CombineAssertions("After 1st Save", () =>
			{
				AssertEquals("ClusterKey", 1, invoice.JZ_ClusterKey);
				AssertEquals("Child ClusterKey", 1, invLine.JI_ClusterKey);
				AssertEquals("Grand-Child ClusterKey", 1, invLineRef.JG_ClusterKey);
			});

			var factory2 = new BusinessObjectFactory(TestConnection);
			var invLine2 = factory2.New<BaseJobComInvoiceLine>();
			invLine2.JI_JZ = invoice.PK;
			var invLineRef2 = factory2.New<JobComInvLineRefs>();
			invLineRef2.JG_JI = invLine.PK;
			factory2.Save();

			CombineAssertions("After Factory2 Save", () =>
			{
				AssertEquals("ClusterKey", 1, invoice.JZ_ClusterKey);
				AssertEquals("Factory2 Child ClusterKey", 1, invLine2.JI_ClusterKey);
				AssertEquals("Factory2 Grand-Child ClusterKey", 1, invLineRef2.JG_ClusterKey);
			});

			var dec1 = Factory.New<BaseJobDeclaration>();
			invoice.JZ_JE = dec1.PK;
			Factory.Save();
			invLine2.Reload();
			invLineRef2.Reload();

			CombineAssertions("After setting FK to Cluster Parent (attaching).", () =>
			{
				AssertEquals("Parent ClusterKey", 2, dec1.JE_ClusterKey);
				AssertEquals("ClusterKey", 2, invoice.JZ_ClusterKey);
				AssertEquals("Child ClusterKey", 2, invLine.JI_ClusterKey);
				AssertEquals("Grand-Child ClusterKey", 2, invLineRef.JG_ClusterKey);

				AssertEquals("Factory2 Child ClusterKey", 2, invLine2.JI_ClusterKey);
				AssertEquals("Factory2 Grand-Child ClusterKey", 2, invLineRef2.JG_ClusterKey);
			});

			var dec2 = Factory.New<BaseJobDeclaration>();
			invoice.JZ_JE = dec2.PK;
			Factory.Save();

			CombineAssertions("After setting FK to another Cluster Parent (changing attachment).", () =>
			{
				AssertEquals("Dec1 (no longer the parent) ClusterKey", 2, dec1.JE_ClusterKey);

				AssertEquals("Parent ClusterKey", 3, dec2.JE_ClusterKey);
				AssertEquals("ClusterKey", 3, invoice.JZ_ClusterKey);
				AssertEquals("Child ClusterKey", 3, invLine.JI_ClusterKey);
				AssertEquals("Grand-Child ClusterKey", 3, invLineRef.JG_ClusterKey);

				AssertEquals("Factory2 Child ClusterKey", 3, invLine2.JI_ClusterKey);
				AssertEquals("Factory2 Grand-Child ClusterKey", 3, invLineRef2.JG_ClusterKey);
			});

			invoice.JZ_JE = ZGuid.Empty;
			Factory.Save();

			CombineAssertions("After setting FK to empty (detaching).", () =>
			{
				AssertEquals("Dec1 (no longer the parent) ClusterKey", 2, dec1.JE_ClusterKey);
				AssertEquals("Dec2 (no longer the parent) ClusterKey", 3, dec2.JE_ClusterKey);

				AssertEquals("ClusterKey", 4, invoice.JZ_ClusterKey);
				AssertEquals("Child ClusterKey", 4, invLine.JI_ClusterKey);
				AssertEquals("Grand-Child ClusterKey", 4, invLineRef.JG_ClusterKey);

				AssertEquals("Factory2 Child ClusterKey", 4, invLine2.JI_ClusterKey);
				AssertEquals("Factory2 Grand-Child ClusterKey", 4, invLineRef2.JG_ClusterKey);
			});
		}

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
		{
			var invHeaderRef = Factory.New<JobComInvoiceHeaderRefs>();
			invHeaderRef.J2_JZ = ClusterKeyEntityToTest.PK;
			var invLine = Factory.New<BaseJobComInvoiceLine>();
			invLine.JI_JZ = ClusterKeyEntityToTest.PK;
			var packingList = Factory.New<CusPackingList>();
			packingList.CUL_JZ = ClusterKeyEntityToTest.PK;
			Factory.Save();

			var quarantineHeaderPk = InsertQuarantineHeader(ClusterKeyEntityToTest);
			var quarantineHeader = (IClusterKeyWorker)Factory.Load<Integration.Customs.AU.IQuarantineExdocHeader>(quarantineHeaderPk);

			return new IClusterKeyWorker[] { invHeaderRef, invLine, packingList, quarantineHeader };
		}

		Guid InsertQuarantineHeader(CommonJobComInvoiceHeader invoice)
		{
			var quarantineHeaderPk = Guid.NewGuid();

			var sql = $@"
				INSERT {QuarantineExDocHeaderSchema.Constants.SqlSchemaName}.{QuarantineExDocHeaderSchema.Constants.TableName}
				(
					{QuarantineExDocHeaderSchema.Constants.PK},
					{QuarantineExDocHeaderSchema.Constants.QH_JZ},
					{QuarantineExDocHeaderSchema.Constants.QH_ClusterKey}
				)
				VALUES
				(
					'{quarantineHeaderPk}',
					'{invoice.PK}',
					{invoice.JZ_ClusterKey}
				)";

			TestConnection.ExecuteNonQuery(sql);
			return quarantineHeaderPk;
		}

		protected override IEnumerable<SchemaGuidColumn> ExemptedCandidateChildrenRepresentedByActualParentClusterKeyFk => actualParentClusterKeyFksOfCandidateChildren;

		readonly SchemaGuidColumn[] actualParentClusterKeyFksOfCandidateChildren = new SchemaGuidColumn[]
		{
			CusHouseContPackInvoiceHeaderPivotSchema.CHZ_JE,
		};

		protected override void UpdateClusterKeyAndSetParentFkToDefault(EnterpriseBusinessObject bizObj, int clusterKeyValue)
		{
			using (DisableConstraint(JobComInvoiceHeaderSchema.Constants.TableName, "Constraint_JZ_JE_JZ_GB"))
			{
				base.UpdateClusterKeyAndSetParentFkToDefault(bizObj, clusterKeyValue);
			}
		}

		static IDisposable DisableConstraint(string tableName, string constraintName, DbConnection connection = null)
		{
			var dbConn = connection ?? Db.Connection;
			return new DisposableAction(
				() => dbConn.ExecuteNonQuery($"IF OBJECT_ID('{constraintName}', 'C') IS NOT NULL ALTER TABLE {tableName} NOCHECK CONSTRAINT[{constraintName}]"),
				() => dbConn.ExecuteNonQuery($"IF OBJECT_ID('{constraintName}', 'C') IS NOT NULL ALTER TABLE {tableName} CHECK CONSTRAINT[{constraintName}]"));
		}

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var dec = (BaseJobDeclaration)NewParentObject();
			return dec.Invoices.AddNew();
		}

		protected override EnterpriseBusinessObject NewParentObject() => Factory.New<BaseJobDeclaration>();

		new CommonJobComInvoiceHeader ClusterKeyEntityToTest => (CommonJobComInvoiceHeader)base.ClusterKeyEntityToTest;
	}
}
