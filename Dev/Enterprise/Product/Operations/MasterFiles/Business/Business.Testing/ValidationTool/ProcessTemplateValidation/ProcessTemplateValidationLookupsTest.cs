using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ProcessTemplateValidationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSeverityList()
		{
			AssertType<ProcessTemplateValidationSeverityList>(Lookups.SeverityList);
		}

		public void TestCondition1List()
		{
			ProcessTaskTemplate.P0_ProcessType = "SHP";
			AssertEquals(", BRK", Lookups.Condition1List.CodesAsString);

			ProcessTaskTemplate.P0_ProcessType = "CRD";
			AssertEquals("IMP", Lookups.Condition1List.CodesAsString);
		}

		public void TestCondition2List()
		{
			ProcessTaskTemplate.P0_ProcessType = "SHP";
			var list = Lookups.Condition2List;
			var listCodes = Lookups.Condition2List.CodesAsString;
			AssertEquals("IMP, EXP, DOM, , LCL, FCL, GRP, LSE, ULD, BBK, BLK, LQD, ROR, CON, LTL, FTL, OBC, UNA, , BCM, BCS, CLM, CLS, ASM, ASS, REL, , MCR", Lookups.Condition2List.CodesAsString);

			ProcessTaskTemplate.P0_ProcessType = "CRD";
			AssertEquals("MCR", Lookups.Condition2List.CodesAsString);
		}

		public void TestContextTypeList()
		{
			AssertArrayEqualsByElements(new string[] { ProcessTemplateValidationContextType.Codes.CargoWise, ProcessTemplateValidationContextType.Codes.GlowPortal }, Lookups.ContextTypeList.OfType<ICodeDescription>().Select(el => el.Code).ToArray());
			AssertArrayEqualsByElements(new string[] { ProcessTemplateValidationContextType.Descriptions.CargoWise, ProcessTemplateValidationContextType.Descriptions.GlowPortal }, Lookups.ContextTypeList.OfType<ICodeDescription>().Select(el => el.Description).ToArray());
		}

		public void TestRequestTypesOnFailure()
		{
			ProcessTaskTemplate.P0_ProcessType = "SBK";
			AssertEquals("(\r\n\tRQT_JobType in \r\n\t(\r\n\t\t'ALL', 'SBK'\r\n\t)\r\n)\r\nAND\r\nRQT_IsActive = 1\r\n", Lookups.RequestTypesOnFailure.CompleteFilter.LiteralTextSqlFormatted);
		}

		ProcessTaskTemplate ProcessTaskTemplate => processTaskTemplate ??= Factory.NewWithValidTestData<ProcessTaskTemplate>();
		ProcessTaskTemplate processTaskTemplate;

		ProcessTemplateValidation TemplateValidation
		{
			get
			{
				if (templateValidation == null)
				{
					templateValidation = Factory.New<ProcessTemplateValidation>();
					templateValidation.P0V_P0_WorkflowTemplate = ProcessTaskTemplate.PK;
				}
				return templateValidation;
			}
		}
		ProcessTemplateValidation templateValidation;

		ProcessTemplateValidationLookups Lookups => lookups ??= TemplateValidation.Lookups;
		ProcessTemplateValidationLookups lookups;
	}
}
