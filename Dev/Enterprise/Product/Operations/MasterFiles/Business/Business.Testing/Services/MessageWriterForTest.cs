using Enterprise.ZArchitecture.Core.Diagnostics;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class MessageWriterForTest : IMessageWriter
	{
		public string MessageResult { get; set; }

		public void WriteMessage(string message)
		{
			MessageResult += message;
		}
	}
}
