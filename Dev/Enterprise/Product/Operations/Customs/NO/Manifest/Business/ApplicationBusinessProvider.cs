using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.NO.Registry;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NO.Manifest.Business;

public class ApplicationBusinessProvider : ASYCUDA.Business.ApplicationBusinessProvider
{
	public override Type AsycudaManifestHeaderType => typeof(AsycudaManifestHeader);

	protected override IReadOnlyList<ZString> CreateCountryCodes() => new[] { (ZString)Core.Constants.CountryCodes.Norway };

	protected override IReadOnlyList<IManifestType> CreateManifestTypes()
	{
		if (NOCustomsDataRegistry.Instance.EnableNOManifests.Value)
		{
			return new NOManifestTypes().All;
		}
		return Array.Empty<IManifestType>();
	}

	public override ZString PackedItemTariffDataGrouping => Core.Constants.CountryCodes.Norway;
	public override ASYCUDA.Business.FeatureProvider FeatureProvider => new FeatureProvider();

	public override ASYCUDA.Business.MessagingProvider MessagingProvider => new MessagingProvider();

	protected override IAsycudaManifestHeaderDataObjectWriter GetAsycudaManifestHeaderDataObjectWriterCore(UniversalDataBuss.Integration.IDataWritingManager manager)
		=> new AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>(manager);

	protected override IAsycudaForCustomsDeclarationDataObjectWriter GetCustomsDeclarationDataObjectWriterCore(UniversalDataBuss.Integration.IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
				=> new AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>(manager, helper);
	public override List<SelectionStyle> SelectNomenclatureModes => new List<SelectionStyle> { SelectionStyle.Heading, SelectionStyle.Subheading, SelectionStyle.EightCharNomenclature, SelectionStyle.Tariff };
}
