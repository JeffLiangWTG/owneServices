using System;
using System.Collections.Generic;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;

namespace Enterprise.Customs.PL.Business;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
public static class Constants
{
	internal static string AllSupportedApplications => $"{ApplicationCode.PLCustoms},{ApplicationCode.PLCustomsNCTS},{ApplicationCode.PLCustomsExitControl}";

	public const string PolishTimeZoneUnloco = "PLWAW";

	public static class SystemName
	{
		public const string AES = "AES";
		public const string AIS = "AIS";
		public const string NCTS = "NCTS";
	}

	public static class MaximumBusinessObjectsAmounts
	{
		public const int MaximumEntryLines = 999;
		public const int MaximumEntryLineContainers = 99;
		public const int MaximumAdditionalInformation = 99;
		public const int MaximumSupportingDocuments = 99;
		public const int MaximumPreviousDocuments = 99;
		public const int MaximumPackages = 99999999;
		public const int WarehouseAddressMaxLength = 35;
	}

	public static class EntryInstructionSubStyle
	{
		public static IReadOnlyCollection<ZString> SimplifiedDeclarationSubStyleList(BusinessObjectFactory factory) =>
			factory.GetCachedValue("Enterprise.Customs.PL.Business.CodeDescriptionPairLists.SimplifiedDeclarationSubStyleList", () => new HashSet<ZString> { SubStyleCodes.C, SubStyleCodes.F, SubStyleCodes.Y });

		public static IReadOnlyCollection<ZString> NormalDeclarationSubStyleList(BusinessObjectFactory factory) =>
			factory.GetCachedValue("Enterprise.Customs.PL.Business.CodeDescriptionPairLists.NormalDeclarationSubStyleList", () => new HashSet<ZString> { SubStyleCodes.A, SubStyleCodes.D });
	}

	public static class ProcedureCodes
	{
		public const string _10 = "10";
		public const string _11 = "11";
		public const string _21 = "21";
		public const string _22 = "22";
		public const string _23 = "23";
		public const string _31 = "31";
		public const string _40 = "40";
		public const string _41 = "41";
		public const string _42 = "42";
		public const string _44 = "44";
		public const string _45 = "45";
		public const string _46 = "46";
		public const string _48 = "48";
		public const string _49 = "49";
		public const string _51 = "51";
		public const string _53 = "53";
		public const string _54 = "54";
		public const string _61 = "61";
		public const string _63 = "63";
		public const string _68 = "68";
		public const string _71 = "71";
		public const string _76 = "76";
		public const string _77 = "77";
		public const string _78 = "78";
		public const string _91 = "91";
		public const string _92 = "92";
		public const string _96 = "96";
	}

	public static class ConcessionCodes
	{
		public const string _2PL = "2PL";
		public const string _3PL = "3PL";
		public const string _4PL = "4PL";
		public const string _5PL = "5PL";
		public const string _6PL = "6PL";
		public const string _7PL = "7PL";
		public const string _1C1 = "1C1";
		public const string _1H2 = "1H2";
		public const string _0V1 = "0V1";
		public const string _0V2 = "0V3";
		public const string _0V3 = "0V3";
		public const string _0V4 = "0V4";
		public const string _0V6 = "0V6";
		public const string _0V7 = "0V7";
		public const string _0V8 = "0V8";
		public const string _0V9 = "0V9";
		public const string _1V0 = "1V0";
		public const string _1V1 = "1V1";
		public const string _1V5 = "1V5";
		public const string _1V6 = "1V6";
		public const string _1V8 = "1V8";
		public const string _1V9 = "1V9";
		public const string _2V0 = "2V0";
		public const string _2V5 = "2V5";
		public const string _2V6 = "2V6";
		public const string _2V7 = "2V7";
		public const string _2V8 = "2V8";
		public const string _2V9 = "2V9";
		public const string _3V0 = "3V0";
		public const string _3V1 = "3V1";
		public const string _3V2 = "3V2";
		public const string _3V3 = "3V3";
		public const string _3V4 = "3V4";
		public const string _3V5 = "3V5";
		public const string _3V6 = "3V6";
		public const string _3V7 = "3V7";
		public const string _3V8 = "3V8";
		public const string _3V9 = "3V9";
		public const string _4V0 = "4V0";
		public const string _4V1 = "4V1";
		public const string _4V2 = "4V2";
		public const string _4V3 = "4V3";
		public const string _4V4 = "4V4";
		public const string _4V5 = "4V5";
		public const string _4V6 = "4V6";
		public const string _4V7 = "4V7";
		public const string _5V5 = "5V5";
		public const string _6A1 = "6A1";
		public const string _6A2 = "6A2";
		public const string _6A3 = "6A3";
		public const string _6A4 = "6A4";
		public const string _6A5 = "6A5";
		public const string _6A6 = "6A6";
		public const string _6A7 = "6A7";
		public const string _6A8 = "6A8";
		public const string _6A9 = "6A9";
		public const string _7A1 = "7A1";
		public const string _7A2 = "7A2";
		public const string _7A3 = "7A3";
		public const string _7A4 = "7A4";
		public const string _7A5 = "7A5";
		public const string _7A6 = "7A6";
		public const string _7A7 = "7A7";
		public const string _7A8 = "7A8";
		public const string _7A9 = "7A9";
		public const string _8A1 = "8A1";
		public const string _8A2 = "8A2";
		public const string _8A3 = "8A3";
		public const string _8A4 = "8A4";
		public const string _8A8 = "8A8";
		public const string D01 = "D01";
		public const string D02 = "D02";
		public const string D03 = "D03";
		public const string D04 = "D04";
		public const string D05 = "D05";
		public const string D06 = "D06";
		public const string D07 = "D07";
		public const string D08 = "D08";
		public const string D09 = "D09";
		public const string D10 = "D10";
		public const string D11 = "D11";
		public const string D12 = "D12";
		public const string D13 = "D13";
		public const string D14 = "D14";
		public const string D15 = "D15";
		public const string D16 = "D16";
		public const string D17 = "D17";
		public const string D18 = "D18";
		public const string D19 = "D19";
		public const string D20 = "D20";
		public const string D21 = "D21";
		public const string D22 = "D22";
		public const string D23 = "D23";
		public const string D24 = "D24";
		public const string D25 = "D25";
		public const string D26 = "D26";
		public const string D27 = "D27";
		public const string D28 = "D28";
		public const string D29 = "D29";
		public const string D30 = "D30";
		public const string D51 = "D51";
		public const string F06 = "F06";
		public const string C01 = "C01";
		public const string C02 = "C02";
		public const string C03 = "C03";
		public const string C04 = "C04";
		public const string C06 = "C06";
		public const string C07 = "C07";
		public const string C08 = "C08";
		public const string C09 = "C09";
		public const string C10 = "C10";
		public const string C11 = "C11";
		public const string C12 = "C12";
		public const string C13 = "C13";
		public const string C14 = "C14";
		public const string C15 = "C15";
		public const string C16 = "C16";
		public const string C17 = "C17";
		public const string C18 = "C18";
		public const string C19 = "C19";
		public const string C20 = "C20";
		public const string C21 = "C21";
		public const string C22 = "C22";
		public const string C23 = "C23";
		public const string C24 = "C24";
		public const string C25 = "C25";
		public const string C26 = "C26";
		public const string C27 = "C27";
		public const string C28 = "C28";
		public const string C29 = "C29";
		public const string C30 = "C30";
		public const string C31 = "C31";
		public const string C32 = "C32";
		public const string C33 = "C33";
		public const string C34 = "C34";
		public const string C35 = "C35";
		public const string C36 = "C36";
		public const string C37 = "C37";
		public const string C38 = "C38";
		public const string C39 = "C39";
		public const string C40 = "C40";
		public const string C41 = "C41";
		public const string B02 = "B02";
		public const string B03 = "B03";
		public const string F01 = "F01";
		public const string F02 = "F02";
		public const string F03 = "F03";
		public const string F15 = "F15";
		public const string F16 = "F16";
		public const string F21 = "F21";
		public const string F22 = "F22";
		public const string F48 = "F48";
	}

	public static class DecimalPlacesForCurrency
	{
		public const int DecimalPlacesForIDRandVND = 9;
		public const int DecimalPlacesForGeneral = 6;
	}

	public static class AdditionalInfoCodes
	{
		public const string _00100 = "00100";
		public const string _00200 = "00200";
		public const string _00500 = "00500";
		public const string _0PL05 = "0PL05";
		public const string _0PL06 = "0PL06";
		public const string _0PL07 = "0PL07";
		public const string _0PL08 = "0PL08";
		public const string _0PL09 = "0PL09";
		public const string _0PL10 = "0PL10";
		public const string _0PL12 = "0PL12";
		public const string _1PL15 = "1PL15";
		public const string _1PL17 = "1PL17";
		public const string _1PL18 = "1PL18";
		public const string _4PL03 = "4PL03";
		public const string _4PL04 = "4PL04";
		public const string _4PL05 = "4PL05";
		public const string _4PL07 = "4PL07";
		public const string _4PL09 = "4PL09";
		public const string _4PL10 = "4PL10";
		public const string _4PL12 = "4PL12";
		public const string _4PL15 = "4PL15";
		public const string _30600 = "30600";
		public const string _EXP04 = "EXP04";
		public const string _EXP15 = "EXP15";
		public const string _POW01 = "POW01";
		public const string _PCS01 = "PCS01";
	}

	public static class SupportingDocumentCodes
	{
		public const string _3DK5 = "3DK5";
		public const string _4DK3 = "4DK3";
		public const string C019 = "C019";
		public const string C504 = "C504";
		public const string C512 = "C512";
		public const string C513 = "C513";
		public const string C514 = "C514";
		public const string C515 = "C515";
		public const string C516 = "C516";
		public const string C601 = "C601";
		public const string C626 = "C626";
		public const string C627 = "C627";
		public const string C651 = "C651";
		public const string C710 = "C710";
		public const string N018 = "N018";
		public const string N325 = "N325";
		public const string N380 = "N380";
		public const string N990 = "N990";
		public const string Y044 = "Y044";
	}

	public static class SubStyleCodes
	{
		public const string A = "A";
		public const string B = "B";
		public const string C = "C";
		public const string D = "D";
		public const string E = "E";
		public const string F = "F";
		public const string R = "R";
		public const string V = "V";
		public const string X = "X";
		public const string Y = "Y";
		public const string Z = "Z";
	}

	public static class ReferenceNumberSpecialNumbers
	{
		public const string CCL = "CCL";
		public const string SDE = "SDE";
		public const string EIR = "EIR";
		public const string RW = "RW";
		public const string ZS = "ZS";
		public const string ZW = "ZW";
	}

	public static class CusCodeTypes
	{
		public const string PES = "PES";
	}

	public const string GlbStaffInstructionText = "W wypadku niewpłacenia w obowiązującym terminie kwoty należności wynikającej z długu celnego, opłat przewidzianych przepisami prawa celnego oraz podatków wykazanych w niniejszym zgłoszeniu celnym lub wpłacenia ich w niepełnej wysokości, niniejsze zgłoszenie celne stanowi podstawę do wystawienia tytułu wykonawczego, zgodnie z art. 3a § 1 pkt 2 i §2 pkt 1 ustawy z dnia 17 czerwca 1966 r.o postępowaniu egzekucyjnym w administracji(Dz.U.z 2016 r.poz. 559, z późn. zm.).";

	public static class RepresentativeType
	{
		public const string _2 = "2";
		public const string _3 = "3";
	}

	public static class OrgHeaderCategory
	{
		public const string BUS = "BUS";
	}

	public static class MessageType
	{
		public static class AES
		{
			public const string CC511C = "CC511C";
			public const string CC513C = "CC513C";
			public const string CC514C = "CC514C";
			public const string CC515C = "CC515C";
			public const string CC566C = "CC566C";
			public const string CC583C = "CC583C";
		}

		public static class ExitControl
		{
			public const string CC507C = "CC507C";
		}
	}

	public static class InterpretationStrings
	{
		public static class MessageTitles
		{
			public static string UPP => Res.GetString("8229B239-B85C-438C-9725-F7EF0A44A1DB", "UPP - Official Confirmation of Submission");
			public static string UPO => Res.GetString("F86BDDDC-EA91-4ED2-AE14-CFAE76BA2C5C", "UPO - Official Confirmation of Receipt");
			public static string NUP => Res.GetString("84638BC7-3999-406E-88F6-9E9256CE3B28", "NUP - Rejection Of Communication");
			public static string CC504 => Res.GetString("134EE0AE-CD26-4D4A-9F0C-D20F07DDBF94", "CC504C - Export Declaration Amendment Acceptance");
			public static string CC509 => Res.GetString("02617D7A-0C6F-4811-8BA4-08EF320FFA6C", "CC509C - Export cancellation decision");
			public static string CC528 => Res.GetString("00358D9B-60BF-42C3-880A-DF6891A0675E", "CC528C - Export MRN Allocation");
			public static string CC529 => Res.GetString("4D2B3E0D-7484-401D-8831-5F75199981DC", "CC529C - Release for export");
			public static string CC531 => Res.GetString("25D451AA-C6DB-434C-A4A3-D7578B991A90", "CC531C - Extended deadline for submitting a supplementary declaration");
			public static string CC551 => Res.GetString("C0BAF284-BC6E-488F-AE18-6C8A8288D45C", "IE551 – Refusal to release the goods for export procedure");
			public static string CC556 => Res.GetString("4417A607-30DD-40CE-A612-CA9AC7538825", "CC556 - Rejection of the customs declaration or its amendment");
			public static string CC582 => Res.GetString("23300C93-BD67-4564-A36F-D14178AB27B3", "CC582C - Query on not exited goods (not finished export operations)");
		}

		public static class CaptionStrings
		{
			public static string Exporter => Res.GetString("0D1EAACE-746F-40B2-85BA-6236825B6C71", "Exporter");
			public static string FunctionalErrors => Res.GetString("43F34D05-DEA0-46AF-A67A-133D1C912737", "Functional errors");
			public static string Location => Res.GetString("468BC4B7-B330-46C0-B7C6-35C8FDE4FFD8", "Location");
			public static string Locations => Res.GetString("31E168FA-EAF4-4073-B39B-556DC8BCBA1A", "Locations");
			public static string Problem => Res.GetString("B82B603E-0F80-411E-96B4-8AA82E06866D", "Problem");
			public static string Error => Res.GetString("8CC729C0-4A23-429B-86A6-7F76669A680E", "Error");
			public static string Errors => Res.GetString("FF91824A-62E3-4448-952B-3CADE2461259", "Errors");
		}

		public static class CommonStrings
		{
			public static string MRN => Res.GetString("4FA28C4C-659A-4CF1-8EFA-63C208707942", "MRN");
			public static string LRN => Res.GetString("7E32BC4F-1592-49C4-835D-29E3733C1674", "LRN");
			public static string OfficeRejectMessage => Res.GetString("062C7AD1-48C5-4E3F-A958-E89DDE32FFB6", "Customs office of export rejecting the message");
			public static string RejectedMessageIdentification => Res.GetString("30F31684-8583-4B12-B5BB-C3196F6DD7D5", "Rejected Message identification");
			public static string BusinessRejectionType => Res.GetString("CE16C4E1-7D53-4041-B156-A747E4D1B223", "Type of Business rejection");
			public static string RejectionDateAndTime => Res.GetString("DFB200DE-D076-436C-9BD8-A03EB818BF08", "Rejection date and time");
			public static string RejectionCode => Res.GetString("9D99D337-0E2D-4348-ACE1-A443D5A763FF", "Rejection Code");
			public static string RejectionReason => Res.GetString("582906AE-266F-4F36-8596-841B8C041490", "Rejection Reason");
			public static string MessageSentOn => Res.GetString("78FF8F99-524D-49F1-896D-2183EF5A6E7B", "Message Sent On");
			public static string CustomsOfficeOfExport => Res.GetString("DAFFD594-FC74-4BB1-A30D-1C3484BDC8D6", "Customs Office of Export");
			public static string CustomsOfficeOfPresentation => Res.GetString("C2C8A35A-7C73-4724-BC53-840765DC7259", "Customs Office of Presentation");
			public static string RefusalReason => Res.GetString("1BB44282-2962-45B3-A325-18C6FA4CB92F", "Refusal reason");
			public static string RequestOnNonExitedExportDate => Res.GetString("6C122D1E-59F0-4EF8-A900-6DB52C57DB2B", "Request on non-exited export date");
			public static string LimitForResponseDate => Res.GetString("F5EC00B8-AE66-4EF8-B579-CD27EF3983D4", "Limit for response date");
			public static string DateOfControl => Res.GetString("7E5FB091-5DFF-4281-BDF1-4613C64B2260", "Date of control");
			public static string AdditionalRefusalRemark => Res.GetString("CFEA3B83-7EB8-4EFB-8C93-B380A436E7CC", "Additional Refusal Remark");
			public static string EORI => Res.GetString("5830F9A1-F17B-4374-9539-385B0A9A1F80", "EORI");
			public static string Name => Res.GetString("DCD37D88-600A-4EF8-8302-C7BC218EDC7B", "Name");
			public static string StreetAndAddress => Res.GetString("CCDBBC0F-B013-4971-9C45-E1D0E4F83A1F", "Street & Address");
			public static string LinkedByMessageIdentification => Res.GetString("32CFA25B-0548-4EED-8E97-BC5700A87506", "Linked by message Identification");
			public static string DeclarationIsReleasedOn => Res.GetString("B8BE1021-2138-4AE8-AE78-F111F5A6687F", "Declaration is released on");
			public static string ControlResult => Res.GetString("8F5F5755-8CAD-4BE1-84F4-A3758DFBA663", "Control Result");
			public static string DeclarationIsAcceptedOn => Res.GetString("B7E7BE52-0C11-47FC-9752-823C0A9AB7D5", "Declaration is accepted on");
			public static string DeclarationAmendmentDate => Res.GetString("236B91DC-67ED-4903-9A97-A9277D31141F", "Declaration amendment date");
			public static string DeclarationAmendmentAcceptanceDate => Res.GetString("C9CFC65F-1F7B-4C77-8366-90DD9D5E4B39", "Declaration amendment acceptance date");
			public static string DeclarationIsInvalidated => Res.GetString("ABDAF9E4-5EBE-49C2-B8ED-5222662E7A34", "Declaration is invalidated");
			public static string InvalidationJustification => Res.GetString("D03089C8-519D-4CE4-BFB1-61283863964B", "Invalidation justification");
			public static string Number => Res.GetString("56613211-5360-4F9F-9F6A-4074AD8C56EB", "No.");
			public static string LodgementOfSupplementaryDeclarationStartDate => Res.GetString("6E78A898-91C9-454B-91BE-3F1AF068BA06", "Lodgement of supplementary declaration start date");
			public static string LodgementOfSupplementaryDeclarationExpiryDate => Res.GetString("76637ABF-3631-41A8-B5A4-EFFC54B43377", "Lodgement of supplementary declaration expiry date");
			public static string TimerExpiryInformation => Res.GetString("88CBD233-8508-4D5A-967E-59EF97FB5F82", "Timer expiry information");
			public static string CustomsOfficeOfExportReferenceNumber => Res.GetString("6880D0E0-CAFD-40ED-8B18-4C78FF275275", "Customs office of export sending the message");
			public static string TransmitDocumentName => Res.GetString("0449331A-548A-4529-9C29-97FF4A9CE89C", "Transmitted document");
			public static string TransmitDocumentNumber => Res.GetString("829721AF-53A9-4FC2-8502-22DC124B03D9", "Transmitted document number");
			public static string ExternalSystemID => Res.GetString("EE57FC31-1950-4040-84C5-15DD78F99E2E", "External system ID");
			public static string IdentyfikatorECIPSEAP => Res.GetString("BF05993A-3AEE-401A-80BE-B95DD4E75A6A", "ECIP/SEAP ID");
			public static string Applicant => Res.GetString("D1C834A4-6167-4B28-8049-F9B978CDC097", "Applicant");
			public static string IssuingSystem => Res.GetString("1A104DC3-7AFD-4CBB-BDD0-051004D00C4E", "Issuing system");
			public static string DateOfCompletion => Res.GetString("B9998C26-96AC-4AF8-9E69-89CBC35DFDD7", "Date of completion");
			public static string DateOfCreation => Res.GetString("2FACEF64-CB21-4B22-BAEA-46B796B1B8B9", "Date of creation");
		}

		public static class InvalidationInitiatedBy
		{
			public static string InitiatedByCustoms => Res.GetString("7018FC12-8F36-41C6-B3E3-89EC304073BF", "Invalidated by customs office of Export");
			public static string InitiatedByDeclarant => Res.GetString("3BCE0983-AAD4-43AB-85AD-D4262948CCAC", "Invalidation applies to the application submitting by the Declarant");
		}

		public static class FunctionalErrorsString
		{
			public static string ErrorPointer => Res.GetString("AF88E5A7-090C-420A-A5A9-00B9C2BABF2B", "Error Pointer");
			public static string ErrorCode => Res.GetString("4D24A0FE-D850-4D70-B215-4560F45B605F", "Error Code");
			public static string ErrorReason => Res.GetString("B6201FBF-6C5E-4D6C-BB40-D12E4AB9F310", "Error Reason");
			public static string OriginalAttributeValue => Res.GetString("CA99FCFA-7F44-440B-B882-E1FF19D0D2E5", "Original Attribute Value");
		}
	}

	public static class AESMessageProvidersConstants
	{
		public const string MessageRecipient = "NECA.PL";
	}

	public static class CusAuthorizationUsageType
	{
		public const string C019 = "C019";
		public const string C506 = "C506";
		public const string C512 = "C512";
		public const string C513 = "C513";
		public const string C514 = "C514";
		public const string C515 = "C515";
		public const string C626 = "C626";
		public const string C627 = "C627";
		public const string C601 = "C601";
		public const string D019 = "D019";
		public const string N990 = "N990";
	}

	public static class ValidationLists
	{
		public static IReadOnlyCollection<ZString> R425AndR481PrimaryPreferenceCodeList(BusinessObjectFactory factory) =>
			factory.GetCachedValue("Enterprise.Customs.PL.Business.R425AndR481PrimaryPreferenceCodeList",
				() => new HashSet<ZString> {
					PrimaryPreferenceCodes._120, PrimaryPreferenceCodes._123, PrimaryPreferenceCodes._125
					, PrimaryPreferenceCodes._128, PrimaryPreferenceCodes._220, PrimaryPreferenceCodes._223
					, PrimaryPreferenceCodes._225, PrimaryPreferenceCodes._320, PrimaryPreferenceCodes._323
					, PrimaryPreferenceCodes._325, PrimaryPreferenceCodes._420
				});

		public static IReadOnlyCollection<ZString> R0031ESupportingDocuments(BusinessObjectFactory factory) =>
			factory.GetCachedValue("Enterprise.Customs.PL.Business.CodeDescriptionPairLists.ValidationLists.R0031ESupportingDocuments",
				() => new HashSet<ZString> { SupportingDocumentCodes._4DK3, SupportingDocumentCodes.C710 });

		public static IReadOnlyCollection<ZString> R0039EProcedures(BusinessObjectFactory factory) =>
			factory.GetCachedValue("Enterprise.Customs.PL.Business.CodeDescriptionPairLists.ValidationLists.R0039EProcedures",
				() => new HashSet<ZString> { "1040", "2140" });

		public static IReadOnlyCollection<ZString> R0030ESupportingDocuments(BusinessObjectFactory factory) =>
			factory.GetCachedValue("Enterprise.Customs.PL.Business.CodeDescriptionPairLists.ValidationLists.R0030ESupportingDocuments",
				() => new HashSet<ZString> { SupportingDocumentCodes._4DK3, SupportingDocumentCodes.C710 });

		public static IReadOnlyCollection<ZString> R0032ESupportingDocuments(BusinessObjectFactory factory) =>
			factory.GetCachedValue("Enterprise.Customs.PL.Business.CodeDescriptionPairLists.ValidationLists.R0032ESupportingDocuments",
				() => new HashSet<ZString> { SupportingDocumentCodes._4DK3, SupportingDocumentCodes.C710 });

		public static IReadOnlyCollection<ZString> R0032EPreviousProcedures(BusinessObjectFactory factory) =>
			factory.GetCachedValue("Enterprise.Customs.PL.Business.CodeDescriptionPairLists.ValidationLists.R0032EPreviousProcedures",
				() => new HashSet<ZString> { ProcedureCodes._51, ProcedureCodes._54 });

		public static IReadOnlyCollection<ZString> SpecialProcedureCodesForPreviousDocuments(BusinessObjectFactory factory) =>
			factory.GetCachedValue("Enterprise.Customs.PL.Business.CodeDescriptionPairLists.ValidationLists.R0025EPreviousDocuments",
				() => new HashSet<ZString>
				{
					PreviousDocumentCodes.MRN,
					PreviousDocumentCodes.CLE,
					PreviousDocumentCodes.SDE,
					PreviousDocumentCodes.OGL,
					PreviousDocumentCodes.ZZZ
				});

		public static IReadOnlyCollection<ZString> BulkPackageCodes(BusinessObjectFactory factory) =>
			factory.GetCachedValue("JobComInvoiceLineLookups|BulkPackageCodeList",
				() => new HashSet<ZString> { "VQ", "VG", "VL", "VY", "VR", "VO", "VS" });

		public static IReadOnlyCollection<ZString> R632Procedures(BusinessObjectFactory factory) =>
			factory.GetCachedValue("Enterprise.Customs.PL.Business.CodeDescriptionPairLists.ValidationLists.R632Procedures",
				() => new HashSet<ZString> { ProcedureCodes._48, ProcedureCodes._61, ProcedureCodes._63, ProcedureCodes._68 });
	}

	public static class ValidationRuleMessagePrefixes
	{
		public const string R293 = "(R293) ";
		public const string R0052E = "[R0052E]";
	}

	public static class SupportingDocumentUnitOfQuantityCodes
	{
		public const string NumberOfItems = "NAR";
		public const string NumberOfCells = "NCL";
		public const string NumberOfPairs = "NPR";
	}

	public static class RefCusCodeListAttributeName
	{
		public const string ItemNumber = "ItemNumber";
	}

	public static class PreviousDocumentCodes
	{
		public const string AAD = "AAD";
		public const string CLE = "CLE";
		public const string ZZZ = "ZZZ";
		public const string MRN = "MRN";
		public const string SDE = "SDE";
		public const string OGL = "OGL";
		public const string NCLE = "NCLE";
		public const string _355 = "355";
		public const string _337 = "337";
	}

	public static class PrimaryPreferenceCodes
	{
		public const string _120 = "120";
		public const string _123 = "123";
		public const string _125 = "125";
		public const string _128 = "128";
		public const string _220 = "220";
		public const string _223 = "223";
		public const string _225 = "225";
		public const string _320 = "320";
		public const string _323 = "323";
		public const string _325 = "325";
		public const string _400 = "400";
		public const string _420 = "420";
	}

	public static class MessageSendingObjectActionCodes
	{
		public const string CC511 = "CC511";
		public const string CC513 = "CC513";
		public const string CC514 = "CC514";
		public const string CC515 = "CC515";
		public const string CC566 = "CC566";
		public const string CC583 = "CC583";
		public const string ZCX05 = "ZCX05";
		public const string ZC415 = "ZC415";
	}

	public static class AdditionalInfoTypes
	{
		public const string INF = "INF";
	}

	public static class InvoiceHeaderValuationCodes
	{
		public const string _11 = "11";
	}

	public static class EDIMessageSubType
	{
		public const string UniversalRejection = "IRJ";
		public const string Fault = "FAL";
		public const string Declaration = "ORG";
		public const string CorrectionAmendment = "COR";
		public const string CusPollingTransaction = "CPT";
	}

	public static class EdiMessageMessageType
	{
		public const string Attachment = "ATT";
		public const string CusPollingTransaction = "CPT";
		public const string ExitControl = "EXT";
	}

	public static class OrgCusCodesLength
	{
		public const int EidAcceptedLength = 2;
		public const int NipAcceptedLength = 10;
		public const int PeselAcceptedLength = 11;
		public const int RegonAcceptedLength_9 = 9;
		public const int RegonAcceptedLength_14 = 14;
	}

	public static class CustomsDestinationCodes
	{
		public const string PlCustoms = "PLCustomsPUESC";
		public const string PlCustomsTest = "PLCustomsPUESCTest";
	}

	public const string ExportText = "Eksport";
	public const string ImportText = "Import";
	public const string CarInformationEmptyElementPlaceholder = "brak";

	public static class InterchangeHeaderAttributes
	{
		internal const string User = "custom.PL.User";
		internal const string EncryptedPassword = "custom.PL.EncryptedPassword";
		internal const string FromMailBox = "custom.PL.FromMailBox";
		internal const string DestinationMailBox = "custom.PL.DestinationMailBox";
	}

	[CodeAlive("QuotaInformationProvider")]
	public static class MeasurementUnitQualifier
	{
		public const decimal KilogramToTonMultiplier = 1000m;
	}

	public static class ChargeTypes
	{
		public const string _1S1 = "1S1";
		public const string _1P1 = "1P1";
		public const string _1T1 = "1T1";
		public const string A00 = "A00";
		public const string A35 = "A35";
		public const string A45 = "A45";
	}

	public enum PresenceType { Forbidden, Required, Optional }

	public static class PUESC
	{
		public static class DocumentHandlingPort
		{
			public const string WsPullNamespace = "http://www.mf.gov.pl/uslugiBiznesowe/WsPull/Usluga/2014/01_v2_0";

			public static class XmlNodes
			{
				public static XmlQualifiedName AcceptDocumentMethod => new("AcceptDocument", WsPullNamespace);
				public static XmlQualifiedName AcceptDocumentResponse => new("AcceptDocumentResponse", WsPullNamespace);
				public static XmlQualifiedName GetDocumentsMethod => new("GetDocuments", WsPullNamespace);
				public static XmlQualifiedName GetDocumentsResponse => new("GetDocumentsResponse", WsPullNamespace);
				public static XmlQualifiedName BusinessErrorFault => new("businessErrorFault", WsPullNamespace);
				public static XmlQualifiedName TechErrorFault => new("techErrorFault", WsPullNamespace);
			}
		}

		public static class SystemMessages
		{
			public const string UPO = "UPO";
			public const string NUP = "NUP";
			public const string NPP = "NPP";
			public const string UPP = "UPP";
		}
	}

	public static class UniversalMessaging
	{
		public static class Event
		{
			public const string Namespace = "http://www.cargowise.com/Schemas/Universal/2012/11";

			public const string RootNodeName = "UniversalEvent";
			public static XmlQualifiedName RootNode => new(RootNodeName, Namespace);

			public const string EventTypeNodeName = "EventType";
			public static class Types
			{
				public const string Rejection = "IRJ";
			}
		}
	}

	public static class CusPollingTransaction
	{
		public static TimeSpan TimeBufferFor1stRetry => TimeSpan.FromMinutes(5);
		public static TimeSpan TimeBufferFor2ndRetry => TimeSpan.FromMinutes(10);
		public static TimeSpan TimeBufferFor3rdOr4thRetry => TimeSpan.FromMinutes(15);
		public static TimeSpan TimeBufferForOtherRetry => TimeSpan.FromMinutes(30);
	}

	public static class XmlTypeRegExPattern
	{
		public const string Date = @"^\d{4}-\d{2}-\d{2}$";
		public const string DateTime = @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d{1,3})?$";
	}

	public static class Rules
	{
		[ThreadSafe] internal static readonly ZString[] R0086EProcedureFirstTwoNumbers = { "11", "21", "31" };
		[ThreadSafe] internal static readonly ZString[] R0086EProcedure = { "1007", "1076", "1077" };
	}
}
