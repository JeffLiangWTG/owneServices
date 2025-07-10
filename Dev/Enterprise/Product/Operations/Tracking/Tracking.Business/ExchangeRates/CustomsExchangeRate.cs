using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Tracking.Business
{
	public class CustomsExchangeRate : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CustomsExchangeRate(DynamicBusinessObject rate, bool isReciprocal) : base(rate.Factory)
		{
			this.Rate = rate;
			this.IsReciprocal = isReciprocal;
		}

		public abstract class Schema
		{
			public const string RX_Code = "RX_Code";
			public const string RX_Desc = "RX_Desc";
			public const string RE_ExpiryDate = "RE_ExpiryDate";
			public const string RE_SellRate = "RE_SellRate";
			public const string Decimals = "Decimals";
		}
#if DEBUG
		public
#endif
			DynamicBusinessObject Rate;

		#region RX_Code

		public ZString RX_Code
		{
			get { return Rate != null ? Rate[Schema.RX_Code].ToString() : string.Empty; }
		}

		public ZPropertyInfo RX_CodeInfo
		{
			get { return GetZPropertyInfo(Schema.RX_Code); }
		}

		#endregion

		#region RX_Desc

		public ZString RX_Desc
		{
			get { return Rate != null ? Rate[Schema.RX_Desc].ToString() : string.Empty; }
		}

		public ZPropertyInfo RX_DescInfo
		{
			get { return GetZPropertyInfo(Schema.RX_Desc); }
		}

		#endregion

		#region RE_ExpiryDate

		public ZDateTime RE_ExpiryDate
		{
			get { return Rate != null ? new ZDateTime(Rate[Schema.RE_ExpiryDate]) : ZDateTime.Empty; }
		}

		public ZPropertyInfo RE_ExpiryDateInfo
		{
			get { return GetZPropertyInfo(Schema.RE_ExpiryDate); }
		}

		#endregion

		#region RE_SellRate

		public ZDecimal RE_SellRate
		{
			get { return Rate != null ? ZDecimal.ParseSafe(Rate[Schema.RE_SellRate].ToString(), ZDecimal.Zero) : ZDecimal.Zero; }
		}

		public ZPropertyInfo RE_SellRateInfo
		{
			get { return GetZPropertyInfo(Schema.RE_SellRate); }
		}

		#endregion

		#region Decimals

		public ZInt Decimals
		{
			get { return 6; }
		}

		public ZPropertyInfo DecimalsInfo
		{
			get { return GetZPropertyInfo(Schema.Decimals); }
		}

#if DEBUG
		public
#endif
		bool IsReciprocal;

		#endregion
	}
}
