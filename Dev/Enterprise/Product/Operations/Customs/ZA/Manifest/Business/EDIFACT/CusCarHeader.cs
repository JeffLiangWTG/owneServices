using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Messaging;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Manifest.Business.EDIFACT
{
	public class CusCarHeader : ICusCarHeader
		, IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider
		, IInterchangeSenderIdProvider
	{
		public CusCarHeader(AsycudaManifestHeader header)
		{
			Header = header;
		}

		public AsycudaManifestHeader Header { get; }

		#region Forwarded Properties

		public BusinessObjectFactory Factory => Header.Factory;
		public ZString CountryCode => Header.AMA_RN_NKCountry;
		public ZString CountryName => Header.CountryName;
		public bool HasBillsAndPacks => Header.HasBillsAndPacks;
		public string MessageFunctionSubTypeForCancel => Header.MessageFunctionSubTypeForCancel;
		public string MessageFunctionSubTypeForAmend => Header.MessageFunctionSubTypeForAmend;
		public ASYCUDA.Business.MessageStatusProvider MessageStatusProvider => Header.MessageStatusProvider;

		public string GetOldAndSetNewCountryMessagingStatus(ZString messageSubTypeString)
		{
			var calculator = ((IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider)this).GetCalculator(CountryCode);
			var newStatus = calculator?.GetMessageAwaitingStatus(messageSubTypeString) ?? ZAMessageStatusList.Codes.AwaitingResponse;
			return Header.SetNewCountryMessagingStatus(newStatus);
		}

		public string SetNewCountryMessagingStatus(ZString newStatus)
		{
			return Header.SetNewCountryMessagingStatus(newStatus);
		}

		#endregion

		#region ICusCarHeader - for messaging

		ManifestDocumentType ICusCarHeader.ManifestDocumentType
		{
			get
			{
				var result = ManifestDocumentType.None;
				var manifestType = Header.AMA_ManifestType;
				if (!manifestType.IsEmpty)
				{
					result = (ManifestDocumentType)Enum.Parse(typeof(ManifestDocumentType), manifestType);
				}
				return result;
			}
		}

		ZString ICusCarHeader.ManifestTypeOrBolNature
		{
			get
			{
				ZString result;
				if (Header?.IsRoad ?? false)
				{
					result = Header.AMA_Nature.ConvertWtgShipmentTypeCodeToAsycudaBolNatureCode();
					if (result.IsEmpty)
					{
						result = Header.AMA_Nature;
					}
				}
				else
				{
					var hasDifferentShipmentTypes = Header?.Bills.Cast<AsycudaBill>()
						.Select(x => x.ABL_ShipmentType)
						.Where(x => !x.IsEmpty)
						.Distinct().IsCountMoreThan(1) ?? false;

					if (hasDifferentShipmentTypes)
					{
						result = ZaShipmentTypeList.Codes.MutualMultipleZzz;
					}
					else
					{
						result = Header?.AMA_Nature.ConvertWtgShipmentTypeCodeToAsycudaBolNatureCode() ?? ZString.Empty;
					}
				}
				return result;
			}
		}

		ZString ICusCarHeader.MRNForAmendOrDelete => MessagingProvider.GetMRNForAmendOrDeleteFromMessages(Factory, CountryCode, ((IEDIMessageCollectionProvider)Header).Messages);

		ZDateTime ICusCarHeader.ManifestDate => Header.AMA_MasterBillIssueDate;

		ZDateTime ICusCarHeader.DepartureDate => Header.AMA_E_DEP;

		ZDateTime ICusCarHeader.ArrivalDate => Header.AMA_E_ARV;

		ZString ICusCarHeader.CW1Reference => Header.AMA_JobReference;

		IEnumerable<ICusCarParty> ICusCarHeader.GetParties(string billIssuer)
		{
			var carHeader = (ICusCarHeader)this;

			var masterCarrierCode = Header?.MasterCarrierCode ?? ZString.Empty;
			yield return new CusCarPartyFromIdentificationCode(masterCarrierCode, PartyType.ReportingCarrier_RL, GetCarrierCountryCode(masterCarrierCode));

			yield return new CusCarPartyFromIdentificationCode(billIssuer, PartyType.GroupingCentre_FZ);
			yield return new CusCarPartyFromIdentificationCode(MessageSenderCode, PartyType.MessageSender_MS);

			var shippingAgent = Header.ShippingAgent;
			if (shippingAgent != null)
			{
				yield return new CusCarPartyFromOrgAddress(shippingAgent, PartyType.TransitPrincipalsAgentOrRep_AH, CountryCode);
			}

			yield return GetCusCarPartyFromIdentificationCodeForCarrier();
		}

		ZString GetCarrierCountryCode(ZString carrierCode)
		{
			var result = CountryCode;
			if ((Header?.CarrierCCCCode ?? ZString.Empty) == carrierCode)
			{
				result = Header?.Carrier?.OA_RN_NKCountryCode ?? ZString.Empty;
			}
			return result;
		}

		CusCarPartyFromIdentificationCode GetCusCarPartyFromIdentificationCodeForCarrier()
		{
			var carrierCode = ((ICusCarHeader)this).CarrierCode;
			var carrierName = GetCarrierName(carrierCode);
			return new CusCarPartyFromIdentificationCode(carrierCode, PartyType.ReportingCarrier_DEG, GetCarrierCountryCode(carrierCode), carrierName);
		}

		IEnumerable<ICusCarPerson> ICusCarHeader.GetPeople(string billIssuer)
		{
			var header = Header;
			if (header != null)
			{
				foreach (CusPerson cusPerson in header.Persons)
				{
					var purpose = cusPerson.CPN_IsPassenger ? PartyType.Passenger_FL : PartyType.Driver_DR; // NB, our "Is/Isn't Passenger"	field cannot give us driver/crew/passenger information.  Instead, we'll assume that a road vehicle has no crew except the driver, so !IsPassenger->driver.
					yield return new CusCarPerson(cusPerson, purpose);
				}
			}
		}

		IEnumerable<ICusTransport> ICusCarHeader.Transports
		{
			get
			{
				var header = Header;
				if (header != null && header.IsTSS)
				{
					yield return new CusTransport(header);
				}
			}
		}

		ZString ICusCarHeader.ConveyanceNumberOrTransportName
		{
			get
			{
				var result = ZString.Empty;
				var header = Header;
				if (header != null)
				{
					if (Header.IsRoad)
					{
						result = header.AMA_VehicleRegistration.Left(10).PadRight(10)
								+ Header.AMA_Trailer1RegNo.PadRight(10)
								+ Header.AMA_Trailer2RegNo.PadRight(10);
					}
					else
					{
						result = header.AMA_Voyage;
					}
				}
				return result;
			}
		}

		ZString ICusCarHeader.AgentType => Header?.AMA_AgentType ?? ZString.Empty;

		ZString ICusCarHeader.TransportMode => Header?.AMA_TransportMode ?? ZString.Empty;

		ZString ICusCarHeader.CarrierCode => Header?.AMA_CarrierCode ?? ZString.Empty;

		ZString GetCarrierName(ZString carrierCode)
		{
			var name = ZString.Empty;
			if (Header?.IsSea ?? false)
			{
				var zzCarrier = Header?.GetMasterZZCarrier(carrierCode, true);
				name = zzCarrier?.ZZ4_Description ?? ZString.Empty;
			}

			return name;
		}

		ZString ICusCarHeader.VesselID => Header?.AMA_RadioCallSign ?? ZString.Empty;

		ZString ICusCarHeader.TransportNationality => Header?.AMA_RN_NKConveyanceNationality ?? ZString.Empty;

		ZString ICusCarHeader.PortOfLoading => Header?.AMA_RL_NKPortOfLoading ?? ZString.Empty;

		ZString ICusCarHeader.PortOfDischarge => Header?.AMA_RL_NKPortOfDischarge ?? ZString.Empty;

		ZString ICusCarHeader.PortOfDischargeCountry => ((ICusCarHeader)this).PortOfDischarge.Left(2);

		ZString ICusCarHeader.PortOfLoadingCountry => ((ICusCarHeader)this).PortOfLoading.Left(2);

		ZString ICusCarHeader.ImportExportNature => Header?.AMA_Nature ?? ZString.Empty;

		ZString ICusCarHeader.ContainerMode => Header?.AMA_ContainerMode ?? ZString.Empty;

		IEnumerable<ICusCarContainer> ICusCarHeader.GetContainersByBillIssuer(ZString billIssuer)
		{
			// Send one message per house bill issuer (pff). That means a header with mixed-issuer bills should report only the containers relevant to the bill for this issuer.
			foreach (var line in ((ICusCarHeader)this).GetLinesByBillIssuer(billIssuer))
			{
				foreach (var pack in line.Packages)
				{
					var c = (pack as CusCarPack)?.Container;
					if (c != null)
					{
						yield return new CusCarContainer(c);
					}
				}
			}
		}

		ZDecimal ICusCarHeader.GetGrossMassInKilosByBillIssuer(ZString billIssuer)
		{
			return ((ICusCarHeader)this).GetLinesByBillIssuer(billIssuer).Sum(line => line.BillWeight.InKilogramsSafe);
		}

		ZString ICusCarHeader.ManifestNumber => Header?.ManifestNumber ?? ZString.Empty;

		ZDateTime ICusCarHeader.ManifestRegistrationDate => Header.RegistrationDate;

		IEnumerable<ICusCarLine> ICusCarHeader.GetLinesByBillIssuer(ZString billIssuer)
		{
			var header = Header;
			if (header != null)
			{
				var supportAssociatedPacks = Header.SupportAssociatedPacks;
				foreach (AsycudaBill bill in header.Bills)
				{
					if (bill.ABL_BillIssuer == billIssuer)
					{
						var ccBillCountry = new CusCarBill(bill);

						if (supportAssociatedPacks)
						{
							foreach (ABLEntryNum abcEntryNum in bill.CustomsEntryNumbers)
							{
								yield return new CusCarEntryNum(abcEntryNum, ccBillCountry);
							}
						}
						else
						{
							yield return ccBillCountry;
						}
					}
				}
			}
		}

		ZString ICusCarHeader.CustomsOffice => Header?.AMA_CustomsOffice ?? ZString.Empty;

		ZString ICusCarHeader.CARN => Header.CARN;

		ZString ICusCarHeader.PlaceOfExit => Header.PlaceOfExit;

		ZDateTime ICusCarHeader.DateAtCustomsOffice => Header.DateAtCustomsOffice;

		IEnumerable<ICusCarContainer> ICusCarHeader.HeaderContainers
		{
			get => Header?.Containers?.OfType<AsycudaContainer>().Select(c => new CusCarContainer(c));
		}

		ZString ICusCarHeader.CustomsCodeForContainerMode
		{
			get => ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Factory,
				Core.Constants.CountryCodes.SouthAfrica,
				RefCusMapTypeList.Codes.CMODE,
				((ICusCarHeader)this).ContainerMode, ZDateTime.Today);
		}

		ZString ICusCarHeader.CustomsCodeForImportExportNature => Header?.CallPurposeCode ?? ZString.Empty;

		ZString ICusCarHeader.MasterBol => Header?.MasterBOL ?? ZString.Empty;

		ZString ICusCarHeader.CARNForAmendOrDelete => Header.RegistrationNumber;

		ZDateTime ICusCarHeader.EstimatedTimeOfLoading => Header.EstimatedTimeOfLoading;

		#endregion

		#region IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider

		IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider EDIFACTMessageProvider => Header;

		EDIFACTMessageStatusCalculator IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider.GetCalculator(string country)
			=> EDIFACTMessageProvider.GetCalculator(country);

		ZString IEDIFACTMessageAttachee.MessageStatus
		{
			get => EDIFACTMessageProvider.MessageStatus;
			set => EDIFACTMessageProvider.MessageStatus = value;
		}

		ZString IEDIFACTMessageAttachee.JobStatus
		{
			get => EDIFACTMessageProvider.JobStatus;
			set => EDIFACTMessageProvider.JobStatus = value;
		}

		void IEDIFACTMessageAttachee.AddMessage(EDIMessage message)
		{
			EDIFACTMessageProvider.AddMessage(message);
		}

		ZString IEDIFACTMessageAttachee.JobIdentification => EDIFACTMessageProvider.JobIdentification;

		BusinessObject IEDIFACTMessageAttachee.TopLevelBusinessObject => EDIFACTMessageProvider.TopLevelBusinessObject;

		bool IEDIFACTMessageAttachee.HasChanges => EDIFACTMessageProvider.HasChanges;

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage => true;

		EDIMessageCollection IEDIMessageCollectionProvider.Messages => EDIFACTMessageProvider.Messages;

		BusinessObjectFactory IEDIMessageCollectionProvider.Factory => Factory;

		#endregion

		#region IAgentCodeProviderForInterchanges

		IInterchangeSenderIdProvider AgentCodeProvider => Header;

		ZString IInterchangeSenderIdProvider.SenderID => AgentCodeProvider.SenderID;

		public string MessageSenderCode => AgentCodeProvider.SenderID;

		#endregion
	}
}
