using System;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Web.WebService.Exceptions
{
	[Serializable]
	public class WebServiceException : ZException
	{
		#region Constructors

#if NETFRAMEWORK
		protected WebServiceException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif

		public WebServiceException(string message)
			: base(message)
		{
		}

		public WebServiceException(string message, Exception inner)
			: base(message, inner)
		{
		}

		#endregion
	}
}
