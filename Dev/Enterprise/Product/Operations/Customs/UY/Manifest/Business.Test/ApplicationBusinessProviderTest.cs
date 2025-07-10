using System;
using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using NUnit.Framework;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	[TestedType(typeof(ApplicationBusinessProvider))]
	sealed class ApplicationBusinessProviderTest : ASYCUDA.Business.Testing.ApplicationBusinessProviderAbstractTest<ApplicationBusinessProvider, AsycudaManifestHeader>
	{
		public override void TestManifestTypes()
		{
			expectedManifestTypes = new UYManifestTypes().All;
			base.TestManifestTypes();
		}

		protected override IEnumerable<IManifestType> ExpectedManifestTypes => expectedManifestTypes;
		IEnumerable<IManifestType> expectedManifestTypes;
		protected override Type ExpectedMessagingProviderType => typeof(MessagingProvider);
		protected override Type ExpectedFeatureProviderType => typeof(FeatureProvider);
		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterType => typeof(AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>);
		protected override Type ExpectedGetCustomsDeclarationDataObjectWriterType => typeof(AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>);
		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterHelperType => typeof(AsycudaManifestHeaderDataObjectWriterHelper);
		protected override Type ExpectedGetAsycudaManifestDataObjectReaderHelperType => typeof(AsycudaManifestDataObjectReaderHelper);
		protected override AsycudaManifestHeader CreateNewManifest()
		{
			var result = base.CreateNewManifest();
			result.AMA_RN_NKCountry = Core.Constants.CountryCodes.Uruguay;
			result.AMA_ManifestType = UYManifestTypes.Codes.MAN;
			return result;
		}
	}
}


