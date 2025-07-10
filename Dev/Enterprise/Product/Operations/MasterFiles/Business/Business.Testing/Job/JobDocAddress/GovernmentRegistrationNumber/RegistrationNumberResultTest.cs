using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RegistrationNumberResultTest : TestCaseWithFactory
	{
		public void TestIsApplicable()
		{
			var result = new RegistrationNumberResult(Factory, false, null);
			AssertEquals(false, result.IsApplicable);
			result = new RegistrationNumberResult(Factory, true, delegate
			{ return new RegistrationNumber(); });
			AssertEquals(true, result.IsApplicable);
		}

		public void TestRegistrationNumber()
		{
			var result = new RegistrationNumberResult(Factory, false, null);
			AssertEquals(ZString.Empty, result.RegistrationNumber);
			result = new RegistrationNumberResult(Factory, true, delegate
			{ return new RegistrationNumber() { Number = "BLAH", NumberType = "GST" }; });
			AssertEquals("BLAH", result.RegistrationNumber);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestInvalidArgument()
		{
			var result = new RegistrationNumberResult(Factory, true, null);
		}
	}
}
