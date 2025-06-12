using CargoWise.eHub.Core.Orchestrations.HttpRetry.Contract;
using NUnit.Framework.Constraints;

namespace CargoWise.eHub.Products.GBCustoms.Core.BT.Tests.TestHelpers
{
	internal class Has : NUnit.Framework.Has
	{
		public static SomeItemsConstraint HttpHeader(string key, string value)
		{
			var expectedHeader = new HttpHeader
			{
				Key = key,
				Value = value
			};

			return new SomeItemsConstraint(new HttpHeaderConstraint(expectedHeader));
		}
	}
}
