using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UEntryType = Enterprise.UniversalDataBuss.DataObjects.Universal.EntryType;
using UShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn
{
	public class CusOutturnHeaderDataObjectWriter<TCusOutturnHeader> : TopLevelDataObjectWriter<TCusOutturnHeader, UShipment>
		where TCusOutturnHeader : CusOutturnHeader
	{
		public CusOutturnHeaderDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected override ZString GetEDIMessageSubType() => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

		protected override DataContextType GetTopLevelDataContextType() => DataContextType.SeaCargoOutturn;

		protected override void PopulateDataObject(TCusOutturnHeader sourceBO, UShipment shipment)
		{
			shipment.TransportMode = new CodeDescriptionPair
			{
				Code = Core.Constants.TransportModes.Sea,
				Description = Core.Constants.TransportModeDescriptions.Sea
			};

			shipment.MessageStatus = new CodeDescriptionPair
			{
				Code = sourceBO.C6_MessageStatus,
				Description = sourceBO.Lookups.OutturnStatusList.GetDescriptionFromCode(sourceBO.C6_MessageStatus)
			};

			shipment.VesselName = sourceBO.C6_VesselName;
			shipment.LloydsIMO = sourceBO.C6_LloydsIMO;
			shipment.VoyageFlightNo = sourceBO.C6_VoyageNum;

			PopulateDateCollection(sourceBO, shipment);
			PopulateOrganizationAddressCollection(writeManager, sourceBO, shipment);
			PopulateAdditionalReferenceCollection(sourceBO, shipment);
			PopulateSubShipmentCollection(sourceBO, shipment);
			PopulateCountrySpecificDetails(sourceBO, shipment);
		}

		protected virtual void PopulateCountrySpecificDetails(TCusOutturnHeader sourceBO, UShipment shipment)
		{
		}

		protected virtual void PopulateSubShipmentCollection(TCusOutturnHeader sourceBO, UShipment shipment)
		{
			var data = ProcessCollection(sourceBO.Outturns.Cast<CusOutturn>().OrderBy(x => x.C5_MasterBill).ThenBy(x => x.C5_HouseBill).ThenBy(x => x.C5_ContainerNumber).ThenBy(x => x.C5_CargoType), new CusOutturnDataObjectWriter<CusOutturn>(writeManager));
			shipment.SetSubShipmentCollection(() => data != null ? new DataObjectList<UShipment>(data) : null);
		}

		protected void PopulateDateCollection(TCusOutturnHeader sourceBO, UShipment shipment)
		{
			shipment.SetDateCollection(() => new List<Date> { { DateType.Arrival, ZBool.False, sourceBO.C6_DateOfArrival } });
		}

		protected void PopulateOrganizationAddressCollection(IDataWritingManager writeManager, TCusOutturnHeader sourceBO, UShipment shipment)
		{
			shipment.AddOrgAddress(writeManager, sourceBO.OutturningPremise, AddressTypes.ArrivalCFSAddress);
		}

		protected void PopulateAdditionalReferenceCollection(TCusOutturnHeader sourceBO, UShipment shipment)
		{
			shipment.SetAdditionalReferenceCollection(() =>
						new DataObjectList<AdditionalReference>
						{
							new AdditionalReference
							{
								Type = new UEntryType
								{
									Code = Constants.AdditionalReference.EntryType.Codes.ControlledPremiseID,
									Description = Constants.AdditionalReference.EntryType.Descriptions.ControlledPremiseID
								},
								ContextInformation = sourceBO.CustomsCountryCode,
								ReferenceNumber = sourceBO.C6_OutturningPremiseID
							},
							new AdditionalReference
							{
								Type = new UEntryType
								{
									Code = Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID,
									Description = Constants.AdditionalReference.EntryType.Descriptions.ResponsiblePartyID
								},
								ContextInformation = sourceBO.CustomsCountryCode,
								ReferenceNumber = sourceBO.C6_ResponsiblePartyID
							}
						});
		}
	}
}
