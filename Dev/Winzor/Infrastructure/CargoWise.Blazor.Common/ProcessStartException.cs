using System;
using System.Runtime.Serialization;

namespace CargoWise.Blazor.Common
{
	[Serializable]
	public class ProcessStartException : Exception
	{
		public ProcessStartException()
		{
		}

		public ProcessStartException(string message)
			: base(message)
		{
		}

		public ProcessStartException(string message, Exception ex)
			: base(message, ex)
		{
		}
	}
}
