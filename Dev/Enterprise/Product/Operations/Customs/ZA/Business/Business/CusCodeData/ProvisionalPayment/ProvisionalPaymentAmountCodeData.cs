using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.MessageBuilders;

namespace Enterprise.Customs.ZA.Business
{
	[CodeProperty(CusCodeData.Schema.CY_Code)]
	[DescriptionProperty(CusCodeData.Schema.CY_Data)]
	public class ProvisionalPaymentAmountCodeData : CusCodeData, IDutyFeeInformation
	{
		public ProvisionalPaymentAmountCodeData(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public new class Schema : CusCodeData.Schema
		{
			public const string CY_Value = "CY_Value";
		}

		#region Overrides

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(CusEntryLine)); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.PPAmount;
		}

		#endregion

		#region New Properties

		public ZDecimal CY_Value
		{
			get { return ZDecimal.ParseSafe(CY_Data, ZDecimal.Zero); }
			set
			{
				var oldValue = CY_Value;
				CY_Data = value.ToString();
				CY_ValueInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CY_ValueInfo => GetZPropertyInfo(Schema.CY_Value);

		internal CusEntryLine ParentEntryLine => Parent as CusEntryLine;

		#endregion

		#region Lookups

		public new ProvisionalPaymentAmountCodeDataLookup Lookups
		{
			get { return (ProvisionalPaymentAmountCodeDataLookup)base.Lookups; }
		}

		protected override CusCodeDataLookups GetNewLookups()
		{
			return new ProvisionalPaymentAmountCodeDataLookup(this);
		}

		#endregion

		#region Validation

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new ProvisionalPaymentAmountCodeDataValidation(this);
		}

		#endregion

		#region IDutyFeeInformation

		ZString IDutyFeeInformation.Code => CY_Code;

		ZDecimal IDutyFeeInformation.Value => CY_Value;

		#endregion
	}
}
