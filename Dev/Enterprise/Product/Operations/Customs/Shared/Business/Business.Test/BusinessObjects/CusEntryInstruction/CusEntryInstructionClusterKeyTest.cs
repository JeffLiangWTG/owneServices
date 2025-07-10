using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusEntryInstruction))]
	class CusEntryInstructionClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
		{
			var entryInstruction = (CusEntryInstruction)ClusterKeyEntityToTest;
			Factory.Save();

			var authorizationUsagePk = InsertAuthorizationUsage(entryInstruction);
			var authorizationUsage = (IClusterKeyWorker)Factory.Load<Integration.Customs.EU.ICusAuthorizationUsage>(authorizationUsagePk);

			return new IClusterKeyWorker[] { authorizationUsage };
		}

		protected override IEnumerable<SchemaGuidColumn> ExemptedCandidateChildrenRepresentedByActualParentClusterKeyFk => actualParentClusterKeyFksOfCandidateChildren;

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var dec = (BaseJobDeclaration)NewParentObject();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = dec.PK;
			return entryInstruction;
		}

		protected override EnterpriseBusinessObject NewParentObject() => Factory.New<BaseJobDeclaration>();

		Guid InsertAuthorizationUsage(CusEntryInstruction entryInstruction)
		{
			var authorizationUsagePk = Guid.NewGuid();

			var sql = $@"
				INSERT {CusAuthorizationUsageSchema.Constants.SqlSchemaName}.{CusAuthorizationUsageSchema.Constants.TableName}
				(
					{CusAuthorizationUsageSchema.Constants.PK},
					{CusAuthorizationUsageSchema.Constants.AGC_ParentTableCode},
					{CusAuthorizationUsageSchema.Constants.AGC_ParentID},
					{CusAuthorizationUsageSchema.Constants.AGC_ClusterKey},
					{CusAuthorizationUsageSchema.Constants.AGC_OH_Owner},
					{CusAuthorizationUsageSchema.Constants.AGC_Code},
					{CusAuthorizationUsageSchema.Constants.AGC_Number}
				)
				VALUES
				(
					'{authorizationUsagePk}',
					'CEI',
					'{entryInstruction.PK}',
					{entryInstruction.CEI_ClusterKey},
					'{GlbCompany.CurrentCompany.GC_OH_OrgProxy}',
					'C~01',
					'N~01'
				)";

			TestConnection.ExecuteNonQuery(sql);

			return authorizationUsagePk;
		}

		readonly SchemaGuidColumn[] actualParentClusterKeyFksOfCandidateChildren = new SchemaGuidColumn[]
		{
			CusEntryHeaderSchema.CH_JE,
			JobComInvoiceLineSchema.JI_JZ,
		};
	}
}
