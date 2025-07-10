using System;

namespace Enterprise.Rating.Web
{
	[Serializable]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
	public class InvalidDepartmentException : Exception
	{
		public InvalidDepartmentException() : base("User HomeDepartment is not valid. You must specify a department that exists.")
		{
		}

		public InvalidDepartmentException(string department) : base($"Provided department of value '{department}' is not valid. You must specify a department that exists.")
		{
		}

#if NETFRAMEWORK
		protected InvalidDepartmentException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
