using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	abstract class UpperTextBaseDataFormat : TextBaseDataFormat
	{
		public UpperTextBaseDataFormat(int min, int max, string validCharacters) : base(min, max, validCharacters)
		{
		}

		public override ZString Preprocess(ZString data)
		{
			return data.ToUpper();
		}
	}
}
