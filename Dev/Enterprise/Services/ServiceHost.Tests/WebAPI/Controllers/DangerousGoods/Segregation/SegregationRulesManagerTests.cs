using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions.Freight.DangerousGoods.SegregationApi;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Services.ServiceHost.Tests.WebAPI.Controllers.DangerousGoods.Segregation
{
	class SegregationRulesManagerTests : TestCaseWithFactory
	{
		Guid _id1;
		UNDGSubstance _undgSubstance1;
		Guid _id2;
		UNDGSubstance _undgSubstance2;

		UNDGSubstance _undgSubstance3;
		UNDGDataItemDTO _undgDataItemDto1;

		UNDGSubstance _undgSubstance5;
		UNDGDataItemDTO _undgDataItemDto2;

		protected override void SetUp()
		{
			base.SetUp();
			_id1 = Guid.NewGuid();
			_undgSubstance1 = Factory.NewWithValidTestData<UNDGSubstance>();

			_id2 = Guid.NewGuid();
			_undgSubstance2 = Factory.NewWithValidTestData<UNDGSubstance>();

			_undgSubstance3 = Factory.NewWithValidTestData<UNDGSubstance>();
			_undgSubstance3.DG_UNNO = "3333";
			_undgDataItemDto1 = new UNDGDataItemDTO
			{
				UNDGSubstanceDTOs = new[]
				{
					new UNDGSubstanceDTO { Unno = _undgSubstance3.DG_UNNO, Variant = _undgSubstance3.DG_Variant, Standard = _undgSubstance3.DG_Standard },
					new UNDGSubstanceDTO { Unno = "1111", Variant = "vA", Standard = "IMO" },
				}
			};

			_undgSubstance5 = Factory.NewWithValidTestData<UNDGSubstance>();
			_undgSubstance5.DG_UNNO = "5555";

			_undgDataItemDto2 = new UNDGDataItemDTO
			{
				UNDGSubstanceDTOs = new[]
				{
					new UNDGSubstanceDTO { Unno = _undgSubstance5.DG_UNNO, Variant = _undgSubstance5.DG_Variant, Standard = _undgSubstance5.DG_Standard },
					new UNDGSubstanceDTO { Unno = "2222", Variant = "vB", Standard = "IMO" },
				}
			};
		}

		public void TestCheckForGuidParam_ShouldReturnErrorMessageIfAnyRuleFails()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();
			var segregationRule3 = new Mock<ISegregationRule>();
			segregationRule1.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message>());
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule1.Setup(x => x.IsExemption).Returns(false);

			segregationRule2.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message> { new Message(MessageType.Error, "Rule 2 failed") });
			segregationRule2.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule2.Setup(x => x.IsExemption).Returns(false);

			segregationRule3.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message>());
			segregationRule3.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule3.Setup(x => x.IsExemption).Returns(false);

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object, segregationRule3.Object };

			var standards = new string[] { "IMO" };
			var ids = new Guid[] { _id1, _id2 };
			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(f => f.Find(It.IsAny<Guid>(), "IMO")).Returns((Guid id, string param) => id == _id1 ? (_undgDataItemDto1.UNDGClassificationData, _undgSubstance1, string.Empty) : (_undgDataItemDto2.UNDGClassificationData, _undgSubstance2, string.Empty));
			var segregationRulesManager = new SegregationRulesManager(segregationRules);
			var dgSubstancePairInfo = segregationRulesManager.Check(standards, ids, databaseService.Object.Find);
			AssertEquals("There should be one information on one pair of substances", 1, dgSubstancePairInfo.Count());
			AssertEquals("Items with Id 1 and 2 should form the pair", (_id1, _id2), (dgSubstancePairInfo.First().Item1, dgSubstancePairInfo.First().Item2));
			AssertEquals("The standard for the returned pair should be IMO", UNDGSubstanceStandardTypes.IMO, dgSubstancePairInfo.First().Standard);
			AssertEquals("The expected message should be returned", MessageType.Error, dgSubstancePairInfo.First().Message.Type);
			AssertEquals("The expected message should be returned", "Rule 2 failed", dgSubstancePairInfo.First().Message.Text);
		}

		public void TestCheckForGuidParam_ShouldReturnNoMessagesIfAnyRuleFailsForExemptedSubstances()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();
			var segregationRule3 = new Mock<ISegregationRule>();
			segregationRule1.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message>());
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule1.Setup(x => x.IsExemption).Returns(false);

			segregationRule2.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message> { new Message(MessageType.Error, "Rule 2 failed") });
			segregationRule2.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule2.Setup(x => x.IsExemption).Returns(false);

			segregationRule3.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message>());
			segregationRule3.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule3.Setup(x => x.IsExemption).Returns(true);

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object, segregationRule3.Object };

			var standards = new string[] { "IMO" };
			var ids = new Guid[] { _id1, _id2 };
			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(f => f.Find(It.IsAny<Guid>(), "IMO")).Returns((Guid id, string param) => id == _id1 ? (_undgDataItemDto1.UNDGClassificationData, _undgSubstance1, string.Empty) : (_undgDataItemDto2.UNDGClassificationData, _undgSubstance2, string.Empty));
			var segregationRulesManager = new SegregationRulesManager(segregationRules);
			var dgSubstancePairInfo = segregationRulesManager.Check(standards, ids, databaseService.Object.Find);
			AssertEquals("There should be no substance pairs with error or warning", 0, dgSubstancePairInfo.Count());
		}

		public void TestCheckForGuidParam_ShouldNotCallTheCheckMethodOfTheRules_WhenTheSubstancesAreIdentical()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();

			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule2.Setup(x => x.ApplicableStandard).Returns("IATA");

			segregationRule1.Setup(x => x.IsExemption).Returns(false);
			segregationRule2.Setup(x => x.IsExemption).Returns(false);

			var id1 = Guid.NewGuid();
			var id2 = Guid.NewGuid();
			var id3 = Guid.NewGuid();

			var undgSubstance1 = Factory.NewWithValidTestData<UNDGSubstance>();
			var undgSubstance2 = Factory.NewWithValidTestData<UNDGSubstance>();

			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(f => f.Find(id1, It.IsAny<string>())).Returns((TestHelper.RegulatedQuantity(), undgSubstance1, string.Empty));
			databaseService.Setup(f => f.Find(id2, It.IsAny<string>())).Returns((TestHelper.RegulatedQuantity(), undgSubstance2, string.Empty));
			databaseService.Setup(f => f.Find(id3, It.IsAny<string>())).Returns((TestHelper.RegulatedQuantity(), undgSubstance1, string.Empty));

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object };
			var standards = new string[] { "IMO", "IATA" };
			var ids = new Guid[] { id1, id2, id3 };

			var segregationRulesManager = new SegregationRulesManager(segregationRules);
			var dgSubstancePairInfo = segregationRulesManager.Check(standards, ids, databaseService.Object.Find);

			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance1, undgSubstance1), Times.Never);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance2, undgSubstance1), Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance1, undgSubstance2), Times.Once);
			segregationRule2.Verify(segregationRule2 => segregationRule2.Check(undgSubstance1, undgSubstance1), Times.Never);
			segregationRule2.Verify(segregationRule2 => segregationRule2.Check(undgSubstance2, undgSubstance1), Times.Once);
			segregationRule2.Verify(segregationRule2 => segregationRule2.Check(undgSubstance1, undgSubstance2), Times.Once);

			segregationRule1.Verify(segregationRule1 => segregationRule1.ApplicableStandard, Times.Exactly(2));
			segregationRule2.Verify(segregationRule2 => segregationRule2.ApplicableStandard, Times.Exactly(2));
			segregationRule1.Verify(segregationRule1 => segregationRule1.IsExemption, Times.Exactly(2));
			segregationRule2.Verify(segregationRule2 => segregationRule2.IsExemption, Times.Exactly(2));
			segregationRule1.VerifyNoOtherCalls();
			segregationRule2.VerifyNoOtherCalls();
			AssertEquals("Messages should be empty as the rules are not mocked.", 0, dgSubstancePairInfo.Count());
		}

		public void TestCheckForGuidParam_ShouldPassTheCheck_WhenTheSubstancesAreIdentical_EvenWhenRuleFails()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();

			var id1 = Guid.NewGuid();
			var id2 = Guid.NewGuid();
			var id3 = Guid.NewGuid();

			var undgSubstance1 = Factory.NewWithValidTestData<UNDGSubstance>();
			var undgSubstance2 = Factory.NewWithValidTestData<UNDGSubstance>();

			segregationRule1.Setup(x => x.Check(undgSubstance1, undgSubstance1)).Returns(new List<Message> { new Message(MessageType.Error, "Identical substance failure") });
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule1.Setup(x => x.IsExemption).Returns(false);

			segregationRule1.Setup(x => x.Check(undgSubstance1, undgSubstance2)).Returns(new List<Message> { new Message(MessageType.Info, "Non identical substance") });
			segregationRule2.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule2.Setup(x => x.IsExemption).Returns(false);

			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(f => f.Find(id1, It.IsAny<string>())).Returns((TestHelper.RegulatedQuantity(), undgSubstance1, string.Empty));
			databaseService.Setup(f => f.Find(id2, It.IsAny<string>())).Returns((TestHelper.RegulatedQuantity(), undgSubstance1, string.Empty));
			databaseService.Setup(f => f.Find(id3, It.IsAny<string>())).Returns((TestHelper.RegulatedQuantity(), undgSubstance2, string.Empty));

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object };
			var standards = new string[] { "IMO" };
			var ids = new Guid[] { id1, id2, id3 };

			var segregationRulesManager = new SegregationRulesManager(segregationRules);
			var dgSubstancePairInfo = segregationRulesManager.Check(standards, ids, databaseService.Object.Find);

			AssertEquals("Messages should contain non-identical substance rule check messages as individual entries", 2, dgSubstancePairInfo.Count());
			AssertEquals("Items with Id 1 and 3 should form the first pair", (id1, id3), (dgSubstancePairInfo.First().Item1, dgSubstancePairInfo.First().Item2));
			AssertEquals("The standard for the first pair should be IMO", UNDGSubstanceStandardTypes.IMO, dgSubstancePairInfo.First().Standard);
			AssertEquals("The expected message should be returned for first pair", MessageType.Info, dgSubstancePairInfo.First().Message.Type);
			AssertEquals("The expected message should be returned for first pair", "Non identical substance", dgSubstancePairInfo.First().Message.Text);
			AssertEquals("Items with Id 2 and 3 should form the second pair", (id2, id3), (dgSubstancePairInfo.ElementAt(1).Item1, dgSubstancePairInfo.ElementAt(1).Item2));
			AssertEquals("The standard for the second pair should be IMO", UNDGSubstanceStandardTypes.IMO, dgSubstancePairInfo.ElementAt(1).Standard);
			AssertEquals("The expected message should be returned for second pair", MessageType.Info, dgSubstancePairInfo.ElementAt(1).Message.Type);
			AssertEquals("The expected message should be returned for second pair", "Non identical substance", dgSubstancePairInfo.ElementAt(1).Message.Text);
		}

		public void TestCheckForGuidParam_ShouldReturnNoMessagesIfAllTheRulesPass()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();
			var segregationRule3 = new Mock<ISegregationRule>();
			segregationRule1.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message>());
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule1.Setup(x => x.IsExemption).Returns(false);

			segregationRule2.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message>());
			segregationRule2.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule2.Setup(x => x.IsExemption).Returns(false);

			segregationRule3.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message>());
			segregationRule3.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule3.Setup(x => x.IsExemption).Returns(false);

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object, segregationRule3.Object };

			var standards = new string[] { "IMO" };
			var ids = new Guid[] { _id1, _id2 };
			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(f => f.Find(It.IsAny<Guid>(), "IMO")).Returns((Guid id, string param) => id == _id1 ? (_undgDataItemDto1.UNDGClassificationData, _undgSubstance1, string.Empty) : (_undgDataItemDto2.UNDGClassificationData, _undgSubstance2, string.Empty));
			var segregationRulesManager = new SegregationRulesManager(segregationRules);
			var dgSubstancePairInfo = segregationRulesManager.Check(standards, ids, databaseService.Object.Find);
			AssertEquals("There should be no substance pairs with error or warning", 0, dgSubstancePairInfo.Count());
		}

		public void TestCheckForGuidParam_ShouldCombineMessagesFromAllTheRules()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();
			var segregationRule3 = new Mock<ISegregationRule>();

			var rule1Messages = new List<Message> {
				new Message(MessageType.Info, "Rule 1 notes"),
				new Message(MessageType.Warning, "Rule 1 warning")
			};
			segregationRule1.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(rule1Messages);
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule1.Setup(x => x.IsExemption).Returns(false);

			var rule2Messages = new List<Message> {
				new Message(MessageType.Error, "Rule 2 failed"),
				new Message(MessageType.Warning, "Rule 2 warning")
			};
			segregationRule2.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(rule2Messages);
			segregationRule2.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule2.Setup(x => x.IsExemption).Returns(false);

			segregationRule3.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message>());
			segregationRule3.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule3.Setup(x => x.IsExemption).Returns(false);

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object, segregationRule3.Object };

			var standards = new string[] { "IMO" };
			var ids = new Guid[] { _id1, _id2 };
			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(f => f.Find(It.IsAny<Guid>(), "IMO")).Returns((Guid id, string param) => id == _id1 ? (_undgDataItemDto1.UNDGClassificationData, _undgSubstance1, string.Empty) : (_undgDataItemDto2.UNDGClassificationData, _undgSubstance2, string.Empty));
			var segregationRulesManager = new SegregationRulesManager(segregationRules);
			var dgSubstancePairInfo = segregationRulesManager.Check(standards, ids, databaseService.Object.Find);
			AssertEquals("There should be four substance pairs", 4, dgSubstancePairInfo.Count());

			AssertEquals("Items with Id 1 and 2 should form the first pair", (_id1, _id2), (dgSubstancePairInfo.First().Item1, dgSubstancePairInfo.First().Item2));
			AssertEquals("Items with Id 1 and 2 should form the second pair", (_id1, _id2), (dgSubstancePairInfo.ElementAt(1).Item1, dgSubstancePairInfo.ElementAt(1).Item2));
			AssertEquals("Items with Id 1 and 2 should form the thrid pair", (_id1, _id2), (dgSubstancePairInfo.ElementAt(2).Item1, dgSubstancePairInfo.ElementAt(2).Item2));
			AssertEquals("Items with Id 1 and 2 should form the fourth pair", (_id1, _id2), (dgSubstancePairInfo.ElementAt(3).Item1, dgSubstancePairInfo.ElementAt(3).Item2));
			AssertEquals("The standard for the first pair should be IMO", UNDGSubstanceStandardTypes.IMO, dgSubstancePairInfo.First().Standard);
			AssertEquals("The standard for the second pair should be IMO", UNDGSubstanceStandardTypes.IMO, dgSubstancePairInfo.ElementAt(1).Standard);
			AssertEquals("The standard for the third pair should be IMO", UNDGSubstanceStandardTypes.IMO, dgSubstancePairInfo.ElementAt(2).Standard);
			AssertEquals("The standard for the fourth pair should be IMO", UNDGSubstanceStandardTypes.IMO, dgSubstancePairInfo.ElementAt(3).Standard);
			AssertEquals(MessageType.Info, dgSubstancePairInfo.First().Message.Type);
			AssertEquals("Rule 1 notes", dgSubstancePairInfo.First().Message.Text);
			AssertEquals(MessageType.Warning, dgSubstancePairInfo.ElementAt(1).Message.Type);
			AssertEquals("Rule 1 warning", dgSubstancePairInfo.ElementAt(1).Message.Text);
			AssertEquals(MessageType.Error, dgSubstancePairInfo.ElementAt(2).Message.Type);
			AssertEquals("Rule 2 failed", dgSubstancePairInfo.ElementAt(2).Message.Text);
			AssertEquals(MessageType.Warning, dgSubstancePairInfo.ElementAt(3).Message.Type);
			AssertEquals("Rule 2 warning", dgSubstancePairInfo.ElementAt(3).Message.Text);
		}

		public void TestCheckForGuidParam_ShouldDisplayOnlyInfoAndWarningMessagesFromAllTheRules_WhenSubstancesAreExempted()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();
			var segregationRule3 = new Mock<ISegregationRule>();

			var rule1Messages = new List<Message> {
				new Message(MessageType.Info, "Rule 1 notes"),
				new Message(MessageType.Warning, "Rule 1 warning")
			};
			segregationRule1.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(rule1Messages);
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule1.Setup(x => x.IsExemption).Returns(false);

			var rule2Messages = new List<Message> {
				new Message(MessageType.Error, "Rule 2 failed"),
				new Message(MessageType.Warning, "Rule 2 warning")
			};
			segregationRule2.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(rule2Messages);
			segregationRule2.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule2.Setup(x => x.IsExemption).Returns(false);

			segregationRule3.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message>());
			segregationRule3.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule3.Setup(x => x.IsExemption).Returns(true);

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object, segregationRule3.Object };

			var standards = new string[] { "IMO" };
			var ids = new Guid[] { _id1, _id2 };
			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(f => f.Find(It.IsAny<Guid>(), "IMO")).Returns((Guid id, string param) => id == _id1 ? (_undgDataItemDto1.UNDGClassificationData, _undgSubstance1, string.Empty) : (_undgDataItemDto2.UNDGClassificationData, _undgSubstance2, string.Empty));
			var segregationRulesManager = new SegregationRulesManager(segregationRules);
			var dgSubstancePairInfo = segregationRulesManager.Check(standards, ids, databaseService.Object.Find);
			AssertEquals("There should be three substance pairs", 3, dgSubstancePairInfo.Count());

			AssertEquals("Items with Id 1 and 2 should form the first pair", (_id1, _id2), (dgSubstancePairInfo.First().Item1, dgSubstancePairInfo.First().Item2));
			AssertEquals("Items with Id 1 and 2 should form the second pair", (_id1, _id2), (dgSubstancePairInfo.ElementAt(1).Item1, dgSubstancePairInfo.ElementAt(1).Item2));
			AssertEquals("Items with Id 1 and 2 should form the thrid pair", (_id1, _id2), (dgSubstancePairInfo.ElementAt(2).Item1, dgSubstancePairInfo.ElementAt(2).Item2));
			AssertEquals("The standard for the first pair should be IMO", UNDGSubstanceStandardTypes.IMO, dgSubstancePairInfo.First().Standard);
			AssertEquals("The standard for the second pair should be IMO", UNDGSubstanceStandardTypes.IMO, dgSubstancePairInfo.ElementAt(1).Standard);
			AssertEquals("The standard for the third pair should be IMO", UNDGSubstanceStandardTypes.IMO, dgSubstancePairInfo.ElementAt(2).Standard);
			AssertEquals(MessageType.Info, dgSubstancePairInfo.First().Message.Type);
			AssertEquals("Rule 1 notes", dgSubstancePairInfo.First().Message.Text);
			AssertEquals(MessageType.Warning, dgSubstancePairInfo.ElementAt(1).Message.Type);
			AssertEquals("Rule 1 warning", dgSubstancePairInfo.ElementAt(1).Message.Text);
			AssertEquals(MessageType.Warning, dgSubstancePairInfo.ElementAt(2).Message.Type);
			AssertEquals("Rule 2 warning", dgSubstancePairInfo.ElementAt(2).Message.Text);
		}

		public void TestCheckForGuidParam_ShouldCallRulesWithTheMatchingStandardForEachPairOfIds()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();
			var segregationRule3 = new Mock<ISegregationRule>();
			segregationRule1.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message>());
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule1.Setup(x => x.IsExemption).Returns(false);

			segregationRule2.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message>());
			segregationRule2.Setup(x => x.ApplicableStandard).Returns("IATA");
			segregationRule2.Setup(x => x.IsExemption).Returns(false);

			segregationRule3.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message>());
			segregationRule3.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule3.Setup(x => x.IsExemption).Returns(false);

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object, segregationRule3.Object };
			var segregationRulesManager = new SegregationRulesManager(segregationRules);

			var id1 = Guid.NewGuid();
			var id2 = Guid.NewGuid();
			var id3 = Guid.NewGuid();
			var id4 = Guid.NewGuid();
			var undgSubstance1 = Factory.NewWithValidTestData<UNDGSubstance>();
			var undgSubstance2 = Factory.NewWithValidTestData<UNDGSubstance>();
			var undgSubstance3 = Factory.NewWithValidTestData<UNDGSubstance>();
			var undgSubstance4 = Factory.NewWithValidTestData<UNDGSubstance>();

			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(f => f.Find(id1, It.IsAny<string>())).Returns((TestHelper.RegulatedQuantity(), undgSubstance1, string.Empty));
			databaseService.Setup(f => f.Find(id2, It.IsAny<string>())).Returns((TestHelper.RegulatedQuantity(), undgSubstance2, string.Empty));
			databaseService.Setup(f => f.Find(id3, It.IsAny<string>())).Returns((TestHelper.RegulatedQuantity(), undgSubstance3, string.Empty));
			databaseService.Setup(f => f.Find(id4, It.IsAny<string>())).Returns((TestHelper.RegulatedQuantity(), undgSubstance4, string.Empty));
			var dgSubstancePairInfo = segregationRulesManager.Check(new string[] { "IMO" }, new Guid[] { id1, id2, id3, id4 }, databaseService.Object.Find);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance1, undgSubstance2), Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance1, undgSubstance3), Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance1, undgSubstance4), Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance2, undgSubstance3), Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance2, undgSubstance4), Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance3, undgSubstance4), Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.ApplicableStandard, Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.IsExemption, Times.Exactly(2));
			segregationRule1.VerifyNoOtherCalls();

			segregationRule2.Verify(segregationRule2 => segregationRule2.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>()), Times.Never);

			segregationRule3.Verify(segregationRule3 => segregationRule3.Check(undgSubstance1, undgSubstance2), Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.Check(undgSubstance1, undgSubstance3), Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.Check(undgSubstance1, undgSubstance4), Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.Check(undgSubstance2, undgSubstance3), Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.Check(undgSubstance2, undgSubstance4), Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.Check(undgSubstance3, undgSubstance4), Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.ApplicableStandard, Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.IsExemption, Times.Exactly(2));
			segregationRule3.VerifyNoOtherCalls();

			AssertEquals("There should be no pairs with error or warning", 0, dgSubstancePairInfo.Count());
		}

		public void TestCheckForGuidParam_ShouldCallRulesWithTheMatchingStandardForEachPairOfIds_ForExemptedSubstances()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();
			var segregationRule3 = new Mock<ISegregationRule>();
			segregationRule1.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message> { new Message(MessageType.Error, "Rule failed") });
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule1.Setup(x => x.IsExemption).Returns(false);

			segregationRule2.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message>());
			segregationRule2.Setup(x => x.ApplicableStandard).Returns("IATA");
			segregationRule2.Setup(x => x.IsExemption).Returns(false);

			segregationRule3.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message>());
			segregationRule3.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule3.Setup(x => x.IsExemption).Returns(true);

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object, segregationRule3.Object };
			var segregationRulesManager = new SegregationRulesManager(segregationRules);

			var id1 = Guid.NewGuid();
			var id2 = Guid.NewGuid();
			var id3 = Guid.NewGuid();
			var id4 = Guid.NewGuid();
			var undgSubstance1 = Factory.NewWithValidTestData<UNDGSubstance>();
			var undgSubstance2 = Factory.NewWithValidTestData<UNDGSubstance>();
			var undgSubstance3 = Factory.NewWithValidTestData<UNDGSubstance>();
			var undgSubstance4 = Factory.NewWithValidTestData<UNDGSubstance>();

			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(f => f.Find(id1, It.IsAny<string>())).Returns((TestHelper.RegulatedQuantity(), undgSubstance1, string.Empty));
			databaseService.Setup(f => f.Find(id2, It.IsAny<string>())).Returns((TestHelper.RegulatedQuantity(), undgSubstance2, string.Empty));
			databaseService.Setup(f => f.Find(id3, It.IsAny<string>())).Returns((TestHelper.RegulatedQuantity(), undgSubstance3, string.Empty));
			databaseService.Setup(f => f.Find(id4, It.IsAny<string>())).Returns((TestHelper.RegulatedQuantity(), undgSubstance4, string.Empty));
			var dgSubstancePairInfo = segregationRulesManager.Check(new string[] { "IMO" }, new Guid[] { id1, id2, id3, id4 }, databaseService.Object.Find);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance1, undgSubstance2), Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance1, undgSubstance3), Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance1, undgSubstance4), Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance2, undgSubstance3), Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance2, undgSubstance4), Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance3, undgSubstance4), Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.ApplicableStandard, Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.IsExemption, Times.Exactly(2));
			segregationRule1.VerifyNoOtherCalls();

			segregationRule2.Verify(segregationRule2 => segregationRule2.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>()), Times.Never);

			segregationRule3.Verify(segregationRule3 => segregationRule3.Check(undgSubstance1, undgSubstance2), Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.Check(undgSubstance1, undgSubstance3), Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.Check(undgSubstance1, undgSubstance4), Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.Check(undgSubstance2, undgSubstance3), Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.Check(undgSubstance2, undgSubstance4), Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.Check(undgSubstance3, undgSubstance4), Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.ApplicableStandard, Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.IsExemption, Times.Exactly(2));
			segregationRule3.VerifyNoOtherCalls();

			AssertEquals("There should be no pairs with error or warning", 0, dgSubstancePairInfo.Count());
		}

		public void TestCheck_ShouldAdjustErrorToWarningBasedOnQuantityClassification()
		{
			var segregationRule = new Mock<ISegregationRule>();
			segregationRule.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message> { new Message(MessageType.Error, "Segregation rule failed") });
			segregationRule.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule.Setup(x => x.IsExemption).Returns(false);

			var segregationRules = new List<ISegregationRule> { segregationRule.Object };
			var standards = new string[] { "IMO" };
			var undgDataItemDTOs = new UNDGDataItemDTO[] { _undgDataItemDto1, _undgDataItemDto2 };
			var databaseService = new Mock<IDatabaseService>();
			_undgDataItemDto1.UNDGClassificationData = TestHelper.ExceptedQuantity();
			_undgDataItemDto2.UNDGClassificationData = TestHelper.LimitedQuantity();

			databaseService.Setup(x => x.Find(It.IsAny<UNDGDataItemDTO>(), It.IsAny<string>())).Returns(TestHelper.MockFind);
			var segregationRulesManager = new SegregationRulesManager(segregationRules);
			var dgSubstancePairInfo = segregationRulesManager.Check(standards, undgDataItemDTOs, databaseService.Object.Find);

			AssertEquals("There should be one message", 1, dgSubstancePairInfo.Count());
			AssertEquals("The error should have been converted to a warning", MessageType.Warning, dgSubstancePairInfo.First().Message.Type);
			AssertEquals("The message text should be preserved", "Segregation rule failed", dgSubstancePairInfo.First().Message.Text);
		}

		public void TestCheck_ShouldNotAdjustErrorToWarningForRegularQuantities()
		{
			var segregationRule = new Mock<ISegregationRule>();
			segregationRule.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message> { new Message(MessageType.Error, "Segregation rule failed") });
			segregationRule.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule.Setup(x => x.IsExemption).Returns(false);

			var segregationRules = new List<ISegregationRule> { segregationRule.Object };
			var standards = new string[] { "IMO" };
			var undgDataItemDTOs = new UNDGDataItemDTO[] { _undgDataItemDto1, _undgDataItemDto2 };
			var databaseService = new Mock<IDatabaseService>();
			_undgDataItemDto1.UNDGClassificationData = TestHelper.RegulatedQuantity();
			_undgDataItemDto2.UNDGClassificationData = TestHelper.RegulatedQuantity();

			databaseService.Setup(x => x.Find(It.IsAny<UNDGDataItemDTO>(), It.IsAny<string>())).Returns(TestHelper.MockFind);
			var segregationRulesManager = new SegregationRulesManager(segregationRules);
			var dgSubstancePairInfo = segregationRulesManager.Check(standards, undgDataItemDTOs, databaseService.Object.Find);

			AssertEquals("There should be one message", 1, dgSubstancePairInfo.Count());
			AssertEquals("The message should remain an error", MessageType.Error, dgSubstancePairInfo.First().Message.Type);
			AssertEquals("The message text should be preserved", "Segregation rule failed", dgSubstancePairInfo.First().Message.Text);
		}

		public void TestCheck_ShouldHandleEmptyQuantityClassifications()
		{
			var segregationRule = new Mock<ISegregationRule>();
			segregationRule.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message> { new Message(MessageType.Error, "Segregation rule failed") });
			segregationRule.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule.Setup(x => x.IsExemption).Returns(false);

			var segregationRules = new List<ISegregationRule> { segregationRule.Object };
			var standards = new string[] { "IMO" };
			var undgDataItemDTOs = new UNDGDataItemDTO[] { _undgDataItemDto1, _undgDataItemDto2 };
			var databaseService = new Mock<IDatabaseService>();
			_undgDataItemDto1.UNDGClassificationData = new UNDGClassificationData();
			_undgDataItemDto2.UNDGClassificationData = TestHelper.ExceptedQuantity();

			databaseService.Setup(x => x.Find(It.IsAny<UNDGDataItemDTO>(), It.IsAny<string>())).Returns(TestHelper.MockFind);
			var segregationRulesManager = new SegregationRulesManager(segregationRules);
			var dgSubstancePairInfo = segregationRulesManager.Check(standards, undgDataItemDTOs, databaseService.Object.Find);

			AssertEquals("There should be one message", 1, dgSubstancePairInfo.Count());
			AssertEquals("When classification is null, the message should remain an error", MessageType.Error, dgSubstancePairInfo.First().Message.Type);
			AssertEquals("The message text should be preserved", "Segregation rule failed", dgSubstancePairInfo.First().Message.Text);
		}

		public void TestCheck_ShouldNotModifyWarningOrInfoMessages()
		{
			var segregationRule = new Mock<ISegregationRule>();
			segregationRule.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message> { new Message(MessageType.Warning, "Warning message"), new Message(MessageType.Info, "Info message") });
			segregationRule.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule.Setup(x => x.IsExemption).Returns(false);

			var segregationRules = new List<ISegregationRule> { segregationRule.Object };
			var standards = new string[] { "IMO" };
			var undgDataItemDTOs = new UNDGDataItemDTO[] { _undgDataItemDto1, _undgDataItemDto2 };
			var databaseService = new Mock<IDatabaseService>();
			_undgDataItemDto1.UNDGClassificationData = TestHelper.ExceptedQuantity();
			_undgDataItemDto2.UNDGClassificationData = TestHelper.LimitedQuantity();

			databaseService.Setup(x => x.Find(It.IsAny<UNDGDataItemDTO>(), It.IsAny<string>())).Returns(TestHelper.MockFind);
			var segregationRulesManager = new SegregationRulesManager(segregationRules);
			var dgSubstancePairInfo = segregationRulesManager.Check(standards, undgDataItemDTOs, databaseService.Object.Find);

			AssertEquals("There should be two messages", 2, dgSubstancePairInfo.Count());
			AssertEquals("The first message should still be a warning", MessageType.Warning, dgSubstancePairInfo.First().Message.Type);
			AssertEquals("The first message text should be preserved", "Warning message", dgSubstancePairInfo.First().Message.Text);
			AssertEquals("The second message should still be info", MessageType.Info, dgSubstancePairInfo.ElementAt(1).Message.Type);
			AssertEquals("The second message text should be preserved", "Info message", dgSubstancePairInfo.ElementAt(1).Message.Text);
		}

		public void TestCheck_ShouldHandleMultipleErrorMessagesCorrectly()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();

			segregationRule1.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message> { new Message(MessageType.Error, "Error 1") });
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule1.Setup(x => x.IsExemption).Returns(false);

			segregationRule2.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message> { new Message(MessageType.Error, "Error 2") });
			segregationRule2.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule2.Setup(x => x.IsExemption).Returns(false);

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object };
			var standards = new string[] { "IMO" };
			var undgDataItemDTOs = new UNDGDataItemDTO[] { _undgDataItemDto1, _undgDataItemDto2 };
			var databaseService = new Mock<IDatabaseService>();
			_undgDataItemDto1.UNDGClassificationData = TestHelper.ExceptedQuantity();
			_undgDataItemDto2.UNDGClassificationData = TestHelper.LimitedQuantity();

			databaseService.Setup(x => x.Find(It.IsAny<UNDGDataItemDTO>(), It.IsAny<string>())).Returns(TestHelper.MockFind);
			var segregationRulesManager = new SegregationRulesManager(segregationRules);
			var dgSubstancePairInfo = segregationRulesManager.Check(standards, undgDataItemDTOs, databaseService.Object.Find);

			AssertEquals("There should be two messages", 2, dgSubstancePairInfo.Count());
			AssertEquals("The first error should be converted to warning", MessageType.Warning, dgSubstancePairInfo.First().Message.Type);
			AssertEquals("The first message text should be preserved", "Error 1", dgSubstancePairInfo.First().Message.Text);
			AssertEquals("The second error should be converted to warning", MessageType.Warning, dgSubstancePairInfo.ElementAt(1).Message.Type);
			AssertEquals("The second message text should be preserved", "Error 2", dgSubstancePairInfo.ElementAt(1).Message.Text);
		}

		public void TestCheck_ShouldHandleMixedMessageTypesCorrectly()
		{
			var segregationRule = new Mock<ISegregationRule>();
			segregationRule.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message> { new Message(MessageType.Error, "Error message"), new Message(MessageType.Warning, "Warning message"), new Message(MessageType.Info, "Info message") });
			segregationRule.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule.Setup(x => x.IsExemption).Returns(false);

			var segregationRules = new List<ISegregationRule> { segregationRule.Object };
			var standards = new string[] { "IMO" };
			var undgDataItemDTOs = new UNDGDataItemDTO[] { _undgDataItemDto1, _undgDataItemDto2 };
			var databaseService = new Mock<IDatabaseService>();
			_undgDataItemDto1.UNDGClassificationData = TestHelper.ExceptedQuantity();
			_undgDataItemDto2.UNDGClassificationData = TestHelper.LimitedQuantity();

			databaseService.Setup(x => x.Find(It.IsAny<UNDGDataItemDTO>(), It.IsAny<string>())).Returns(TestHelper.MockFind);
			var segregationRulesManager = new SegregationRulesManager(segregationRules);
			var dgSubstancePairInfo = segregationRulesManager.Check(standards, undgDataItemDTOs, databaseService.Object.Find);

			AssertEquals("There should be three messages", 3, dgSubstancePairInfo.Count());
			AssertEquals("The error should be converted to warning", MessageType.Warning, dgSubstancePairInfo.First().Message.Type);
			AssertEquals("The first message text should be preserved", "Error message", dgSubstancePairInfo.First().Message.Text);
			AssertEquals("The warning should remain a warning", MessageType.Warning, dgSubstancePairInfo.ElementAt(1).Message.Type);
			AssertEquals("The second message text should be preserved", "Warning message", dgSubstancePairInfo.ElementAt(1).Message.Text);
			AssertEquals("The info should remain info", MessageType.Info, dgSubstancePairInfo.ElementAt(2).Message.Type);
			AssertEquals("The third message text should be preserved", "Info message", dgSubstancePairInfo.ElementAt(2).Message.Text);
		}

		public void TestCheckForUNDGDataItemDTOParam_ShouldReturnErrorMessageIfAnyRuleFails()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();
			var segregationRule3 = new Mock<ISegregationRule>();
			segregationRule1.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message>());
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule1.Setup(x => x.IsExemption).Returns(false);

			segregationRule2.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message> { new Message(MessageType.Error, "Rule 2 failed") });
			segregationRule2.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule2.Setup(x => x.IsExemption).Returns(false);

			segregationRule3.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message>());
			segregationRule3.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule3.Setup(x => x.IsExemption).Returns(false);

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object, segregationRule3.Object };

			var standards = new string[] { "IMO" };
			var undgDataItemDtos = new UNDGDataItemDTO[] { _undgDataItemDto1, _undgDataItemDto2 };
			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(x => x.Find(It.IsAny<UNDGDataItemDTO>(), It.IsAny<string>())).Returns(TestHelper.MockFind);
			var segregationRulesManager = new SegregationRulesManager(segregationRules);
			var dgSubstancePairInfo = segregationRulesManager.Check(standards, undgDataItemDtos, databaseService.Object.Find);
			AssertEquals("There should be one pair with error", 1, dgSubstancePairInfo.Count());
			AssertEquals("The standard for the returned pair should be IMO", UNDGSubstanceStandardTypes.IMO, dgSubstancePairInfo.First().Standard);
			AssertEquals("The expected message should be returned", MessageType.Error, dgSubstancePairInfo.First().Message.Type);
			AssertEquals("The expected message should be returned", "Rule 2 failed", dgSubstancePairInfo.First().Message.Text);
		}

		public void TestCheckForUNDGDataItemDTOParam_ShouldReturnNoMessageIfAnyRuleFailsForExemptedSubstances()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();
			var segregationRule3 = new Mock<ISegregationRule>();
			segregationRule1.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message>());
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule1.Setup(x => x.IsExemption).Returns(false);

			segregationRule2.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message> { new Message(MessageType.Error, "Rule 2 failed") });
			segregationRule2.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule2.Setup(x => x.IsExemption).Returns(false);

			segregationRule3.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message>());
			segregationRule3.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule3.Setup(x => x.IsExemption).Returns(true);

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object, segregationRule3.Object };

			var standards = new string[] { "IMO" };
			var undgDataItemDtos = new UNDGDataItemDTO[] { _undgDataItemDto1, _undgDataItemDto2 };
			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(x => x.Find(It.IsAny<UNDGDataItemDTO>(), It.IsAny<string>())).Returns(TestHelper.MockFind);
			var segregationRulesManager = new SegregationRulesManager(segregationRules);
			var dgSubstancePairInfo = segregationRulesManager.Check(standards, undgDataItemDtos, databaseService.Object.Find);
			AssertEquals("There should be no substance pairs with error or warning", 0, dgSubstancePairInfo.Count());
		}

		public void TestCheckForUNDGDataItemDTOParam_ShouldReturnNoMessagesIfAllTheRulesPass()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();
			var segregationRule3 = new Mock<ISegregationRule>();
			segregationRule1.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message>());
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule1.Setup(x => x.IsExemption).Returns(false);

			segregationRule2.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message>());
			segregationRule2.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule2.Setup(x => x.IsExemption).Returns(false);

			segregationRule3.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message>());
			segregationRule3.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule3.Setup(x => x.IsExemption).Returns(false);

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object, segregationRule3.Object };

			var standards = new string[] { "IMO" };
			var undgDataItemDtos = new UNDGDataItemDTO[] { _undgDataItemDto1, _undgDataItemDto2 };
			var segregationRulesManager = new SegregationRulesManager(segregationRules);
			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(x => x.Find(It.IsAny<UNDGDataItemDTO>(), It.IsAny<string>())).Returns(TestHelper.MockFind);
			var dgSubstancePairInfo = segregationRulesManager.Check(standards, undgDataItemDtos, databaseService.Object.Find);
			AssertEquals("There should be no pairs with error or warning", 0, dgSubstancePairInfo.Count());
		}

		public void TestCheckForUNDGDataItemDTOParam_ShouldCombineMessagesFromAllTheRules()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();
			var segregationRule3 = new Mock<ISegregationRule>();

			var rule1Messages = new List<Message> {
				new Message(MessageType.Info, "Rule 1 notes"),
				new Message(MessageType.Warning, "Rule 1 warning")
			};
			segregationRule1.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(rule1Messages);
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule1.Setup(x => x.IsExemption).Returns(false);

			var rule2Messages = new List<Message> {
				new Message(MessageType.Error, "Rule 2 failed"),
				new Message(MessageType.Warning, "Rule 2 warning")
			};
			segregationRule2.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(rule2Messages);
			segregationRule2.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule2.Setup(x => x.IsExemption).Returns(false);

			segregationRule3.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message>());
			segregationRule3.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule3.Setup(x => x.IsExemption).Returns(false);

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object, segregationRule3.Object };

			var standards = new string[] { "IMO" };
			var undgDataItemDtos = new UNDGDataItemDTO[] { _undgDataItemDto1, _undgDataItemDto2 };
			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(x => x.Find(It.IsAny<UNDGDataItemDTO>(), It.IsAny<string>())).Returns(TestHelper.MockFind);
			var segregationRulesManager = new SegregationRulesManager(segregationRules);
			var dgSubstancePairInfo = segregationRulesManager.Check(standards, undgDataItemDtos, databaseService.Object.Find);
			AssertEquals("There should be four substance pairs", 4, dgSubstancePairInfo.Count());

			AssertEquals("UNDGDataItem 1 and 2 should form the first pair", (_undgDataItemDto1, _undgDataItemDto2), (dgSubstancePairInfo.First().Item1, dgSubstancePairInfo.First().Item2));
			AssertEquals("UNDGDataItem 1 and 2 should form the second pair", (_undgDataItemDto1, _undgDataItemDto2), (dgSubstancePairInfo.ElementAt(1).Item1, dgSubstancePairInfo.ElementAt(1).Item2));
			AssertEquals("UNDGDataItem 1 and 2 should form the thrid pair", (_undgDataItemDto1, _undgDataItemDto2), (dgSubstancePairInfo.ElementAt(2).Item1, dgSubstancePairInfo.ElementAt(2).Item2));
			AssertEquals("UNDGDataItem 1 and 2 should form the fourth pair", (_undgDataItemDto1, _undgDataItemDto2), (dgSubstancePairInfo.ElementAt(3).Item1, dgSubstancePairInfo.ElementAt(3).Item2));
			AssertEquals("The standard for the first pair should be IMO", UNDGSubstanceStandardTypes.IMO, dgSubstancePairInfo.First().Standard);
			AssertEquals("The standard for the second pair should be IMO", UNDGSubstanceStandardTypes.IMO, dgSubstancePairInfo.ElementAt(1).Standard);
			AssertEquals("The standard for the third pair should be IMO", UNDGSubstanceStandardTypes.IMO, dgSubstancePairInfo.ElementAt(2).Standard);
			AssertEquals("The standard for the fourth pair should be IMO", UNDGSubstanceStandardTypes.IMO, dgSubstancePairInfo.ElementAt(3).Standard);
			AssertEquals(MessageType.Info, dgSubstancePairInfo.First().Message.Type);
			AssertEquals("Rule 1 notes", dgSubstancePairInfo.First().Message.Text);
			AssertEquals(MessageType.Warning, dgSubstancePairInfo.ElementAt(1).Message.Type);
			AssertEquals("Rule 1 warning", dgSubstancePairInfo.ElementAt(1).Message.Text);
			AssertEquals(MessageType.Error, dgSubstancePairInfo.ElementAt(2).Message.Type);
			AssertEquals("Rule 2 failed", dgSubstancePairInfo.ElementAt(2).Message.Text);
			AssertEquals(MessageType.Warning, dgSubstancePairInfo.ElementAt(3).Message.Type);
			AssertEquals("Rule 2 warning", dgSubstancePairInfo.ElementAt(3).Message.Text);
		}

		public void TestCheckForUNDGDataItemDtoParam_ShouldReturnOnlyInfoAndWarningMessagesFromAllTheRules_ForExemptedSubstances()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();
			var segregationRule3 = new Mock<ISegregationRule>();

			var rule1Messages = new List<Message> {
				new Message(MessageType.Info, "Rule 1 notes"),
				new Message(MessageType.Warning, "Rule 1 warning")
			};
			segregationRule1.Setup(x => x.Check(It.IsAny<UNDGSubstance>(),It.IsAny<UNDGSubstance>())).Returns(rule1Messages);
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule1.Setup(x => x.IsExemption).Returns(false);

			var rule2Messages = new List<Message> {
				new Message(MessageType.Error, "Rule 2 failed"),
				new Message(MessageType.Warning, "Rule 2 warning")
			};
			segregationRule2.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(rule2Messages);
			segregationRule2.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule2.Setup(x => x.IsExemption).Returns(false);

			segregationRule3.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message>());
			segregationRule3.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule3.Setup(x => x.IsExemption).Returns(true);

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object, segregationRule3.Object };

			var standards = new string[] { "IMO" };
			var undgDataItemDtos = new UNDGDataItemDTO[] { _undgDataItemDto1, _undgDataItemDto2 };
			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(x => x.Find(It.IsAny<UNDGDataItemDTO>(), It.IsAny<string>())).Returns(TestHelper.MockFind);
			var segregationRulesManager = new SegregationRulesManager(segregationRules);
			var dgSubstancePairInfo = segregationRulesManager.Check(standards, undgDataItemDtos, databaseService.Object.Find);
			AssertEquals("There should be four substance pairs", 3, dgSubstancePairInfo.Count());

			AssertEquals("Substance1 and Substance2 should form the first pair", (_undgDataItemDto1, _undgDataItemDto2), (dgSubstancePairInfo.First().Item1, dgSubstancePairInfo.First().Item2));
			AssertEquals("Substance1 and Substance2 should form the second pair", (_undgDataItemDto1, _undgDataItemDto2), (dgSubstancePairInfo.ElementAt(1).Item1, dgSubstancePairInfo.ElementAt(1).Item2));
			AssertEquals("Substance1 and Substance2 should form the third pair", (_undgDataItemDto1, _undgDataItemDto2), (dgSubstancePairInfo.ElementAt(2).Item1, dgSubstancePairInfo.ElementAt(2).Item2));
			AssertEquals("The standard for the first pair should be IMO", UNDGSubstanceStandardTypes.IMO, dgSubstancePairInfo.First().Standard);
			AssertEquals("The standard for the second pair should be IMO", UNDGSubstanceStandardTypes.IMO, dgSubstancePairInfo.ElementAt(1).Standard);
			AssertEquals("The standard for the third pair should be IMO", UNDGSubstanceStandardTypes.IMO, dgSubstancePairInfo.ElementAt(2).Standard);
			AssertEquals(MessageType.Info, dgSubstancePairInfo.First().Message.Type);
			AssertEquals("Rule 1 notes", dgSubstancePairInfo.First().Message.Text);
			AssertEquals(MessageType.Warning, dgSubstancePairInfo.ElementAt(1).Message.Type);
			AssertEquals("Rule 1 warning", dgSubstancePairInfo.ElementAt(1).Message.Text);
			AssertEquals(MessageType.Warning, dgSubstancePairInfo.ElementAt(2).Message.Type);
			AssertEquals("Rule 2 warning", dgSubstancePairInfo.ElementAt(2).Message.Text);
		}

		public void TestCheckForUNDGDataItemDTOParam_ShouldCallRulesWithTheMatchingStandardForEachPairOfIds()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();
			var segregationRule3 = new Mock<ISegregationRule>();
			segregationRule1.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message>());
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule1.Setup(x => x.IsExemption).Returns(false);

			segregationRule2.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message>());
			segregationRule2.Setup(x => x.ApplicableStandard).Returns("IATA");
			segregationRule2.Setup(x => x.IsExemption).Returns(false);

			segregationRule3.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message>());
			segregationRule3.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule3.Setup(x => x.IsExemption).Returns(false);

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object, segregationRule3.Object };
			var segregationRulesManager = new SegregationRulesManager(segregationRules);

			var undgSubstance1 = Factory.NewWithValidTestData<UNDGSubstance>();
			var undgSubstance2 = Factory.NewWithValidTestData<UNDGSubstance>();
			var undgSubstance3 = Factory.NewWithValidTestData<UNDGSubstance>();
			var undgSubstance4 = Factory.NewWithValidTestData<UNDGSubstance>();
			var undgDataItemDto1 = new UNDGDataItemDTO
			{
				UNDGSubstanceDTOs = new[]
				{
					new UNDGSubstanceDTO { Unno = undgSubstance1.DG_UNNO, Variant = undgSubstance1.DG_Variant, Standard = undgSubstance1.DG_Standard },
				}
			};
			var undgDataItemDto2 = new UNDGDataItemDTO
			{
				UNDGSubstanceDTOs = new[]
				{
					new UNDGSubstanceDTO { Unno = undgSubstance2.DG_UNNO, Variant = undgSubstance2.DG_Variant, Standard = undgSubstance2.DG_Standard },
				}
			};
			var undgDataItemDto3 = new UNDGDataItemDTO
			{
				UNDGSubstanceDTOs = new[]
				{
					new UNDGSubstanceDTO { Unno = undgSubstance3.DG_UNNO, Variant = undgSubstance3.DG_Variant, Standard = undgSubstance3.DG_Standard },
				}
			};
			var undgDataItemDto4 = new UNDGDataItemDTO
			{
				UNDGSubstanceDTOs = new[]
				{
					new UNDGSubstanceDTO { Unno = undgSubstance4.DG_UNNO, Variant = undgSubstance4.DG_Variant, Standard = undgSubstance4.DG_Standard },
				}
			};

			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(f => f.Find(undgDataItemDto1, It.IsAny<string>())).Returns((undgDataItemDto1.UNDGClassificationData, undgSubstance1, string.Empty));
			databaseService.Setup(f => f.Find(undgDataItemDto2, It.IsAny<string>())).Returns((undgDataItemDto2.UNDGClassificationData, undgSubstance2, string.Empty));
			databaseService.Setup(f => f.Find(undgDataItemDto3, It.IsAny<string>())).Returns((undgDataItemDto3.UNDGClassificationData, undgSubstance3, string.Empty));
			databaseService.Setup(f => f.Find(undgDataItemDto4, It.IsAny<string>())).Returns((undgDataItemDto4.UNDGClassificationData, undgSubstance4, string.Empty));

			var dgSubstancePairInfo = segregationRulesManager.Check(new string[] { "IMO" }, new UNDGDataItemDTO[] { undgDataItemDto1, undgDataItemDto2, undgDataItemDto3, undgDataItemDto4 }, databaseService.Object.Find);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance1, undgSubstance2), Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance1, undgSubstance3), Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance1, undgSubstance4), Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance2, undgSubstance3), Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance2, undgSubstance4), Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance3, undgSubstance4), Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.ApplicableStandard, Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.IsExemption, Times.Exactly(2));
			segregationRule1.VerifyNoOtherCalls();

			segregationRule2.Verify(segregationRule2 => segregationRule2.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>()), Times.Never);

			segregationRule3.Verify(segregationRule3 => segregationRule3.Check(undgSubstance1, undgSubstance2), Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.Check(undgSubstance1, undgSubstance3), Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.Check(undgSubstance1, undgSubstance4), Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.Check(undgSubstance2, undgSubstance3), Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.Check(undgSubstance2, undgSubstance4), Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.Check(undgSubstance3, undgSubstance4), Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.ApplicableStandard, Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.IsExemption, Times.Exactly(2));
			segregationRule3.VerifyNoOtherCalls();

			AssertEquals("There should be no pairs with error or warning", 0, dgSubstancePairInfo.Count());
		}

		public void TestCheckForUNDGDataItemDTOParam_ShouldCallRulesWithTheMatchingStandardForEachPairOfIds_ForExemptedSubstances()
		{
			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();
			var segregationRule3 = new Mock<ISegregationRule>();
			segregationRule1.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message> { new Message(MessageType.Error, "Rule failed") });
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule1.Setup(x => x.IsExemption).Returns(false);

			segregationRule2.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message>());
			segregationRule2.Setup(x => x.ApplicableStandard).Returns("IATA");
			segregationRule2.Setup(x => x.IsExemption).Returns(false);

			segregationRule3.Setup(x => x.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>())).Returns(new List<Message>());
			segregationRule3.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule3.Setup(x => x.IsExemption).Returns(true);

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object, segregationRule3.Object };
			var segregationRulesManager = new SegregationRulesManager(segregationRules);

			var undgSubstance1 = Factory.NewWithValidTestData<UNDGSubstance>();
			var undgSubstance2 = Factory.NewWithValidTestData<UNDGSubstance>();
			var undgSubstance3 = Factory.NewWithValidTestData<UNDGSubstance>();
			var undgSubstance4 = Factory.NewWithValidTestData<UNDGSubstance>();
			var undgDataItemDto1 = new UNDGDataItemDTO
			{
				UNDGSubstanceDTOs = new[]
				{
					new UNDGSubstanceDTO { Unno = undgSubstance1.DG_UNNO, Variant = undgSubstance1.DG_Variant, Standard = undgSubstance1.DG_Standard },
				}
			};
			var undgDataItemDto2 = new UNDGDataItemDTO
			{
				UNDGSubstanceDTOs = new[]
				{
					new UNDGSubstanceDTO { Unno = undgSubstance2.DG_UNNO, Variant = undgSubstance2.DG_Variant, Standard = undgSubstance2.DG_Standard },
				}
			};
			var undgDataItemDto3 = new UNDGDataItemDTO
			{
				UNDGSubstanceDTOs = new[]
				{
					new UNDGSubstanceDTO { Unno = undgSubstance3.DG_UNNO, Variant = undgSubstance3.DG_Variant, Standard = undgSubstance3.DG_Standard },
				}
			};
			var undgDataItemDto4 = new UNDGDataItemDTO
			{
				UNDGSubstanceDTOs = new[]
				{
					new UNDGSubstanceDTO { Unno = undgSubstance4.DG_UNNO, Variant = undgSubstance4.DG_Variant, Standard = undgSubstance4.DG_Standard },
				}
			};

			var databaseService = new Mock<IDatabaseService>();
			databaseService.Setup(f => f.Find(undgDataItemDto1, It.IsAny<string>())).Returns((undgDataItemDto1.UNDGClassificationData, undgSubstance1, string.Empty));
			databaseService.Setup(f => f.Find(undgDataItemDto2, It.IsAny<string>())).Returns((undgDataItemDto2.UNDGClassificationData, undgSubstance2, string.Empty));
			databaseService.Setup(f => f.Find(undgDataItemDto3, It.IsAny<string>())).Returns((undgDataItemDto3.UNDGClassificationData, undgSubstance3, string.Empty));
			databaseService.Setup(f => f.Find(undgDataItemDto4, It.IsAny<string>())).Returns((undgDataItemDto4.UNDGClassificationData, undgSubstance4, string.Empty));

			var dgSubstancePairInfo = segregationRulesManager.Check(new string[] { "IMO" }, new UNDGDataItemDTO[] { undgDataItemDto1, undgDataItemDto2, undgDataItemDto3, undgDataItemDto4 }, databaseService.Object.Find);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance1, undgSubstance2), Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance1, undgSubstance3), Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance1, undgSubstance4), Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance2, undgSubstance3), Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance2, undgSubstance4), Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.Check(undgSubstance3, undgSubstance4), Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.ApplicableStandard, Times.Once);
			segregationRule1.Verify(segregationRule1 => segregationRule1.IsExemption, Times.Exactly(2));
			segregationRule1.VerifyNoOtherCalls();

			segregationRule2.Verify(segregationRule2 => segregationRule2.Check(It.IsAny<UNDGSubstance>(), It.IsAny<UNDGSubstance>()), Times.Never);

			segregationRule3.Verify(segregationRule3 => segregationRule3.Check(undgSubstance1, undgSubstance2), Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.Check(undgSubstance1, undgSubstance3), Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.Check(undgSubstance1, undgSubstance4), Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.Check(undgSubstance2, undgSubstance3), Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.Check(undgSubstance2, undgSubstance4), Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.Check(undgSubstance3, undgSubstance4), Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.ApplicableStandard, Times.Once);
			segregationRule3.Verify(segregationRule3 => segregationRule3.IsExemption, Times.Exactly(2));
			segregationRule3.VerifyNoOtherCalls();

			AssertEquals("There should be no pairs with error or warning", 0, dgSubstancePairInfo.Count());
		}

		public void TestCheckForUNDGDataItemDTOWithPreloadData()
		{
			var oldFactory = new BusinessObjectFactory();
			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();
			var segregationRule3 = new Mock<ISegregationRule>();
			segregationRule1.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message>());
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule1.Setup(x => x.IsExemption).Returns(false);

			segregationRule2.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message>());
			segregationRule2.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule2.Setup(x => x.IsExemption).Returns(false);

			segregationRule3.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message>());
			segregationRule3.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule3.Setup(x => x.IsExemption).Returns(false);

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object, segregationRule3.Object };

			var undgSubstance1 = TestHelper.CreateUNDGSubstance(oldFactory, "1111", "1", "codeA", "vA");
			var undgSubstance3 = TestHelper.CreateUNDGSubstance(oldFactory, "1111", "1", "codeA", "vC");
			var undgSubstance2 = TestHelper.CreateUNDGSubstance(oldFactory, "2222", "2", "codeB", "vB");
			var undgSubstance4 = TestHelper.CreateUNDGSubstance(oldFactory, "2222", "2", "codeB", "vD");

			var undgDataItemDto1 = new UNDGDataItemDTO
			{
				UNDGSubstanceDTOs = new[]
				{
					new UNDGSubstanceDTO { Unno = "1111", Variant = "vA", Standard = "IMO" },
					new UNDGSubstanceDTO { Unno = "1111", Variant = "vC", Standard = "IMO" },
				}
			};

			var undgDataItemDto2 = new UNDGDataItemDTO
			{
				UNDGSubstanceDTOs = new[]
				{
					new UNDGSubstanceDTO { Unno = "2222", Variant = "vB", Standard = "IMO" },
					new UNDGSubstanceDTO { Unno = "2222", Variant = "vD", Standard = "IMO" },
				},
			};
			var undgDataItemDtos = new UNDGDataItemDTO[] { undgDataItemDto1, undgDataItemDto2 };
			var standards = new string[] { "IMO" };

			oldFactory.Save();

			var newFactory = new BusinessObjectFactory();
			var databaseService = new DatabaseService(newFactory);
			var segregationRulesManager = new SegregationRulesManager(segregationRules);

			databaseService.PreloadData(undgDataItemDtos);
			var expectedDbHitsNoPrint = new Dictionary<string, int>()
			{
				{ UNDGSubstanceSchema.Constants.TableName, 1 },
			};
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHitsNoPrint, newFactory))
			{
				var dgSubstancePairInfo = segregationRulesManager.Check(standards, undgDataItemDtos, databaseService.Find);
				AssertEquals("There should be no substance pairs with error or warning", 0, dgSubstancePairInfo.Count());
			}
		}

		public void TestCheckForGuidWithPreloadData()
		{
			var oldFactory = new BusinessObjectFactory();
			var shipment = CreateShipment(oldFactory);
			var packline = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();

			var segregationRule1 = new Mock<ISegregationRule>();
			var segregationRule2 = new Mock<ISegregationRule>();
			var segregationRule3 = new Mock<ISegregationRule>();
			segregationRule1.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message>());
			segregationRule1.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule1.Setup(x => x.IsExemption).Returns(false);

			segregationRule2.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message>());
			segregationRule2.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule2.Setup(x => x.IsExemption).Returns(false);

			segregationRule3.Setup(x => x.Check(_undgSubstance1, _undgSubstance2)).Returns(new List<Message>());
			segregationRule3.Setup(x => x.ApplicableStandard).Returns("IMO");
			segregationRule3.Setup(x => x.IsExemption).Returns(false);

			var segregationRules = new List<ISegregationRule> { segregationRule1.Object, segregationRule2.Object, segregationRule3.Object };

			var undgSubstance1 = TestHelper.CreateUNDGSubstance(oldFactory, "1111", "1", "codeA", "vA");
			var undgDataItem1 = CreateUNDGDataItem(packline, undgSubstance1, 1, 1);
			var undgSubstance2 = TestHelper.CreateUNDGSubstance(oldFactory, "2222", "2", "codeB", "vB");
			var undgDataItem2 = CreateUNDGDataItem(packline, undgSubstance2, 1, 1);
			var undgSubstance3 = TestHelper.CreateUNDGSubstance(oldFactory, "3333", "3", "codeC", "vC");
			var undgDataItem3 = CreateUNDGDataItem(packline, undgSubstance3, 1, 1);
			var undgSubstance4 = TestHelper.CreateUNDGSubstance(oldFactory, "4444", "4", "codeD", "vD");
			var undgDataItem4 = CreateUNDGDataItem(packline, undgSubstance4, 1, 1);

			var id1 = undgDataItem1.PK.ToGuid();
			var id2 = undgDataItem2.PK.ToGuid();
			var id3 = undgDataItem3.PK.ToGuid();
			var id4 = undgDataItem4.PK.ToGuid();

			var ids = new Guid[] { id1, id2, id3, id4 };
			var standards = new string[] { "IMO" };

			undgDataItem1.SubstancePK = undgSubstance1.PK;
			undgDataItem2.SubstancePK = undgSubstance2.PK;
			undgDataItem3.SubstancePK = undgSubstance3.PK;
			undgDataItem4.SubstancePK = undgSubstance4.PK;

			oldFactory.Save();

			var newFactory = new BusinessObjectFactory();
			var databaseService = new DatabaseService(newFactory);
			var segregationRulesManager = new SegregationRulesManager(segregationRules);

			databaseService.PreloadData(ids, standards);
			var expectedDbHitsNoPrint = new Dictionary<string, int>()
			{
				{ UNDGDataItemSchema.Constants.TableName, 1 },
				{ UNDGSubstancePivotSchema.Constants.TableName, 1 },
				{ UNDGSubstanceSchema.Constants.TableName, 1 },
			};
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHitsNoPrint, newFactory))
			{
				var dgSubstancePairInfo = segregationRulesManager.Check(standards, ids, databaseService.Find);
				AssertEquals("There should be no substance pairs with error or warning", 0, dgSubstancePairInfo.Count());
			}
		}

		public UNDGDataItem CreateUNDGDataItem(ForwardingPackLine packLine, UNDGSubstance undgSubstance, decimal weight, decimal volume, string unitOfWeight = "KG", string unitOfVolume = "M3")
		{
			var uNDGDataItem = packLine.UNDGs.AddNew();
			uNDGDataItem.DI_DG = undgSubstance.PK;
			uNDGDataItem.DI_IMOClass = undgSubstance.DG_Class;
			uNDGDataItem.DI_DGWeight = weight;
			uNDGDataItem.DI_UnitOfWeight = unitOfWeight;
			uNDGDataItem.DI_DGVolume = volume;
			uNDGDataItem.DI_UnitOfVolume = unitOfVolume;
			return uNDGDataItem;
		}

		ForwardingShipment CreateShipment(BusinessObjectFactory factory)
		{
			var shipment = factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_HouseBill = "HOUSEBILL001";

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";

			var shipper = factory.New<OrgHeader>();
			shipper.OH_FullName = "MAERSK";
			shipper.OH_RL_NKClosestPort = "AUSYD";
			shipper.MainAddress.Address1 = "Unit 13";
			shipper.MainAddress.Address2 = "4 Lost Lane";
			shipper.MainAddress.City = "Sydney";
			shipper.MainAddress.Postcode = "2000";
			shipper.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = factory.New<OrgHeader>();
			consignee.OH_FullName = "MR Consignee";
			consignee.OH_RL_NKClosestPort = "SGSIN";
			consignee.MainAddress.Address1 = "Unit 1";
			consignee.MainAddress.Address2 = "4 What Lane";
			consignee.MainAddress.City = "Auckland";
			consignee.MainAddress.Postcode = "5022";
			consignee.MainAddress.OA_RN_NKCountryCode = "SG";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_UniqueConsignRef = "CONSOL0001";
			departureConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			departureConsol.JK_RL_NKLoadPort = "AUSYD";
			departureConsol.JK_RL_NKDischargePort = "MYKUL";
			departureConsol.JK_BookingReference = "BKG001";
			departureConsol.JK_MasterBillNum = "081-0000001";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_UniqueConsignRef = "CONSOL0002";
			arrivalConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			arrivalConsol.JK_RL_NKLoadPort = "MYKUL";
			arrivalConsol.JK_RL_NKDischargePort = "SGSIN";
			arrivalConsol.JK_BookingReference = "BKG002";
			arrivalConsol.JK_MasterBillNum = "001-0000001";

			var container = departureConsol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA";

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 1;
			packline1.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packline1.JL_ActualWeight = 2000;
			packline1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packline1.JL_ActualVolume = 1.3;
			packline1.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;

			container.PackLines.Add(packline1);

			var contact = factory.New<OrgContact>();
			contact.OC_ContactName = "Pumpernickel";
			contact.OC_Phone = "8000 1234";
			contact.OC_OH = shipper.PK;

			factory.Save();

			return shipment;
		}
	}
}
