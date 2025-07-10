using System.Collections.Generic;

namespace Enterprise.Customs.PL.NCTS.Business;

public static class Constants
{
	public const string MessageRecipient = "NTA.PL";

	public static class MessageTypeCodes
	{
		public const string IE007 = "IE007";
		public const string IE013 = "IE013";
		public const string IE014 = "IE014";
		public const string IE015 = "IE015";
		public const string IE044 = "IE044";
		public const string IE054 = "IE054";
		public const string IE141 = "IE141";
		public const string IE170 = "IE170";
		public const string IE051 = "IE051";
	}

	public static class MessageSubTypeCodes
	{
		public const string IE007 = "007";
		public const string IE013 = "013";
		public const string IE014 = "014";
		public const string IE015 = "015";
		public const string IE044 = "044";
		public const string IE054 = "054";
		public const string IE141 = "141";
		public const string IE170 = "170";
		public const string IE051 = "051";
	}

	public static class AdditionalInfoCodes
	{
		public const string _POW01 = "POW01";
		public const string _PCS01 = "PCS01";
	}

	public static class InterpretationStrings
	{
		public static class CommonStrings
		{
			public static string LRN => Res.GetString("229DA278-B3D6-4571-855B-3BEA3D47E77A", "LRN");

			public static string MRN => Res.GetString("DA05A794-D116-4B5B-990D-E7FF0D1780B1", "MRN");

			public static string SerialNumber => Res.GetString("F62A139C-63A1-40CD-9A55-BB35F6BE41A3", "Sr.");

			public static string TotalNumberOfHouse => Res.GetString("410EE741-094C-4E88-9399-779A73CC3692", "Total Number Of House");

			public static string TotalNumberOfItems => Res.GetString("C1D51A0F-2004-4838-A0E2-C523268276B7", "Total Number Of Items");

			public static string NoBorderBoldFont => Res.GetString("94AA6590-544E-4B23-A62A-DA1250134F3E", "no-border bold-font");
		}

		public static class MessageTitles
		{
			public static string IE004 => Res.GetString("304FF8D1-BFD6-42CB-8BA7-C48B38808226", "IE004 - Amendment Accepted");

			public static string IE019 => Res.GetString("BFEB1CB4-8450-4B31-A835-FF3964074388", "IE019 - Major Discrepancies Observed During Checking At Destination");

			public static string IE009 => Res.GetString("805C8E65-1E86-45F6-8A7A-BA4630E2FBAC", "IE009 - Invalidation Response");

			public static string IE022 => Res.GetString("040B1073-BC60-4F57-9A1D-11E0A9713B06", "IE022 - Amendment Requested");

			public static string IE025Full => Res.GetString("6992A6C7-C6E2-4B28-AE8B-A23CCED691DD", "IE025 - Full Release Of Goods From Transit Procedure");

			public static string IE025Part => Res.GetString("AF86912A-DFA6-4411-BDE5-51212BD35BEE", "IE025 - Partial Release Of Goods From Transit Procedure");

			public static string IE025No => Res.GetString("889CFE61-43F1-4A0F-94B0-5ABE25D1538C", "IE025 - No Release Of Goods From Transit Procedure");

			public static string IE029 => Res.GetString("FF5B04C1-1815-426B-BA9F-694078920529", "IE029 - Released For Transit");

			public static string IE029SC => Res.GetString("920169A1-2F8C-42E3-9E25-C116035332CB", "IE029SC - Released For Transit");

			public static string IE035 => Res.GetString("ACAF4418-3423-46B5-BD8D-9CDCFBEFF226", "IE035 - Recovery/Collection Procedure Started");

			public static string IE043 => Res.GetString("E7C2F852-7011-4101-B512-AB43FD64DD6D", "IE043 - Unloading Permission");

			public static string IE045 => Res.GetString("58E0CD95-395D-446E-BFF9-DD3CFDDFE57F", "IE045 - Write-Off");

			public static string IE055 => Res.GetString("0E55DBA1-2748-4693-860E-51FC88558583", "IE055 - Guarantee Invalid");

			public static string IE056 => Res.GetString("0374F4B5-1A7B-4F1D-8706-6E2488C05AAB", "IE056 - Functional Error");

			public static string IE057 => Res.GetString("57E64502-A5E2-487C-9C14-C43BC07FAE22", "IE057 - Functional Error");

			public static string IE060 => Res.GetString("F5C9D898-77B3-47AB-B3A1-10E6A612EBCB", "IE060 - Control Decision");

			public static string IE140 => Res.GetString("752EB441-ECB2-4DAB-82C5-31ED5890C41E", "IE140 - Consignment Under Search Procedure");

			public static string IE182 => Res.GetString("407ED4D1-7E8B-404E-BB80-A97F697ABBB0", "IE182 - Incident Reported During Transit");

			public static string IE906 => Res.GetString("97E550D8-64D9-4684-A91F-06EAC0386B98", "IE906 - Functional Error");

			public static string IE917 => Res.GetString("2A6E3C69-ECE8-4ADB-94DC-2048C169BC12", "IE917 - Syntax error");

			public static string IE928 => Res.GetString("D65AC0A2-2A18-4FC6-81BE-81D541AB0A87", "IE928 - Positive Acknowledgment");

			public static string IE051 => Res.GetString("CFF34357-27A0-45CE-ABB9-76BE9989EC61", "IE051 - Not Released For Transit");
		}

		public static class Consignment
		{
			public static string grossMass => Res.GetString("ABE988A7-9952-4E61-A38F-8DD2019880DF", "Total Gross Weight");
		}

		public static class CTLControl
		{
			public static string ContinueUnloading => Res.GetString("54A8A792-3B9F-4876-B663-C9E9980168D9", "Continue Unloading Notes");
		}

		public static class Invalidation
		{
			public static string InvalidationRequestedDate => Res.GetString("4F79B7A8-C49A-43EC-B606-431DEF0BBBF3", "Invalidation Requested Date");

			public static string Decision => Res.GetString("E2422ABD-0E13-4D8A-A32C-20C572059111", "Decision");

			public static string DecisionDateAndTime => Res.GetString("4699DCA2-02F4-4BC0-95C7-EB93A75D5147", "Decision Date & Time");

			public static string Justification => Res.GetString("FFE4A961-3B99-487D-958C-7981BC725874", "Justification");

			public static string RequestAccepted => Res.GetString("ba030be6-a22d-4711-b557-ebffcffd0404", "Invalidation Request Accepted");

			public static string RequestRejected => Res.GetString("1c2e548a-316e-4191-8f41-b1874a30aea0", "Invalidation Request Rejected");
		}

		public static class XmlError
		{
			public static string Caption => Res.GetString("4B7B43D5-647F-4E31-87A1-0DC431EA2EFF", "XML Error");

			public static string LineNumber => Res.GetString("377CC882-4163-49E0-A779-8F6BA55A031B", "Line No.");

			public static string ColumnReference => Res.GetString("43603D9C-1445-4C28-93D5-BC205C2DC2A5", "Column Reference");

			public static string Pointer => Res.GetString("FEE65602-AB2D-4B21-A4A0-16BE39AB5CDE", "Pointer");

			public static string Code => Res.GetString("DC702B17-A331-49D1-86B0-A3FC7922BCFA", "Code");

			public static string AdditionalText => Res.GetString("5BA85A58-DB64-4448-BDEF-B5810C7DD9F1", "Additional Text");

			public static string SubmittedValue => Res.GetString("B4762DAF-4D07-415D-9F18-87BA97E96FCA", "Submitted Value");
		}

		public static class TransitOperation
		{
			public static string MessageSentOn => Res.GetString("EC2B08CE-1C60-4FD6-9080-5B3845F52E6C", "Message Sent On");

			public static string CustomsOfficeOfDeparture => Res.GetString("A1AB8019-F2DB-4954-863D-BCCB9976F5F1", "Customs Office Of Departure");

			public static string CustomsOfficeOfDestinationActual => Res.GetString("A208D1B0-3B42-4C8C-8FA0-0A90E353AD80", "Customs Office of Destination Actual");

			public static string CustomsOfficeOfIncidentRegistered => Res.GetString("C92A0B7B-CC81-4CDB-A82B-A7EAE7BD9A6B", "Customs Office Of Incident Registered");

			public static string TraderAtDestination => Res.GetString("2C9BF1F6-4404-485C-AE22-117D073F087E", "Trader At Destination");

			public static string BusinessRejectionType => Res.GetString("1E244F52-861A-48CA-9B3F-5E7BABFC81D4", "Business Rejection Type");

			public static string RejectionDateTime => Res.GetString("5D8EDDE3-9E46-47B9-81F8-FAE17E5012F9", "Rejection Date & Time");

			public static string RejectionCode => Res.GetString("701E6F4B-497E-4A00-9E2E-37C11ACA4AF2", "Rejection Code");

			public static string RejectionReason => Res.GetString("D7C95E62-882D-459A-BB6A-BFF1D3E6F674", "Rejection Reason");

			public static string ControlNotificationDateAndTime => Res.GetString("680FAC4A-8783-4FE0-B4D3-705330865F7F", "Control Notification Date & Time");

			public static string IncidentNotificationDateAndTime => Res.GetString("2F4FBAFB-BD80-45D0-BB46-44BC9766301F", "Incident Notification Date & Time");

			public static string NotificationType => Res.GetString("06B79136-87B1-4CFB-9EB6-4AE28967CB2A", "Notification Type");

			public static string CustomsOfficeOfDestination => Res.GetString("8C3BB917-D9B7-42D4-9A82-C986A0A9560F", "Customs Office Of Destination");

			public static string DeclarationType => Res.GetString("A28CDE16-FE39-4ADC-A9C2-8992DEE5CF31", "Declaration Type");

			public static string DeclarationAcceptanceDate => Res.GetString("6FA52C81-449F-45F1-BE10-6B35FA0F170B", "Declaration Acceptance Date");

			public static string TIR => Res.GetString("AB39FA30-1CDF-4A5C-923C-A6327343A234", "TIR Carnet Number");

			public static string ReleaseDate => Res.GetString("88E32F42-3913-49C7-A88A-6CF0126E3CB3", "Release Date");

			public static string ReleaseIndicator => Res.GetString("0177EDF0-0D87-4C2B-B761-CE4F218A8861", "Release Indicator");

			public static string GrossWeight => Res.GetString("918792F6-2E9C-4862-BCFD-90B5FE2201BD", "Gross Weight");

			public static string WriteOffDate => Res.GetString("FA7A04D5-4D15-4106-A0DA-548A974BF427", "Write Off Date");

			public static string AcceptanceDate => Res.GetString("8E7ADB58-7581-4B5F-8147-8E7FF761B440", "Acceptance Date");

			public static string LRNSubmissionDate => Res.GetString("BBBEBB93-37B0-40B9-B568-21840D549B23", "LRN Submission Date");

			public static string ReleaseRejectionCode => Res.GetString("1D1F4CFB-99D2-4D6E-B716-7CB2E2A6053F", "Release Rejection Code");

			public static string AdditionalRejectionRemark => Res.GetString("E2272B49-F3F8-47C2-93FE-639F6989DFAA", "Additional Release Remark");

			public static string AmendmentSubmissionDateAndTime => Res.GetString("CB82C870-2854-4030-9AF9-939885C52A16", "Amendment Submission Date");

			public static string AmendmentAcceptanceDateAndTime => Res.GetString("E0018B9A-BE8C-4A06-8CB7-351ED25A77CB", "Amendment Acceptance Date");

			public static string AmendmentNotificationDate => Res.GetString("2969B9BF-A4FF-4669-BB0D-9524A7AA027F", "Amendment Notification Date");

			public static string DestinationTrader => Res.GetString("8389A217-4EBC-4490-B7B6-484FBC6719E1", "Destination Trader (EORI)");

			public static string LimitForResponseDate => Res.GetString("916C77CC-606C-4787-A17D-11CE7ABD8FC6", "Limit For Response Date");

			public static string CustomsOfficeOfEnquiryAtDeparture => Res.GetString("FE5D01C4-D686-47B3-8FEB-58C290E9354F", "Customs Office of Enquiry at Departure");

			public static string NonArrivedMovementDate => Res.GetString("A606ED63-2A32-41EB-BF5F-6F7F80E385E3", "Non-Arrived Movement Date");

			public static string DiscrepanciesNotificationDate => Res.GetString("5645BABA-37E6-4DD4-900B-88149699BB85", "Discrepancies Notification Date");

			public static string DiscrepanciesNotificationText => Res.GetString("53FA9703-EFCF-4787-8E6B-74E89A49D6A2", "Discrepancies Notification Text");
		}

		public static class FunctionalError
		{
			public static string Caption => Res.GetString("55E92140-9645-4102-95D7-7E2A852B881F", "Functional Error");

			public static string Code => Res.GetString("1EA5B255-5ED4-48AF-A781-1DD63F390204", "Code");

			public static string Reason => Res.GetString("0F819243-5DC5-4C1F-B9D1-C4881C21FF4E", "Reason");

			public static string ErrorPointer => Res.GetString("7D4654AD-BC3D-46F0-A9AE-EA7B9EAF9CB8", "Error Pointer");

			public static string ErrorReferenceField => Res.GetString("3B0EDEDB-0103-401E-A4BD-E13C29C11E5B", "Error Reference Field");
		}

		public static class ConsignmentIncidentsString
		{
			public static string IncidentLocationAndCoordinates => Res.GetString("874EB38B-5B10-45A6-B6BA-9EA091569B39", "Incident Location & Co-ordinates");

			public static string IncidentCode => Res.GetString("E0F67F8F-3B40-4B32-A0D1-76D9EF875943", "Incident Code");

			public static string AdditionalTextInfo => Res.GetString("217D7E4A-44C5-409B-885E-ADA2C074AE76", "Additional Text Info");
		}

		public static class Representative
		{
			public static string Caption => Res.GetString("CFFFEFCF-7B96-48CC-9F11-7376D0444537", "Representative");

			public static string EORI => Res.GetString("C9F5C724-61FA-43F3-9382-A348BF7C39DA", "EORI");

			public static string Status => Res.GetString("F67E6FE8-CD54-4AAD-91E6-3BE0CAC682A4", "Status");

			public static string Status2Description => Res.GetString("0F4B9213-00FB-45E3-B9F4-0699250C64C4", "Representative - direct representation (within the meaning of Article 18(1) of the Code)");
		}

		public static class Guarantor
		{
			public static string Caption => Res.GetString("5675A9CF-F8AB-4766-916A-BADE490CC930", "Guarantor");

			public static string EORI => Res.GetString("316E9EDD-8D67-4E16-934B-555DE91EE0C0", "EORI");

			public static string Name => Res.GetString("A831ECC1-DA20-4F5A-B6BF-040D8166C1D9", "Name");

			public static string StreetAddress => Res.GetString("2C7880EC-9B45-4D6C-95C0-93ECD19F032E", "Street & Address");

			public static string Postcode => Res.GetString("32343A9F-6454-4DC2-8A2E-E92927E134BD", "Postcode");

			public static string City => Res.GetString("B8200879-64DF-49C8-9035-7B4E2DD3845C", "City");

			public static string Country => Res.GetString("F9B379B3-47A7-4B4B-BA18-7F1D0DF8CC04", "Country");
		}

		public static class HolderOfTheTransitProcedure
		{
			public static string Caption => Res.GetString("1BC91E57-B28D-4523-8D84-3A0BCA160631", "Holder of the Transit Procedure");

			public static string EORI => Res.GetString("1BD88BA6-231D-48BE-ACE1-1DCF0D51385E", "EORI");

			public static string TIR => Res.GetString("61B902DD-A70F-49E1-99F0-0B71BC95A23E", "TIR");

			public static string TIR_HolderID => Res.GetString("6355511D-055D-491D-A7F3-F082AE6DD5BB", "TIR Holder Identification Number");

			public static string TIRIdentificationNumber => Res.GetString("5A5751E0-9CE0-4F95-8111-F2A8F21A9E0C", "TIR Holder Identification Number");

			public static string Name => Res.GetString("C62D9596-823E-48A1-9646-48DC2E41C5ED", "Name");

			public static string StreetAddress => Res.GetString("47A53DA8-0316-444B-B316-5F9962282F55", "Street & Address");

			public static string Postcode => Res.GetString("117236D9-69BD-412B-88B2-0026C2402F1D", "Postcode");

			public static string City => Res.GetString("38B1AE24-10F0-40D6-B5F6-190177D009DD", "City");

			public static string Country => Res.GetString("CA1DB850-1876-437E-91A0-5E50F86BC157", "Country");
		}

		public static class RecoveryNotification
		{
			public static string RecoveryNotificationDate => Res.GetString("4B7022F4-8E33-45AA-BA2C-F4683FE5C67D", "Recovery Notification Date");

			public static string RecoveryNotificationText => Res.GetString("45867485-938A-44A1-A100-FAAF6598DBBC", "Recovery Notification Text");

			public static string RecoveryAmountClaimed => Res.GetString("F8362795-07E6-415C-AA28-B5B03E4BE39C", "Recovery Amount Claimed");
		}

		public static class TypeofControl
		{
			public static string Caption => Res.GetString("BA49DD9A-8D73-4B3D-8D7E-2008B7B49E5B", "Type of Control");

			public static string Code => Res.GetString("027C49FF-B72C-40F2-AB78-0BCF9E49F39B", "Code");

			public static string Description => Res.GetString("4E21BFCE-9BE6-4BD7-9641-E2DCA218608D", "Description");
		}

		public static class RequestedDocuments
		{
			public static string Caption => Res.GetString("C897FC2B-67B7-4A96-9CF5-3626D6CD7515", "Requested Documents");

			public static string DocumentType => Res.GetString("DDB87354-F951-468C-90A4-3A3D60A65B4A", "Document Type");

			public static string Description => Res.GetString("EDEEAF52-E9BB-48AE-86F7-2941C4B8E9C4", "Description");
		}

		public static class GuaranteeReferences
		{
			public static string Caption => Res.GetString("1664F27C-A46C-420C-9416-78F4B22B734D", "Guarantee References");

			public static string Number => Res.GetString("C15548EB-00C1-4484-907F-E5DCC0357CEA", "Guarantee Reference Number");

			public static string InvalidationReason => Res.GetString("6BCB7DB9-F08A-4448-9BA2-ECB7BA3A060B", "Reason For Invalidation");
		}

		public static class CountrySpecificDataPL
		{
			public static string NumberOfPackages => Res.GetString("7AB90150-6967-4BAC-AF6F-EF9A2AEFAF89", "Number Of Packages");

			public static string TotalGoodsItems => Res.GetString("5F236179-7B27-4FB7-B14B-352CF9D0ED91", "Total Goods Items");
		}

		public static class PartialRelease
		{
			public static string Caption => Res.GetString("CA58066A-DFF0-4608-9623-B9CE6D897653", "Partial Release");

			public static string HouseSequenceNumber => Res.GetString("E136D869-1E75-4760-B494-779389F2974A", "House Sequence Number");

			public static string TotalItemsReleased => Res.GetString("E79DBC7F-0A16-4DA0-BAE4-E92BF60D2794", "Total Items Released");
		}

		public static class TraderAtDestination
		{
			public static string Caption => Res.GetString("26518182-B294-4955-9047-E6352617D10A", "Trader At Destination");

			public static string Eori => Res.GetString("87AF2775-CC62-4668-B9A7-EF5CE257FA23", "EORI");
		}
	}

	public static class ValidationCaptions
	{
		public static string PrincipalCaption => Res.GetString("59882582-4B81-4EA1-8ADE-3F3F81DFAF5B", "Principal");

		public static string RepresentativeCaption => Res.GetString("DF4341DA-66B0-4B9B-BDE4-038BB98CAAA5", "Representative");
	}

	public static class ReleaseType
	{
		public const string ClosedFullRelease = "1";
		public const string DiscrepancyResolutionPartialRelease = "2";
		public const string ClosedPartialRelease = "3";
		public const string DiscrepancyResolutionNoRelease = "4";
	}

	public static class ServiceTypes
	{
		public static string CTL => Res.GetString("BA4B5F62-FCE1-4012-9E15-EFBEBE67CD53", "CTL");
	}

	public static class RefCusCodeListType
	{
		public const string CL164 = "CL164";
	}

	internal static class GuaranteeTypeSet
	{
		public static IReadOnlyCollection<string> GuaranteeTypeWithReference = new HashSet<string> {
			EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver, //0
			EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee, // 1
			EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor, //2
			EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.CashDepositGuarantee, //3
			EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.FlatRateVoucher, //4
			EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiverSecuredAmountNotGreaterThan500Eur, //5
			EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeWithMultipleUsage // 9 
		};
	}
}
