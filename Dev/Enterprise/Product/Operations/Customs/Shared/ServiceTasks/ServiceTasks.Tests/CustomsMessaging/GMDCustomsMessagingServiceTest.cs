using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

[assembly: HostedService("#@2",
	"#@2 Testing",
	"TST",
	typeof(Enterprise.Customs.ServiceTasks.Testing.GMDCustomsMessagingServiceTestHelper),
	CanRunInAnyBranch = true,
	MinimumPeriod = "1Minute",
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding("#@2",
	EDIInterchangeSchema.Constants.TableName,
	new[] {
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.GenericMessageDelivery,
		EDIInterchangeSchema.Constants.EI_InterchangeType + "=#$1"
	},
	"TEST NUDGING"
	)]
namespace Enterprise.Customs.ServiceTasks.Testing
{
	[TestsSubclassesOf(typeof(GMDCustomsMessagingService))]
	public abstract class GMDCustomsMessagingServiceTest<T> : ServiceTaskTestCase<T> where T : GMDCustomsMessagingService
	{
		public void TestEndToEnd()
		{
			var testData = SetupDataForTesting();
			Factory.Save();
			ErrorReporter.Clear();
			var serviceTask = CreateServiceTask();
			var logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;
			serviceTask.RunTask();
			AssertResult(new BusinessObjectFactory(), testData, serviceTask);
		}

		protected virtual void AssertResult(BusinessObjectFactory factory, GMDCustomsMessagingServiceTestHelperData testData, T serviceTask)
		{
			var interchange = factory.Load<EDIInterchange>(testData.InterchangePK);
			AssertEquals("EI_Status", EDIInterchange.Status.Received, interchange.EI_Status);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"TEST NUDGING",
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.GenericMessageDelivery,
						EDIInterchangeSchema.Constants.EI_InterchangeType + "=#$1"),
				};
			}
		}

		protected abstract T CreateServiceTask();

		protected abstract GMDCustomsMessagingServiceTestHelperData SetupDataForTesting();
	}

	public class GMDCustomsMessagingServiceTestHelperData
	{
		public ZGuid InterchangePK;
	}

	[TestedType(typeof(GMDCustomsMessagingServiceTestHelper))]
	class GMDCustomsMessagingServiceTest : GMDCustomsMessagingServiceTest<GMDCustomsMessagingServiceTestHelper>
	{
		protected override GMDCustomsMessagingServiceTestHelperData SetupDataForTesting()
		{
			var nzCompany = Factory.New<GlbCompany>();
			nzCompany.FillWithValidTestData();
			nzCompany.GC_Code = "NZ1";
			nzCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var nzBranch = nzCompany.Branches.AddNew();
			nzBranch.GB_RL_NKHomePort = "NZAKL";
			nzBranch.GB_Code = "NZ1";
			var auCompany = Factory.New<GlbCompany>();
			auCompany.FillWithValidTestData();
			auCompany.GC_Code = "AU2";
			auCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var auBranch1 = auCompany.Branches.AddNew();
			auBranch1.GB_RL_NKHomePort = "AUSYD";
			auBranch1.GB_Code = "AU1";
			var auBranch2 = auCompany.Branches.AddNew();
			auBranch2.GB_RL_NKHomePort = "AUSYD";
			auBranch2.GB_Code = "AU2";
			var auBranch3 = auCompany.Branches.AddNew();
			auBranch3.GB_RL_NKHomePort = "AUSYD";
			auBranch3.GB_Code = "AU3";
			var interchange1 = CreateInterchange(GMDCustomsMessagingServiceTestHelper.InterchangeTypeForTesting1, auBranch1.PK);
			var interchange2 = CreateInterchange(GMDCustomsMessagingServiceTestHelper.InterchangeTypeForTesting2, auBranch3.PK);
			var interchange3 = CreateInterchange("K#$", nzBranch.PK);
			return new TestData() { InterchangePK = interchange1.PK, Interchange2PK = interchange2.PK, Interchange3PK = interchange3.PK };
		}

		class TestData : GMDCustomsMessagingServiceTestHelperData
		{
			public ZGuid Interchange2PK;
			public ZGuid Interchange3PK;
		}

		protected override void AssertResult(BusinessObjectFactory factory, GMDCustomsMessagingServiceTestHelperData testDataBase, GMDCustomsMessagingServiceTestHelper serviceTask)
		{
			var testData = (TestData)testDataBase;
			var auBranch1 = factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "AU1");
			var auBranch3 = factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "AU3");
			AssertContainsExactElementsInAnyOrder(new[] { auBranch1.PK, auBranch3.PK }, serviceTask.ProcessedBranchPKs.ToArray());
			AssertInterchange(factory.Load<EDIInterchange>(testData.InterchangePK), EDIInterchange.Status.Received, "TEST PROCESSED");
			AssertInterchange(factory.Load<EDIInterchange>(testData.Interchange2PK), EDIInterchange.Status.Received, "TEST PROCESSED");
			AssertInterchange(factory.Load<EDIInterchange>(testData.Interchange3PK), EDIInterchange.Status.Queued, "");
		}

		protected override GMDCustomsMessagingServiceTestHelper CreateServiceTask() => new GMDCustomsMessagingServiceTestHelper();

		void AssertInterchange(EDIInterchange interchange, ZString status, ZString headerText)
		{
			AssertEquals("EI_Status", status, interchange.EI_Status);
			AssertEquals("EI_HeaderText", headerText, interchange.EI_HeaderText);
		}

		EDIInterchange CreateInterchange(ZString interchangeType, ZGuid branchPK)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.GenericMessageDelivery;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_From = "ABC";
			interchange.EI_To = "DEF";
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_GB = branchPK;
			return interchange;
		}

		public void TestEnsureServiceTaskProcessingGMDAreNotDuplicated()
		{
			var interchangeTypeRegex = new Regex(@"^EI_InterchangeType=(.*)$", RegexOptions.IgnoreCase);
			var dictionary = new Dictionary<string, List<HostedServiceBusinessObjectBindingAttribute>>();
			foreach (var attribute in AssemblyMetaDataReader.GetAttributes<HostedServiceBusinessObjectBindingAttribute>()
				.Where(x => x.Table == EDIInterchangeSchema.Constants.TableName))
			{
				var gmdMatchingPredicate = EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchangeTypeList.Codes.GenericMessageDelivery;
				var foundGMD = false;
				var interchangeType = "";
				foreach (var predicate in attribute.Predicates.Select(x => x.Replace(" ", "").Replace("\t", "")))
				{
					if (gmdMatchingPredicate.Equals(predicate, StringComparison.OrdinalIgnoreCase))
					{
						foundGMD = true;
					}
					else
					{
						var interchangeTypeMatch = interchangeTypeRegex.Match(predicate);
						if (interchangeTypeMatch.Success)
						{
							interchangeType = interchangeTypeMatch.Groups[1].Value;
						}
					}
				}
				if (foundGMD)
				{
					var list = dictionary.GetOrAdd(interchangeType);
					list.Add(attribute);
				}
			}
			var allowedDuplicateInterchangeType = GetAllowedDuplicateInterchangeType();
			var duplicateFoundMessage = new ZStringBuilder();
			var duplicateAllowedButNotMatching = new ZStringBuilder();
			foreach (var pair in dictionary.OrderBy(x => x.Key))
			{
				if (pair.Value.Count > 1)
				{
					if (allowedDuplicateInterchangeType.TryGetValue(pair.Key, out var list))
					{
						foreach (var attribute in pair.Value.OrderBy(x => x.ServiceTaskCode))
						{
							var predicateData = string.Join(", ", attribute.Predicates);
							if (list.Contains(predicateData))
							{
								list.Remove(predicateData);
							}
							else
							{
								duplicateAllowedButNotMatching.Append($"Interchange Type:'{pair.Key}' Attribute: {attribute.ServiceTaskCode} {attribute.QueueName} ({string.Join(", ", attribute.Predicates)})");
							}
						}
						if (list.Count == 0)
						{
							allowedDuplicateInterchangeType.Remove(pair.Key);
						}
					}
					else
					{
						duplicateFoundMessage.Append($"Interchange Type '{pair.Key}':\r\n{GetAttributeDetails(pair.Value)}");
					}
				}
			}
			var failMessage = new ZStringBuilder();
			if (allowedDuplicateInterchangeType.Count > 0)
			{
				failMessage.Append($"The following Interchange Types allow duplication but expected predicates were not matched:\r\n{string.Join("\r\n", allowedDuplicateInterchangeType.Select(x => $"Interchange Type:'{x.Key} Predicates:'\r\n{string.Join("\r\n", x.Value)}"))}");
			}
			if (!duplicateFoundMessage.IsEmpty)
			{
				failMessage.Append($"The following Interchange Types are duplicated:\r\n{duplicateFoundMessage.ToStringWithNewLineBetweenAppends()}");
			}
			if (!duplicateAllowedButNotMatching.IsEmpty)
			{
				failMessage.Append($"The following Interchange Types allow duplication but unexpected predicates were found:\r\n{duplicateAllowedButNotMatching.ToStringWithNewLineBetweenAppends()}");
			}
			Assert(failMessage.ToStringWithDelimiterBetweenAppends("\r\n\r\n"), failMessage.IsEmpty);
		}

		string GetAttributeDetails(List<HostedServiceBusinessObjectBindingAttribute> attributes)
		{
			var result = new ZStringBuilder();
			foreach (var attribute in attributes.OrderBy(x => x.ServiceTaskCode))
			{
				result.Append($"{attribute.ServiceTaskCode} {attribute.QueueName} ({string.Join(", ", attribute.Predicates)})");
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		Dictionary<string, List<string>> GetAllowedDuplicateInterchangeType()
		{
			var result = new Dictionary<string, List<string>>();
			return result;
		}
	}

	class GMDCustomsMessagingServiceTestHelper : GMDCustomsMessagingService
	{
		public const string InterchangeTypeForTesting1 = "#$1";
		public const string InterchangeTypeForTesting2 = "#$2";
		protected override IEnumerable<ZString> InterchangeTypes => new ZString[] { InterchangeTypeForTesting1, InterchangeTypeForTesting2 };
		public GMDInboundInterchangeProcessor GMDInboundInterchangeProcessorForTesting;
		protected override GMDInboundInterchangeProcessor GetNewGMDInboundInterchangeProcessor()
		{
			GMDInboundInterchangeProcessorForTesting = new GMDInboundInterchangeProcessorTestHelper(InterchangeTypes);
			return GMDInboundInterchangeProcessorForTesting;
		}

		public List<ZGuid> ProcessedBranchPKs => processedBranchPKs ?? (processedBranchPKs = new List<ZGuid>());
		List<ZGuid> processedBranchPKs;

		protected override void Process(CancellationToken token)
		{
			ProcessedBranchPKs.Add(GlbBranch.CurrentBranch.PK);
			base.Process(token);
		}
	}
}
