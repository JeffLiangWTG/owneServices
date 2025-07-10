using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class CustomsContainerModeConverterTest : TestCase
	{
		public void TestConvert_ShouldReturnCodeIfExistsInList()
		{
			var containerModeList = new CodeDescriptionPairList();
			containerModeList.AddPair("AAA");
			var converter = new CustomsContainerModeConverter<Shipment>(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), containerModeList, new ContainerMode { Code = "AAA" }, null, null);
			AssertEquals("AAA", converter.Convert().Code);
		}

		public void TestConvert_ShouldConvertBCNToFCX()
		{
			var containerModeList = new CodeDescriptionPairList();
			containerModeList.AddPair(Core.Constants.ContainerModes.AIR);
			containerModeList.AddPair(Core.Constants.ContainerModes.FCLMixedShipper);
			var converter = new CustomsContainerModeConverter<Shipment>(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), containerModeList, new ContainerMode { Code = Core.Constants.ContainerModes.BuyersConsol }, null, null);
			AssertEquals(Core.Constants.ContainerModes.FCLMixedShipper, converter.Convert().Code);
		}

		public void TestConvert_ShouldReturnCodeFromSubConverter()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var subContainerModeList = new CodeDescriptionPairList();
			subContainerModeList.AddPair("AAA");
			var subConverter = new CustomsContainerModeConverter<Shipment>(shipment, subContainerModeList, new ContainerMode { Code = "AAA" }, null, null);
			AssertEquals("AAA", subConverter.Convert().Code);

			var containerModeList = new CodeDescriptionPairList();
			var converter = new CustomsContainerModeConverter<Shipment>(shipment, containerModeList, null, _ => subConverter, null);
			AssertEquals("AAA", converter.Convert().Code);
		}

		public void TestConvert_ShouldApplyFallback_AndReturnCNTIfExistsInList()
		{
			var containerModeList = new CodeDescriptionPairList();
			containerModeList.AddPair(Core.Constants.ContainerModes.AIR);
			containerModeList.AddPair(Core.Constants.ContainerModes.Containerised);
			var converter = new CustomsContainerModeConverter<Shipment>(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), containerModeList, null, null, _ => true);
			AssertEquals(Core.Constants.ContainerModes.Containerised, converter.Convert().Code);
		}

		public void TestConvert_ShouldApplyFallback_AndReturnLCLIfExistsInList()
		{
			var containerModeList = new CodeDescriptionPairList();
			containerModeList.AddPair(Core.Constants.ContainerModes.AIR);
			containerModeList.AddPair(Core.Constants.ContainerModes.LCL);
			var converter = new CustomsContainerModeConverter<Shipment>(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), containerModeList, null, null, _ => true);
			AssertEquals(Core.Constants.ContainerModes.LCL, converter.Convert().Code);
		}

		public void TestConvert_CNTIsPriorToLCL_WhenApplyFallback()
		{
			var containerModeList = new CodeDescriptionPairList();
			containerModeList.AddPair(Core.Constants.ContainerModes.LCL);
			containerModeList.AddPair(Core.Constants.ContainerModes.Containerised);
			var converter = new CustomsContainerModeConverter<Shipment>(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), containerModeList, null, null, _ => true);
			AssertEquals(Core.Constants.ContainerModes.Containerised, converter.Convert().Code);
		}

		public void TestConvert_ShouldReturnNull_IfNoRequirementIsSatisfied()
		{
			var converter = new CustomsContainerModeConverter<Shipment>(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), new CodeDescriptionPairList(), null, null, null);
			AssertNull(converter.Convert());
		}

		public void TestConvert_ShouldReturnAsInput_IfNotFound()
		{
			var converter = new CustomsContainerModeConverter<Shipment>(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), new CodeDescriptionPairList(), new ContainerMode { Code = "JEG" }, null, null);
			AssertEquals("JEG", converter.Convert().Code);
		}
	}
}
