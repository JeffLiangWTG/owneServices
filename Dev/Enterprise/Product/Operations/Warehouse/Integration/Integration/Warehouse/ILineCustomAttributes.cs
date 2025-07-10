using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface ILineCustomAttributes
	{
		ZString CustomAttrib1 { get; }
		ZString CustomAttrib2 { get; }
		ZString CustomAttrib3 { get; }
		ZString CustomAttrib4 { get; }
		ZString CustomAttrib5 { get; }
		ZString CustomAttrib6 { get; }

		ZDecimal CustomDecimal1 { get; }
		ZDecimal CustomDecimal2 { get; }
		ZDecimal CustomDecimal3 { get; }
		ZDecimal CustomDecimal4 { get; }
		ZDecimal CustomDecimal5 { get; }

		ZDateTime CustomDate1 { get; }
		ZDateTime CustomDate2 { get; }
		ZDateTime CustomDate3 { get; }
		ZDateTime CustomDate4 { get; }
		ZDateTime CustomDate5 { get; }

		ZBool CustomFlag1 { get; }
		ZBool CustomFlag2 { get; }
		ZBool CustomFlag3 { get; }
		ZBool CustomFlag4 { get; }
		ZBool CustomFlag5 { get; }

		ZString CustomTextBlob1 { get; }

		void SetCustomAttributes(ILineCustomAttributes src);
	}
}
