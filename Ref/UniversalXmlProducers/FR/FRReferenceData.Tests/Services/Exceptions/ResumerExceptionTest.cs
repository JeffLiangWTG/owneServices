using System;
using CargoWise.RefDbRepo.FRReferenceData.Services.Exceptions;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Services.Exceptions
{
	[TestFixture]
	class ResumerExceptionTest
	{
		[Test]
		public void TestException()
		{
			try
			{
				ExceptionThrowerHelper();
			}
			catch (ResumerException e)
			{
				Assert.That(e.Message == "My own error message.");
				Assert.That(e.InnerException.Message == "Index was outside the bounds of the array.");
			}
		}

		void ExceptionThrowerHelper()
		{
			try
			{
				var dummyArray = new string[] { "A" };
				var crashingVar = dummyArray[1];
			}
			catch (Exception e)
			{
				throw new ResumerException("My own error message.", e);
			}
		}
	}
}

