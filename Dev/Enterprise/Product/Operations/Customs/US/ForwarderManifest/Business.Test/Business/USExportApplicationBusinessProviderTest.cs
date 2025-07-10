using System;
using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.US.DataRegistry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	[TestedType(typeof(USExportApplicationBusinessProvider))]
	sealed class USExportApplicationBusinessProviderTest : ASYCUDA.Business.Testing.ApplicationBusinessProviderAbstractTest<USExportApplicationBusinessProvider, USExportAsycudaManifestHeader>
	{
		protected override IEnumerable<IManifestType> ExpectedManifestTypes
		{
			get
			{
				if (USCustomsDataRegistry.Instance.EnableExportManifest.Value)
				{
					return new IManifestType[] { new USManifestType(USExportManifestTypes.Codes.EFM, "US Export Manifest", GetTransportModes(),
						new[] { ManifestBase.ApplicationCodeTypeList.Codes.Consolidator, ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine }, MessageLevel.Bill, ShipmentTypeList.Export22Only()) };
				}
				return Array.Empty<IManifestType>();
			}
		}

		IEnumerable<string> GetTransportModes()
		{
			return new[] { Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Road, Core.Constants.TransportModes.Rail };
		}

		protected override Type ExpectedMessagingProviderType => typeof(USMessagingProvider);

		protected override Type ExpectedFeatureProviderType => typeof(USFeatureProvider);

		protected override Type ExpectedGetCustomsDeclarationDataObjectWriterType => typeof(ASYCUDA.Business.UniversalDataTransfer.AsycudaForCustomsDeclarationDataObjectWriter<USExportAsycudaBill, USExportAsycudaPack, AsycudaPackedItem>);

		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterType => typeof(ASYCUDA.Business.UniversalDataTransfer.AsycudaManifestHeaderDataObjectWriter<USExportAsycudaManifestHeader>);

		protected override USExportAsycudaManifestHeader CreateNewManifest()
		{
			var result = base.CreateNewManifest();
			result.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			result.AMA_ManifestType = USExportManifestTypes.Codes.EFM;
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			USCustomsDataRegistry.Instance.EnableExportManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			USCustomsDataRegistry.Instance.EnableExportManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}
	}
}
