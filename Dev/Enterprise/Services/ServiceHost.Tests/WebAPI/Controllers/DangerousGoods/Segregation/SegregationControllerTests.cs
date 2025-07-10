using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Results;
using CargoWise.Definitions.Freight.DangerousGoods.SegregationApi;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation;
using Moq;
using Newtonsoft.Json;

namespace Enterprise.Services.ServiceHost.Tests.WebAPI.Controllers.DangerousGoods.Segregation
{
	class SegregationControllerTests : TestCaseWithFactory
	{
		Guid _id1;
		UNDGDataItemDTO _undgDataItemDto1;
		UNDGSubstance _undgSubstance1;
		Guid _id2;
		UNDGDataItemDTO _undgDataItemDto2;
		UNDGSubstance _undgSubstance2;

		UNDGSubstance _undgSubstance3;
		UNDGClassificationData _classificationData1;
		UNDGSubstance _undgSubstance5;
		UNDGClassificationData _classificationData2;

		protected override void SetUp()
		{
			base.SetUp();
			_id1 = Guid.NewGuid();
			_classificationData1 = TestHelper.RegulatedQuantity();
			_undgSubstance1 = Factory.NewWithValidTestData<UNDGSubstance>();

			_id2 = Guid.NewGuid();
			_classificationData2 = TestHelper.RegulatedQuantity();
			_undgSubstance2 = Factory.NewWithValidTestData<UNDGSubstance>();

			_undgSubstance3 = Factory.NewWithValidTestData<UNDGSubstance>();
			_undgSubstance3.DG_UNNO = "3333";
			_undgDataItemDto1 = new UNDGDataItemDTO
			{
				UNDGSubstanceDTOs = new[]
				{
					new UNDGSubstanceDTO { Unno = _undgSubstance3.DG_UNNO, Variant = _undgSubstance3.DG_Variant, Standard = _undgSubstance3.DG_Standard },
				}
			};

			_undgSubstance5 = Factory.NewWithValidTestData<UNDGSubstance>();
			_undgSubstance5.DG_UNNO = "5555";
			_undgDataItemDto2 = new UNDGDataItemDTO
			{
				UNDGSubstanceDTOs = new[]
				{
					new UNDGSubstanceDTO { Unno = _undgSubstance5.DG_UNNO, Variant = _undgSubstance5.DG_Variant, Standard = _undgSubstance5.DG_Standard },
				}
			};
		}

		public void TestCheck_ShouldNotReturnPairsWithErrorOrWarning_IfThereAreNoRules()
		{
			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(x => x.Find(It.IsAny<Guid>(), "IMO")).Returns((Guid id, string param) => id == _id1 ? (_classificationData1, _undgSubstance1, string.Empty) : (_classificationData2, _undgSubstance2, string.Empty));
			var controller = GetController(new List<ISegregationRule>(), databaseService.Object);

			var standards = new string[] { "IMO" };
			var ids = new Guid[] { _id1, _id2 };
			var segregationQuery = new SegregationQuery { Standards = standards, Ids = ids };
			var result = controller.Check(segregationQuery).ExecuteAsync(CancellationToken.None).Result;
			var content = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();

			AssertEquals(HttpStatusCode.OK, result.StatusCode);
			AssertEquals("{\"DGPairInfo\":[]}", content);
		}

		public void TestCheck_ShouldReturnOnePairWithErrorMessage()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();
			var segregationRule3 = new Mock<ISegregationRule>();
			segregationRule1.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message>());
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");

			segregationRule2.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message> { new Message(MessageType.Error, "Rule 2 failed") });
			segregationRule2.Setup(x => x.ApplicableStandard).Returns("IMO");

			segregationRule3.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message>());
			segregationRule3.Setup(x => x.ApplicableStandard).Returns("IMO");

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object, segregationRule3.Object };
			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(x => x.Find(It.IsAny<Guid>(), "IMO")).Returns((Guid id, string param) => id == _id1 ? (_classificationData1, _undgSubstance1, string.Empty) : (_classificationData2, _undgSubstance2, string.Empty));
			var controller = GetController(segregationRules, databaseService.Object);

			var ids = new Guid[] { _id1, _id2 };
			var standards = new string[] { "IMO" };
			var segregationQuery = new SegregationQuery { Standards = standards, Ids = ids };
			var result = controller.Check(segregationQuery).ExecuteAsync(CancellationToken.None).Result;
			var content = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			var expectedContent = $"{{\"DGPairInfo\":[{{\"Item1\":\"{_id1}\",\"Item2\":\"{_id2}\",\"Standard\":\"IMO\",\"Message\":{{\"Type\":\"{MessageType.Error}\",\"Text\":\"Rule 2 failed\"}}}}]}}";

			AssertEquals("Http status code 200 should be returned", HttpStatusCode.OK, result.StatusCode);
			AssertEquals("Result should contain one error message from the second rule", expectedContent.Replace("\u00A0", " "), content);
		}

		public void TestCheck_ShouldNotReturnPairsWithError_IfSubstancesAreExempted()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();
			var segregationRule3 = new Mock<ISegregationRule>();
			segregationRule1.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message>());
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");

			segregationRule2.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message> { new Message(MessageType.Error, "Rule 2 failed") });
			segregationRule2.Setup(x => x.ApplicableStandard).Returns("IMO");

			segregationRule3.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message>());
			segregationRule3.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule3.Setup(x => x.IsExemption).Returns(true);

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object, segregationRule3.Object };
			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(x => x.Find(It.IsAny<Guid>(), "IMO")).Returns((Guid id, string param) => id == _id1 ? (_classificationData1, _undgSubstance1, string.Empty) : (_classificationData2, _undgSubstance2, string.Empty));
			var controller = GetController(segregationRules, databaseService.Object);

			var ids = new Guid[] { _id1, _id2 };
			var standards = new string[] { "IMO" };
			var segregationQuery = new SegregationQuery { Standards = standards, Ids = ids };
			var result = controller.Check(segregationQuery).ExecuteAsync(CancellationToken.None).Result;
			var content = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			
			AssertEquals(HttpStatusCode.OK, result.StatusCode);
			AssertEquals("{\"DGPairInfo\":[]}", content);
		}

		public void TestCheck_ShouldNotReturnPairsWithError_IfAllRulesPassed()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();
			var segregationRule3 = new Mock<ISegregationRule>();

			segregationRule1.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message> { new Message(MessageType.Info, "Rule 1 notes") });
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");

			segregationRule2.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message>());
			segregationRule2.Setup(x => x.ApplicableStandard).Returns("IMO");

			segregationRule3.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message> { new Message(MessageType.Warning, "Rule 3 passed with a warning") });
			segregationRule3.Setup(x => x.ApplicableStandard).Returns("IMO");

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object, segregationRule3.Object };
			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(x => x.Find(It.IsAny<Guid>(), "IMO")).Returns((Guid id, string param) => id == _id1 ? (_classificationData1, _undgSubstance1, string.Empty) : (_classificationData2, _undgSubstance2, string.Empty));
			var controller = GetController(segregationRules, databaseService.Object);

			var ids = new Guid[] { _id1, _id2 };
			var standards = new string[] { "IMO" };
			var segregationQuery = new SegregationQuery { Standards = standards, Ids = ids };
			var result = controller.Check(segregationQuery).ExecuteAsync(CancellationToken.None).Result;
			var content = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			var expectedContent = $"{{\"DGPairInfo\":[{{\"Item1\":\"{_id1}\",\"Item2\":\"{_id2}\",\"Standard\":\"IMO\",\"Message\":{{\"Type\":\"{MessageType.Info}\",\"Text\":\"Rule 1 notes\"}}}},{{\"Item1\":\"{_id1}\",\"Item2\":\"{_id2}\",\"Standard\":\"IMO\",\"Message\":{{\"Type\":\"{MessageType.Warning}\",\"Text\":\"Rule 3 passed with a warning\"}}}}]}}";

			AssertEquals("Http status code 200 should be returned", HttpStatusCode.OK, result.StatusCode);
			AssertEquals("Result should contain warning and information messages", expectedContent.Replace("\u00A0", " "), content);
		}

		public void TestCheck_ShouldReturnPairsWithWarningOrInfo_IfSubstancesAreExempted()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();
			var segregationRule3 = new Mock<ISegregationRule>();
			var segregationRule4 = new Mock<ISegregationRule>();

			segregationRule1.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message> { new Message(MessageType.Info, "Rule 1 notes") });
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");

			segregationRule2.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message> { new Message(MessageType.Error, "Rule 2 failed") });
			segregationRule2.Setup(x => x.ApplicableStandard).Returns("IMO");

			segregationRule3.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message> { new Message(MessageType.Warning, "Rule 3 passed with a warning") });
			segregationRule3.Setup(x => x.ApplicableStandard).Returns("IMO");

			segregationRule4.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message>());
			segregationRule4.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule4.Setup(x => x.IsExemption).Returns(true);

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object, segregationRule3.Object, segregationRule4.Object };
			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(x => x.Find(It.IsAny<Guid>(), "IMO")).Returns((Guid id, string param) => id == _id1 ? (_classificationData1, _undgSubstance1, string.Empty) : (_classificationData2, _undgSubstance2, string.Empty));
			var controller = GetController(segregationRules, databaseService.Object);

			var ids = new Guid[] { _id1, _id2 };
			var standards = new string[] { "IMO" };
			var segregationQuery = new SegregationQuery { Standards = standards, Ids = ids };
			var result = controller.Check(segregationQuery).ExecuteAsync(CancellationToken.None).Result;
			var content = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			var expectedContent = $"{{\"DGPairInfo\":[{{\"Item1\":\"{_id1}\",\"Item2\":\"{_id2}\",\"Standard\":\"IMO\",\"Message\":{{\"Type\":\"{MessageType.Info}\",\"Text\":\"Rule 1 notes\"}}}},{{\"Item1\":\"{_id1}\",\"Item2\":\"{_id2}\",\"Standard\":\"IMO\",\"Message\":{{\"Type\":\"{MessageType.Warning}\",\"Text\":\"Rule 3 passed with a warning\"}}}}]}}";

			AssertEquals("Http status code 200 should be returned", HttpStatusCode.OK, result.StatusCode);
			AssertEquals("Result should contain warning and information messages", expectedContent.Replace("\u00A0", " "), content);
		}

		public void TestCheck_ShouldNotReturnPairsWithErrorOrWarning_WhenSubstancesAreSameWithDifferentDataItems()
		{
			var id1 = Guid.NewGuid();
			var id2 = Guid.NewGuid();

			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();

			var undgDataItem1 = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem1.DI_DGWeight = 20;
			undgDataItem1.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var undgDataItem2 = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem2.DI_DGWeight = 10;
			undgDataItem2.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var undgSubstance1 = Factory.NewWithValidTestData<UNDGSubstance>();
			segregationRule1.Setup(x => x.Check(undgSubstance1, undgSubstance1)).Returns(new List<Message> { new Message(MessageType.Info, "Rule failure notes") });
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");

			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(x => x.Find(id1, "IMO")).Returns((Guid id, string param) => (TestHelper.RegulatedQuantity(), undgSubstance1, string.Empty));
			databaseService.Setup(x => x.Find(id2, "IMO")).Returns((Guid id, string param) => (TestHelper.RegulatedQuantity(), undgSubstance1, string.Empty));

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object };
			var controller = GetController(segregationRules, databaseService.Object);

			var ids = new Guid[] { id1, id2 };
			var standards = new string[] { "IMO" };
			var segregationQuery = new SegregationQuery { Standards = standards, Ids = ids };
			var result = controller.Check(segregationQuery).ExecuteAsync(CancellationToken.None).Result;
			var content = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();

			AssertEquals("Http status code 200 should be returned", HttpStatusCode.OK, result.StatusCode);
			AssertEquals("Result should contain no pairs with error or warning messages", "{\"DGPairInfo\":[]}", content);
		}

		public void TestCheck_ShouldFilterOutNullMessagesReturnedFromRules()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			segregationRule1.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message> { null, new Message(MessageType.Info, "Rule 1 notes"), null });
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");
			var segregationRules = new List<ISegregationRule> { segregationRule1.Object };

			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(x => x.Find(It.IsAny<Guid>(), "IMO")).Returns((Guid id, string param) => id == _id1 ? (_classificationData1, _undgSubstance1, string.Empty) : (_classificationData2, _undgSubstance2, string.Empty));
			var controller = GetController(segregationRules, databaseService.Object);

			var ids = new Guid[] { _id1, _id2 };
			var standards = new string[] { "IMO" };
			var segregationQuery = new SegregationQuery { Standards = standards, Ids = ids };
			HttpResponseMessage result = null;
			AssertNoExceptionThrown(() => result = controller.Check(segregationQuery).ExecuteAsync(CancellationToken.None).Result);
			AssertEquals("Http status code 200 should be returned", HttpStatusCode.OK, result.StatusCode);
			var content = string.Empty;
			var expectedContent = $"{{\"DGPairInfo\":[{{\"Item1\":\"{_id1}\",\"Item2\":\"{_id2}\",\"Standard\":\"IMO\",\"Message\":{{\"Type\":\"{MessageType.Info}\",\"Text\":\"Rule 1 notes\"}}}}]}}";
			AssertNoExceptionThrown(() => content = result.Content.ReadAsStringAsync().GetAwaiter().GetResult());
			AssertEquals("Result should not contain null messages", expectedContent.Replace("\u00A0", " "), content);
		}

		public void TestCheck_ShouldReturnEmptyArrayWhenThereAreNoMessages()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			segregationRule1.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message> { null, null, null });
			var segregationRules = new List<ISegregationRule> { segregationRule1.Object };
			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(x => x.Find(It.IsAny<Guid>(), "IMO")).Returns((Guid id, string param) => id == _id1 ? (_classificationData1, _undgSubstance1, string.Empty) : (_classificationData2, _undgSubstance2, string.Empty));
			var controller = GetController(segregationRules, databaseService.Object);
			HttpResponseMessage result = null;

			var ids = new Guid[] { _id1, _id2 };
			var standards = new string[] { "IMO" };
			var segregationQuery = new SegregationQuery { Standards = standards, Ids = ids };
			AssertNoExceptionThrown(() => result = controller.Check(segregationQuery).ExecuteAsync(CancellationToken.None).Result);
			AssertEquals("Http status code 200 should be returned", HttpStatusCode.OK, result.StatusCode);
			var content = string.Empty;
			AssertNoExceptionThrown(() => content = result.Content.ReadAsStringAsync().GetAwaiter().GetResult());
			AssertEquals("Result should contain no pairs with error or warning messages", "{\"DGPairInfo\":[]}", content);
		}

		public void TestCheck_ShouldRejectEmptyListOfGuids()
		{
			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(x => x.Find(It.IsAny<Guid>(), "IMO")).Returns((Guid id, string param) => id == _id1 ? (_classificationData1, _undgSubstance1, string.Empty) : (_classificationData2, _undgSubstance2, string.Empty));
			var controller = GetController(new List<ISegregationRule>(), databaseService.Object);
			var ids = Array.Empty<Guid>();
			var standards = new string[] { "IMO" };
			var segregationQuery = new SegregationQuery { Standards = standards, Ids = ids };
			var result = controller.Check(segregationQuery);
			var content = result as BadRequestErrorMessageResult;
			AssertEquals("List of Ids cannot be empty", content.Message);
		}

		public void TestCheck_ShouldReturnBadRequestIfASubstanceCannotBeFound()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			segregationRule1.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message> { null, null, null });
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");
			var segregationRules = new List<ISegregationRule> { segregationRule1.Object };
			var databaseService = new Mock<IDatabaseService>();
			var errorMessage = "Cannot find UNDG substance for standard IMO and UNDG data item with PK = 1234";
			databaseService.Setup(x => x.Find(It.IsAny<Guid>(), "IMO")).Returns((null, null, errorMessage));
			var controller = GetController(segregationRules, databaseService.Object);
			var ids = new Guid[] { _id1, _id2 };
			var standards = new string[] { "IMO" };
			var segregationQuery = new SegregationQuery { Standards = standards, Ids = ids };
			var result = controller.Check(segregationQuery);
			var content = result as BadRequestErrorMessageResult;
			AssertEquals(errorMessage, content.Message);
		}

		public void TestCheckUnsaved_ShouldNotReturnPairsWithErrorOrWarning_IfThereAreNoRules()
		{
			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(x => x.Find(It.IsAny<UNDGDataItemDTO>(), It.IsAny<string>())).Returns(TestHelper.MockFind);
			var controller = GetController(new List<ISegregationRule>(), databaseService.Object);

			var standards = new string[] { "IMO" };
			var undgDataItemDtos = new UNDGDataItemDTO[] { _undgDataItemDto1, _undgDataItemDto2 };
			var segregationEntityQuery = new SegregationEntityQuery { Standards = standards, UNDGDataItemDTOs = undgDataItemDtos };
			var result = controller.CheckUnsaved(segregationEntityQuery).ExecuteAsync(CancellationToken.None).Result;
			var content = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();

			AssertEquals(HttpStatusCode.OK, result.StatusCode);
			AssertEquals("{\"DGPairInfo\":[]}", content);
		}

		public void TestCheckUnsaved_ShouldReturnOnePairWithErrorMessage()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();
			var segregationRule3 = new Mock<ISegregationRule>();
			segregationRule1.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message>());
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");

			segregationRule2.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message> { new Message(MessageType.Error, "Rule 2 failed") });
			segregationRule2.Setup(x => x.ApplicableStandard).Returns("IMO");

			segregationRule3.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message>());
			segregationRule3.Setup(x => x.ApplicableStandard).Returns("IMO");

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object, segregationRule3.Object };
			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(x => x.Find(It.IsAny<UNDGDataItemDTO>(), It.IsAny<string>())).Returns(TestHelper.MockFind);
			var controller = GetController(segregationRules, databaseService.Object);

			var undgDataItemDtos = new UNDGDataItemDTO[] { _undgDataItemDto1, _undgDataItemDto2 };
			var standards = new string[] { "IMO" };
			var segregationEntityQuery = new SegregationEntityQuery { Standards = standards, UNDGDataItemDTOs = undgDataItemDtos };
			var result = controller.CheckUnsaved(segregationEntityQuery).ExecuteAsync(CancellationToken.None).Result;
			var content = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			var expectedContent = $"{{\"DGPairInfo\":[{{\"Item1\":{JsonConvert.SerializeObject(_undgDataItemDto1)},\"Item2\":{JsonConvert.SerializeObject(_undgDataItemDto2)},\"Standard\":\"IMO\",\"Message\":{{\"Type\":\"{MessageType.Error}\",\"Text\":\"Rule 2 failed\"}}}}]}}";

			AssertEquals("Http status code 200 should be returned", HttpStatusCode.OK, result.StatusCode);
			AssertEquals("Result should contain one error message from the second rule", expectedContent.Replace("\u00A0", " "), content);
		}

		public void TestCheckUnsaved_ShouldNotReturnPairsWithErrorOrWarning_IfAllRulesPassed()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();
			var segregationRule3 = new Mock<ISegregationRule>();

			segregationRule1.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message> { new Message(MessageType.Info, "Rule 1 notes") });
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");

			segregationRule2.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message>());
			segregationRule2.Setup(x => x.ApplicableStandard).Returns("IMO");

			segregationRule3.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message> { new Message(MessageType.Warning, "Rule 3 passed with a warning") });
			segregationRule3.Setup(x => x.ApplicableStandard).Returns("IMO");

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object, segregationRule3.Object };
			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(x => x.Find(It.IsAny<UNDGDataItemDTO>(), It.IsAny<string>())).Returns(TestHelper.MockFind);
			var controller = GetController(segregationRules, databaseService.Object);

			var undgDataItemDtos = new UNDGDataItemDTO[] { _undgDataItemDto1, _undgDataItemDto2 };
			var standards = new string[] { "IMO" };
			var segregationEntityQuery = new SegregationEntityQuery { Standards = standards, UNDGDataItemDTOs = undgDataItemDtos };
			var result = controller.CheckUnsaved(segregationEntityQuery).ExecuteAsync(CancellationToken.None).Result;
			var content = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			var expectedContent = $"{{\"DGPairInfo\":[{{\"Item1\":{JsonConvert.SerializeObject(_undgDataItemDto1)},\"Item2\":{JsonConvert.SerializeObject(_undgDataItemDto2)},\"Standard\":\"IMO\",\"Message\":{{\"Type\":\"{MessageType.Info}\",\"Text\":\"Rule 1 notes\"}}}},{{\"Item1\":{JsonConvert.SerializeObject(_undgDataItemDto1)},\"Item2\":{JsonConvert.SerializeObject(_undgDataItemDto2)},\"Standard\":\"IMO\",\"Message\":{{\"Type\":\"{MessageType.Warning}\",\"Text\":\"Rule 3 passed with a warning\"}}}}]}}";

			AssertEquals("Http status code 200 should be returned", HttpStatusCode.OK, result.StatusCode);
			AssertEquals("Result should contain warning and information messages", expectedContent.Replace("\u00A0", " "), content);
		}

		public void TestCheckUnsaved_ShouldFilterOutNullMessagesReturnedFromRules()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			segregationRule1.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message> { null, new Message(MessageType.Info, "Rule 1 notes"), null });
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");
			var segregationRules = new List<ISegregationRule> { segregationRule1.Object };
			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(x => x.Find(It.IsAny<UNDGDataItemDTO>(), It.IsAny<string>())).Returns(TestHelper.MockFind);
			var controller = GetController(segregationRules, databaseService.Object);

			var undgDataItemDtos = new UNDGDataItemDTO[] { _undgDataItemDto1, _undgDataItemDto2 };
			var standards = new string[] { "IMO" };
			var segregationEntityQuery = new SegregationEntityQuery { Standards = standards, UNDGDataItemDTOs = undgDataItemDtos };
			HttpResponseMessage result = null;
			AssertNoExceptionThrown(() => result = controller.CheckUnsaved(segregationEntityQuery).ExecuteAsync(CancellationToken.None).Result);
			AssertEquals("Http status code 200 should be returned", HttpStatusCode.OK, result.StatusCode);
			var content = string.Empty;
			var expectedContent = $"{{\"DGPairInfo\":[{{\"Item1\":{JsonConvert.SerializeObject(_undgDataItemDto1)},\"Item2\":{JsonConvert.SerializeObject(_undgDataItemDto2)},\"Standard\":\"IMO\",\"Message\":{{\"Type\":\"{MessageType.Info}\",\"Text\":\"Rule 1 notes\"}}}}]}}";
			AssertNoExceptionThrown(() => content = result.Content.ReadAsStringAsync().GetAwaiter().GetResult());
			AssertEquals("Result should not contain null messages", expectedContent.Replace("\u00A0", " "), content);
		}

		public void TestCheckUnsaved_ShouldReturnEmptyArrayWhenThereAreNoMessages()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			segregationRule1.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message> { null, null, null });
			var segregationRules = new List<ISegregationRule> { segregationRule1.Object };
			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(x => x.Find(It.IsAny<UNDGDataItemDTO>(), It.IsAny<string>())).Returns(TestHelper.MockFind);
			var controller = GetController(segregationRules, databaseService.Object);
			HttpResponseMessage result = null;

			var undgDataItemDtos = new UNDGDataItemDTO[] { _undgDataItemDto1, _undgDataItemDto2 };
			var standards = new string[] { "IMO" };
			var segregationEntityQuery = new SegregationEntityQuery { Standards = standards, UNDGDataItemDTOs = undgDataItemDtos };
			AssertNoExceptionThrown(() => result = controller.CheckUnsaved(segregationEntityQuery).ExecuteAsync(CancellationToken.None).Result);
			AssertEquals("Http status code 200 should be returned", HttpStatusCode.OK, result.StatusCode);
			var content = string.Empty;
			AssertNoExceptionThrown(() => content = result.Content.ReadAsStringAsync().GetAwaiter().GetResult());
			AssertEquals("Result should not contain any messages", "{\"DGPairInfo\":[]}", content);
		}

		public void TestCheckUnsaved_ShouldRejectEmptyListOfUNDGDataItemDTOs()
		{
			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(x => x.Find(It.IsAny<UNDGDataItemDTO>(), It.IsAny<string>())).Returns(TestHelper.MockFind);
			var controller = GetController(new List<ISegregationRule>(), databaseService.Object);
			var undgDataItemDtos = Array.Empty<UNDGDataItemDTO>();
			var standards = new string[] { "IMO" };
			var segregationEntityQuery = new SegregationEntityQuery { Standards = standards, UNDGDataItemDTOs = undgDataItemDtos };
			var result = controller.CheckUnsaved(segregationEntityQuery);
			var content = result as BadRequestErrorMessageResult;
			AssertEquals("List of UNDGDataItemDTOs cannot be empty", content.Message);
		}

		SegregationController GetController(IEnumerable<ISegregationRule> rules, IDatabaseService databaseService)
		{
			var segregationRulesCollection = new SegregationRulesManager(rules);
			var validator = new SegregationQueryValidator();
			var controller = new SegregationController(segregationRulesCollection, validator, databaseService);
			controller.Request = new HttpRequestMessage();
			controller.Configuration = new HttpConfiguration();
			return controller;
		}
	}
}
