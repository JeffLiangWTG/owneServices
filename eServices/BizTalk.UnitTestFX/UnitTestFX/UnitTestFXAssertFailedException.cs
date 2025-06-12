using System;
using System.Runtime.Serialization;

namespace CargoWise.BizTalk.UnitTestFX
{
	[Serializable]
	public class UnitTestFXAssertFailedException : Exception
	{
		public UnitTestFXAssertFailedException(string message) : base(message)
		{ }

		public UnitTestFXAssertFailedException(string message, Exception ex) : base(message, ex)
		{ }

		protected UnitTestFXAssertFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{ }
	}
}
