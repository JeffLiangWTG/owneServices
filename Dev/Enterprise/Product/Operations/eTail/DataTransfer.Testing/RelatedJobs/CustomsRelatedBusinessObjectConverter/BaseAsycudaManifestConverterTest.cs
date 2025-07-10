using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.DataTransfer.Testing
{
	abstract class BaseAsycudaManifestConverterTest<TConverter> : CustomsRelatedBusinessObjectConverterBaseTest<TConverter>
		where TConverter : AsycudaManifestConverter
	{
		public void TestContainerisedContainerModes()
		{
			AssertContainerModeConvertSuccess(Core.Constants.ContainerModes.FCL, Core.Constants.ContainerModes.Containerised);
			AssertContainerModeConvertSuccess(Core.Constants.ContainerModes.LCL, Core.Constants.ContainerModes.Containerised);
			AssertContainerModeConvertSuccess(Core.Constants.ContainerModes.FCLMixedShipper, Core.Constants.ContainerModes.Containerised);
		}

		public void TestIsAsycudaContainerModes()
		{
			AssertContainerModeConvertSuccess(Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModes.BreakBulk);
			AssertContainerModeConvertSuccess(Core.Constants.ContainerModes.Bulk, Core.Constants.ContainerModes.Bulk);
			AssertContainerModeConvertSuccess(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModes.Containerised);
			AssertContainerModeConvertSuccess(Core.Constants.ContainerModes.Liquid, Core.Constants.ContainerModes.Liquid);
			AssertContainerModeConvertSuccess(Core.Constants.ContainerModes.Other, Core.Constants.ContainerModes.Other);
		}

		public void TestOtherContainerModes()
		{
			AssertContainerModeConvertSuccess(Core.Constants.ContainerModes.AgentConsol, string.Empty);
			AssertContainerModeConvertSuccess(Core.Constants.ContainerModes.OnBoardCourier, string.Empty);
			AssertContainerModeConvertSuccess(Core.Constants.ContainerModes.RollOnRollOff, string.Empty);
			AssertContainerModeConvertSuccess(string.Empty, string.Empty);
		}

		void AssertContainerModeConvertSuccess(string testMode, string expectedMode)
		{
			var shipment = SetupTestShipment(Core.Constants.TransportModes.Sea);
			shipment.ArrivalConsol.JK_ConsolMode = testMode;

			Factory.Save();

			using (RegistryItemToEnable?.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(LoginCountry))
			{
				var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

				var success = converter.TryConvert(out var errorMsg);
				Assert("Precondition: Convert successfully", success);

				var converterFactory = converter.CustomsRelatedBusinessCollection.Single().Factory;

				var manifestHeaderType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(AsycudaManifestHeaderSchema.Constants.Prefix);
				var manifestHeaders = converterFactory.Load(manifestHeaderType, new ZQuery());
				var manifestHeader = manifestHeaders.Cast<AsycudaManifestHeader>().FirstOrDefault();

				AssertEquals($"Container mode should be {expectedMode}", expectedMode, manifestHeader.AMA_ContainerMode);
			}
		}
	}
}
