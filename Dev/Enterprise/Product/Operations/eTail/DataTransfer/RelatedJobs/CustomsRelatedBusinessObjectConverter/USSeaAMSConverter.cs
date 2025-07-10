using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.eTail.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.eTail.DataTransfer
{
	public class USSeaAMSConverter : CustomsRelatedBusinessObjectConverter
	{
		public USSeaAMSConverter(BaseHVLVRelatedJobCommand relatedJobCommand)
			: base(relatedJobCommand)
		{
		}

		public override RecipientRoleType? RecipientRoleType => UniversalDataBuss.Integration.RecipientRoleType.HSA;

		protected override RecipientRoleType PickupOrDeliveryCartageRole => UniversalDataBuss.Integration.RecipientRoleType.DCA;

		public override DataContextType MasterBillDataContextType => DataContextType.USAMS;

		public override CodeDescriptionPair ManifestType => new CodeDescriptionPair
		{
			Code = DirectionTypeList.Codes.NVOCC,
			Description = DirectionTypeList.Descriptions.NVOCC,
		};

		protected override bool ShouldStripNonWesternEuropeanCharacters => HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersUSSeaAMS.Value;

		protected override void OnConversionFactorySaving(BusinessObjectFactory factory)
		{
			if (!Shipment.HasSeaAMSTransferredLog())
			{
				var seaAMS = CustomsRelatedBusinessCollection.OfType<CusInBondHeader>().FirstOrDefault();
				seaAMS.OnSaving();

				var shipmentParameters = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, "AMS"),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Mode, TransportModes.Sea),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, seaAMS.BH_JobReference)
				};
				Shipment.Logs.AddNew(AutoEvents.Transferred, shipmentParameters.ToArray());

				var headerParameters = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, "HVL"),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.JobNumber, Shipment.JobNumber)
				};
				seaAMS.Logs.AddNew(AutoEvents.Transferred, headerParameters.ToArray());
			}
		}

		protected override void OnConversionSucceeded()
		{
			Shipment.SetSecurityFilingFirstUsageTimeForAllItems();
		}
	}
}
