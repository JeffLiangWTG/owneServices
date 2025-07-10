using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer
{
	public class PortManifestDataObjectWriter : ITopLevelDataObjectWriter
	{
		public PortManifestDataObjectWriter(IDataWritingManager manager, ZString senderID, ZString portCode, ZString direction, ZString messagePurpose)
		{
			this.billOfLadingDataObjectWriter = ((IShipmentDataContextManager)typeof(BillOfLading).GetUniversalDataContextManager()).GetShipmentDataObjectWriter(manager);
			this.senderID = senderID;
			this.portCode = portCode;
			this.direction = direction;
			this.messagePurpose = messagePurpose;
		}
		readonly ITopLevelDataObjectWriter billOfLadingDataObjectWriter;
		readonly ZString senderID;
		readonly ZString portCode;
		readonly ZString direction;
		readonly ZString messagePurpose;

		public DataContextType TopLevelDataContextType => DataContextType.BillOfLading;

		public ZString EDIMessageSubType => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

		public ZString RootElementName => typeof(BillOfLading).GetAttribute<RootElementAttribute>().RootElementName;

		public ITopLevelDataObject GetDataObject(BusinessObject sourceBO)
		{
			var dataObject = (UniversalShipment)billOfLadingDataObjectWriter.GetDataObject(sourceBO);
			var sailing = GetCorrectSailing(sourceBO);

			if (sailing != null)
			{
				dataObject.VesselName = sailing.JX_JV_NKVessel;
				dataObject.VoyageFlightNo = sailing.JX_JV_VoyageFlight;
				dataObject.PortOfLoading = UNLOCO.New(sailing.PortOfLoading);
				dataObject.PortOfDischarge = UNLOCO.New(sailing.PortOfDischarge);
			}

			var vessel = sailing?.Vessel;

			AddAddInfos(dataObject, sourceBO.Factory, vessel);

			if (!messagePurpose.IsEmpty)
			{
				dataObject.DataContext.SetDocumentaryOverride((NoResString)"Port Manifest", messagePurpose, null, true, 1, 1);
			}

			if (!string.IsNullOrEmpty(vessel?.RV_LloydsNumber))
			{
				dataObject.LloydsIMO = vessel.RV_LloydsNumber;
			}

			AddNIFRegistrationNumberIfNotExistsForDepartureCTOAndArrivalCTO(dataObject);

			return dataObject;
		}

		JobSailing GetCorrectSailing(BusinessObject sourceBO)
		{
			var matchedTransport = (sourceBO as BillOfLading)?.Transports?.OfType<Transport>()
				.FirstOrDefault(transport =>
					{
						if (direction == Constants.PortDirection.Load)
						{
							return transport.JW_RL_NKLoadPort == portCode;
						}
						else if (direction == Constants.PortDirection.Discharge)
						{
							return transport.JW_RL_NKDiscPort == portCode;
						}
						return false;
					}
				);
			return matchedTransport?.Sailing ?? (sourceBO as BillOfLading)?.Sailing;
		}

		void AddAddInfos(UniversalShipment dataObject, BusinessObjectFactory factory, RefVessel vessel)
		{
			var addInfos = new List<AddInfo>()
			{
				new AddInfo()
				{
					Key = "SenderID",
					Value = senderID
				},
				new AddInfo()
				{
					Key = "OperationalPort_Code",
					Value = portCode
				},
				new AddInfo()
				{
					Key = "OperationalPort_Name",
					Value = factory.LoadFromUniqueKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, portCode)?.RL_PortName ?? ZString.Empty
				},
				new AddInfo()
				{
					Key = (NoResString)"Direction",
					Value = direction
				},
				new AddInfo()
				{
					Key = "VesselType",
					Value = vessel?.RV_VesselType ?? ZString.Empty
				},
			};

			if (dataObject.AddInfoCollection != null)
			{
				dataObject.AddInfoCollection.AddRange(addInfos);
			}
			else
			{
				dataObject.SetAddInfoCollection(() => addInfos);
			}
		}

		void AddNIFRegistrationNumberIfNotExistsForDepartureCTOAndArrivalCTO(UniversalShipment dataObject)
		{
			if (portCode.SubstringSafe(0, 2) == Core.Constants.CountryCodes.Spain)
			{
				foreach (var transport in dataObject.TransportLegCollection)
				{
					AddNIFRegistrationNumberIfNotExistsForDepartureCTOAndArrivalCTOCore(transport.ArrivalCTO);
					AddNIFRegistrationNumberIfNotExistsForDepartureCTOAndArrivalCTOCore(transport.DepartureCTO);
				}
			}
		}

		void AddNIFRegistrationNumberIfNotExistsForDepartureCTOAndArrivalCTOCore(OrganizationAddress organizationAddress)
		{
			if ((organizationAddress?.GovRegNumType?.Code ?? ZString.Empty) == OrgCusCode.SpainCodeTypes.NIF
				&& (!organizationAddress?.RegistrationNumberCollection?.Any(registrationNumber => (registrationNumber.Type?.Code ?? ZString.Empty) == OrgCusCode.SpainCodeTypes.NIF) ?? true))
			{
				var registrationNumberCollection = organizationAddress?.RegistrationNumberCollection ?? new List<RegistrationNumber>();

				registrationNumberCollection.Add(new RegistrationNumber
				{
					CountryOfIssue = new Country() { Code = CountryCodes.Spain, Name = (NoResString)"Spain" },
					Value = organizationAddress.GovRegNum,
					Type = new RegistrationNumberType { Code = organizationAddress.GovRegNumType.Code, Description = organizationAddress.GovRegNumType.Description }
				});

				organizationAddress.SetRegistrationNumberCollection(() => registrationNumberCollection);
			}
		}
	}
}
