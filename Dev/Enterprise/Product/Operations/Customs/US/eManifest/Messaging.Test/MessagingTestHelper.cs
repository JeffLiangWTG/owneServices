using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business.MessageBuilders.eManifest;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq;
using EDIMessage = Enterprise.Customs.US.eManifest.Business.EDIMessage;
using IAddress = Enterprise.Customs.Business.MessageBuilders.eManifest.IAddress;

namespace Enterprise.Customs.US.eManifest.Messaging.Testing
{
	public static class MessagingTestHelper
	{
		public static void SetupPostMasterEmailGroup(BusinessObjectFactory factory)
		{
			var group = factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			var staff = @group.Staff.AddNew();
			staff.GS_FullName = "blah";
			staff.GS_EmailAddress = "blah@blah.com";
			staff.GS_Code = "ZAC";
		}

		public static EDIMessage GetTransmitEDIMessage(BusinessObjectFactory factory, string type)
		{
			var message = factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.USeManifest;
			message.EM_MessageType = type;
			message.EM_MessageText = string.Format("Message Text {0}", Enterprise.Messaging.Business.EDIMessage.MessageNumberPlaceHolder);
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			return message;
		}

		public static EDIMessage GetReceivedEDIMessage(BusinessObjectFactory factory, string type, string subType, string messageNum)
		{
			var message = GetReceivedEDIMessage(factory, type, subType, ZDateTime.Now);
			message.MessageNumberStrategy = new TestMessageNumberStrategy(messageNum);
			message.EM_MessageNum = messageNum;
			return message;
		}

		public static EDIMessage GetReceivedEDIMessage(BusinessObjectFactory factory, string type, string subType, ZDateTime? createTime = null)
		{
			var message = factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.USeManifest;
			message.EM_MessageType = type;
			message.EM_MessageSubType = subType;
			message.EM_MessageText = string.Format("Message Text {0}", Enterprise.Messaging.Business.EDIMessage.MessageNumberPlaceHolder);
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_Status = EDIMessageStatusList.Codes.Received;
			message.EM_SystemCreateTimeUtc = createTime.GetValueOrDefault();
			return message;
		}

		public static EDIMessage CreateMessage(BusinessObjectFactory factory, string interchangeString)
		{
			var interchange = EDIInterchange.CreateNewInterchangeFromString(factory, interchangeString.Replace("\r\n", "'"), EDIInterchange.ApplicationCodes.USeManifest, false, true);
			interchange.SpawnMessagesFromInterchageTextAndMarkAsReceived();
			return factory.Load<EDIMessage>(interchange.ContainedMessages[0].PK);
		}

		public static EDIInterchange GetReceivedInterchange(BusinessObjectFactory factory, string bodyText)
		{
			var interchange = factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.USeManifest;
			interchange.EI_From = EDIMessage.ApplicationCodes.USCustoms;
			interchange.EI_To = EDIInterchange.ApplicationCodes.USeManifest;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_BodyText = bodyText;
			return interchange;
		}

		internal static EDIMessage GetSentEDIMessage(BusinessObjectFactory factory, string messageType, string messageNum, string text = null)
		{
			var sentMessage = GetSentEDIMessage(factory, messageType, messageNum, GlbStaff.CurrentUser, GlbBranch.CurrentBranch, ZDateTime.Now, text);
			sentMessage.EM_MessageNum = messageNum;
			return sentMessage;
		}

		internal static EDIMessage GetSentEDIMessage(BusinessObjectFactory factory, string messageType, string messageNum, GlbStaff userToNotify, GlbBranch branch, ZDateTime createTime, string text = null)
		{
			var sentMessage = factory.New<EDIMessage>();
			sentMessage.EM_MessageType = messageType;
			sentMessage.MessageNumberStrategy = new TestMessageNumberStrategy(messageNum);
			sentMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			sentMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
			sentMessage.EM_MessageText = (text ?? Enterprise.Messaging.Business.EDIMessage.MessageNumberPlaceHolder).Replace("\r\n", "'");
			sentMessage.EM_SystemCreateUser = userToNotify.GS_Code;
			sentMessage.EM_GB = branch.PK;
			sentMessage.EM_SystemCreateTimeUtc = createTime;
			return sentMessage;
		}

		internal static Trip GetTripAwaitingReply(BusinessObjectFactory factory, string pk, string reference)
		{
			var trip = factory.NewWithPrimaryKey<Trip>(new Guid(pk));
			trip.FillWithValidTestData();
			trip.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
			trip.BH_ImportTransportMode = TransportModes.Codes.Road;
			trip.BH_GB = GlbBranch.CurrentBranch.PK;
			trip.BH_JobReference = reference;
			trip.BH_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			return trip;
		}

		internal static Trip GetTripAwaitingReply(BusinessObjectFactory factory, string pk, string tripReference, string carrierCode)
		{
			var trip = factory.NewWithPrimaryKey<Trip>(new Guid(pk));
			trip.FillWithValidTestData();
			trip.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
			trip.BH_ImportTransportMode = TransportModes.Codes.Road;
			trip.BH_GB = GlbBranch.CurrentBranch.PK;
			trip.BH_VoyageNumber = tripReference;
			trip.BH_CarrierSCAC = carrierCode;
			trip.BH_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			return trip;
		}

		internal static ICompleteManifest GetCompleteManifestDataWithoutAmendmentReason(BusinessObjectFactory factory, string messageType, bool noCrewId = false)
		{
			var mock = new Mock<ICompleteManifest>();
			mock.Setup(m => m.Factory).Returns(factory);
			mock.Setup(m => m.IsFinalized).Returns(ZBool.True);
			mock.Setup(m => m.Messages).Returns(new EDIMessageCollection(GetCusEntryNumber(factory)));
			mock.Setup(m => m.TransmissionReferenceNumber).Returns(messageType + 1);
			mock.Setup(m => m.CarrierCode).Returns(new ZString("LOCK"));
			mock.Setup(m => m.MethodOfTransportation).Returns(new ZString(TransportModes.Codes.Road));
			mock.Setup(m => m.TripReference).Returns(messageType == MessageTypes.Codes.UnassociatedShipments ? new ZString("SYSTEM") : new ZString("LOCKMAN0000001"));
			mock.Setup(m => m.EstimatedDateOfArrival).Returns(new ZDateTime(2011, 08, 26, 4, 23, 0));
			mock.Setup(m => m.FirstExpectedPortOfArrival).Returns(new ZString("2704"));
			mock.Setup(m => m.AmendmentReasonCode).Returns(ZString.Empty);
			mock.Setup(m => m.TransitDirectionCode).Returns(new ZString(TransitDirectionCodes.Codes.Importation));
			mock.Setup(m => m.CrewMembers).Returns(new[] { GetCrewMember(noCrewId), GetCrewMember(true, noCrewId) });
			mock.Setup(m => m.Conveyance).Returns(GetConveyance());
			mock.Setup(m => m.Equipment).Returns(new[] { GetEquipment("8987964", "68465464"), GetEquipment(ZString.Empty, "68465464"), GetEquipment(ZString.Empty, ZString.Empty) });
			mock.Setup(m => m.DepartmentOfTransportationNumber).Returns(new ZString("46456487"));

			var shipments = new[] { GetShipment(MessageActionCodes.Codes.Original), GetShipment(MessageActionCodes.Codes.Change, false, false, false), GetShipment(MessageActionCodes.Codes.Cancellation) };

			mock.Setup(m => m.Shipments).Returns(shipments);
			return mock.Object;
		}

		internal static ICompleteManifest GetCompleteManifestData(BusinessObjectFactory factory, string messageType, MessageSubTypes action, bool isFinalized, bool noCrewId = false, bool splitMessage_Original = false, bool isBRASSShipment = false)
		{
			var mock = new Mock<ICompleteManifest>();
			mock.Setup(m => m.Factory).Returns(factory);
			mock.Setup(m => m.IsFinalized).Returns(new ZBool(isFinalized));
			mock.Setup(m => m.Messages).Returns(new EDIMessageCollection(GetCusEntryNumber(factory)));
			mock.Setup(m => m.TransmissionReferenceNumber).Returns(messageType + 1);
			mock.Setup(m => m.CarrierCode).Returns(new ZString("LOCK"));
			mock.Setup(m => m.MethodOfTransportation).Returns(new ZString(TransportModes.Codes.Road));
			mock.Setup(m => m.TripReference).Returns(messageType == MessageTypes.Codes.UnassociatedShipments ? new ZString("SYSTEM") : new ZString("LOCKMAN0000001"));
			mock.Setup(m => m.EstimatedDateOfArrival).Returns(new ZDateTime(2011, 08, 26, 4, 23, 0));
			mock.Setup(m => m.FirstExpectedPortOfArrival).Returns(new ZString("2704"));
			mock.Setup(m => m.AmendmentReasonCode).Returns(splitMessage_Original ? ZString.Empty : new ZString(AmendmentReasonCodes.Codes.C03));
			mock.Setup(m => m.TransitDirectionCode).Returns(new ZString(TransitDirectionCodes.Codes.Importation));
			mock.Setup(m => m.CrewMembers).Returns(new[] { GetCrewMember(noCrewId), GetCrewMember(true, noCrewId) });
			mock.Setup(m => m.Conveyance).Returns(GetConveyance());
			mock.Setup(m => m.Equipment).Returns(new[] { GetEquipment("8987964", "68465464"), GetEquipment(ZString.Empty, "68465464"), GetEquipment(ZString.Empty, ZString.Empty) });
			mock.Setup(m => m.DepartmentOfTransportationNumber).Returns(new ZString("46456487"));
			IEnumerable<IShipment> shipments;
			if (messageType == MessageTypes.Codes.CompleteTrip || messageType == MessageTypes.Codes.PreliminaryTrip)
			{
				shipments = action == MessageSubTypes.Create ? new[] { GetShipment(MessageActionCodes.Codes.Link, isBRASSShipment: isBRASSShipment), GetShipment(MessageActionCodes.Codes.Link, false, isBRASSShipment: isBRASSShipment) } : new[] { GetShipment(MessageActionCodes.Codes.Link, isBRASSShipment: isBRASSShipment), GetShipment(MessageActionCodes.Codes.DeLink, false, isBRASSShipment: isBRASSShipment) };
			}
			else
			{
				shipments = action == MessageSubTypes.Create ? new[] { GetShipment(MessageActionCodes.Codes.Original, true, true, false, isBRASSShipment: isBRASSShipment), GetShipment(MessageActionCodes.Codes.Original, false, isBRASSShipment: isBRASSShipment) } : new[] { GetShipment(MessageActionCodes.Codes.Original, isBRASSShipment: isBRASSShipment), GetShipment(MessageActionCodes.Codes.Change, false, false, false, isBRASSShipment: isBRASSShipment), GetShipment(MessageActionCodes.Codes.Cancellation, isBRASSShipment: isBRASSShipment) };
			}

			mock.Setup(m => m.Shipments).Returns(shipments);
			return mock.Object;
		}

		static ICrew GetCrewMember(bool passenger = false, bool noId = false)
		{
			var mock = new Mock<ICrew>();
			if (passenger)
			{
				mock.Setup(m => m.CrewId).Returns(noId ? ZString.Empty : new ZString("1234"));
				mock.Setup(m => m.CrewType).Returns(new ZString(CrewTypes.Codes.Passenger));
				mock.Setup(m => m.IdType).Returns(new ZString(CrewACEIdTypes.Codes.ProximityCardId));
				mock.Setup(m => m.LastName).Returns(new ZString("TURNER"));
				mock.Setup(m => m.FirstName).Returns(new ZString("BILL"));
				mock.Setup(m => m.MiddleName).Returns(new ZString("BOOTSTRAP"));
				mock.Setup(m => m.DateOfBirth).Returns(new ZDate(1970, 12, 30));
				mock.Setup(m => m.Gender).Returns(new ZString(Constants.Genders.Man));
				mock.Setup(m => m.Citizenship).Returns(new ZString(Constants.CountryCodes.UnitedStates));
				mock.Setup(m => m.HazmatEndorsement).Returns(ZString.Empty);
				var result = new[] { GetTravelDocument(TravelDocumentTypes.Codes.Passport, "EA12343", Constants.CountryCodes.UnitedStates, ZString.Empty) };
				mock.Setup(m => m.TravelDocuments).Returns(result);
			}
			else
			{
				mock.Setup(m => m.CrewId).Returns(noId ? ZString.Empty : new ZString("0000041153"));
				mock.Setup(m => m.CrewType).Returns(new ZString(CrewTypes.Codes.ResponsibleParty));
				mock.Setup(m => m.IdType).Returns(new ZString(CrewACEIdTypes.Codes.Id));
				mock.Setup(m => m.LastName).Returns(new ZString("AOMAD"));
				mock.Setup(m => m.FirstName).Returns(new ZString("CHRIS"));
				mock.Setup(m => m.MiddleName).Returns(ZString.Empty);
				mock.Setup(m => m.DateOfBirth).Returns(new ZDate(1935, 09, 19));
				mock.Setup(m => m.Gender).Returns(new ZString(Constants.Genders.Man));
				mock.Setup(m => m.Citizenship).Returns(new ZString(Constants.CountryCodes.UnitedStates));
				mock.Setup(m => m.HazmatEndorsement).Returns(new ZString("8456"));
				var result = new[] { GetTravelDocument(TravelDocumentTypes.Codes.CommercialDriversLicense, "P100971204141", Constants.CountryCodes.UnitedStates, USStatesList.Codes.Virginia), GetTravelDocument(TravelDocumentTypes.Codes.Passport, "15504141", Constants.CountryCodes.UnitedStates, ZString.Empty), GetTravelDocument(TravelDocumentTypes.Codes.OtherTravelDocument, "13465", ZString.Empty, ZString.Empty) };
				mock.Setup(m => m.TravelDocuments).Returns(result);
				var addressMock = new Mock<IAddress>();
				addressMock.Setup(m => m.IsEmpty).Returns(ZBool.False);
				addressMock.Setup(m => m.Address).Returns(new ZString("11107 SUNSET HILLS ROAD"));
				addressMock.Setup(m => m.City).Returns(new ZString("RESTON"));
				addressMock.Setup(m => m.StateOrProvince).Returns(new ZString(USStatesList.Codes.Virginia));
				addressMock.Setup(m => m.Country).Returns(new ZString(Constants.CountryCodes.UnitedStates));
				addressMock.Setup(m => m.Postcode).Returns(new ZString("20190"));
				mock.Setup(m => m.USAddress).Returns(addressMock.Object);
			}

			return mock.Object;
		}

		static ITravelDocument GetTravelDocument(ZString type, ZString number, ZString country, ZString state)
		{
			var docMock = new Mock<ITravelDocument>();
			docMock.Setup(m => m.TravelDocumentType).Returns(type);
			docMock.Setup(m => m.TravelDocumentNumber).Returns(number);
			docMock.Setup(m => m.CountryOfIssuance).Returns(country);
			docMock.Setup(m => m.StateOrProvinceOfIssuance).Returns(state);
			return docMock.Object;
		}

		static IConveyance GetConveyance()
		{
			var mock = new Mock<IConveyance>();
			mock.Setup(m => m.EquipmentType).Returns(new ZString(ConveyanceTypes.Codes.PickupTruck));
			mock.Setup(m => m.EquipmentId).Returns(new ZString("1234567890"));
			mock.Setup(m => m.LicensePlates).Returns(new[] { GetLicensePlate("BBDD11"), GetLicensePlate("AABB23") });
			mock.Setup(m => m.SealNumbers).Returns(new ZString[] { "12345", "45613" });
			mock.Setup(m => m.TransponderId).Returns(new ZString("789543218"));
			mock.Setup(m => m.ConveyanceACEId).Returns(new ZString("64894654"));
			mock.Setup(m => m.ConveyanceId).Returns(new ZString("46765464"));
			mock.Setup(m => m.IITEntityIndicators).Returns(new ZString[] { IITEntityIndicatorCodes.Codes.EC, IITEntityIndicatorCodes.Codes.MC });
			var insuranceMock = new Mock<IInsurance>();
			insuranceMock.Setup(m => m.InsuranceName).Returns(new ZString("Hazmat Shipment Insurance"));
			insuranceMock.Setup(m => m.InsurancePolicyNumber).Returns(new ZString("23494564"));
			insuranceMock.Setup(m => m.InsuranceYearPolicyIssue).Returns(new ZInt(2011));
			insuranceMock.Setup(m => m.InsuranceAmount).Returns(new ZDecimal(2000000));
			mock.Setup(m => m.Insurance).Returns(insuranceMock.Object);
			return mock.Object;
		}

		static ILicensePlate GetLicensePlate(ZString plateNumber)
		{
			var mock = new Mock<ILicensePlate>();
			mock.Setup(m => m.LicensePlateNumber).Returns(plateNumber);
			mock.Setup(m => m.StateOrProvinceOfRegistration).Returns(new ZString(USStatesList.Codes.Illinois));
			mock.Setup(m => m.CountryOfRegistration).Returns(new ZString(Constants.CountryCodes.UnitedStates));
			return mock.Object;
		}

		static IEquipment GetEquipment(ZString aceId, ZString id)
		{
			var mock = new Mock<IEquipment>();
			mock.Setup(m => m.EquipmentType).Returns(new ZString("T1"));
			mock.Setup(m => m.EquipmentId).Returns(id);
			mock.Setup(m => m.EquipmentACEId).Returns(aceId);
			mock.Setup(m => m.LicensePlates).Returns(new[] { GetLicensePlate("BA12YY"), GetLicensePlate("230JIU") });
			mock.Setup(m => m.SealNumbers).Returns(new ZString[] { "56484", "64845" });
			mock.Setup(m => m.IITEntityIndicators).Returns(new ZString[] { IITEntityIndicatorCodes.Codes.MC, IITEntityIndicatorCodes.Codes.MI });
			return mock.Object;
		}

		static IShipment GetShipment(ZString action, bool fda = true, bool inBond = true, bool inBondNumber = true, bool isBRASSShipment = false)
		{
			var mock = new Mock<IShipment>();
			mock.Setup(m => m.ShipmentActionCode).Returns(action);
			mock.Setup(m => m.ShipmentType).Returns(new ZString(isBRASSShipment ? ShipmentTypes.Codes.BRASS : inBond ? ShipmentTypes.Codes.Inbond : ShipmentTypes.Codes.GoodsAstray));
			mock.Setup(m => m.ShipmentControlNumber).Returns(new ZString("1234654894"));
			mock.Setup(m => m.ShipmentIdentifier).Returns(new ZString("684864684"));
			mock.Setup(m => m.PortOrPointOfLoading).Returns(new ZString("Port of Victoria, CA"));
			mock.Setup(m => m.PortOrPointOfLoadingCodeType).Returns(new ZString(PortCodeTypes.Codes.LocationName));
			mock.Setup(m => m.PlaceOfReceipt).Returns(new ZString("Tahsis"));
			mock.Setup(m => m.ServiceType).Returns(new ZString(ServiceTypes.Codes.CollectOnDelivery));
			mock.Setup(m => m.TransferDestinationFIRMSCode).Returns(new ZString("9874"));
			mock.Setup(m => m.BoardedQuantity).Returns(new ZInt(6));
			mock.Setup(m => m.ShipmentAmendmentReasonCode).Returns(new ZString("01"));
			mock.Setup(m => m.FDAFreightIndicator).Returns(new ZBool(fda));
			mock.Setup(m => m.ShipmentWasOutOfUSFor45DaysOrLessIndicator).Returns(new ZBool(!inBond));
			var commodities = new[] { GetCommodity(GetEquipment("8987964", "68465464")), GetCommodity(GetEquipment(ZString.Empty, ZString.Empty), true) };
			mock.Setup(m => m.Commodities).Returns(commodities);
			mock.Setup(m => m.Parties).Returns(new[] { GetParty(PartyTypes.Codes.Consignee, "Consignee"), GetParty(PartyTypes.Codes.Shipper, "Shipper", false) });
			if (inBond)
			{
				var inbondMock = new Mock<IInBond>();
				inbondMock.Setup(m => m.InbondType).Returns(new ZString(InbondTypes.Codes.ImmediateExportation));
				inbondMock.Setup(m => m.InbondDestination).Returns(new ZString("1234"));
				inbondMock.Setup(m => m.OnwardCarrier).Returns(new ZString("4567"));
				inbondMock.Setup(m => m.BondedCarrier).Returns(new ZString("456789134654"));
				inbondMock.Setup(m => m.Inbond7512Number).Returns(inBondNumber ? new ZString("1234569876543211") : ZString.Empty);
				inbondMock.Setup(m => m.TransferCarrier).Returns(new ZString("321465987456"));
				inbondMock.Setup(m => m.ForeignPortOfDestination).Returns(new ZString("Port of Victoria, CA"));
				inbondMock.Setup(m => m.ForeignPortOfDestinationCodeType).Returns(new ZString(PortCodeTypes.Codes.LocationName));
				inbondMock.Setup(m => m.EstimatedDateOfUSExit).Returns(new ZDate(2011, 08, 31));
				inbondMock.Setup(m => m.MexicanPedimentoNumber).Returns("98765413265478");
				mock.Setup(m => m.InBond).Returns(inbondMock.Object);
			}
			else
			{
				mock.Setup(m => m.InBond).Returns((IInBond)null);
				mock.Setup(m => m.ExportDate).Returns(new ZDate(2011, 08, 31));
			}

			return mock.Object;
		}

		static ICommodity GetCommodity(IEquipment equipment, bool pounds = false)
		{
			var mock = new Mock<ICommodity>();
			mock.Setup(m => m.CargoGrossWeight).Returns(new ZDecimal(2.65));
			mock.Setup(m => m.WeightUnitOfMeasure).Returns(new ZString(pounds ? Constants.Weight.Pounds : Constants.Weight.Tonnes));
			mock.Setup(m => m.DescriptionOfCargo).Returns(new ZString("FRENCH DARK CHOCOLATE\n\rA LONG DESCRIPTION LINE WHICH SHOULD BE SPLIT OVER TWO FTX SEGMENTS"));
			mock.Setup(m => m.NumberOfPackages).Returns(new ZInt(100));
			mock.Setup(m => m.TypeOfPackages).Returns(new ZString(Constants.PkgUnit.Box));
			mock.Setup(m => m.ShippingMarks).Returns(new ZString("MARKS LINE 1\r\nMARKS LINE 2\r\nMARKS LINE 3\r\nMARKS LINE 4\r\nMARKS LINE 5\r\nMARKS LINE 6\r\nMARKS LINE 7\r\nMARKS LINE 8\r\nMARKS LINE 9\r\nMARKS LINE 10\r\nA LONG MARKS LINE THAT SHOULD BE SPLIT INTO TWO SEGMENTS"));
			mock.Setup(m => m.HarmonizedNumbers).Returns(new ZString[] { "6601100000", "6602001000", "6602001001", "6602001002", "6602001003", "6602001002" });
			mock.Setup(m => m.HazardousGoodsDetails).Returns(new[] { GetHazmatDetails("123"), GetHazmatDetails("456") });
			mock.Setup(m => m.VehicleIdentificationNumbers).Returns(new ZString[] { "64987465456", "4654879874", "65465464654", "6546546546", "654684984", "1354899" });
			mock.Setup(m => m.C4Codes).Returns(new ZString[] { "13213213", "21646544", "789798798", "23132132321", "65465465464", "71471741147" });
			mock.Setup(m => m.CustomsValue).Returns(new ZInt(2500));
			mock.Setup(m => m.CountryOfOrigin).Returns(new ZString(Constants.CountryCodes.France));
			mock.Setup(m => m.Equipment).Returns(equipment);
			return mock.Object;
		}

		static IHazardousGoods GetHazmatDetails(ZString code)
		{
			var mock = new Mock<IHazardousGoods>();
			mock.Setup(m => m.HazardousGoodsCode).Returns(code);
			mock.Setup(m => m.HazardousGoodsContactName).Returns(new ZString("Big Boss"));
			mock.Setup(m => m.HazardousGoodsContactPhone).Returns(new ZString("31264846516"));
			return mock.Object;
		}

		static IParty GetParty(ZString type, ZString name, bool phone = true)
		{
			var mock = new Mock<IParty>();
			mock.Setup(m => m.IsEmpty).Returns(ZBool.False);
			mock.Setup(m => m.PartyType).Returns(type);
			mock.Setup(m => m.PartyName).Returns(name);
			mock.Setup(m => m.PartyId).Returns(new ZString("54654"));
			mock.Setup(m => m.PartyIdType).Returns(new ZString(PartyIdTypes.Codes.ACE));
			mock.Setup(m => m.ABIRoutingCode).Returns(new ZString("5646464"));
			mock.Setup(m => m.Phone).Returns(phone ? new ZString("31264846516") : ZString.Empty);
			mock.Setup(m => m.Email).Returns(new ZString("party@test.net"));
			mock.Setup(m => m.Address).Returns(new ZString("12 Party Street"));
			mock.Setup(m => m.City).Returns(new ZString("Chicago"));
			mock.Setup(m => m.StateOrProvince).Returns(new ZString(USStatesList.Codes.Illinois));
			mock.Setup(m => m.Country).Returns(new ZString(Constants.CountryCodes.UnitedStates));
			mock.Setup(m => m.Postcode).Returns(new ZString("2009"));
			return mock.Object;
		}

		static CusEntryNumber GetCusEntryNumber(BusinessObjectFactory factory)
		{
			var cusEntryNumber = factory.New<CusEntryNumber>();
			cusEntryNumber.CE_ParentTable = "JobDeclaration";
			return cusEntryNumber;
		}

		internal class TestMessageNumberStrategy : IMessageNumberStrategy
		{
			internal TestMessageNumberStrategy(string messageNumber)
			{
				this.messageNumber = messageNumber;
			}

			public string GetMessageReferenceNumber() => messageNumber;
			readonly string messageNumber;
		}
	}
}
