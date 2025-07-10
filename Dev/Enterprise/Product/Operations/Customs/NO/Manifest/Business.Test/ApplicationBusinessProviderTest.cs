using System;
using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.NO.Registry;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(ApplicationBusinessProvider))]
[BooleanRegistryItemTest(typeof(NOCustomsDataRegistry), nameof(NOCustomsDataRegistry.EnableNOManifests))]
sealed class ApplicationBusinessProviderTest : ASYCUDA.Business.Testing.ApplicationBusinessProviderAbstractTest<ApplicationBusinessProvider, AsycudaManifestHeader>
{
	public override void TestManifestTypes()
	{
		expectedManifestTypes = new NOManifestTypes().All;
		base.TestManifestTypes();

		using (NOCustomsDataRegistry.Instance.EnableNOManifests.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
		{
			expectedManifestTypes = Array.Empty<IManifestType>();
			base.TestManifestTypes();
		}
	}

	public void TestGetPackedItemTariffDataGrouping()
	{
		var header = CreateNewManifest();
		AssertEquals("NO", header.ApplicationBusinessProvider.PackedItemTariffDataGrouping);
		AssertEquals("HSN", header.ApplicationBusinessProvider.PackedItemTariffType);
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
		result.AMA_RN_NKCountry = Core.Constants.CountryCodes.Norway;
		result.AMA_ManifestType = NOManifestTypes.Codes.DMO;
		return result;
	}
}
