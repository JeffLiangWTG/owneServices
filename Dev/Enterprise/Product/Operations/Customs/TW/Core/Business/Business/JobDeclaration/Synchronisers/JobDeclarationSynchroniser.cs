using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class JobDeclarationSynchroniser : Customs.Business.JobDeclarationSynchroniser
	{
		public JobDeclarationSynchroniser(JobDeclaration destination)
			: base(destination)
		{
		}

		public new JobDeclaration Destination => (JobDeclaration)base.Destination;

		protected override Customs.Business.PackingSynchroniser GetPackingSynchroniser() => new PackingSynchroniser(this, Destination);

		protected override ZBool ShouldSynchroniseContainerModeWithPackingMode => false;

		protected override void HookSupplierAndImporter()
		{
			AddDocAddressSynchroniser(Destination.SupplierDocumentaryAddress, Source.ConsignorDocumentaryAddress);
			AddDocAddressSynchroniser(Destination.ImporterDocumentaryAddress, Source.ConsigneeDocumentaryAddress);
		}

		void AddDocAddressSynchroniser(TWJobDocAddress destinationDocAddress, JobDocAddress sourceDocAddress)
		{
			Synchronisers.Add(new FieldSynchroniser(destinationDocAddress.E2_OA_AddressInfo,
				() => sourceDocAddress.E2_AddressOverride ? ZGuid.Empty : sourceDocAddress.E2_OA_Address,
				() => GetJobDocAddressRelatedInfos(sourceDocAddress),
				() => ShouldSyncDocAddressBeEditable(sourceDocAddress) && !sourceDocAddress.E2_AddressOverride,
				() => !sourceDocAddress.E2_AddressOverride)
			{ JobDocAddressPersistingSyncronizedValue = destinationDocAddress });

			Synchronisers.Add(new FieldSynchroniser(destinationDocAddress.E2_AddressOverrideInfo, () => sourceDocAddress.E2_AddressOverride, () => new ZPropertyInfo[] { sourceDocAddress.E2_AddressOverrideInfo }, () => !sourceDocAddress.E2_AddressOverride, () => sourceDocAddress.E2_AddressOverride));
			Synchronisers.Add(new FieldSynchroniser(destinationDocAddress.E2_CompanyNameInfo, sourceDocAddress.E2_CompanyNameInfo));
			Synchronisers.Add(new FieldSynchroniser(destinationDocAddress.E2_Address1Info, sourceDocAddress.E2_Address1Info));
			Synchronisers.Add(new FieldSynchroniser(destinationDocAddress.E2_Address2Info, sourceDocAddress.E2_Address2Info));
			Synchronisers.Add(new FieldSynchroniser(destinationDocAddress.E2_AdditionalAddressInformationInfo, sourceDocAddress.E2_AdditionalAddressInformationInfo));
			Synchronisers.Add(new FieldSynchroniser(destinationDocAddress.E2_RN_NKCountryCodeInfo, sourceDocAddress.E2_RN_NKCountryCodeInfo));
			Synchronisers.Add(new FieldSynchroniser(destinationDocAddress.E2_CityInfo, sourceDocAddress.E2_CityInfo));
			Synchronisers.Add(new FieldSynchroniser(destinationDocAddress.E2_PostcodeInfo, sourceDocAddress.E2_PostcodeInfo));
			Synchronisers.Add(new FieldSynchroniser(destinationDocAddress.E2_StateInfo, sourceDocAddress.E2_StateInfo));
			Synchronisers.Add(new FieldSynchroniser(destinationDocAddress.E2_ContactInfo, sourceDocAddress.E2_ContactInfo));
			Synchronisers.Add(new FieldSynchroniser(destinationDocAddress.E2_EmailInfo, sourceDocAddress.E2_EmailInfo));
			Synchronisers.Add(new FieldSynchroniser(destinationDocAddress.E2_PhoneInfo, sourceDocAddress.E2_PhoneInfo));
		}

		protected override bool ShouldGetTransportsFromShipmentIfNoHookedConsol => true;
	}
}
