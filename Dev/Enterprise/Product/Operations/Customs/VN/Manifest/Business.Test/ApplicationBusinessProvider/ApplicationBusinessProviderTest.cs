using System;
using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using NUnit.Framework;

namespace Enterprise.Customs.VN.Manifest.Business.Testing
{
	[TestedType(typeof(ApplicationBusinessProvider))]
	sealed class ApplicationBusinessProviderTest : ASYCUDA.Business.Testing.ApplicationBusinessProviderAbstractTest<ApplicationBusinessProvider, AsycudaManifestHeader>
	{
		public override void TestManifestTypes()
		{
			using (VNCustomsDataRegistry.Instance.EnableVNManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				_expectedManifestTypes = new VNManifestTypes().All;
				base.TestManifestTypes();
			}

			using (VNCustomsDataRegistry.Instance.EnableVNManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				_expectedManifestTypes = Array.Empty<IManifestType>();
				base.TestManifestTypes();
			}
		}

		protected override IEnumerable<IManifestType> ExpectedManifestTypes => _expectedManifestTypes;
		IEnumerable<IManifestType> _expectedManifestTypes;

		protected override Type ExpectedMessagingProviderType => null;
		protected override Type ExpectedFeatureProviderType => typeof(FeatureProvider);
		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterType => typeof(AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>);
		protected override Type ExpectedGetCustomsDeclarationDataObjectWriterType => typeof(AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>);
		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterHelperType => typeof(AsycudaManifestHeaderDataObjectWriterHelper);
		protected override Type ExpectedGetAsycudaManifestDataObjectReaderHelperType => typeof(AsycudaManifestDataObjectReaderHelper);
		protected override AsycudaManifestHeader CreateNewManifest()
		{
			var result = base.CreateNewManifest();
			result.AMA_RN_NKCountry = Core.Constants.CountryCodes.VietNam;
			result.AMA_ManifestType = VNManifestTypes.Codes.VSW;
			return result;
		}
	}
}
