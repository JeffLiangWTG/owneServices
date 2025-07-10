using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class ApplicationBusinessProvider : ASYCUDA.Business.ApplicationBusinessProvider
	{
		public override Type AsycudaManifestHeaderType => typeof(AsycudaManifestHeader);

		public override string ApplicationCode => ApplicationCodeTypeList.Codes.TRETrade;

		public override MessagingProvider MessagingProvider => null;

		public override ASYCUDA.Business.FeatureProvider FeatureProvider => new FeatureProvider();

		protected override IReadOnlyList<ZString> CreateCountryCodes() => new[] { (ZString)Core.Constants.CountryCodes.Turkey };

		protected override IReadOnlyList<IManifestType> CreateManifestTypes()
		{
			return new TRETradeManifestTypes().All;
		}

		protected override IAsycudaManifestHeaderDataObjectWriter GetAsycudaManifestHeaderDataObjectWriterCore(UniversalDataBuss.Integration.IDataWritingManager manager)
		{
			return new AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>(manager);
		}

		protected override IAsycudaForCustomsDeclarationDataObjectWriter GetCustomsDeclarationDataObjectWriterCore(UniversalDataBuss.Integration.IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
		{
			return new AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>(manager, helper);
		}

		protected override AsycudaManifestDataObjectReaderHelper GetAsycudaManifestDataObjectReaderHelperCore(string countryCode)
		{
			return new TRETradeHVLVAsycudaManifestDataObjectReaderHelper(countryCode, factory);
		}

		public override ZString PackedItemTariffDataGrouping => Core.Constants.CountryCodes.Turkey;
		public override List<SelectionStyle> SelectNomenclatureModes => new List<SelectionStyle> { SelectionStyle.Heading, SelectionStyle.Subheading, SelectionStyle.EightCharNomenclature, SelectionStyle.Tariff };
	}
}
