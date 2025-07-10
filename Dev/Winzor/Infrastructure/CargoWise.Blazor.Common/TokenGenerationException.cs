using System;
using System.Runtime.Serialization;

namespace CargoWise.Blazor.Common
{
	[Serializable]
	public class TokenGenerationException : Exception
	{
		public TokenGenerationException()
		{
		}

		public TokenGenerationException(string message)
			: base(message)
		{
		}

		public TokenGenerationException(string message, Exception ex)
			: base(message, ex)
		{
		}
	}
}
