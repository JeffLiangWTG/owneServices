using System.Diagnostics;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[DebuggerDisplay("Target = {FieldName}")]
	public class NominatedMessageError : AutoNominatedMessageError
	{
		public NominatedMessageError()
			: base()
		{
		}
	}
}
