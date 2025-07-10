using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class PortAuthorityMessageBuilderFactoryTest : BaseAgencyTest
	{
		public void TestIFCSUM11()
		{
			AssertBuilder<IFCSUM11>(PortAuthorityVersionList.Codes.V11);
		}

		public void TestIFCSUM20()
		{
			AssertBuilder<IFCSUM20>(PortAuthorityVersionList.Codes.V20);
		}

		[ExpectException(typeof(ArgumentOutOfRangeException))]
		public void TestUnknownVersion()
		{
			PortAuthorityMessageBuilderFactory.GetBuilder("XXX");
		}

		public void TestAllValidVersionsCodesAreSupported()
		{
			foreach (string value in GetAllVersionCodes())
			{
				AssertNotNull(string.Format("should support '{0}'", value), PortAuthorityMessageBuilderFactory.GetBuilder(value));
			}
		}

		#region Implementation

		IEnumerable<string> GetAllVersionCodes()
		{
			foreach (CodeDescriptionPair pair in new PortAuthorityVersionList())
			{
				yield return pair.Code;
			}
		}

		void AssertBuilder<T>(string version)
			where T : IPortAuthorityMessageBuilder
		{
			IPortAuthorityMessageBuilder builder = PortAuthorityMessageBuilderFactory.GetBuilder(version);
			AssertNotNull(string.Format("GetBuilder(\"{0}\") should not return null", version), builder);
			AssertEquals(string.Format("GetBuilder(\"{0}\") should return a builder of the correct type", version), typeof(T), builder.GetType());
		}

		#endregion
	}
}
