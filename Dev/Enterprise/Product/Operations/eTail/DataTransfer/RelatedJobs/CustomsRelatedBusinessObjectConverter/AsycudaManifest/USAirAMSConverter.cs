using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.eTail.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.eTail.DataTransfer
{
	public class USAirAMSConverter : AsycudaManifestConverter
	{
		public USAirAMSConverter(BaseHVLVRelatedJobCommand relatedJobCommand)
			: base(relatedJobCommand)
		{
		}

		public override RecipientRoleType? RecipientRoleType => UniversalDataBuss.Integration.RecipientRoleType.ASY;

		protected override RecipientRoleType PickupOrDeliveryCartageRole => UniversalDataBuss.Integration.RecipientRoleType.DCA;

		public override CodeDescriptionPair ManifestType => new CodeDescriptionPair
		{
			Code = ACEManifestTypes.Codes.IAM,
			Description = ACEManifestTypes.Descriptions.IAM,
		};

		protected override bool ShouldStripNonWesternEuropeanCharacters => HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersUSAirAMS.Value;

		protected override string EntryCountryCode => CountryCodes.UnitedStates;

		protected override void OnConversionFactorySaving(BusinessObjectFactory factory)
		{
			if (!Shipment.HasAirAMSTransferredLog())
			{
				var airAMS = CustomsRelatedBusinessCollection.OfType<AsycudaManifestHeader>().FirstOrDefault();
				airAMS.OnSaving();

				var shipmentParameters = new List<KeyValuePair<string, string>>();
				shipmentParameters.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, CustomsModuleCodes.Codes.AutomatedManifestSystem));
				shipmentParameters.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.Mode, TransportModes.Air));
				shipmentParameters.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, airAMS.AMA_JobReference));
				Shipment.Logs.AddNew(AutoEvents.Transferred, shipmentParameters.ToArray());

				var headerParameters = new List<KeyValuePair<string, string>>();
				headerParameters.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, "HVL"));
				headerParameters.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.JobNumber, Shipment.JobNumber));
				airAMS.Logs.AddNew(AutoEvents.Transferred, headerParameters.ToArray());
			}
		}

		protected override void OnConversionSucceeded()
		{
			Shipment.SetSecurityFilingFirstUsageTimeForAllItems();
		}
	}
}
