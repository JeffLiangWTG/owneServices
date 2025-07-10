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
	public class FWSLicense : AutoFWSLicense, IFWSLicense
	{
		public FWSLicense(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoFWSLicense.Schema
		{
			public const string US_TypeDesc = "US_TypeDesc";
		}

		public FWSHeader Header
		{
			get { return (FWSHeader)Parent; }
		}

		public void UpdateAddInfoProperties()
		{
			if (Data != null && HasChanges)
			{
				Data.UpdateRelatedPropertyInfo();
			}
		}

		#region Override Properties

		[ResourceStringData("Enterprise.Customs.US.Business.FWSLicense|US_Number", Caption = "License Number", ShortCaption = "Number")]
		public override ZString US_Number
		{
			get { return base.US_Number; }
			set { base.US_Number = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.FWSLicense|US_Type", Caption = "License Type", ShortCaption = "Type")]
		public override ZString US_Type
		{
			get { return base.US_Type; }
			set { base.US_Type = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.FWSLicense|US_TypeDesc", Caption = "License Type Description", ShortCaption = "Lic. Type Desc.")]
		public ZString US_TypeDesc
		{
			get { return AddInfoLookups.LicenseTypes.GetDescriptionFromCode(US_Type); }
		}

		public ZPropertyInfo US_TypeDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_TypeDesc); }
		}
		#endregion

		#region IFWSLicense Members

		ZString IFWSLicense.Type
		{
			get { return US_Type; }
		}

		ZString IFWSLicense.Number
		{
			get { return US_Number; }
		}

		#endregion
	}
}
