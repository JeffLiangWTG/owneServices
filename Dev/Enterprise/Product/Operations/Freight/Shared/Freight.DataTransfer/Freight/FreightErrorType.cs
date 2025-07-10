using System;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.DataTransfer
{
	[Serializable]
	public class FreightErrorType : ErrorType
	{
		public FreightErrorType(string message) : base(message)
		{
		}

		public static FreightErrorType LengthWidthHeightDimensionTypeMustBeSame { get { return new FreightErrorType(Res.GetString("1a4bb48c-df1b-427f-82b6-02cd6cfbde65", "Dimension Type on Length/Width/Height must all be the same")); } }
		public static FreightErrorType ContainerNumberNotFound { get { return new FreightErrorType(Res.GetString("282efc3a-ccbd-4775-bcc9-787f68650718", "Container number not found")); } }
	}
}
