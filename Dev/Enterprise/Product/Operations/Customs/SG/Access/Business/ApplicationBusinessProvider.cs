using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.SG.Access.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using AsycudaUniversalEventMessageFailureProcessor = Enterprise.Customs.SG.Access.Business.UniversalDataTransfer.AsycudaUniversalEventMessageFailureProcessor;
using AsycudaUniversalEventMessagePinProcessor = Enterprise.Customs.SG.Access.Business.UniversalDataTransfer.AsycudaUniversalEventMessagePinProcessor;
using AsycudaUniversalEventMessageSuccessProcessor = Enterprise.Customs.SG.Access.Business.UniversalDataTransfer.AsycudaUniversalEventMessageSuccessProcessor;

namespace Enterprise.Customs.SG.Access.Business
{
	public class ApplicationBusinessProvider : ASYCUDA.Business.ApplicationBusinessProvider
	{
		public override Type AsycudaManifestHeaderType => typeof(AsycudaManifestHeader);

		protected override IReadOnlyList<ZString> CreateCountryCodes() => new[] { (ZString)Core.Constants.CountryCodes.Singapore };

		protected override IReadOnlyList<IManifestType> CreateManifestTypes()
		{
			if ((bool)ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable.Value)
			{
				return new SGManifestTypes().All;
			}

			return Array.Empty<IManifestType>();
		}

		public override ASYCUDA.Business.MessagingProvider MessagingProvider => new MessagingProvider();

		public override ASYCUDA.Business.FeatureProvider FeatureProvider => new FeatureProvider();

		protected override IAsycudaManifestHeaderDataObjectWriter GetAsycudaManifestHeaderDataObjectWriterCore(IDataWritingManager manager) => new SGAsycudaManifestHeaderDataObjectWriter(manager);
		protected override AsycudaManifestHeaderDataObjectWriterHelper GetAsycudaManifestHeaderDataObjectWriterHelperCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return new SGAsycudaManifestHeaderDataObjectWriterHelper((AsycudaManifestHeader)header);
		}
		protected override IAsycudaForCustomsDeclarationDataObjectWriter GetCustomsDeclarationDataObjectWriterCore(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper) => new SGAsycudaForCustomsDeclarationDataObjectWriter(manager, (SGAsycudaManifestHeaderDataObjectWriterHelper)helper);
		protected override AsycudaManifestDataObjectReaderHelper GetAsycudaManifestDataObjectReaderHelperCore(string countryCode) => new SGAsycudaManifestDataObjectReaderHelper(factory);

		protected override AsycudaUniversalEventMessageProcessor GetNewAsycudaUniversalEventMessageProcessorCore(IXmlSessionTracker logger, Event universalEvent, ASYCUDA.Business.AsycudaEDIMessage ediMessage, ASYCUDA.Business.AsycudaManifestHeader baseManifestHeader)
		{
			var manifestHeader = (AsycudaManifestHeader)baseManifestHeader;
			AsycudaUniversalEventMessageProcessor processor = null;
			switch (universalEvent.DataContext.ActionPurposeCode)
			{
				case Constants.ActionPurpose.AEP:
					processor = new AsycudaUniversalEventMessageSuccessProcessor(logger, universalEvent, ediMessage, manifestHeader);
					break;
				case Constants.ActionPurpose.ERR:
					processor = new AsycudaUniversalEventMessageFailureProcessor(logger, universalEvent, ediMessage, manifestHeader);
					break;
				case Constants.ActionPurpose.PIN:
					processor = new AsycudaUniversalEventMessagePinProcessor(logger, universalEvent, ediMessage, manifestHeader);
					break;
			}
			return processor;
		}

		protected override ZDateTime GetEffectiveDateForDutyRateCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			var sgHeader = header as AsycudaManifestHeader;
			var effectiveDutyDate = ZDateTime.Today;
			if (sgHeader != null)
			{
				if (sgHeader.IsImport)
				{
					effectiveDutyDate = sgHeader.AMA_E_ARV;
				}
				else if (sgHeader.IsExport)
				{
					effectiveDutyDate = sgHeader.AMA_E_DEP;
				}
			}
			return effectiveDutyDate.IsValid ? effectiveDutyDate : ZDateTime.Today;
		}

		public override ZString PackedItemTariffDataGrouping => Core.Constants.CountryCodes.Singapore;

		public override ZString PackedItemTariffType => ZString.Empty;
	}
}
