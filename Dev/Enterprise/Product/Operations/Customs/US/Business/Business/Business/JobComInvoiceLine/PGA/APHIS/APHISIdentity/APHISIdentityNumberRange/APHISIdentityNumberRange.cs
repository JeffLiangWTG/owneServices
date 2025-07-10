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
	[GlowDataDefinition("IUSAPHISIdentityNumberRange")]
	public class APHISIdentityNumberRange : AutoAPHISIdentityNumberRange, INumberRange
	{
		public APHISIdentityNumberRange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public APHISProduct Product
		{
			get
			{
				var identity = Identity;
				return identity == null ? null : identity.Product;
			}
		}

		public APHISIdentity Identity
		{
			get { return (APHISIdentity)Parent; }
		}

		#region Override Properties

		[ResourceStringData("Enterprise.Customs.US.Business.APHISIdentityNumberRange|US_StartNumber", Caption = "Start Number", ShortCaption = "Start")]
		public override ZString US_StartNumber
		{
			get { return base.US_StartNumber; }
			set { base.US_StartNumber = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISIdentityNumberRange|US_EndNumber", Caption = "End Number", ShortCaption = "End")]
		public override ZString US_EndNumber
		{
			get { return base.US_EndNumber; }
			set { base.US_EndNumber = value; }
		}

		#endregion

		#region INumberRange Members

		ZString INumberRange.StartNumber
		{
			get { return US_StartNumber; }
		}

		ZString INumberRange.EndNumber
		{
			get { return US_EndNumber; }
		}

		#endregion
	}
}
