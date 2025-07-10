using System;

namespace Enterprise.Warehouse.Web.WebService.Exceptions
{
	[Serializable]
	public class WebServiceLoginException : WebServiceException
	{
		#region Constructors

#if NETFRAMEWORK
		protected WebServiceLoginException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
		public WebServiceLoginException(string message)
			: base(message)
		{
		}

		public WebServiceLoginException(string message, Exception inner)
			: base(message, inner)
		{
		}

		#endregion
	}
}
