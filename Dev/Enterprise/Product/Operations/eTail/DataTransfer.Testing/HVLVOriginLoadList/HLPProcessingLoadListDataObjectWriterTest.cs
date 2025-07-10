using CargoWise.EntityFramework.Testing;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing;

[TestsSubclassesOf(typeof(HLPProcessingLoadListDataObjectWriter))]
abstract class HLPProcessingLoadListDataObjectWriterTest : TestCaseWithFactory
{
	public void TestPopulateContainerMode_TransportModeIsSea()
	{
		AssertPopulateContainerMode(TransportModes.Sea, "", ContainerModes.LCL);
	}

	public void TestPopulateContainerMode_TransportModeIsRail()
	{
		AssertPopulateContainerMode(TransportModes.Rail, "", ContainerModes.LCL);
	}

	public void TestPopulateContainerMode_TransportModeIsAir()
	{
		AssertPopulateContainerMode(TransportModes.Air, "", ContainerModes.Loose);
	}

	public void TestPopulateContainerMode_TransportModeIsAirAndContainerNumberIsSpecified()
	{
		AssertPopulateContainerMode(TransportModes.Air, "CON001", ContainerModes.ULD);
	}

	public void TestPopulateContainerMode_TransportModeIsRoad()
	{
		AssertPopulateContainerMode(TransportModes.Road, "", ContainerModes.LTL);
	}

	void AssertPopulateContainerMode(string transportMode, string containerNumber, string expectedContainerCode)
	{
		var loadList = Factory.New<HVLVOriginLoadList>();
		loadList.HVL_TransportMode = transportMode;
		loadList.HVL_ContainerNumber = containerNumber;

		var dataObject = GetDataObject(loadList);
		AssertEquals(expectedContainerCode, dataObject.ContainerMode?.Code);
	}

	protected abstract Shipment GetDataObject(HVLVOriginLoadList loadList);
}
