using System;
using System.Linq;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC044CConsignmentProviderTest : DataProviderTestCase<CC044CConsignmentProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsArrivalMovementHeader", "Value cannot be null.\r\nParameter name: movementHeader", () => new CC044CConsignmentProvider(null, null));
			AssertExceptionThrown<ArgumentNullException>("Null NctsHeader", "Value cannot be null.\r\nParameter name: NctsArrivalMovementHeader.Header", () => new CC044CConsignmentProvider(Factory.New<NctsArrivalMovementHeader>(), null));
		});
	}

	public void TestGrossMass_OusideTransitionPeriod()
	{
		const decimal testGrossMass = 123456789.123m;
		TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
		{
			movementHeader.BM_GrossWeightUnloaded = testGrossMass;
			movementHeader.BM_NoChangesToReport = ZBool.False;
			AssertEquals("GrossMass in Kgs is not restricted by rules for transition period", 123456789.123m, Provider.GrossMass);
		});
	}

	public void TestGrossMass_InTransitionPeriod()
	{
		const decimal testGrossMass = 123456789.123m;
		TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
		{
			movementHeader.BM_GrossWeightUnloaded = testGrossMass;
			movementHeader.BM_NoChangesToReport = ZBool.False;
			AssertEquals("GrossMass in Kgs rounded by rules for transition period", 123456789.12m, Provider.GrossMass);
		});
	}

	public void TestGrossMass_NoDiscrepancies()
	{
		movementHeader.BM_GrossWeightUnloaded = 1000m;
		movementHeader.BM_NoChangesToReport = ZBool.True;
		AssertNull(Provider.GrossMass);
	}

	public void TestTransportEquipment()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty List", 0, Provider.TransportEquipment.Count);

			nctsHeader.ArrivalHeaderContainers.AddNew();
			nctsHeader.ArrivalHeaderContainers.AddNew();

			AssertEquals("2 HeaderContainers", 2, GetProvider().TransportEquipment.Count);
			AssertEquals("1st HeaderContainer sequenceNumber", "1", GetProvider().TransportEquipment.First().SequenceNumber);
			AssertEquals("2nd HeaderContainer sequenceNumber", "2", GetProvider().TransportEquipment.Last().SequenceNumber);
		});
	}

	public void TestDepartureTransportMeans()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty DepartureTransportMeans", 0, Provider.DepartureTransportMeans.Count);

			movementHeader.ArrivalTransportInfos.AddNew();
			movementHeader.ArrivalTransportInfos.AddNew();

			AssertEquals("Should have 2 DepartureTransportMeans", 2, GetProvider().DepartureTransportMeans.Count);
			AssertEquals("Sequence number should start from 1", "1", GetProvider().DepartureTransportMeans.First().SequenceNumber);
			AssertEquals("Sequence number for 2nd DepartureTransportMeans should be 2", "2", GetProvider().DepartureTransportMeans.Last().SequenceNumber);
		});
	}

	public void TestHouseConsignment()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty HouseConsignment", 0, Provider.HouseConsignment.Count);

			nctsHeader.Bills.AddNew();
			nctsHeader.Bills.AddNew();

			AssertEquals("Should have 2 HouseConsignments", 2, GetProvider().HouseConsignment.Count);
			AssertEquals("Sequence number should start from 1", "1", GetProvider().HouseConsignment.First().SequenceNumber);
			AssertEquals("Sequence number for 2nd HouseConsignments should be 2", "2", GetProvider().HouseConsignment.Last().SequenceNumber);
		});
	}

	public void TestSupportingDocument()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty SupportingDocuments", 0, Provider.SupportingDocument.Count);

			movementHeader.SupportingDocuments.AddNew();
			movementHeader.SupportingDocuments.AddNew();

			AssertEquals("Not empty SupportingDocuments", 2, GetProvider().SupportingDocument.Count);
			AssertEquals("1st item should start with sequenceNumber 1", "1", GetProvider().SupportingDocument.First().SequenceNumber);
			AssertEquals("2nd item should have sequenceNumber 2", "2", GetProvider().SupportingDocument.Last().SequenceNumber);
		});
	}

	public void TestTransportDocument()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty TRA AdditionalInfos", 0, Provider.TransportDocument.Count);

			AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.TransportDocument);
			AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.TransportDocument);

			AssertEquals("Not Empty TRA AdditionalInfos", 2, GetProvider().TransportDocument.Count);
			AssertEquals("1st item should start with sequenceNumber 1", "1", GetProvider().TransportDocument.First().SequenceNumber);
			AssertEquals("2nd item should have sequenceNumber 2", "2", GetProvider().TransportDocument.Last().SequenceNumber);

			AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.AdditionalReference);
			AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.AdditionalInformation);
			AddAdditionalInfosDocumentOfType(ZString.Empty);
		});
	}

	public void TestAdditionalReference()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty REF AdditionalInfos", 0, Provider.AdditionalReference.Count);

			AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.AdditionalReference);
			AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.AdditionalReference);

			AssertEquals("Not Empty REF AdditionalInfos", 2, GetProvider().AdditionalReference.Count);
			AssertEquals("1st item should start with sequenceNumber 1", "1", GetProvider().AdditionalReference.First().SequenceNumber);
			AssertEquals("2nd item should have sequenceNumber 2", "2", GetProvider().AdditionalReference.Last().SequenceNumber);

			AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.TransportDocument);
			AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.AdditionalInformation);
			AddAdditionalInfosDocumentOfType(ZString.Empty);

			AssertEquals("Only REF AdditionalInfos should be included", 2, GetProvider().AdditionalReference.Count);
		});
	}

	void AddAdditionalInfosDocumentOfType(ZString subType)
	{
		var document = movementHeader.AdditionalDocuments.AddNew();
		document.CSI_SubType = subType;
	}

	ICC044C GetCC044CProvider() => new CC044CProvider(movementHeader, "CC044C");

	protected override CC044CConsignmentProvider GetProvider()
	{
		return new CC044CConsignmentProvider(movementHeader, GetCC044CProvider());
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		movementHeader = nctsHeader.ArrivalMovementHeader;
	}

	NctsHeader nctsHeader;
	NctsArrivalMovementHeader movementHeader;
}
