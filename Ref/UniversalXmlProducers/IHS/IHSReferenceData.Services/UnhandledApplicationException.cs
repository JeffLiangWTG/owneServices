using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.IHSReferenceData.Services
{
	public class UnhandledApplicationException : Exception
	{
		public UnhandledApplicationException()
		{
		}

		public UnhandledApplicationException(Exception ex) : base(ex.Message, ex)
		{
		}

		public UnhandledApplicationException(string message) : base(message)
		{ }

		public UnhandledApplicationException(string message, Exception ex) : base(message, ex)
		{ }
	}
}
