using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	[CodeProperty(CusCodeData.Schema.CY_Code)]
	[DescriptionProperty(CusCodeData.Schema.CY_Data)]
	public class VPBAmountCodeData : CusCodeData
	{
		public VPBAmountCodeData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : CusCodeData.Schema
		{
			public const string CY_Value = "CY_Value";
		}

		internal static ZString FormatToCY_Code(ZString lineNumber)
		{
			return ZString.Format("Line{0}", lineNumber);
		}

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

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(CUSDECEDIMessage)); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.VPBAmount;
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new VPBAmountCodeDataValidation(this);
		}
	}

	public class VPBAmountCodeDataValidation : CusCodeDataValidation
	{
		public VPBAmountCodeDataValidation(VPBAmountCodeData parent) : base(parent)
		{
		}

		protected override void CheckCY_Code()
		{
		}
	}
}
