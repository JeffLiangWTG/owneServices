using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics.CodeAnalysis;

namespace CargoWise.eHub.Portal.Helpers
{
	[Serializable]
	[ExcludeFromCodeCoverageAttribute]
	public class ValidationException : Exception
	{
		public ValidationException() { }

		public ValidationException(string message) : base(message) { }

		public ValidationException(string message, Exception inner) : base(message, inner) { }

		protected ValidationException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}