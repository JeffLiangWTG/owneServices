using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[GlowDataDefinition("ITTBCOLAAndCertificate")]
	public class TTBCOLAAndCertificate : AutoTTBCOLAAndCertificate, ITTBCOLAAndCertificate
	{
		public TTBCOLAAndCertificate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoTTBCOLAAndCertificate.Schema
		{
			public const string US_COLAExemptionCodeDesc = "US_COLAExemptionCodeDesc";
			public const string HasForeignCertificate = USTTBCOLAAndCertificateAddInfo.Schema.HasForeignCertificate;
		}

		public new TTBLine Parent
		{
			get { return base.Parent as TTBLine; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.TTBCOLAAndCertificate|US_COLA", Caption = "Certificate of Label Approval ID", MediumCaption = "COLA ID", ShortCaption = "COLA")]
		public override ZString US_COLA
		{
			get { return base.US_COLA; }
			set { base.US_COLA = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.TTBCOLAAndCertificate|US_COLAExemptionCode", Caption = "COLA Exemption Code", MediumCaption = "COLA Exempt.", ShortCaption = "Exemption")]
		public override ZString US_COLAExemptionCode
		{
			get { return base.US_COLAExemptionCode; }
			set { base.US_COLAExemptionCode = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.TTBCOLAAndCertificate|US_COLAExemptionCodeDesc", Caption = "COLA Exemption Code Description")]
		public ZString US_COLAExemptionCodeDesc
		{
			get { return AddInfoLookups.COLAExemptionCodes.GetDescriptionFromCode(US_COLAExemptionCode); }
		}

		public ZPropertyInfo US_COLAExemptionCodeDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_COLAExemptionCodeDesc); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.TTBCOLAAndCertificate|HasForeignCertificate", Caption = "Has Foreign Certificate?", MediumCaption = "Has Cert.?", ShortCaption = "Cert.?", FullDescription = "Has Foreign Certificate? (For Wine – a certificate attesting to the origin, appellation, or proper cellar treatment of natural wine; and for Distilled Spirits - a certificate attesting to the age, origin, authenticity, or class and type).")]
		public ZBool HasForeignCertificate
		{
			get { return Data.HasForeignCertificate; }
			set { Data.HasForeignCertificate = value; }
		}

		public ZPropertyInfo HasForeignCertificateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.HasForeignCertificate, x => Data.HasForeignCertificateInfo); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.TTBCOLAAndCertificate|US_ForeignCertificateCountry", Caption = "Foreign Certificate Country/Region", MediumCaption = "Cert. Country/Region", ShortCaption = "Ctry/Rgn.", FullDescription = "The Issuer's country/region of the Foreign Certificate (For Wine – a certificate attesting to the origin, appellation, or proper cellar treatment of natural wine; and for Distilled Spirits - a certificate attesting to the age, origin, authenticity, or class and type).")]
		public override ZString US_ForeignCertificateCountry
		{
			get { return base.US_ForeignCertificateCountry; }
			set { base.US_ForeignCertificateCountry = value; }
		}

		#region ITTBCOLAAndCertificate Members

		ZString ITTBCOLAAndCertificate.COLA
		{
			get { return US_COLA; }
		}

		ZString ITTBCOLAAndCertificate.ForeignCertificateCountry
		{
			get { return US_ForeignCertificateCountry; }
		}

		ZString ITTBCOLAAndCertificate.ExemptionCode
		{
			get { return US_COLAExemptionCode; }
		}

		#endregion

		public void UpdateAddInfoProperties()
		{
			if (Data != null && HasChanges)
			{
				Data.UpdateRelatedPropertyInfo();
			}
		}
	}
}
