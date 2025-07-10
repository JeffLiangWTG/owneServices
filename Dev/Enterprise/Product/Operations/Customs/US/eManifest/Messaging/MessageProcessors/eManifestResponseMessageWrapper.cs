using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Edifact.D08A.Elements;
using Enterprise.Edifact.D08A.Messages.CUSRES;
using Enterprise.Edifact.D08A.Segments;
using Enterprise.ZArchitecture.Core;
using Converter = Enterprise.Customs.US.eManifest.Messaging.CompleteManifestDataConverter;

namespace Enterprise.Customs.US.eManifest.Messaging.MessageProcessors
{
	public class eManifestResponseMessageWrapper
	{
		internal eManifestResponseMessageWrapper(BusinessObjectFactory factory, CUSRESMessage cusres)
		{
			Factory = factory;
			this.cusres = Argument.NotNull(cusres, "cusres", "Supported EDIFACT message type is D03B CUSRES");
			transportDetails = D08AMessageUtilities.GetTransportDetails(cusres.TDT, Converter.IdentificationCodes.ACEId)
							   ?? D08AMessageUtilities.GetTransportDetails(cusres.TDT, Converter.IdentificationCodes.ConveyanceId);
		}

		#region Message Type

		internal bool IsValidResponse
		{
			get { return IsAcceptedResponse || IsErrorResponse || IsStatusUpdate; }
		}

		internal bool IsAcceptedResponse
		{
			get { return NotificationType == DocumentNameCodeList.CustomsClearanceNotice && NotificationCode == ManifestTransmittal; }
		}

		internal bool IsErrorResponse
		{
			get { return NotificationType == DocumentNameCodeList.CustomsClearanceNotice && !IsAcceptedResponse; }
		}

		internal bool IsAcceptedWithErrors
		{
			get
			{
				var result = false;
				foreach (var notification in Notifications)
				{
					if (notification.Code == ManReturnedToPreliminary)
					{
						result = true;
					}
					else if (notification.Code == ManifestRejected)
					{
						result = false;
						break;
					}
				}
				return result;
			}
		}

		internal bool IsStatusUpdate
		{
			get { return NotificationType == DocumentNameCodeList.CargoStatus && (!NotificationCode.IsEmpty || Shipments.Any(s => s.Notifications.Any())); }
		}

		internal ZString NotificationCode
		{
			get { return Notifications.Select(n => n.Code).FirstOrDefault(); }
		}

		DocumentNameCodeList NotificationType
		{
			get { return D08AMessageUtilities.GetMessageCode(cusres.BGM); }
		}

		const string ManifestTransmittal = "081";
		const string ManifestRejected = "509";
		const string ManReturnedToPreliminary = "511";

		#endregion

		#region Notifications

		internal IEnumerable<Notification> Notifications
		{
			get
			{
				return from pair in D08AMessageUtilities.GetErrorCodesAndRejectComments(cusres.Group4)
					   select new Notification(NotificationType, "Trip", pair[0], pair[1]);
			}
		}

		internal class Notification : ITableInterpretation
		{
			internal Notification(DocumentNameCodeList type, string caption, string code, string comments)
			{
				this.type = type;
				this.caption = caption;
				Code = code;
				this.comments = comments;
			}

			#region Implementation of ITableInterpretation

			string ITableInterpretation.Caption
			{
				get { return caption + (type == DocumentNameCodeList.CargoStatus ? " Status Notifications" : " Errors"); }
			}

			IEnumerable<string> ITableInterpretation.Titles
			{
				get
				{
					yield return "Code";
					if (type == DocumentNameCodeList.CargoStatus)
					{
						yield return "Notification";
					}
					else
					{
						yield return "Error";
						yield return "Possible Reasons";
					}
				}
			}

			IEnumerable<object> ITableValues.Values
			{
				get
				{
					yield return Code;
					yield return comments;
					if (type != DocumentNameCodeList.CargoStatus)
					{
						yield return TableInterpretation.WrapText(new ErrorReasons().GetDescriptionFromCode(Code) ?? string.Empty, 60);
					}
				}
			}

			#endregion

			readonly DocumentNameCodeList type;
			readonly string caption;
			internal string Code { get; private set; }
			readonly string comments;
		}

		#endregion

		#region Reported Data

		internal string DataDescription
		{
			get { return IsErrorResponse ? " Invalid Data" : " Reported Data"; }
		}

		#region Trip

		internal ZString TransmissionReferenceNumber
		{
			get { return D08AMessageUtilities.GetFreeText(cusres.FTX, TextSubjectCodeQualifierList.PartyInformation); }
		}

		public ZDateTime ProcessingDate
		{
			get { return D08AMessageUtilities.GetDateTime(cusres.DTM, DateOrTimeOrPeriodFunctionCodeQualifierList.ProcessingStartDateTime); }
		}

		public ZString TripReference
		{
			get
			{
				var result = D08AMessageUtilities.GetMessageReference(cusres.BGM);
				return result == "SYSTEM" ? ZString.Empty : result;
			}
		}

		public ZString MethodOfTransportation
		{
			get
			{
				var type = transportDetails != null ? transportDetails.ModeOfTransport.TransportModeNameCode : string.Empty;
				return Converter.GetMethodOfTransportationFromQualifier(Factory, type).ToCodeDescription<TransportModes>(type);
			}
		}

		public ZString CarrierCode
		{
			get { return transportDetails != null ? transportDetails.Carrier.CarrierIdentifier : string.Empty; }
		}

		public ZString CarrierACEId
		{
			get { return D08AMessageUtilities.GetPartyReference(cusres.Group1, PartyFunctionCodeQualifierList.Carrier); }
		}

		public ZString FirstExpectedPortOfArrival
		{
			get { return D08AMessageUtilities.GetLocation(cusres.LOC, LocationFunctionCodeQualifierList.PlaceOfArrival); }
		}

		public ZString DistrictPortOfEntry
		{
			get { return D08AMessageUtilities.GetLocation(cusres.LOC, LocationFunctionCodeQualifierList.PortOfEntry); }
		}

		public ZDateTime EstimatedDateOfArrival
		{
			get { return D08AMessageUtilities.GetDateTime(cusres.DTM, DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansArrivalDateTimeEstimated); }
		}

		public ZString TransitDirectionCode
		{
			get { return transportDetails != null ? transportDetails.TransitDirectionIndicatorCode : string.Empty; }
		}

		public ZString AmendmentReasonCode
		{
			get { return D08AMessageUtilities.GetReference(cusres.Group3, ReferenceCodeQualifierList.GetFromString("RFA")); }
		}

		internal ConveyanceWrapper Conveyance
		{
			get { return new ConveyanceWrapper(cusres, transportDetails, cusres.Group3.Cast<SegmentGroup3>().TakeWhile(IsNotNewEquipmentGroup).ToList()); }
		}

		internal IEnumerable<EquipmentWrapper> Equipment
		{
			get
			{
				var equipmentReferenceGroups = new List<SegmentGroup3>();
				foreach (var group3 in cusres.Group3.Cast<SegmentGroup3>().SkipWhile(IsNotNewEquipmentGroup))
				{
					if (equipmentReferenceGroups.Count > 0 && !IsNotNewEquipmentGroup(group3))
					{
						yield return new EquipmentWrapper(equipmentReferenceGroups);
						equipmentReferenceGroups = new List<SegmentGroup3>();
					}
					equipmentReferenceGroups.Add(group3);
				}
				if (equipmentReferenceGroups.Count > 0)
				{
					yield return new EquipmentWrapper(equipmentReferenceGroups);
				}
			}
		}

		internal IEnumerable<CrewMemberWrapper> CrewMembers
		{
			get { return from SegmentGroup6 group6 in cusres.Group6 where !IsShipmentGroup(group6) select new CrewMemberWrapper(Factory, group6); }
		}

		internal IEnumerable<ShipmentWrapper> Shipments
		{
			get { return shipments ?? (shipments = (from SegmentGroup6 group6 in cusres.Group6 where IsShipmentGroup(group6) select new ShipmentWrapper(Factory, group6, NotificationType)).ToArray()); }
		}

		IEnumerable<ShipmentWrapper> shipments;

		#region Implementation

		static bool IsNotNewEquipmentGroup(SegmentGroup3 segmentGroup3)
		{
			var qualifiers = new[] { ReferenceCodeQualifierList.TransportEquipmentIdentifier, ReferenceCodeQualifierList.EquipmentNumber };
			return !segmentGroup3.RFF.Cast<RFFSegment>().Any(rff => qualifiers.Contains(rff.Reference.ReferenceCodeQualifier));
		}

		bool IsShipmentGroup(SegmentGroup6 group6)
		{
			var shipment = new ShipmentWrapper(Factory, group6, NotificationType);
			return !shipment.ShipmentControlNumber.IsEmpty;
		}

		#endregion

		#endregion

		#region Conveyance

		internal class ConveyanceWrapper
		{
			internal ConveyanceWrapper(CUSRESMessage cusres, TDTSegment transportDetails, IEnumerable<SegmentGroup3> referenceGroups)
			{
				this.cusres = cusres;
				this.transportDetails = transportDetails;
				this.referenceGroups = referenceGroups;
			}

			public ZString ConveyanceType
			{
				get
				{
					var type = transportDetails != null ? transportDetails.TransportMeans.TransportMeansDescription : string.Empty;
					return new ZString(type).ToCodeDescription<ConveyanceTypes>();
				}
			}

			public ZString ConveyanceACEId
			{
				get { return D08AMessageUtilities.GetTransportIdentifier(cusres.TDT, Converter.IdentificationCodes.ACEId); }
			}

			public ZString ConveyanceId
			{
				get { return D08AMessageUtilities.GetTransportIdentifier(cusres.TDT, Converter.IdentificationCodes.ConveyanceId); }
			}

			public ZString VehicleIdentificationNumber
			{
				get { return D08AMessageUtilities.GetTransportIdentifier(cusres.TDT, Converter.IdentificationCodes.VIN); }
			}

			public ZString DepartmentOfTransportationNumber
			{
				get { return D08AMessageUtilities.GetTransportIdentifier(cusres.TDT, Converter.IdentificationCodes.DOTNumber); }
			}

			public ZString TransponderId
			{
				get { return D08AMessageUtilities.GetTransportIdentifier(cusres.TDT, Converter.IdentificationCodes.TransponderId); }
			}

			public ZString InsuranceDetails
			{
				get { return D08AMessageUtilities.GetFreeText(cusres.FTX, TextSubjectCodeQualifierList.InsuranceInformation); }
			}

			public ZString SealNumbers
			{
				get { return D08AMessageUtilities.GetReferences(referenceGroups, ReferenceCodeQualifierList.TransportEquipmentSealIdentifier).ToStringDelimited("; "); }
			}

			public ZString IITEntityIndicators
			{
				get { return D08AMessageUtilities.GetReferences(referenceGroups, ReferenceCodeQualifierList.GetFromString("IIT")).ToStringDelimited("; "); }
			}

			internal IEnumerable<LicensePlateWrapper> LicensePlates
			{
				get
				{
					return from referenceGroup in D08AMessageUtilities.GetReferenceGroups(referenceGroups, ReferenceCodeQualifierList.AdditionalReferenceNumber)
						   select new LicensePlateWrapper(referenceGroup, ReferenceCodeQualifierList.AdditionalReferenceNumber);
				}
			}

			readonly CUSRESMessage cusres;
			readonly TDTSegment transportDetails;
			readonly IEnumerable<SegmentGroup3> referenceGroups;
		}

		#endregion

		#region Equipment

		internal class EquipmentWrapper
		{
			internal EquipmentWrapper(IEnumerable<SegmentGroup3> referenceGroups)
			{
				this.referenceGroups = referenceGroups;
			}

			public ZString EquipmentType
			{
				get
				{
					var type = D08AMessageUtilities.GetReference(referenceGroups, ReferenceCodeQualifierList.MutuallyDefinedReferenceNumber);
					return type.ToCodeDescription<EquipmentTypes>();
				}
			}

			public ZString EquipmentACEId
			{
				get { return D08AMessageUtilities.GetReference(referenceGroups, ReferenceCodeQualifierList.TransportEquipmentIdentifier); }
			}

			public ZString EquipmentId
			{
				get { return D08AMessageUtilities.GetReference(referenceGroups, ReferenceCodeQualifierList.EquipmentNumber); }
			}

			public ZString SealNumbers
			{
				get { return D08AMessageUtilities.GetReferences(referenceGroups, ReferenceCodeQualifierList.TransportEquipmentSealIdentifier).ToStringDelimited("; "); }
			}

			public ZString IITEntityIndicators
			{
				get { return D08AMessageUtilities.GetReferences(referenceGroups, ReferenceCodeQualifierList.GetFromString("IIT")).ToStringDelimited("; "); }
			}

			internal IEnumerable<LicensePlateWrapper> LicensePlates
			{
				get
				{
					return from referenceGroup in D08AMessageUtilities.GetReferenceGroups(referenceGroups, ReferenceCodeQualifierList.VehicleLicenceNumber)
						   select new LicensePlateWrapper(referenceGroup, ReferenceCodeQualifierList.VehicleLicenceNumber);
				}
			}

			readonly IEnumerable<SegmentGroup3> referenceGroups;
		}

		#endregion

		#region License Plate

		internal class LicensePlateWrapper
		{
			internal LicensePlateWrapper(SegmentGroup3 segmentGroup3, ReferenceCodeQualifierList licensePlateQualifier)
			{
				this.segmentGroup3 = segmentGroup3;
				this.licensePlateQualifier = licensePlateQualifier;
			}

			public ZString LicensePlateNumber
			{
				get { return D08AMessageUtilities.GetReference(segmentGroup3.RFF, licensePlateQualifier); }
			}

			public ZString CountryOfRegistration
			{
				get { return D08AMessageUtilities.GetLocation(segmentGroup3.LOC, LocationFunctionCodeQualifierList.PlaceOfRegistration, Converter.IdentificationCodes.Country); }
			}

			public ZString StateOrProvinceOfRegistration
			{
				get { return D08AMessageUtilities.GetLocation(segmentGroup3.LOC, LocationFunctionCodeQualifierList.PlaceOfRegistration, Converter.IdentificationCodes.CountrySubEntity); }
			}

			readonly SegmentGroup3 segmentGroup3;
			readonly ReferenceCodeQualifierList licensePlateQualifier;
		}

		#endregion

		#region Crew Member

		internal class CrewMemberWrapper
		{
			internal CrewMemberWrapper(BusinessObjectFactory factory, SegmentGroup6 group6)
			{
				this.factory = factory;
				this.group6 = group6;
				text = group6.FTX.Count > 0 ? group6.FTX[0].TextLiteral : new TextLiteralElements();
				nadSegment = (from SegmentGroup7 group7 in group6.Group7 from NADSegment nad in group7.NAD select nad).FirstOrDefault() ?? new NADSegment();
			}

			public ZString CrewType
			{
				get
				{
					var qualifier = nadSegment.PartyFunctionCodeQualifier;
					return Converter.GetCrewTypeFromQualifier(factory, qualifier).ToCodeDescription<CrewTypes>(qualifier);
				}
			}

			public ZString CrewId
			{
				get { return nadSegment.PartyIdentificationDetails.PartyIdentifier; }
			}

			public ZString IdType
			{
				get
				{
					var idType = nadSegment.PartyIdentificationDetails.CodeListIdentificationCode;
					return Converter.GetCrewACEIdTypeFromIdentificationCode(factory, idType).ToCodeDescription<CrewACEIdTypes>(idType);
				}
			}

			public ZString FirstName
			{
				get { return nadSegment.PartyName.PartyName2; }
			}

			public ZString MiddleName
			{
				get { return nadSegment.PartyName.PartyName3; }
			}

			public ZString LastName
			{
				get { return nadSegment.PartyName.PartyName1; }
			}

			public ZDate DateOfBirth
			{
				get { return D08AMessageUtilities.GetDateTime(group6.DTM, DateOrTimeOrPeriodFunctionCodeQualifierList.PersonBirthDateTime).Date; }
			}

			public ZString Gender
			{
				get { return text.FreeText1; }
			}

			public ZString Citizenship
			{
				get { return text.FreeText3; }
			}

			public ZString HazmatEndorsement
			{
				get { return text.FreeText2; }
			}

			internal TravelDocumentWrapper TravelDocument
			{
				get { return new TravelDocumentWrapper(factory, group6.DOC, group6.LOC); }
			}

			internal AddressWrapper USAddress
			{
				get
				{
					var group7 = group6.Group7.Count > 0 ? group6.Group7[0] : new SegmentGroup7();
					return new AddressWrapper(group7.NAD);
				}
			}

			#region TravelDocumentWrapper

			internal class TravelDocumentWrapper
			{
				internal TravelDocumentWrapper(BusinessObjectFactory factory, DOCSegmentMessageSection docSection, LOCSegmentMessageSection locSection)
				{
					this.factory = factory;
					doc = docSection.Count > 0 ? docSection[0] : new DOCSegment();
					loc = locSection;
				}

				public ZString TravelDocumentType
				{
					get
					{
						var code = doc.DocumentMessageName.DocumentNameCode;
						return Converter.GetTravelDocumentTypeFromCode(factory, code).ToCodeDescription<TravelDocumentTypes>(code);
					}
				}

				public ZString TravelDocumentNumber
				{
					get { return doc.DocumentMessageDetails.DocumentIdentifier; }
				}

				public ZString CountryOfIssuance
				{
					get { return D08AMessageUtilities.GetLocation(loc, LocationFunctionCodeQualifierList.PlaceOfDocumentIssue, Converter.IdentificationCodes.Country); }
				}

				public ZString StateOrProvinceOfIssuance
				{
					get { return D08AMessageUtilities.GetLocation(loc, LocationFunctionCodeQualifierList.PlaceOfDocumentIssue, Converter.IdentificationCodes.CountrySubEntity); }
				}

				readonly DOCSegment doc;
				readonly BusinessObjectFactory factory;
				readonly LOCSegmentMessageSection loc;
			}

			#endregion

			readonly BusinessObjectFactory factory;
			readonly SegmentGroup6 group6;
			readonly TextLiteralElements text;
			readonly NADSegment nadSegment;
		}

		#endregion

		#region Address

		internal class AddressWrapper
		{
			internal AddressWrapper(NADSegmentMessageSection nadSection)
			{
				nad = nadSection.Count > 0 ? nadSection[0] : new NADSegment();
			}

			public ZString Address
			{
				get
				{
					return nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier1
							   + nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier2
							   + nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier3;
				}
			}

			public ZString City
			{
				get { return nad.CityName; }
			}

			public ZString StateOrProvince
			{
				get { return nad.CountrySubdivisionDetails.CountrySubdivisionIdentifier; }
			}

			public ZString Country
			{
				get { return nad.CountryIdentifier; }
			}

			public ZString Postcode
			{
				get { return nad.PostalIdentificationCode; }
			}

			protected NADSegment nad;
		}

		#endregion

		#region Shipment

		internal class ShipmentWrapper
		{
			internal ShipmentWrapper(BusinessObjectFactory factory, SegmentGroup6 group6, DocumentNameCodeList notificationType)
			{
				this.factory = factory;
				this.group6 = group6;
				this.notificationType = notificationType;
			}

			internal IEnumerable<Notification> Notifications
			{
				get
				{
					return from pair in D08AMessageUtilities.GetErrorCodesAndRejectComments(group6.Group14)
						   select new Notification(notificationType, string.Format("Shipment {0}", ShipmentControlNumber), pair[0], pair[1]);
				}
			}

			internal ZString StatusNotificationCode
			{
				get
				{
					var notification = D08AMessageUtilities.GetErrorCodesAndRejectComments(group6.Group14).FirstOrDefault();
					return notification != null ? notification[0] : string.Empty;
				}
			}

			public ZDateTime StatusNotificationDate
			{
				get { return D08AMessageUtilities.GetDateTime(group6.DTM, DateOrTimeOrPeriodFunctionCodeQualifierList.ProcessingStartDateTime); }
			}

			public ZString ShipmentReleaseType
			{
				get
				{
					var typeCode = D08AMessageUtilities.GetDocType(group6.DOC, DocumentNameCodeList.GoodsDeclarationForImportation);
					var type = Converter.GetEntryTypeFromCode(factory, typeCode);
					var codeDescription = type.ToCodeDescription<ShipmentTypes>(typeCode);
					if (codeDescription == typeCode)
					{
						codeDescription = type.ToCodeDescription<InbondTypes>(typeCode);
					}

					return codeDescription;
				}
			}

			public ZString ShipmentControlNumber
			{
				get { return D08AMessageUtilities.GetReference(group6.RFF, ReferenceCodeQualifierList.WaybillNumber); }
			}

			public ZString ShipmentIdentifier
			{
				get { return D08AMessageUtilities.GetReference(group6.RFF, ReferenceCodeQualifierList.ShipmentReferenceNumber); }
			}

			public ZString PortOrPointOfLoading
			{
				get { return D08AMessageUtilities.GetLocation(group6.LOC, LocationFunctionCodeQualifierList.PlaceOfLoading); }
			}

			public ZString PlaceOfReceipt
			{
				get { return D08AMessageUtilities.GetLocation(group6.LOC, LocationFunctionCodeQualifierList.GoodsReceiptPlace); }
			}

			public ZString DistrictPortOfEntry
			{
				get { return D08AMessageUtilities.GetLocation(group6.LOC, LocationFunctionCodeQualifierList.FilingLocation); }
			}

			public ZString ArrivalFIRMSCode
			{
				get { return D08AMessageUtilities.GetLocation(group6.LOC, LocationFunctionCodeQualifierList.GoodsDepot); }
			}

			public ZString InbondPortOfUSDestination
			{
				get { return D08AMessageUtilities.GetLocation(group6.LOC, LocationFunctionCodeQualifierList.PlaceOfDeparture); }
			}

			public ZString ServiceType
			{
				get { return ZString.Empty; } //TODO: There is no ServiceType in the spec TEST IT
			}

			public ZString TransferDestinationFIRMSCode
			{
				get { return D08AMessageUtilities.GetLocation(group6.LOC, LocationFunctionCodeQualifierList.PlaceOfTransfer); }
			}

			public ZInt BoardedQuantity
			{
				get
				{
					var values = D08AMessageUtilities.GetFreeTextGroupBySegment(group6.Group12, TextSubjectCodeQualifierList.MutuallyDefined);
					return ZInt.ParseSafe(values.FirstOrDefault(), 0);
				}
			}

			public ZString FDAFreightIndicator
			{
				get
				{
					var indicator = D08AMessageUtilities.GetProcessingIndicator(group6.GEI, ProcessingInformationCodeQualifierList.GetFromString("7"));
					return new ZString(indicator).ToCodeDescription<FDAFreightIndicators>();
				}
			}

			public ZString IITEntityIndicator
			{
				get { return D08AMessageUtilities.GetReference(group6.RFF, ReferenceCodeQualifierList.GetFromString("IIT")); }
			}

			public ZString ShipmentAmendmentReasonCode
			{
				get { return D08AMessageUtilities.GetReference(group6.RFF, ReferenceCodeQualifierList.GetFromString("RFA")); }
			}

			internal IEnumerable<PartyWrapper> Parties
			{
				get
				{
					return from SegmentGroup7 group7 in group6.Group7
						   where group7.NAD.Cast<NADSegment>().Any(nad => Converter.IsPartyQualifier(nad.PartyFunctionCodeQualifier))
						   select new PartyWrapper(factory, group7);
				}
			}

			internal CommodityWrapper Commodity
			{
				get { return new CommodityWrapper(group6); }
			}

			internal InBondWrapper InBond
			{
				get { return new InBondWrapper(group6); }
			}

			#region Party

			internal class PartyWrapper : AddressWrapper
			{
				internal PartyWrapper(BusinessObjectFactory factory, SegmentGroup7 group7)
					: base(group7.NAD)
				{
					this.factory = factory;
					this.group7 = group7;
				}

				public ZString PartyType
				{
					get
					{
						var qualifier = nad.PartyFunctionCodeQualifier;
						return Converter.GetPartyTypeFromQualifier(factory, qualifier).ToCodeDescription<PartyTypes>(qualifier);
					}
				}

				public ZString PartyId
				{
					get { return nad.PartyIdentificationDetails.PartyIdentifier; }
				}

				public ZString PartyIdType
				{
					get
					{
						var idType = nad.PartyIdentificationDetails.CodeListIdentificationCode;
						return Converter.GetPartyIdTypeFromIdentificationCode(factory, idType).ToCodeDescription<PartyIdTypes>(idType);
					}
				}

				public ZString PartyName
				{
					get { return nad.PartyName.PartyName1 + nad.PartyName.PartyName2; }
				}

				public ZString ABIRoutingCode
				{
					get { return nad.NameAndAddress.NameAndAddressDescription1; }
				}

				public ZString Phone
				{
					get { return D08AMessageUtilities.GetContact(group7.Group8, CommunicationMeansTypeCodeList.Telephone); }
				}

				public ZString Email
				{
					get { return D08AMessageUtilities.GetContact(group7.Group8, CommunicationMeansTypeCodeList.ElectronicMail); }
				}

				readonly BusinessObjectFactory factory;
				readonly SegmentGroup7 group7;
			}

			#endregion

			#region Commodity

			internal class CommodityWrapper
			{
				internal CommodityWrapper(SegmentGroup6 group6)
				{
					this.group6 = group6;
				}

				public ZInt NumberOfPackages
				{
					get { return D08AMessageUtilities.GetQuantity(group6.PAC).ToZInt(); }
				}

				public ZString TypeOfPackages
				{
					get { return D08AMessageUtilities.GetUnitOfMeasure(group6.PAC); }
				}

				public ZDecimal CargoGrossWeight
				{
					get { return D08AMessageUtilities.GetQuantity(group6.MEA, MeasurementPurposeCodeQualifierList.ItemWeight); }
				}

				public ZString WeightUnitOfMeasure
				{
					get { return D08AMessageUtilities.GetUnitOfMeasure(group6.MEA, MeasurementPurposeCodeQualifierList.ItemWeight).ToCodeDescription<WeightUnits>(); }
				}

				public ZString DescriptionOfCargo
				{
					get { return D08AMessageUtilities.GetFreeTextAsEnumerable(group6.Group12, TextSubjectCodeQualifierList.GoodsItemDescription).ToStringDelimited(string.Empty); }
				}

				public ZString ShippingMarks
				{
					get { return D08AMessageUtilities.GetShippingMarks(group6.PCI); }
				}

				public ZInt CustomsValue
				{
					get { return D08AMessageUtilities.GetAmount(group6.Group9, MonetaryAmountTypeCodeQualifierList.GoodsItemForCustomsDeclaredValueAmount).ToZInt(); }
				}

				public ZString CountryOfOrigin
				{
					get { return D08AMessageUtilities.GetLocation(group6.LOC, LocationFunctionCodeQualifierList.CountryOfOrigin); }
				}

				public ZString HarmonizedNumbers
				{
					get { return D08AMessageUtilities.GetCodes(group6.Group12, Converter.IdentificationCodes.HarmonizedTariffCode).ToStringDelimited("; "); }
				}

				public ZString HazardousMaterialsDetails
				{
					get { return D08AMessageUtilities.GetFreeTextGroupBySegment(group6.Group12, TextSubjectCodeQualifierList.DangerousGoodsAdditionalInformation).ToStringDelimited("\r\n\r\n"); }
				}

				public ZString VehicleIdentificationNumbers
				{
					get { return D08AMessageUtilities.GetFreeTextAsEnumerable(group6.Group12, TextSubjectCodeQualifierList.ProductInformation).ToStringDelimited("; "); }
				}

				public ZString C4Codes
				{
					get { return D08AMessageUtilities.GetCodes(group6.Group12, Converter.IdentificationCodes.C4Code).ToStringDelimited("; "); }
				}

				readonly SegmentGroup6 group6;
			}

			#endregion

			#region InBond

			internal class InBondWrapper
			{
				internal InBondWrapper(SegmentGroup6 group6)
				{
					this.group6 = group6;
				}

				public ZString InbondDestination
				{
					get { return D08AMessageUtilities.GetLocation(group6.LOC, LocationFunctionCodeQualifierList.CustomsOfficeOfDestinationTransit); }
				}

				public ZString OnwardCarrier
				{
					get { return D08AMessageUtilities.GetPartyReference(group6.Group7, PartyFunctionCodeQualifierList.GetFromString("OCG")); }
				}

				public ZString BondedCarrier
				{
					get { return D08AMessageUtilities.GetPartyReference(group6.Group7, PartyFunctionCodeQualifierList.GoodsCustodian); }
				}

				public ZString Inbond7512Number
				{
					get { return D08AMessageUtilities.GetDocReference(group6.DOC, DocumentNameCodeList.GoodsDeclarationForCustomsTransit); }
				}

				public ZString TransferCarrier
				{
					get { return D08AMessageUtilities.GetPartyReference(group6.Group7, PartyFunctionCodeQualifierList.ConnectingCarrier); }
				}

				public ZString ForeignPortOfDestination
				{
					get { return D08AMessageUtilities.GetLocation(group6.LOC, LocationFunctionCodeQualifierList.PlaceOfDestination); }
				}

				public ZDate EstimatedDateOfUSExit
				{
					get { return D08AMessageUtilities.GetDateTime(group6.DTM, DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansDepartureDateTimeEstimated).Date; }
				}

				public ZString MexicanPedimentoNumber
				{
					get { return D08AMessageUtilities.GetReference(group6.RFF, ReferenceCodeQualifierList.GoodsDeclarationNumber); }
				}

				readonly SegmentGroup6 group6;
			}

			#endregion

			readonly BusinessObjectFactory factory;
			readonly SegmentGroup6 group6;
			readonly DocumentNameCodeList notificationType;
		}

		#endregion

		#endregion

		readonly CUSRESMessage cusres;
		readonly TDTSegment transportDetails;
		internal BusinessObjectFactory Factory { get; private set; }
	}
}
