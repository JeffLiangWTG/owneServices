using System;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[Serializable]
	public class FackInvalidCompressedFileException : Exception, Enterprise.Integration.Customs.AU.IInvalidCompressedFileExceptionProvider
	{
		public FackInvalidCompressedFileException(string message)
		: base(message)
		{
		}

#if NETFRAMEWORK
		protected FackInvalidCompressedFileException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
		}
#endif
	}
}
