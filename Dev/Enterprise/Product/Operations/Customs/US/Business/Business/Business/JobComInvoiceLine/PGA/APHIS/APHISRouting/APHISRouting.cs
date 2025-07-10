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
	[GlowDataDefinition("IUSAPHISRoutings")]
	public class APHISRouting : AutoAPHISRouting, IAPHISRouting
	{
		public APHISRouting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoAPHISRouting.Schema
		{
			public const string US_TypeDesc = "US_TypeDesc";
		}

		public APHISHeader Header
		{
			get { return (APHISHeader)Parent; }
		}

		#region Override Properties

		[ResourceStringData("Enterprise.Customs.US.Business.APHISRouting|US_Country", Caption = "Routing Ctry/Rgn.", ShortCaption = "Ctry/Rgn.")]
		public override ZString US_Country
		{
			get { return base.US_Country; }
			set { base.US_Country = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISRouting|US_State", Caption = "Routing State", ShortCaption = "State")]
		public override ZString US_State
		{
			get { return base.US_State; }
			set { base.US_State = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISRouting|US_Type", Caption = "Routing Type", ShortCaption = "Type")]
		public override ZString US_Type
		{
			get { return base.US_Type; }
			set { base.US_Type = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISRouting|US_TypeDesc", Caption = "Routing Type Description", ShortCaption = "Routing Desc.")]
		public ZString US_TypeDesc
		{
			get { return AddInfoLookups.RoutingTypeList.GetDescriptionFromCode(US_Type); }
		}

		public ZPropertyInfo US_TypeDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_TypeDesc); }
		}

		#endregion

		#region IAPHISRouting Members

		ZString IAPHISRouting.RoutingType
		{
			get { return US_Type; }
		}

		ZString IAPHISRouting.RoutingCountry
		{
			get { return US_Country; }
		}

		ZString IAPHISRouting.RoutingState
		{
			get { return US_State; }
		}

		#endregion
	}
}
