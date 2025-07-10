using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.US.DataRegistry.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class USExportApplicationBusinessProvider : ApplicationBusinessProvider
	{
		public override Type AsycudaManifestHeaderType => typeof(USExportAsycudaManifestHeader);

		public override MessagingProvider MessagingProvider => new USMessagingProvider();

		public override FeatureProvider FeatureProvider => new USFeatureProvider();

		protected override IReadOnlyList<ZString> CreateCountryCodes()
		{
			return new ZString[] { Core.Constants.CountryCodes.UnitedStates };
		}

		protected override IReadOnlyList<IManifestType> CreateManifestTypes()
		{
			if (USCustomsDataRegistry.Instance.EnableExportManifest.Value)
			{
				return new IManifestType[] { new USManifestType(USExportManifestTypes.Codes.EFM, "US Export Manifest", GetTransportModes(),
					new[] { ManifestBase.ApplicationCodeTypeList.Codes.Consolidator, ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine }, MessageLevel.Bill, ShipmentTypeList.Export22Only()) };
			}

			return Array.Empty<IManifestType>();
		}

		public override IEnumerable<(ZString, ZString)> GetManifestDescriptions(BusinessObjectFactory factory, IEnumerable<ZString> countryCodes, Func<IManifestType, bool> filter)
		{
			return new[] { (new ZString(Core.Constants.CountryCodes.UnitedStates), new ZString(Res.GetString("USExportManifestMenuItemDescription", "United States - Export Manifest"))) };
		}

		protected override IAsycudaManifestHeaderDataObjectWriter GetAsycudaManifestHeaderDataObjectWriterCore(UniversalDataBuss.Integration.IDataWritingManager manager)
		{
			return new AsycudaManifestHeaderDataObjectWriter<USExportAsycudaManifestHeader>(manager);
		}

		protected override IAsycudaForCustomsDeclarationDataObjectWriter GetCustomsDeclarationDataObjectWriterCore(UniversalDataBuss.Integration.IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
		{
			return new AsycudaForCustomsDeclarationDataObjectWriter<USExportAsycudaBill, USExportAsycudaPack, AsycudaPackedItem>(manager, helper);
		}

		IEnumerable<string> GetTransportModes()
		{
			return new[] { Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Road, Core.Constants.TransportModes.Rail };
		}
	}
}
