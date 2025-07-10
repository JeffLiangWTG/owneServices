using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class CusEntryNumValidation : Common.CusEntryNumValidation
	{
		public CusEntryNumValidation(CusEntryNumber entryNumber)
			: base(entryNumber)
		{
		}

		public new CusEntryNumber Parent => (CusEntryNumber)base.Parent;

		protected override void CheckCE_EntryNum()
		{
			base.CheckCE_EntryNum();
			if (Parent.CE_EntryType == CusEntryNumberTypes.UnitedStates.ITN && !IsValidITNNumber(Parent.CE_EntryNum))
			{
				Parent.CE_EntryNumInfo.AddMessageError(AESITNNumberLengthExceeded);
			}

			if (Parent.CE_EntryType == CusEntryNumberTypes.UnitedStates.InBond && !Regex.IsMatch(Parent.CE_EntryNum, @"^\d{9}$"))
			{
				Parent.CE_EntryNumInfo.AddMessageError(InBondNumberLengthExceeded);
			}
		}

		bool IsValidITNNumber(ZString itnNumber) => itnNumber.Length == 15 && itnNumber.StartsWith("X") && itnNumber.SubstringSafe(1).IsNumbersOnlyOrEmpty && ZDateTime.TryParseExact(itnNumber.SubstringSafe(1, 8), out _, "yyyyMMdd");

		internal static string AESITNNumberLengthExceeded
		{
			get { return Res.GetString("98C3CEC5-A3DE-4A66-A392-4DA96944C809", "The number must start with the letter \"X\", followed by the year, month and day of acceptance in the AES, and six randomly assigned digits."); }
		}

		internal static string InBondNumberLengthExceeded
		{
			get { return Res.GetString("C61DE93E-91D8-4CC4-AB26-52C04CDCF86A", "In-Bond Number should be 9 digits."); }
		}
	}
}
