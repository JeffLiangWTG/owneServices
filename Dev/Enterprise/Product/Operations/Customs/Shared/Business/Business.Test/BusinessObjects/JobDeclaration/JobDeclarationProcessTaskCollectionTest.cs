using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobDeclarationProcessTaskCollection<BaseJobDeclaration>))]
	sealed class JobDeclarationProcessTaskCollectionTest : ProcessTaskCollectionTest<JobDeclarationProcessTaskCollection<BaseJobDeclaration>>
	{
		#region OriginCountry / DestinationCountry

		public void TestOriginCountry()
		{
			Declaration.JE_RL_NKOrigin = "MYPKG";
			AssertEquals("MY", Collection.OriginCountry);
		}

		public void TestDestinationCountry()
		{
			Declaration.JE_RL_NKFinalDestination = "MYPKG";
			AssertEquals("MY", Collection.DestinationCountry);
		}

		#endregion

		#region IsConditionMet

		public void TestIsCondition1or2Met_ForImport()
		{
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals(false, Declaration.IsImport);
			AssertEquals(false, Collection.IsCondition1Met(JobDeclarationWorkflowCondition1CodeList.Codes.Import));
			AssertEquals(false, Collection.IsCondition2Met(JobDeclarationWorkflowCondition2CodeList.Codes.Import, ""));

			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals(true, Declaration.IsImport);
			AssertEquals(true, Collection.IsCondition1Met(JobDeclarationWorkflowCondition1CodeList.Codes.Import));
			AssertEquals(true, Collection.IsCondition2Met(JobDeclarationWorkflowCondition2CodeList.Codes.Import, ""));
		}

		public void TestIsCondition1or2Met_ForExport()
		{
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals(false, Declaration.IsExport);
			AssertEquals(false, Collection.IsCondition1Met(JobDeclarationWorkflowCondition1CodeList.Codes.Export));
			AssertEquals(false, Collection.IsCondition2Met(JobDeclarationWorkflowCondition2CodeList.Codes.Export, ""));

			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals(true, Declaration.IsExport);
			AssertEquals(true, Collection.IsCondition1Met(JobDeclarationWorkflowCondition1CodeList.Codes.Export));
			AssertEquals(true, Collection.IsCondition2Met(JobDeclarationWorkflowCondition2CodeList.Codes.Export, ""));
		}

		public void TestIsCondition1or2Met_ForNotExport()
		{
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals(false, Collection.IsCondition1Met(JobDeclarationWorkflowCondition1CodeList.Codes.NotExport));
			AssertEquals(false, Collection.IsCondition2Met(JobDeclarationWorkflowCondition2CodeList.Codes.NotExport, ""));

			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals(true, Collection.IsCondition1Met(JobDeclarationWorkflowCondition1CodeList.Codes.NotExport));
			AssertEquals(true, Collection.IsCondition2Met(JobDeclarationWorkflowCondition2CodeList.Codes.NotExport, ""));

			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals(true, Collection.IsCondition1Met(JobDeclarationWorkflowCondition1CodeList.Codes.NotExport));
			AssertEquals(true, Collection.IsCondition2Met(JobDeclarationWorkflowCondition2CodeList.Codes.NotExport, ""));
		}

		public void TestIsCondition1or2Met_ForImportOnly()
		{
			string code = "IMO";
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals(true, Collection.IsCondition1Met(code));
			AssertEquals(true, Collection.IsCondition2Met(code, ""));

			Declaration.JE_MessageType = "LVS";
			AssertEquals(false, Collection.IsCondition1Met(code));
			AssertEquals(false, Collection.IsCondition2Met(code, ""));

			Declaration.JE_MessageType = "LVX";
			AssertEquals(false, Collection.IsCondition1Met(code));
			AssertEquals(false, Collection.IsCondition2Met(code, ""));
		}

		#endregion

		#region Implementation

		new JobDeclarationProcessTaskCollection<BaseJobDeclaration> Collection
		{
			get { return base.Collection; }
		}

		protected override JobDeclarationProcessTaskCollection<BaseJobDeclaration> GetCollectionToTestCore()
		{
			return new JobDeclarationProcessTaskCollection<BaseJobDeclaration>(Declaration);
		}

		BaseJobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				}
				return declaration;
			}
		}
		BaseJobDeclaration declaration;

		#endregion
	}
}
