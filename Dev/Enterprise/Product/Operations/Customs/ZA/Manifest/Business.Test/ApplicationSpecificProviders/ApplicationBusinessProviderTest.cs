using System;
using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.ZA.Manifest.Business.UniversalDataTransfer;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	[TestedType(typeof(ApplicationBusinessProvider))]
	sealed class ApplicationBusinessProviderTest : ASYCUDA.Business.Testing.ApplicationBusinessProviderAbstractTest<ApplicationBusinessProvider, AsycudaManifestHeader>
	{
		protected override IEnumerable<IManifestType> ExpectedManifestTypes => new ZaManifestTypes().All;
		protected override Type ExpectedMessagingProviderType => typeof(MessagingProvider);
		protected override Type ExpectedFeatureProviderType => typeof(FeatureProvider);
		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterType => typeof(AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>);
		protected override Type ExpectedGetCustomsDeclarationDataObjectWriterType => typeof(AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>);
		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterHelperType => typeof(ZAAsycudaManifestHeaderDataObjectWriterHelper);
		protected override Type ExpectedGetAsycudaManifestDataObjectReaderHelperType => typeof(ZAAsycudaManifestDataObjectReaderHelper);
	}

	sealed class ApplicationBusinessProvider_ForTesting : ApplicationBusinessProvider
	{
		public override ASYCUDA.Business.MessagingProvider MessagingProvider => new MessagingProvider_ForTesting();
	}

	sealed class MessageStatusProvider_ForTesting : MessageStatusProvider
	{
		public override bool MessageStatusCanBeReset(IMessageParent parent) => parent.MessageStatus == MessageStatusCodeList.Codes.Sent;
	}

	sealed class MessagingProvider_ForTesting : MessagingProvider
	{
		public override ASYCUDA.Business.MessageStatusProvider MessageStatusProvider => new MessageStatusProvider_ForTesting();
	}
}
