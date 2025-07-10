using System;

namespace Enterprise.Customs.US.Messaging.Business
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public sealed class ApplicationIdentifierAttribute : Attribute
	{
		public ApplicationIdentifierAttribute(string applicationIdentifier)
			: this(applicationIdentifier, CBPEDIInterchange.ApplicationCodes.USCustomsImport)
		{
		}

		public ApplicationIdentifierAttribute(string applicationIdentifier, string applicationCode)
		{
			if (string.IsNullOrEmpty(applicationIdentifier))
			{
				throw new ArgumentException("applicationIdentifier must have a non empty value");
			}
			ApplicationIdentifier = applicationIdentifier;
			if (string.IsNullOrEmpty(applicationCode))
			{
				throw new ArgumentException("applicationCode must have a non empty value");
			}
			ApplicationCode = applicationCode;
		}

		public readonly string ApplicationIdentifier;
		public readonly string ApplicationCode;
	}
}
