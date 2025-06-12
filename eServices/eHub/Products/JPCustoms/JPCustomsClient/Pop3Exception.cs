using System;

namespace CargoWise.eHub.Products.JPCustoms.Client
{
	public class Pop3Exception : Exception
	{
		public Pop3Exception()
			: base()
		{
		}

		public Pop3Exception(string message)
			: base(message)
		{
		}
	}
}
