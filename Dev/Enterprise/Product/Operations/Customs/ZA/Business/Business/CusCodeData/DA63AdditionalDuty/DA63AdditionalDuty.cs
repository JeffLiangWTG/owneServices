using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.DocumentWrappers;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public class DA63AdditionalDuty : CusCodeData
	{
		public DA63AdditionalDuty(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : CusCodeData.Schema
		{
			public new const int CY_CodeMaxLength = 3;
			public const int CY_Value_DecimalPlaces = 2;

			public const string OriginValue = "OriginValue";
			public const string CY_Value = "CY_Value";
		}

		#region CY_Code

		[MaxLength(Schema.CY_CodeMaxLength)]
		[CargoWiseOne.ResourceStrings.ResourceStringData("Enterprise.Customs.ZA.Business.DA63AdditionalDuty|CY_Code", Caption = "Part")]
		public override ZString CY_Code
		{
			get => base.CY_Code;
			set
			{
				var oldValue = CY_Code;
				base.CY_Code = value;
				if (CY_Code != oldValue)
				{
					ClearCY_ValueIfNeeded();
					OriginValueInfo.RefreshBinding();
				}
				CY_CodeInfo.RefreshBinding(oldValue);
			}
		}

		void ClearCY_ValueIfNeeded()
		{
			if (CY_Code.IsEmpty)
			{
				CY_Value = ZDecimal.Zero;
			}
		}

		#endregion

		#region CY_Value

		[DecimalPrecision(Schema.CY_Value_DecimalPlaces)]
		[CargoWiseOne.ResourceStrings.ResourceStringData("Enterprise.Customs.ZA.Business.DA63AdditionalDuty|CY_Value", Caption = "Value")]
		[ReadOnlyMember(nameof(CY_Value_ReadOnly))]
		public ZDecimal CY_Value
		{
			get => ZDecimal.ParseSafe(CY_Data, ZDecimal.Zero);
			set
			{
				var oldValue = CY_Value;
				CY_Data = value.ToString(Schema.CY_Value_DecimalPlaces);
				if (oldValue != CY_Value)
				{
					Validation.ValidateCY_Value();
				}
				CY_ValueInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CY_ValueInfo => GetZPropertyInfo(Schema.CY_Value);

		public ZBool CY_Value_ReadOnly => CY_Code.IsEmpty;

		#endregion

		#region OriginValue

		[ReadOnly(true)]
		[DecimalPrecision(Schema.CY_Value_DecimalPlaces)]
		[CargoWiseOne.ResourceStrings.ResourceStringData("Enterprise.Customs.ZA.Business.DA63AdditionalDuty|OriginValue", Caption = "Original Value")]
		public ZDecimal OriginValue => GetOriginValueCore(CY_Code);

		public ZPropertyInfo OriginValueInfo => GetZPropertyInfo(Schema.OriginValue);

		ZDecimal GetOriginValueCore(ZString part)
		{
			var result = ZDecimal.Zero;

			var invoiceLine = InvoiceLine;
			if (!part.IsEmpty && invoiceLine.IsDA63WithOriginalEntry)
			{
				var fees = invoiceLine.ImportBOEntryLine.GetCalcFeeValues();
				if (part == S1P2BDuty)
				{
					result = fees.S1P2BDuty;
				}
				else
				{
					var excluding12B = fees.CustomsDutiesExcluding12B
						.Concat(fees.Penalties)
						.Concat(fees.ProvisionalPayments)
						.GroupBy(x => x.Code)
						.Select(x => new DutyFeeInformationDocWrapper(x.Key, x.Sum(y => y.Value)));

					result = excluding12B.FirstOrDefault(x => x.Code == part)?.Value ?? ZDecimal.Zero;
				}
			}

			return result;
		}

		#endregion

		public const string S1P2BDuty = "12B";

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobComInvoiceLine));

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			CY_Type = CusCodeDataTypeList.Codes.DA63AdditionalDuty;
			CY_Value = ZDecimal.Zero;
		}

		public JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)Parent;

		protected override CusCodeDataLookups GetNewLookups() => new DA63AdditionalDutyLookups(this);

		public new DA63AdditionalDutyLookups Lookups => (DA63AdditionalDutyLookups)base.Lookups;

		protected override CusCodeDataValidation GetNewValidation() => new DA63AdditionalDutyValidation(this);

		public new DA63AdditionalDutyValidation Validation => (DA63AdditionalDutyValidation)base.Validation;
	}
}
