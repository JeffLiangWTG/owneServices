using System.Diagnostics;

namespace Enterprise.Customs.Business
{
	[DebuggerDisplay("Message = {Message}")]
	public class MessageLog : AutoMessageLog
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public MessageLog(string message, string targetCode)
			: base()
		{
			this.Message = message;
			this.TargetCode = targetCode;
		}
	}
}
