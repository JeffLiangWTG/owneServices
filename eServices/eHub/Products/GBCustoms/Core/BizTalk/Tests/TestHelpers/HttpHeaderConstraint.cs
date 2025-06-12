using System;
using CargoWise.eHub.Core.Orchestrations.HttpRetry.Contract;
using NUnit.Framework.Constraints;

namespace CargoWise.eHub.Products.GBCustoms.Core.BT.Tests.TestHelpers
{
	internal class HttpHeaderConstraint : Constraint
	{
		private readonly HttpHeader expectedHeader;

		public HttpHeaderConstraint(HttpHeader expectedHeader)
		{
			if (expectedHeader == null)
			{
				throw new ArgumentNullException("expectedHeader");
			}

			this.expectedHeader = expectedHeader;
		}

		public override string Description
		{
			get { return expectedHeader.ToString(); }
		}

		public override ConstraintResult ApplyTo<TActual>(TActual actual)
		{
			var isMatched = false;
			var actualHeader = actual as HttpHeader;

			if (actualHeader != null)
			{
				isMatched =
					actualHeader.Key == expectedHeader.Key &&
					actualHeader.Value == expectedHeader.Value;
			}

			return new ConstraintResult(this, actual, isMatched);
		}
	}
}
