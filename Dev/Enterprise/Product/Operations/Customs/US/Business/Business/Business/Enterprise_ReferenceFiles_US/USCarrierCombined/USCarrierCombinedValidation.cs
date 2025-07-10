using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCarrierCombinedValidation
//
//    This class should be used for overriding validation in AutoUSCarrierCombinedValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USCarrierCombinedValidation : AutoUSCarrierCombinedValidation
	{
		public USCarrierCombinedValidation(AutoUSCarrierCombined parent)
			: base(parent)
		{
		}

		protected new USCarrierCombined Parent
		{
			get { return (USCarrierCombined)base.Parent; }
		}

		protected override void CheckUI_Code()
		{
			base.CheckUI_Code();
			if (!Parent.UI_Code.IsEmpty)
			{
				var query = new ZQuery(USCarrierSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				query.AddToFilter(USCarrierSchema.USC_Code, Parent.UI_Code);
				var anotherCarrier = Parent.Factory.LoadTop1<Internal.USCarrier>(query);
				if (anotherCarrier != null)
				{
					Parent.UI_CodeInfo.AddError(DuplicateCarrierCode);
				}
			}
		}

		public static string DuplicateCarrierCode
		{
			get { return Res.GetString("71D746A6-5ACD-4CCF-AF9E-0C2825C06BD8", "The code entered already exists on another Carrier record."); }
		}

		protected override void CheckUI_ModeOfTransportation()
		{
			base.CheckUI_ModeOfTransportation();
			ValidateUI_AirwayBillPrefix();
		}

		protected override void CheckUI_AirwayBillPrefix()
		{
			base.CheckUI_AirwayBillPrefix();
			if (Parent.IsAir && !Parent.UI_AirwayBillPrefix.IsEmpty)
			{
				var airwayBillPrefix = Parent.UI_AirwayBillPrefix.KeepAlphanumericCharacters().Replace(" ", "");
				if (airwayBillPrefix.Length != 3)
				{
					Parent.UI_AirwayBillPrefixInfo.AddWarning(InvalidAirWaybillPrefix);
				}
			}
		}

		public static string InvalidAirWaybillPrefix
		{
			get { return Res.GetString("65913DD4-FB90-47AD-B30A-0AF265394C6C", "Invalid Air Waybill Prefix format; an Air Waybill Prefix should be a 3 alphanumeric characters."); }
		}

		protected override void CheckUI_AddressIsNotEmpty()
		{
		}

		protected override void CheckUI_AirwayBillPrefixIsNotEmpty()
		{
		}

		protected override void CheckUI_ModeOfTransportationIsNotEmpty()
		{
		}
	}
}
