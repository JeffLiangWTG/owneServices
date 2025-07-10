using System;

namespace CargoWise.RefDbRepo.ESReferenceData.Services
{
	public class JSONException : Exception
	{
		public JSONException()
		{
		}

		public JSONException(string message) : base(message)
		{
		}

		public JSONException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
