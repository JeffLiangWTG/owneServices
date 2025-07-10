using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.US.ACEManifest.Business.Messaging;
using Enterprise.Customs.US.ACEManifest.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class ApplicationBusinessProvider : ASYCUDA.Business.ApplicationBusinessProvider
	{
		public override Type AsycudaManifestHeaderType => typeof(AsycudaManifestHeader);

		protected override IReadOnlyList<ZString> CreateCountryCodes() => new[] { (ZString)Core.Constants.CountryCodes.UnitedStates };

		protected override IReadOnlyList<IManifestType> CreateManifestTypes()
		{
			return new ACEManifestTypes().All;
		}

		public override IEnumerable<(ZString, ZString)> GetManifestDescriptions(BusinessObjectFactory factory, IEnumerable<ZString> countryCodes, Func<IManifestType, bool> filter)
		{
			return new[] { (new ZString(Core.Constants.CountryCodes.UnitedStates), new ZString(Res.GetString("AIRAMSManifestMenuItemDescription", "United States - Air AMS (Import)"))) };
		}

		public override ASYCUDA.Business.MessagingProvider MessagingProvider => new MessagingProvider();

		public override ASYCUDA.Business.FeatureProvider FeatureProvider => new FeatureProvider();

		#region Workflow trigger action - Send Global Manifest(SGM)
		public override bool SupportsAutoSendGlobalManifest(string manifestType)
		{
			return manifestType == new ACEManifestTypes().IAM.Code;
		}

		public override IProcessor GetSendGlobalManifestProcessor(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			if (header is AsycudaManifestHeader aimHeader)
			{
				return new SendBillsProcessor(aimHeader);
			}
			else
			{
				return null;
			}
		}
		#endregion

		protected override IAsycudaManifestHeaderDataObjectWriter GetAsycudaManifestHeaderDataObjectWriterCore(IDataWritingManager manager)
		{
			return new AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>(manager);
		}

		protected override AsycudaManifestHeaderDataObjectWriterHelper GetAsycudaManifestHeaderDataObjectWriterHelperCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return new ACEAsycudaManifestHeaderDataObjectWriterHelper((AsycudaManifestHeader)header);
		}

		protected override IAsycudaForCustomsDeclarationDataObjectWriter GetCustomsDeclarationDataObjectWriterCore(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
		{
			return new AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>(manager, (ACEAsycudaManifestHeaderDataObjectWriterHelper)helper);
		}

		protected override AsycudaManifestDataObjectReaderHelper GetAsycudaManifestDataObjectReaderHelperCore(string countryCode)
		{
			return new ACEAsycudaManifestDataObjectReaderHelper(factory);
		}
	}
}
