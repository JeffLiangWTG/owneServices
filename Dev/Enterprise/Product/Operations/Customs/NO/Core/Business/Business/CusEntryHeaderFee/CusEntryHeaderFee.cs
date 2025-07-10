using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.NO.Business
{
	public class CusEntryHeaderFee : AutoCusEntryHeaderFee, IDutyCategory
	{
		[ReadOnly(true)]
		[ResourceStringData("7802DC81-0330-44B0-A3F2-AEF734D4677F", Caption = "Duty")]
		public override ZString Duty
		{
			get => base.Duty;
		}

		[ReadOnly(true)]
		[DecimalPlaces(0)]
		[ResourceStringData("DF40EC24-8BB5-4B2B-A7F2-3B8E3CA590E0", Caption = "Amount")]
		public override ZDecimal Amount
		{
			get => base.Amount;
		}

		public bool IsCustomsDuty => DutyCode == NOCustomDutyCodeList.Codes.TL1.Substring(0, 2);

		public bool IsAgriculturalDuty => DutyCode == NOCustomDutyCodeList.Codes.RT100.Substring(0, 2);

		public bool IsExciseDuty => !IsCustomsDuty && !IsAgriculturalDuty && !IsVAT;

		public bool IsVAT => DutyCode == NOCustomDutyCodeList.Codes.MV1.Substring(0, 2);

		public string DutyCode => Duty.ToUpper();
	}
}
