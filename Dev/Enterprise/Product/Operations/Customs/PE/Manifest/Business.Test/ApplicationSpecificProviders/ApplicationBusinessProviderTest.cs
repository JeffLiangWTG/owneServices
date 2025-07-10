using System;
using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using NUnit.Framework;

namespace Enterprise.Customs.PE.Manifest.Business.Testing
{
	[TestedType(typeof(ApplicationBusinessProvider))]
	sealed class ApplicationBusinessProviderTest : ApplicationBusinessProviderAbstractTest<ApplicationBusinessProvider, AsycudaManifestHeader>
	{
		public override void TestManifestTypes()
		{
			expectedManifestTypes = new PEManifestTypes().All;
			base.TestManifestTypes();

			using (PECustomsDataRegistry.Instance.EnablePEManifests.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				expectedManifestTypes = Array.Empty<IManifestType>();
				base.TestManifestTypes();
			}
		}

		protected override IEnumerable<IManifestType> ExpectedManifestTypes => expectedManifestTypes;
		IEnumerable<IManifestType> expectedManifestTypes;

		protected override Type ExpectedMessagingProviderType => null;

		protected override Type ExpectedFeatureProviderType => typeof(FeatureProvider);

		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterType => typeof(AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>);

		protected override Type ExpectedGetCustomsDeclarationDataObjectWriterType => typeof(AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>);

		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterHelperType => typeof(AsycudaManifestHeaderDataObjectWriterHelper);

		protected override Type ExpectedGetAsycudaManifestDataObjectReaderHelperType => typeof(AsycudaManifestDataObjectReaderHelper);

		protected override AsycudaManifestHeader CreateNewManifest()
		{
			var result = base.CreateNewManifest();
			result.AMA_RN_NKCountry = Core.Constants.CountryCodes.Peru;
			result.AMA_ManifestType = PEManifestTypes.Codes.MAN;
			return result;
		}
	}
}
