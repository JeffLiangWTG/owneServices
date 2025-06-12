using System;

namespace CargoWise.eHub.Products.NZCustoms.Common
{
	public class NZCustomsInvalidOperationException : Exception
	{
		public NZCustomsInvalidOperationException(string errorMessage) 
			: base(errorMessage)
		{ }
	}
}