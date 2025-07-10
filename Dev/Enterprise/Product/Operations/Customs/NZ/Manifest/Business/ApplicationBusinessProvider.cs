using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.NZ.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.NZ.Manifest.Business
{
	public class ApplicationBusinessProvider : ASYCUDA.Business.ApplicationBusinessProvider
	{
		public override Type AsycudaManifestHeaderType => typeof(AsycudaManifestHeader);

		protected override IReadOnlyList<ZString> CreateCountryCodes() => new[] { (ZString)Core.Constants.CountryCodes.NewZealand };

		protected override IReadOnlyList<IManifestType> CreateManifestTypes()
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.NewZealand)
			{
				var isICRMANEnabled = NZCustomsDataRegistry.Instance.EnableInwardCargoReportManifest.Value;
				if (isICRMANEnabled)
				{
					return new NZManifestTypes().All;
				}
				else
				{
					return new[] { new NZManifestTypes().OCR };
				}
			}

			return Array.Empty<IManifestType>();
		}

		public override ASYCUDA.Business.MessagingProvider MessagingProvider => new MessagingProvider();

		public override ASYCUDA.Business.FeatureProvider FeatureProvider => new FeatureProvider();

		protected override IAsycudaManifestHeaderDataObjectWriter GetAsycudaManifestHeaderDataObjectWriterCore(IDataWritingManager manager)
		{
			return new AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>(manager);
		}

		protected override AsycudaManifestHeaderDataObjectWriterHelper GetAsycudaManifestHeaderDataObjectWriterHelperCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return new AsycudaManifestHeaderDataObjectWriterHelper((AsycudaManifestHeader)header);
		}

		protected override IAsycudaForCustomsDeclarationDataObjectWriter GetCustomsDeclarationDataObjectWriterCore(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
		{
			return new AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>(manager, helper);
		}

		protected override AsycudaManifestDataObjectReaderHelper GetAsycudaManifestDataObjectReaderHelperCore(string countryCode)
		{
			return new AsycudaManifestDataObjectReaderHelper(countryCode, factory);
		}
	}
}
