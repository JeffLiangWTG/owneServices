using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	[CodeProperty(CusCodeData.Schema.CY_Data)]
	[DescriptionProperty(CusCodeData.Schema.Description)]
	public abstract class VoucherOfCorrectionValue : CusCodeData
	{
		protected VoucherOfCorrectionValue(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : CusCodeData.Schema
		{
			public const string CY_Value = "CY_Value";
		}

		[DecimalPlaces(2)]
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

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = VOCValueType;
		}

		protected abstract ZString VOCValueType { get; }

		protected override CusCodeDataLookups GetNewLookups()
		{
			return new VoucherOfCorrectionValueLookups(this);
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new VoucherOfCorrectionValueValidation(this);
		}
	}
}
