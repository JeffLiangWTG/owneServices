using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(CC014CProvider))]
sealed class CC014CProviderTest : MessageHeaderProviderAbstractTest<CC014CProvider>
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new CC014CProvider(null));

	public void TestMRN()
	{
		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "MRN";
		AssertEquals("MRN", Provider.MRN);
	}

	public void TestLRN() => CombineAssertions(() =>
	{
		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "MRN";
		nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRNOfTheNCT";
		AssertNullOrEmpty("When MRN is filled, LRN must be empty", Provider.LRN);
		nctsHeader.ArrivalMrnFromUser = ZString.Empty;
		AssertEquals("When MRN is empty, LRN must be filled", "LRNOfTheNCT", Provider.LRN);
	});

	public void TestHolderOfTheTransitProcedure() => CombineAssertions(() =>
	{
		AssertNotNull("Not Null", Provider.HolderOfTheTransitProcedure);
		AssertType<HolderOfTheTransitProcedureProvider>("Type", Provider.HolderOfTheTransitProcedure);
	});

	public void TestCustomsOfficeOfDeparture()
	{
		var movementHeader = nctsHeader.MovementHeader;
		var customsOfficeOfDeparture = movementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
		customsOfficeOfDeparture.CY_Data = "DepID";

		AssertEquals("DepID", Provider.CustomsOfficeOfDeparture);
	}

	public void TestInvalidationRequestDateAndTime() => CombineAssertions(() =>
	{
		AssertLessThan(Math.Abs((ZDateTime.Now.ToDateTime() - Provider.InvalidationRequestDateAndTime).TotalSeconds), 5d);
		AssertEquals(0, Provider.InvalidationRequestDateAndTime.Millisecond);
	});

	public void TestInvalidationDecisionDateAndTime_NCTS() => CombineAssertions(() =>
	{
		var messageVersionRegistryCollection = new MessageVersionRegistryCollection
		{
			new MessageVersionRegistry { DomainCode = MessageVersionRegistry.NCTSP5DomainCode, TargetSystemName = "NCTS.NL" }
		};
		using (NLCustomsRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
		{
			AssertLessThan(Math.Abs((ZDateTime.Now.ToDateTime() - Provider.InvalidationDecisionDateAndTime).TotalSeconds), 5d);
			AssertEquals(0, Provider.InvalidationDecisionDateAndTime.Millisecond);
		}
	});

	public void TestInvalidationDecisionDateAndTime_NTA()
	{
		var messageVersionRegistryCollection = new MessageVersionRegistryCollection
		{
			new MessageVersionRegistry { DomainCode = MessageVersionRegistry.NCTSP5DomainCode, TargetSystemName = "NTA" }
		};
		using (NLCustomsRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
		{
			AssertEquals(DateTime.MinValue, Provider.InvalidationDecisionDateAndTime);
		}
	}

	public void TestInvalidationDecision_NCTS()
	{
		var messageVersionRegistryCollection = new MessageVersionRegistryCollection
		{
			new MessageVersionRegistry { DomainCode = MessageVersionRegistry.NCTSP5DomainCode, TargetSystemName = "NCTS.BE" }
		};
		using (NLCustomsRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
		{
			AssertEquals(true, Provider.InvalidationDecision);
		}
	}

	public void TestInvalidationDecision_NTA()
	{
		var messageVersionRegistryCollection = new MessageVersionRegistryCollection
		{
			new MessageVersionRegistry { DomainCode = MessageVersionRegistry.NCTSP5DomainCode, TargetSystemName = "NTA" }
		};
		using (NLCustomsRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
		{
			AssertEquals(false, Provider.InvalidationDecision);
		}
	}

	public void TestInvalidationInitiatedByCustoms()
	{
		AssertEquals(false, Provider.InvalidationInitiatedByCustoms);
	}

	public void TestInvalidationJustification()
	{
		action.Justification = "K=V*InvalidationJustification=InvalidationJustificationText*O=V";
		AssertEquals("K=V*InvalidationJustification=InvalidationJustificationText*O=V", Provider.InvalidationJustification);
	}

	protected override bool HasSendingActionParameter => true;

	protected override string MessageType => NLConstants.WCoTypeCodes.DeclarationInvalidationRequest;

	protected override string MovementType => NctsMovementType.Codes.Departure;
}
