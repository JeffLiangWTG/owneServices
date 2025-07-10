using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CargoWise.Blazor.SessionBroker
{
	public class InvalidDatabaseConfigurationException : Exception
	{
		public InvalidDatabaseConfigurationException(string message) : base(message)
		{
		}

		public InvalidDatabaseConfigurationException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public InvalidDatabaseConfigurationException()
		{
		}
	}
}
