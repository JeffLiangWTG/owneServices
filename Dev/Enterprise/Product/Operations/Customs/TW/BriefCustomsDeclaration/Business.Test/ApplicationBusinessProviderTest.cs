using System;
using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.ManifestBase;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(ApplicationBusinessProvider))]
	sealed class ApplicationBusinessProviderTest : ASYCUDA.Business.Testing.ApplicationBusinessProviderAbstractTest<ApplicationBusinessProvider, AsycudaManifestHeader>
	{
		protected override IEnumerable<IManifestType> ExpectedManifestTypes => new TWManifestTypes().All;
		protected override Type ExpectedMessagingProviderType => typeof(MessagingProvider);
		protected override Type ExpectedFeatureProviderType => typeof(FeatureProvider);
		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterType => typeof(AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>);
		protected override Type ExpectedGetCustomsDeclarationDataObjectWriterType => typeof(AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>);
		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterHelperType => typeof(AsycudaManifestHeaderDataObjectWriterHelper);
		protected override Type ExpectedGetAsycudaManifestDataObjectReaderHelperType => typeof(AsycudaManifestDataObjectReaderHelper);
		protected override AsycudaManifestHeader CreateNewManifest()
		{
			var header = base.CreateNewManifest();
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Taiwan;
			header.AMA_ManifestType = TWManifestTypes.Codes.ImportLowValueDutyFreeGoods;
			return header;
		}
	}
}
