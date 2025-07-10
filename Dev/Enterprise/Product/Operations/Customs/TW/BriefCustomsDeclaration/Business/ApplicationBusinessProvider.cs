using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class ApplicationBusinessProvider : ASYCUDA.Business.ApplicationBusinessProvider
	{
		public override Type AsycudaManifestHeaderType => typeof(AsycudaManifestHeader);

		public override ASYCUDA.Business.FeatureProvider FeatureProvider => new FeatureProvider();

		public override ASYCUDA.Business.MessagingProvider MessagingProvider => new MessagingProvider();

		protected override IReadOnlyList<ZString> CreateCountryCodes() => new[] { (ZString)Core.Constants.CountryCodes.Taiwan };

		protected override IReadOnlyList<IManifestType> CreateManifestTypes() => new TWManifestTypes().All;

		protected override IAsycudaManifestHeaderDataObjectWriter GetAsycudaManifestHeaderDataObjectWriterCore(UniversalDataBuss.Integration.IDataWritingManager manager)
			=> new AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>(manager);

		protected override IAsycudaForCustomsDeclarationDataObjectWriter GetCustomsDeclarationDataObjectWriterCore(UniversalDataBuss.Integration.IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
			=> new AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>(manager, helper);

		public override ZString PackedItemTariffDataGrouping => Core.Constants.CountryCodes.Taiwan;

		public override ZString PackedItemTariffType => Universal.Constants.TariffTypes.HarmonizedSystem;
	}
}
