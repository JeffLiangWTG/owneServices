using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	public class DpsResultModelTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var list = new List<DpsResponseWithScreeningParty>();
			var resultModel = new DpsResultModel(list, Factory);
			AssertEquals(0, resultModel.ScreenedPartyModels.Count);
			AssertEquals(Factory, resultModel.Factory);
		}

		public void TestConstructor_ArgumentNullException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = new DpsResultModel(null, Factory));
			AssertExceptionThrown<ArgumentNullException>(() => _ = new DpsResultModel(new List<DpsResponseWithScreeningParty>(), null));
		}
	}
}
