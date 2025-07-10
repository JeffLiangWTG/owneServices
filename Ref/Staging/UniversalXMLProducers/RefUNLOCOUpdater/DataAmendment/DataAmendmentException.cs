using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.DataAmendment
{
	public class DataAmendmentException :Exception
	{
		public DataAmendmentException(string message) : base(message)
		{
		}

		public DataAmendmentException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public DataAmendmentException()
		{
		}
	}
}
