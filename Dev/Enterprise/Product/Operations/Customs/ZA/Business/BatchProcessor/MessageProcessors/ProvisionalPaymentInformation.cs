using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	internal class ProvisionalPaymentAdditionalInfo
	{
		#region ctor

		public ProvisionalPaymentAdditionalInfo(ZString input, ZString lineNumber)
		{
			var section = input.Split(';');
			foreach (var segment in section)
			{
				var segmentPair = segment.Split(new char[] { '=' }, 2);
				if (segmentPair.Length == 2)
				{
					var key = segmentPair[0].Trim();
					var value = segmentPair[1].Trim();
					switch (key)
					{
						case "!PPNo":
							this.PPNo = value;
							break;
						case "!DutyType":
							this.DutyType = value;
							break;
						case "!PPAmount":
							this.PPAmount = ZDecimal.ParseSafe(value, ZDecimal.Zero);
							break;
						case "!Expiry Date":
							ZDateTime date;
							this.ExpiryDate = ZDateTime.TryParseExact(value, out date, "yyyy/MM/dd") ? date : ZDateTime.Empty;
							break;
					}
				}
			}
			EntryLineNumber = lineNumber.Trim();
		}

		#endregion

		const string HeaderLevelIndicator = "1";

		#region Properties

		internal ZString PPNo { get; private set; }
		internal ZString DutyType { get; private set; }
		internal ZDecimal PPAmount { get; private set; }
		internal ZDateTime ExpiryDate { get; private set; }

		internal ZString EntryLineNumber { get; private set; }

		internal ZBool IsValid => !PPNo.IsEmpty && !DutyType.IsEmpty && !PPAmount.IsEmpty;
		internal ZBool IsHeaderLevelInfo => IsValid && EntryLineNumber == HeaderLevelIndicator;

		#endregion
	}
}
