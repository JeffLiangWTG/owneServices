using System.Data;
using CargoWise.ComponentModel;
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
	[GlowDataDefinition("IUSAPHISSources")]
	public class APHISSource : AutoAPHISSource, ISource
	{
		public APHISSource(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoAPHISSource.Schema
		{
			public const string US_SourceTypeCodeDesc = "US_SourceTypeCodeDesc";
		}

		public ZString ProgramType
		{
			get
			{
				var header = Header;
				return header == null ? ZString.Empty : header.US_ProgramType;
			}
		}

		public APHISHeader Header
		{
			get { return (APHISHeader)Parent; }
		}

		public bool IsOtherTreatmentType
		{
			get { return US_ProcessingTypeCode == OtherTreatmentType; }
		}
		const string OtherTreatmentType = "ATR";

		#region Override Properties

		[ResourceStringData("Enterprise.Customs.US.Business.APHISSource|US_CountryCode", Caption = "Source Ctry/Rgn.", ShortCaption = "Ctry/Rgn.")]
		public override ZString US_CountryCode
		{
			get { return base.US_CountryCode; }
			set { base.US_CountryCode = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISSource|US_GeographicLocation", Caption = "Geographic Location", ShortCaption = "Location")]
		public override ZString US_GeographicLocation
		{
			get { return base.US_GeographicLocation; }
			set { base.US_GeographicLocation = value; }
		}

		[ReadOnlyMember(nameof(US_ProcessingDescription_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISSource|US_ProcessingDescription", Caption = "Processing Description", ShortCaption = "Pro Desc.")]
		public override ZString US_ProcessingDescription
		{
			get { return base.US_ProcessingDescription; }
			set { base.US_ProcessingDescription = value; }
		}

		bool US_ProcessingDescription_ReadOnly
		{
			get { return !IsOtherTreatmentType; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISSource|US_ProcessingEndDate", Caption = "Processing End Date", ShortCaption = "End Date")]
		public override ZDateTime US_ProcessingEndDate
		{
			get { return base.US_ProcessingEndDate; }
			set { base.US_ProcessingEndDate = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISSource|US_ProcessingStartDate", Caption = "Processing Start Date", ShortCaption = "Start Date")]
		public override ZDateTime US_ProcessingStartDate
		{
			get { return base.US_ProcessingStartDate; }
			set { base.US_ProcessingStartDate = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISSource|US_ProcessingTypeCode", Caption = "Processing Type", ShortCaption = "Processing")]
		public override ZString US_ProcessingTypeCode
		{
			get { return base.US_ProcessingTypeCode; }
			set
			{
				var oldValue = US_ProcessingTypeCode;
				base.US_ProcessingTypeCode = value;
				if (!IsCopying && oldValue != US_ProcessingTypeCode)
				{
					if (US_ProcessingTypeCode.IsEmpty)
					{
						US_ProcessingDescription = ZString.Empty;
					}
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISSource|US_SourceTypeCode", Caption = "Source Type", ShortCaption = "Type")]
		public override ZString US_SourceTypeCode
		{
			get { return base.US_SourceTypeCode; }
			set { base.US_SourceTypeCode = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISSource|US_SourceTypeCodeDesc", Caption = "Source Type Description", ShortCaption = "Type Desc.")]
		public ZString US_SourceTypeCodeDesc
		{
			get { return AddInfoLookups.SourceTypes.GetDescriptionFromCode(US_SourceTypeCode); }
		}

		public ZPropertyInfo US_SourceTypeCodeDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_SourceTypeCodeDesc); }
		}

		#endregion

		#region ISourceDetail Members

		ZString ISource.SourceTypeCode
		{
			get { return US_SourceTypeCode; }
		}

		ZString ISource.CountryCode
		{
			get { return US_CountryCode; }
		}

		ZString ISource.GeographicLocation
		{
			get { return US_GeographicLocation; }
		}

		ZDate ISource.ProcessingStartDate
		{
			get { return US_ProcessingStartDate.IsValid ? US_ProcessingStartDate.Date : ZDate.Empty; }
		}

		ZDate ISource.ProcessingEndDate
		{
			get { return US_ProcessingEndDate.IsValid ? US_ProcessingEndDate.Date : ZDate.Empty; }
		}

		ZString ISource.ProcessingTypeCode
		{
			get { return US_ProcessingTypeCode; }
		}

		ZString ISource.ProcessingDescription
		{
			get { return US_ProcessingDescription; }
		}

		#endregion
	}
}
