using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace CargoWise.Billing.API
{
	[Serializable]
	public sealed class ValidationException : Exception
	{
		public ValidationException()
			: this(DefaultMessage)
		{
		}

		public ValidationException(string message)
			: this(message, Enumerable.Empty<string>())
		{
		}

		public ValidationException(string message, IEnumerable<string> errors)
			: base(message)
		{
			this.errors = errors.ToList();
		}

		public ValidationException(string message, Exception inner)
			: this(message, Enumerable.Empty<string>(), inner)
		{
		}

		public ValidationException(string message, IEnumerable<string> errors, Exception inner)
			: base(message, inner)
		{
			this.errors = errors.ToList();
		}

		private ValidationException(SerializationInfo info, StreamingContext context)
#pragma warning disable SYSLIB0051 // Type or member is obsolete
			: base(info, context)
#pragma warning restore SYSLIB0051 // Type or member is obsolete
		{
			this.errors = (List<string>)info.GetValue("errors", typeof(List<string>));
		}

		public IEnumerable<string> Errors
		{
			get { return errors.AsEnumerable(); }
		}

#pragma warning disable SYSLIB0003 // Type or member is obsolete
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
#pragma warning restore SYSLIB0003 // Type or member is obsolete

#pragma warning disable CS0672 // Member overrides obsolete member
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
#pragma warning restore CS0672 // Member overrides obsolete member
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("errors", errors);
#pragma warning disable SYSLIB0051 // Type or member is obsolete
			base.GetObjectData(info, context);
#pragma warning restore SYSLIB0051 // Type or member is obsolete
		}
		readonly List<string> errors;
		const string DefaultMessage = "Validation failed.";
	}
}
