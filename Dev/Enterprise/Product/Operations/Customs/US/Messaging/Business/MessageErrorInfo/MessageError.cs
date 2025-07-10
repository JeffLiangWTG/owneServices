using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.US.Messaging.Business
{
	[Immutable]
	public class MessageError
	{
		internal MessageError(ZString errorCode, ZString shortDesc, ZString narrative)
		{
			this.errorCode = errorCode.TrimEnd(' ', '\t');
			this.shortDesc = shortDesc.TrimEnd(' ', '\t');
			this.narrative = narrative.TrimEnd(' ', '\t');
		}

		readonly ZString errorCode = ZString.Empty;
		public ZString ErrorCode
		{
			get { return errorCode; }
		}

		readonly ZString narrative = ZString.Empty;
		public ZString Narrative
		{
			get { return narrative; }
		}

		readonly ZString shortDesc = ZString.Empty;
		public ZString ShortDesc
		{
			get { return shortDesc; }
		}
	}
}
