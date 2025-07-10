using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefUNLOCO.Loader))]
	sealed class RefUNLOCOLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new RefUNLOCO.Loader(Factory);
		}

		public void TestLoad()
		{
			AssertNotNull(((RefUNLOCO.Loader)GetNewLoaderToTest()).Load("AUSYD"));
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestLoad_WithNullArgument()
		{
			var testCountry = new RefUNLOCO.Loader(null);
		}
	}
}
