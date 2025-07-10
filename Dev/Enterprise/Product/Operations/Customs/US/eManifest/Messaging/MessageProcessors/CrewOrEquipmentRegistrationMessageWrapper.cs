using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Edifact.D08A.Elements;
using Enterprise.Edifact.D08A.Messages.MEDPID;
using Enterprise.Edifact.D08A.Segments;
using Enterprise.ZArchitecture.Core;
using Converter = Enterprise.Customs.US.eManifest.Messaging.CompleteManifestDataConverter;

namespace Enterprise.Customs.US.eManifest.Messaging.MessageProcessors
{
	class CrewOrEquipmentRegistrationMessageWrapper
	{
		internal CrewOrEquipmentRegistrationMessageWrapper(BusinessObjectFactory factory, MEDPIDMessage cusres)
		{
			Factory = factory;
			medpid = Argument.NotNull(cusres, "medpid", "Supported EDIFACT message type is D02A MEDPID");
		}

		#region Message Type

		internal bool IsValidResponse
		{
			get { return IsAcceptedResponse || IsErrorResponse; }
		}

		internal bool IsAcceptedResponse
		{
			get { return MessageFunction == MessageFunctionCodeList.AcceptedWithoutReserves; }
		}

		internal bool IsErrorResponse
		{
			get { return MessageFunction == MessageFunctionCodeList.TransactionOnHold; }
		}

		MessageFunctionCodeList MessageFunction
		{
			get { return D08AMessageUtilities.GetMessageFunction(medpid.BGM); }
		}

		#endregion

		#region Reported Data

		internal ZString TransmissionReferenceNumber
		{
			get
			{
				var crew = CrewMembers.FirstOrDefault();
				if (crew != null)
				{
					return crew.UniqueKey;
				}

				var equipment = Equipment.FirstOrDefault();
				if (equipment != null)
				{
					return equipment.UniqueKey;
				}

				return ZString.Empty;
			}
		}

		internal IEnumerable<EquipmentWrapper> Equipment
		{
			get
			{
				return from SegmentGroup2 group2 in medpid.Group2
					   where CheckInfoType(group2, RegistrationInfoTypes.Codes.Equipment, RegistrationInfoTypes.Codes.Conveyance)
					   select new EquipmentWrapper(group2);
			}
		}

		internal IEnumerable<CrewMemberWrapper> CrewMembers
		{
			get
			{
				return from SegmentGroup2 group2 in medpid.Group2
					   where CheckInfoType(group2, RegistrationInfoTypes.Codes.Crew)
					   select new CrewMemberWrapper(Factory, group2);
			}
		}

		static bool CheckInfoType(SegmentGroup2 group2, params string[] types)
		{
			return (from GISSegment gis in group2.GIS where types.Contains(gis.ProcessingIndicator.ProcessingIndicatorDescription) select gis).Any();
		}

		#region Equipment

		internal class EquipmentWrapper : RegistrationDataWrapper
		{
			internal EquipmentWrapper(SegmentGroup2 group2)
				: base(group2)
			{
				this.group2 = group2;
				loc = group2.LOC.Cast<LOCSegment>().FirstOrDefault() ?? new LOCSegment();
			}

			public ZString EquipmentType
			{
				get
				{
					var code = (from IHCSegment ihc in group2.IHC
								let type = (ZString)ihc.PersonInheritedCharacteristicDetails.InheritedCharacteristicDescription
								select type).FirstOrDefault();
					return new EquipmentTypes().GetEquipmentTypes().GetCodeDescription(code);
				}
			}

			public ZString EquipmentId
			{
				get { return D08AMessageUtilities.GetReference(group2.RFF, ReferenceCodeQualifierList.EquipmentNumber); }
			}

			public ZString ConveyanceId
			{
				get { return D08AMessageUtilities.GetReference(group2.RFF, ReferenceCodeQualifierList.TransportMeansJourneyIdentifier); }
			}

			public ZString VehicleIdentificationNumber
			{
				get { return D08AMessageUtilities.GetReference(group2.RFF, ReferenceCodeQualifierList.VehicleIdentificationNumberVin); }
			}

			public ZString TransponderId
			{
				get { return D08AMessageUtilities.GetReference(group2.RFF, ReferenceCodeQualifierList.TransactionReferenceNumber); }
			}

			public ZString LicensePlateNumber
			{
				get { return D08AMessageUtilities.GetReference(group2.RFF, ReferenceCodeQualifierList.VehicleLicenceNumber); }
			}

			public ZString CountryOfLicensePlateRegistration
			{
				get { return loc.LocationIdentification.LocationName; }
			}

			public ZString StateOrProvinceOfLicensePlateRegistration
			{
				get { return loc.RelatedLocationOneIdentification.FirstRelatedLocationName; }
			}

			internal string UniqueKey
			{
				get
				{
					var key = new StringBuilder();
					key.Append(EquipmentType);
					key.Append(EquipmentId);
					key.Append(ConveyanceId);
					key.Append(VehicleIdentificationNumber);
					key.Append(LicensePlateNumber);
					key.Append(TransponderId);
					return key.ToString().Replace(" ", "");
				}
			}

			protected override string GetID()
			{
				return !EquipmentId.IsEmpty ? EquipmentId : !ConveyanceId.IsEmpty ? ConveyanceId : LicensePlateNumber;
			}

			readonly LOCSegment loc;
		}

		#endregion

		#region Crew Member

		internal class CrewMemberWrapper : RegistrationDataWrapper
		{
			internal CrewMemberWrapper(BusinessObjectFactory factory, SegmentGroup2 group2)
				: base(group2)
			{
				this.factory = factory;
				pnaSegment = group2.PNA.Cast<PNASegment>().FirstOrDefault() ?? new PNASegment();
			}

			public ZString FirstName
			{
				get { return pnaSegment.NameComponentDetails1.NameComponentDescription; }
			}

			public ZString MiddleName
			{
				get
				{
					var result = ZString.Empty;
					var middleName = pnaSegment.NameComponentDetails2.NameComponentDescription;
					if (!middleName.IsNullOrEmpty())
					{
						var splitNames = middleName.Split(' ').Where(x => !x.IsNullOrEmpty());
						splitNames.ForEach(x => x.Trim());
						result = splitNames.ToStringDelimited(" ");
					}
					return result;
				}
			}

			public ZString LastName
			{
				get { return pnaSegment.NameComponentDetails3.NameComponentDescription; }
			}

			public ZDate DateOfBirth
			{
				get { return D08AMessageUtilities.GetDateTime(group2.DTM, DateOrTimeOrPeriodFunctionCodeQualifierList.PersonBirthDateTime).Date; }
			}

			public ZString Gender
			{
				get { return (from SegmentGroup3 group3 in group2.Group3 from PDISegment pdi in group3.PDI select pdi.GenderCode).FirstOrDefault(); }
			}

			public ZString Citizenship
			{
				get { return (from NATSegment nat in group2.NAT select nat.NationalityDetails.NationalityNameCode).FirstOrDefault(); }
			}

			public ZString HazmatEndorsement
			{
				get { return D08AMessageUtilities.GetReference(group2.RFF, ReferenceCodeQualifierList.DangerousGoodsTransportLicenceNumber); }
			}

			internal string UniqueKey
			{
				get
				{
					var key = new StringBuilder();
					key.Append(FirstName);
					key.Append(MiddleName);
					key.Append(LastName);
					key.Append(Gender);
					key.Append(DateOfBirth);
					key.Append(Citizenship);
					key.Append(DriverLicense);
					return key.ToString().Replace(" ", "");
				}
			}

			ZString FullName
			{
				get
				{
					var builder = new ZStringBuilder();
					builder.AppendIfNotEmpty(FirstName);
					builder.AppendIfNotEmpty(MiddleName);
					builder.AppendIfNotEmpty(LastName);
					return builder.ToStringWithDelimiterBetweenAppends(" ");
				}
			}

			ZString DriverLicense
			{
				get
				{
					var result = GetTravelDocument(TravelDocumentTypes.Codes.CommercialDriversLicense)
								 ?? GetTravelDocument(TravelDocumentTypes.Codes.EnhancedDriversLicense)
								 ?? GetTravelDocument(TravelDocumentTypes.Codes.DrivingLicenseNational);
					return result != null ? result.TravelDocumentNumber : ZString.Empty;
				}
			}

			TravelDocumentWrapper GetTravelDocument(string type)
			{
				return TravelDocuments.FirstOrDefault(t => t.DocType == type);
			}

			protected override string GetID()
			{
				return FullName;
			}

			internal IEnumerable<TravelDocumentWrapper> TravelDocuments
			{
				get
				{
					var qualifiersToSkip = new[]
					{
						ReferenceCodeQualifierList.GovernmentAgencyReferenceNumber,
						ReferenceCodeQualifierList.DangerousGoodsTransportLicenceNumber,
						ReferenceCodeQualifierList.StandardCarrierAlphaCodeScacNumber
					};
					return from RFFSegment rff in group2.RFF
						   where !qualifiersToSkip.Contains(rff.Reference.ReferenceCodeQualifier)
						   select new TravelDocumentWrapper(factory, rff, group2.LOC);
				}
			}

			#region TravelDocumentWrapper

			internal class TravelDocumentWrapper
			{
				internal TravelDocumentWrapper(BusinessObjectFactory factory, RFFSegment rff, LOCSegmentMessageSection locSection)
				{
					this.locSection = locSection;
					qualifier = rff.Reference.ReferenceCodeQualifier;
					identifier = new ZString(rff.Reference.ReferenceIdentifier);
					DocType = Converter.GetTravelDocumentTypeFromQualifier(factory, qualifier);
				}

				public ZString TravelDocumentType
				{
					get { return DocType.ToCodeDescription<TravelDocumentTypes>(qualifier); }
				}

				public ZString TravelDocumentNumber
				{
					get { return DocType == TravelDocumentTypes.Codes.Passport ? identifier.SubstringSafe(2) : identifier; }
				}

				public ZString CountryOfIssuance
				{
					get
					{
						switch (DocType)
						{
							case TravelDocumentTypes.Codes.Passport:
								return new ZString(identifier).Left(2);
							case TravelDocumentTypes.Codes.CommercialDriversLicense:
							case TravelDocumentTypes.Codes.EnhancedDriversLicense:
							case TravelDocumentTypes.Codes.DrivingLicenseNational:
								return (from LOCSegment loc in locSection
										where loc.RelatedLocationTwoIdentification.CodeListIdentificationCode == Converter.IdentificationCodes.Country
										select loc.RelatedLocationTwoIdentification.SecondRelatedLocationName).FirstOrDefault();
							case TravelDocumentTypes.Codes.PermanentResidentCard1:
							case TravelDocumentTypes.Codes.PermanentResidentCard2:
								return (from LOCSegment loc in locSection
										where loc.LocationIdentification.CodeListIdentificationCode == Converter.IdentificationCodes.CountrySubEntity
										select loc.LocationIdentification.LocationName).FirstOrDefault();
							default:
								return ZString.Empty;
						}
					}
				}

				public ZString StateOrProvinceOfIssuance
				{
					get
					{
						switch (DocType)
						{
							case TravelDocumentTypes.Codes.CommercialDriversLicense:
							case TravelDocumentTypes.Codes.EnhancedDriversLicense:
							case TravelDocumentTypes.Codes.DrivingLicenseNational:
								return (from LOCSegment loc in locSection
										where loc.RelatedLocationTwoIdentification.CodeListIdentificationCode == Converter.IdentificationCodes.StateOfDriverLicense
										select loc.RelatedLocationTwoIdentification.SecondRelatedLocationName).FirstOrDefault();
							default:
								return ZString.Empty;
						}
					}
				}

				readonly LOCSegmentMessageSection locSection;
				readonly ReferenceCodeQualifierList qualifier;
				readonly ZString identifier;
				internal ZString DocType { get; private set; }
			}

			#endregion

			readonly BusinessObjectFactory factory;
			readonly PNASegment pnaSegment;
		}

		#endregion

		#region Common Data

		internal abstract class RegistrationDataWrapper
		{
			protected RegistrationDataWrapper(SegmentGroup2 group2)
			{
				this.group2 = group2;
			}

			internal bool IsAccepted
			{
				get { return (from FTXSegment ftx in group2.FTX where ftx.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.GeneralInformation select ftx).Any(); }
			}

			internal string InfoDescription
			{
				get { return new RegistrationInfoTypes().GetDescriptionFromCode(InfoType); }
			}

			internal string InfoType
			{
				get { return (from GISSegment gis in group2.GIS select gis.ProcessingIndicator.ProcessingIndicatorDescription).FirstOrDefault(); }
			}

			public ZString CarrierCode
			{
				get { return D08AMessageUtilities.GetReference(group2.RFF, ReferenceCodeQualifierList.StandardCarrierAlphaCodeScacNumber); }
			}

			public ZString ACEId
			{
				get
				{
					var result = D08AMessageUtilities.GetFreeText(group2.FTX, TextSubjectCodeQualifierList.GeneralInformation).Split(' ').LastOrDefault();
					if (result.IsEmpty)
					{
						result = D08AMessageUtilities.GetReference(group2.RFF, ReferenceCodeQualifierList.GovernmentAgencyReferenceNumber);
					}

					return result;
				}
			}

			internal IEnumerable<Notification> Notifications
			{
				get
				{
					return from text in D08AMessageUtilities.GetFreeTextGroupBySegment(group2.FTX, TextSubjectCodeQualifierList.ErrorDescriptionFreeText)
						   select new Notification(string.Format("{0} ({1})", InfoDescription, GetID()), text.Left(3), text.SubstringSafe(3));
				}
			}

			protected abstract string GetID();

			#region Notification

			internal class Notification : ITableInterpretation
			{
				internal Notification(string caption, string code, string description)
				{
					this.caption = caption;
					Code = code;
					this.description = description;
				}

				#region Implementation of ITableInterpretation

				string ITableInterpretation.Caption
				{
					get { return caption + " Errors"; }
				}

				IEnumerable<string> ITableInterpretation.Titles
				{
					get
					{
						yield return "Code";
						yield return "Error";
					}
				}

				IEnumerable<object> ITableValues.Values
				{
					get
					{
						yield return Code;
						yield return description;
					}
				}

				#endregion

				readonly string caption;
				internal string Code { get; private set; }
				readonly string description;
			}

			#endregion

			protected SegmentGroup2 group2;
		}

		#endregion

		#endregion

		readonly MEDPIDMessage medpid;
		internal BusinessObjectFactory Factory { get; private set; }
	}
}
