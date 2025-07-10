using System.Collections.Generic;
using CargoWise.Common;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class EDIClientApplicationDescriptorsTest : TransactionedTestCase
	{
		public void TestValues()
		{
			AssertEquals("Application exists in Values collection", true, new List<IEDIClientApplicationDescriptor>(ApplicationDescriptors.Values).Count >= 1);
			foreach (var provider in ApplicationDescriptors.Values)
			{
				AssertNotNull(provider);
			}
		}

		[ExpectNoExceptions]
		public void TestDuplicateDictKey()
		{
			recurseAndNull(2, ApplicationDescriptors);
			AssertEquals("DuplicateApplicationAdded", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}
		IEDIClientApplicationDescriptor recurseAndNull(int n, EDIClientApplicationDescriptors instance)
		{
			if (n > 0)
			{
				return instance.GetApplicationDescriptorFromAnyCreator("SUP", () => { return recurseAndNull(n - 1, instance); });
			}
			return null;
		}

		public void TestTryGetValueSafe()
		{
			string wrongCode = ")(_*#*(";
			AssertNull(ApplicationDescriptors.GetValue(wrongCode));
			AssertNotNull(ApplicationDescriptors.GetValue("EAN"));
		}

		EDIClientApplicationDescriptors ApplicationDescriptors
		{
			get { return new EDIClientApplicationDescriptors(); }
		}
	}
}
