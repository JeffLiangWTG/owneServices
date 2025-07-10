namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input
{
	using CargoWise.Types;

	[InputBlock("PGA")]
	[ApplicationIdentifier("XP", Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsExport)]
	public partial class AESCommShipPGAXP : MessageBlock
	{
		public AESCommShipPGAXP()
			: base("PGA")
		{
		}

		/// <summary>
		/// Three character Identification of the Participating Government Agency.
		/// For EPA, the valid value allowed is EP1.
		/// For AMS, the valid value allowed is AM1.
		/// For TTB, the valid value allowed is TTB.
		/// For ATF, the valid value allowed is AT6.
		/// For FWS, the valid value allowed is FW7 and FW8.
		/// For DEA, the valid value allowed is DEA.
		/// For NMFS, the valid values allowed are NM7, NM8 and NM9.
		/// </summary>
		[MessageBlockString(3, 4, "M")]
		public ZString PGAID;

		/// <summary>
		/// See PGA record layout in Appendix Q.
		/// </summary>
		[MessageBlockString(73, 7, "C", ShouldTrimBegining = false)]
		public ZString PGAData;
	}

	public class EPA_EP1PGADetails : MessageBlock
	{
		public EPA_EP1PGADetails()
			: base("")
		{
		}

		/// <summary>
		/// If ‘Y’, information below is required.
		/// </summary>
		[MessageBlockString(1, 1, "M")]
		public ZString EPALicenceRequiredIndicator;

		/// <summary>
		/// 01-12 EPA Consent Number
		/// </summary>
		[MessageBlockString(12, 2, "C")]
		public ZString EPAConsentNumber;

		/// <summary>
		/// 13-24 RCRA Hazardous Waste Manifest
		///  Tracking Number. 
		/// </summary>
		[MessageBlockString(12, 14, "C")]
		public ZString EPAHazardousWasteManifestTrackingNumber;

		[MessageBlockString(3, 26, "C")]
		public ZString EPAUnitOfQuantity;

		[MessageBlockDecimal(10, 29, "C", 0)]
		public ZDecimal EPANetQuantity;
	}

	public class AMS_AM1PGADetails : MessageBlock
	{
		public AMS_AM1PGADetails()
			: base("")
		{
		}

		/// <summary>
		/// AMS Export Certificate Number
		/// </summary>
		[MessageBlockString(13, 1, "M")]
		public ZString AMSExportCertificateNumber;
	}

	public class ATF_AT6PGADetails : MessageBlock
	{
		public ATF_AT6PGADetails()
			: base("")
		{
		}

		[MessageBlockString(20, 1, "C")]
		public ZString FFLNumber;

		/// <summary>
		/// 1,2
		/// </summary>
		[MessageBlockString(1, 21, "C")]
		public ZString FFLExemptionCode;

		[MessageBlockString(11, 22, "C")]
		public ZString PermitNumber;

		/// <summary>
		/// 1,2
		/// </summary>
		[MessageBlockString(1, 33, "C")]
		public ZString PermitExemptionCode;

		[MessageBlockDecimal(9, 34, "M", 0)]
		public ZDecimal PermitQuantity;

		/// <summary>
		/// Valid values for exports: 
		/// AW – ANY OTHER WEAPON
		/// DD – DESTRUCTIVE DEVICE
		/// MG – MACHINEGUN
		/// SI - SILENCER
		/// SR – SHORT BARRELED RIFLE
		/// SS – SHORT BARRELED SHOTGUN
		/// 
		/// Space fill if not used
		/// </summary>
		[MessageBlockString(4, 43, "C")]
		public ZString CategoryCode;

		/// 
		/// Space fill if not used
		/// </summary>
		[MessageBlockString(28, 47, "C")]
		public ZString Description;
	}

	public class DEAPGADetails : MessageBlock
	{
		public DEAPGADetails()
			: base("")
		{
		}

		[MessageBlockString(4, 1, "M")]
		public ZString DrugCode;

		[MessageBlockDecimal(14, 5, "M", 4)]
		public ZDecimal Quantity;

		/// <summary>
		/// G, MG, KG, MCG
		/// </summary>
		[MessageBlockString(3, 19, "M")]
		public ZString UnitofMeasure;

		/// <summary>
		/// E, T
		/// </summary>
		[MessageBlockString(1, 22, "M")]
		public ZString TransactionType;

		/// <summary>
		/// DEA Permit Number or DEA Transaction authorization number
		/// </summary>
		[MessageBlockString(7, 23, "M")]
		public ZString PermitTransactionID;

		/// <summary>
		/// The DEA registration number or company identification number. DEA assigns identification numbers to companies that are not registrants.
		/// </summary>
		[MessageBlockString(9, 30, "M")]
		public ZString Entity;
	}

	public class FWS_FW7PGADetails : MessageBlock
	{
		public FWS_FW7PGADetails()
			: base("")
		{
		}

		/// <summary>
		/// Number indicating export is authorized by FWS and will be validated.
		/// Format: CCYY, a two letter alphanumeric port code and a 7 digit unique number Sample: 2015CH1234567
		/// </summary>
		[MessageBlockString(14, 1, "C")]
		public ZString EDEcsConfirmatioNnumber;

		/// <summary>
		/// The Taxonomic Serial Number (TSN) representing the species or other taxonomic classification of an animal.
		/// </summary>
		[MessageBlockString(19, 15, "C")]
		public ZString TaxonomicSerial;

		/// <summary>
		/// Code indicating the specific FWS processing codes based upon purpose of which the data set is related
		/// B – Breeding in captivity
		/// E – Educational
		/// H – Hunting trophy
		/// M – Biomedical Research
		/// P – Personal
		/// Q – Circus/Travelling Exhibition
		/// S - Scientific
		/// T – Commercial
		/// Y – Reintroduction into the wild
		/// Z - Zoo
		/// </summary>
		[MessageBlockString(1, 34, "C")]
		public ZString FWSPurposeCode;

		/// <summary>
		/// Description Codes assigned by FWS.
		/// </summary>
		[MessageBlockString(3, 35, "C")]
		public ZString FWSDescriptionCode;

		/// <summary>
		/// Two letter ISO code that identifies the country where the species of animal was taken from the wild or born.  For shipments of wildlife (sea turtles, fish, etc) that were landed (introduced) after harvest on the high seas, enter ZZ,
		/// </summary>
		[MessageBlockString(2, 38, "C")]
		public ZString SpeciesCountryOfOrigin;

		/// <summary>
		/// A code indicating the FWS source of animal
		/// W – Specimens taken from the wild
		/// R – Specimens originating from a ranching operation
		/// O – Pre-convention specimens
		/// F – animals that do not qualify as captive-bred under CITES
		/// U – Source unknown(lack of information must be justified)
		/// C – Animals bred in captivity(from parents that mated in captivity)
		/// I – Confiscated or seized specimens
		/// D – CITES Appendix I animals commercially bred in CITES
		/// J – Specimens that are domesticated
		/// X – Specimens taken on the high seas
		/// </summary>
		[MessageBlockString(1, 40, "C")]
		public ZString SourceCode;

		/// <summary>
		/// Code indicating FWS exemption certification
		/// FW1- Certification of No Wildlife
		/// FW2 – Salmonid Certification
		/// </summary>
		[MessageBlockString(3, 41, "C")]
		public ZString ExemptionCertificationCode;

		/// <summary>
		/// A code representing the FWS Wildlife Category Codes
		/// </summary>
		[MessageBlockString(3, 44, "C")]
		public ZString FWSWildlifeCategoryCode;

		/// <summary>
		/// 2-letter US state code where species was born, bred, taken from the wild
		/// </summary>
		[MessageBlockString(2, 47, "C")]
		public ZString StateOfSpeciesOrigin;
	}

	public class FWS_FW8PGADetails : MessageBlock
	{
		public FWS_FW8PGADetails()
			: base("")
		{
		}

		/// <summary>
		/// Clear description of the commercial line item in English. Provide the description according to other agency instructions
		/// </summary>
		[MessageBlockString(70, 1, "C")]
		public ZString CommercialDescription;
	}

	public class NMFS_NM7PGADetails : MessageBlock
	{
		public NMFS_NM7PGADetails()
			: base("")
		{
		}

		/// <summary>
		/// Description of product
		/// </summary>
		[MessageBlockString(70, 1, "C")]
		public ZString Description;

		/// <summary>
		/// AMR – Antarctic Marine Living Resources
		/// HMS – Highly Migratory Species
		/// </summary>
		[MessageBlockString(3, 71, "M")]
		public ZString GovtAgencyProgramCode;
	}

	public class NMFS_NM8PGADetails : MessageBlock
	{
		public NMFS_NM8PGADetails()
			: base("")
		{
		}

		/// <summary>
		/// See code list
		/// </summary>
		[MessageBlockString(3, 1, "C")]
		public ZString ProcessingTypeCode;

		/// <summary>
		/// At least one of the Catch, Statistical or re- export documents listed above is required for all exports or re-exports of HMS products except for shark fins.  The document required is determined by the regulatory parameters involving the particular species (HTS), ocean harvest area and export-re-export status. These documents must be submitted at the time of the filing to CBP via DIS.
		/// </summary>
		[MessageBlockString(3, 4, "C")]
		public ZString DocumentIdentifier;

		/// <summary>
		/// Enter the unique/specific number that appears on every document submitted via DIS. This is the specific ‘serialized’ number assigned to the individual reporting form for that specific harvest.
		/// </summary>
		[MessageBlockString(30, 7, "C")]
		public ZString DocumentNumber;

		/// <summary>
		/// Document Image Indicator should be ‘Y’. 
		/// </summary>
		[MessageBlockString(1, 37, "C")]
		public ZString DocumentImagesIndicator;

		/// <summary>
		/// Country code or ZZ for high seas international waters harvests
		/// </summary>
		[MessageBlockString(2, 38, "C")]
		public ZString SourceCountry;

		/// <summary>
		/// See ISO code
		/// </summary>
		[MessageBlockString(2, 40, "C")]
		public ZString CommodityHarvestingVesselCountryOfRegistry;

		/// <summary>
		/// Where the shipment originated/was harvested, use valid codes for the ocean area (appendix)
		/// </summary>
		[MessageBlockString(3, 42, "C")]
		public ZString GeographicLocation;
	}

	public class NMFS_NM9PGADetails : MessageBlock
	{
		public NMFS_NM9PGADetails()
			: base("")
		{
		}

		/// <summary>
		/// AMLR permit number for the exporter of record
		/// </summary>
		[MessageBlockString(14, 1, "C")]
		public ZString PermitNumber;

		/// <summary>
		/// Include total quantity by weight being exported
		/// </summary>
		[MessageBlockDecimal(15, 15, "C", 0)]
		public ZDecimal LPCOQty;

		/// <summary>
		/// Kilograms = KG
		/// </summary>
		[MessageBlockString(2, 30, "C")]
		public ZString LPCOUnitOfMeasure;

		/// <summary>
		/// The original Catch document number (example, CL-15-0001-E)
		/// </summary>
		[MessageBlockString(12, 32, "C")]
		public ZString CatchDocument;

		/// <summary>
		/// Re-export Approval number provided by NMFS
		/// </summary>
		[MessageBlockString(12, 44, "C")]
		public ZString ReExportNumber;
	}

	public class TTBPGADetails : MessageBlock
	{
		public TTBPGADetails()
			: base("")
		{
		}

		/// <summary>
		/// TTB issued export permit number Format: Left justified: XX-XXX-XX-XXXXX or XX-XX-XXXXX or XXX-XX-XXXXX or XX-XXXXXXXXX
		/// </summary>
		[MessageBlockString(15, 1, "C")]
		public ZString TTBPermitRegistryNumber;

		/// <summary>
		/// Date products left the TTB bonded premises. The date of removal by manufacturer or export warehouse. This date cannot be > export date (error).  Format: MMDDYYYY
		/// </summary>
		[MessageBlockDate(16, "M", "yyyyMMdd")]
		public ZDate Date;

		/// <summary>
		/// Serial # from the applicable TTB export form. Format: Left Justified: XXXXXXX
		/// </summary>
		[MessageBlockString(11, 24, "C")]
		public ZString Serial;

		/// <summary>
		/// Valid values '1'  or space fill if not used 
		/// </summary>
		[MessageBlockString(1, 35, "C")]
		public ZString TTBDisclaimer;
	}
}
