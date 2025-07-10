using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingBillOfLadingNumberGeneratorTargetTest : NumberGeneratorTargetTest
	{
		public void TestSelectionAndParameters()
		{
			var mock = new Mock<IBillGenerationSupport>();
			mock.Setup(m => m.Factory).Returns(Factory);
			mock.Setup(m => m.TransportMode).Returns("");
			mock.Setup(m => m.ServiceLevel).Returns("");

			Set(FreightDataRegistry.Instance.HouseBillNumberCustomisation, "", "111");
			Set(FreightDataRegistry.Instance.HouseBillNumberCustomisation_SEA, "STD", "222");
			Set(FreightDataRegistry.Instance.HouseBillNumberCustomisation_AIR, "D2D", "333");
			Set(FreightDataRegistry.Instance.HouseBillNumberCustomisation_RAIL, "DEF", "444");
			Set(FreightDataRegistry.Instance.HouseBillNumberCustomisation_ROAD, "DIR", "555");

			NumberGeneratorContext context = new NumberGeneratorContext();
			ForwardingBillOfLadingNumberGeneratorTarget target = new ForwardingBillOfLadingNumberGeneratorTarget(mock.Object)
			{
				Context = context
			};
			AssertCustomisation("Should find the HouseBillNumberCustomisation", "111", target.NumberCustomisation);
			AssertLocation(FreightDataRegistry.Instance.HouseBillNumberCustomisation, target.NumberCustomisationLocation);
			AssertEquals(JobShipmentSchema.JS_HouseBill.MaxLength, target.MaxLength);
			AssertEquals("bill of lading", target.Name);

			mock = new Mock<IBillGenerationSupport>();
			mock.Setup(m => m.Factory).Returns(Factory);
			mock.Setup(m => m.TransportMode).Returns(Constants.TransportModes.Sea);
			mock.Setup(m => m.ServiceLevel).Returns("STD");
			target = new ForwardingBillOfLadingNumberGeneratorTarget(mock.Object) { Context = context };
			AssertCustomisation("Should find the HouseBillNumberCustomisation", "222", target.NumberCustomisation);
			AssertLocation(FreightDataRegistry.Instance.HouseBillNumberCustomisation, target.NumberCustomisationLocation);
			AssertEquals(JobShipmentSchema.JS_HouseBill.MaxLength, target.MaxLength);
			AssertEquals("bill of lading", target.Name);

			mock = new Mock<IBillGenerationSupport>();
			mock.Setup(m => m.Factory).Returns(Factory);
			mock.Setup(m => m.TransportMode).Returns(Constants.TransportModes.Air);
			mock.Setup(m => m.ServiceLevel).Returns("D2D");
			target = new ForwardingBillOfLadingNumberGeneratorTarget(mock.Object) { Context = context };
			AssertCustomisation("Should find the HouseBillNumberCustomisation", "333", target.NumberCustomisation);
			AssertLocation(FreightDataRegistry.Instance.HouseBillNumberCustomisation, target.NumberCustomisationLocation);
			AssertEquals(JobShipmentSchema.JS_HouseBill.MaxLength, target.MaxLength);
			AssertEquals("bill of lading", target.Name);

			mock = new Mock<IBillGenerationSupport>();
			mock.Setup(m => m.Factory).Returns(Factory);
			mock.Setup(m => m.TransportMode).Returns(Constants.TransportModes.Rail);
			mock.Setup(m => m.ServiceLevel).Returns("DEF");
			target = new ForwardingBillOfLadingNumberGeneratorTarget(mock.Object) { Context = context };
			AssertCustomisation("Should find the HouseBillNumberCustomisation", "444", target.NumberCustomisation);
			AssertLocation(FreightDataRegistry.Instance.HouseBillNumberCustomisation, target.NumberCustomisationLocation);
			AssertEquals(JobShipmentSchema.JS_HouseBill.MaxLength, target.MaxLength);
			AssertEquals("bill of lading", target.Name);

			mock = new Mock<IBillGenerationSupport>();
			mock.Setup(m => m.Factory).Returns(Factory);
			mock.Setup(m => m.TransportMode).Returns(Constants.TransportModes.Road);
			mock.Setup(m => m.ServiceLevel).Returns("DIR");
			target = new ForwardingBillOfLadingNumberGeneratorTarget(mock.Object) { Context = context };
			AssertCustomisation("Should find the HouseBillNumberCustomisation", "555", target.NumberCustomisation);
			AssertLocation(FreightDataRegistry.Instance.HouseBillNumberCustomisation, target.NumberCustomisationLocation);
			AssertEquals(JobShipmentSchema.JS_HouseBill.MaxLength, target.MaxLength);
			AssertEquals("bill of lading", target.Name);
		}
	}
}
