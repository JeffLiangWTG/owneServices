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
	[GlowDataDefinition("IUSAPHISLicenses")]
	public class APHISLicense : AutoAPHISLicense, IAPHISLicense
	{
		public APHISLicense(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoAPHISLicense.Schema
		{
			public const string US_DateQualifierDesc = "US_DateQualifierDesc";
			public const string US_TypeDesc = "US_TypeDesc";
			public const string LicenseHolderOrgPK = "LicenseHolderOrgPK";
		}

		public APHISHeader Header
		{
			get { return (APHISHeader)Parent; }
		}

		public ZString ProgramType
		{
			get
			{
				var header = Header;
				return header == null ? ZString.Empty : header.US_ProgramType;
			}
		}

		#region Override Properties

		[ResourceStringData("Enterprise.Customs.US.Business.APHISLicense|US_Date", Caption = "License Date", ShortCaption = "Date")]
		public override ZDateTime US_Date
		{
			get { return base.US_Date; }
			set { base.US_Date = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISLicense|US_DateQualifier", Caption = "Date Qualifier", ShortCaption = "Qualifier")]
		public override ZString US_DateQualifier
		{
			get { return base.US_DateQualifier; }
			set { base.US_DateQualifier = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISLicense|US_DateQualifierDesc", Caption = "Date Qualifier Description", ShortCaption = "Qualifier Desc.")]
		public ZString US_DateQualifierDesc
		{
			get { return AddInfoLookups.DateQualifiers.GetDescriptionFromCode(US_DateQualifier); }
		}

		public ZPropertyInfo US_DateQualifierDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_DateQualifierDesc); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISLicense|US_StateDescription", Caption = "State/Province Description")]
		public override ZString US_StateDescription
		{
			get { return base.US_StateDescription; }
			set { base.US_StateDescription = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISLicense|US_RN_CountryCode", Caption = "Country/Region")]
		public override ZString US_RN_CountryCode
		{
			get { return base.US_RN_CountryCode; }
			set { base.US_RN_CountryCode = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISLicense|US_Number", Caption = "License Number", ShortCaption = "Number")]
		public override ZString US_Number
		{
			get { return base.US_Number; }
			set { base.US_Number = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISLicense|US_Quantity", Caption = "Quantity", ShortCaption = "Qty")]
		public override ZDecimal US_Quantity
		{
			get { return base.US_Quantity; }
			set { base.US_Quantity = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISLicense|US_Type", Caption = "License Type", ShortCaption = "Type")]
		public override ZString US_Type
		{
			get { return base.US_Type; }
			set { base.US_Type = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISLicense|US_TypeDesc", Caption = "License Type Description", ShortCaption = "Lic. Type Desc.")]
		public ZString US_TypeDesc
		{
			get { return AddInfoLookups.LicenseTypes.GetDescriptionFromCode(US_Type); }
		}

		public ZPropertyInfo US_TypeDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_TypeDesc); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISLicense|US_UnitOfMeasure", Caption = "Unit Of Measure", ShortCaption = "UQ")]
		public override ZString US_UnitOfMeasure
		{
			get { return base.US_UnitOfMeasure; }
			set { base.US_UnitOfMeasure = value; }
		}

		#endregion

		#region IAPHISLicense Members

		ZString IAPHISLicense.Location
		{
			get { return US_RN_CountryCode; }
		}

		ZString IAPHISLicense.LocationDescription
		{
			get { return US_StateDescription; }
		}

		ZDecimal IAPHISLicense.Quantity
		{
			get { return US_Quantity; }
		}

		ZString IAPHISLicense.UnitOfMeasure
		{
			get { return US_UnitOfMeasure; }
		}

		#endregion

		#region ILicense Members

		ZString ILicense.TransactionType => ZString.Empty;

		ZString ILicense.Type
		{
			get { return US_Type; }
		}

		ZString ILicense.Number
		{
			get { return US_Number; }
		}

		ZString ILicense.DateQualifier
		{
			get { return US_DateQualifier; }
		}

		ZDate ILicense.Date
		{
			get { return US_Date.IsValid ? US_Date.Date : ZDate.Empty; }
		}

		#endregion
	}
}
