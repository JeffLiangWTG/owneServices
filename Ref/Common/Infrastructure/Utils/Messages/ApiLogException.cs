using System;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public class ApiLogException
	{
		public string RequestUri { get; set; }
		public string UserId { get; set; }
		public Exception Exception { get; set; }
	}
}
