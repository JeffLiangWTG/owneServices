using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageBuilders.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(SAD507DocumentPageWrapper))]
	sealed class SAD507DocumentPageWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSAD507Information()
		{
			CombineAssertions(() =>
			{
				var containers = new List<CusContainerDocWrapper>();
				var addInfos = new List<AdditionalInformationDocWrapperWithLineNumber>();
				var tester = new SAD507DocumentPageWrapper(containers, addInfos);
				AssertEquals(0, tester.Containers.Count);
				AssertEquals(0, tester.AdditionalInformations.Count);
				containers.Add(new CusContainerDocWrapper(new ContainerInformationForTest(), Factory));
				containers.Add(new CusContainerDocWrapper(new ContainerInformationForTest(), Factory));
				addInfos.Add(new AdditionalInformationDocWrapperWithLineNumber(new AdditionalInformationDocWrapper(new AdditionalInformationForTest()), "1", ZDateTime.Today, Factory));
				addInfos.Add(new AdditionalInformationDocWrapperWithLineNumber(new AdditionalInformationDocWrapper(new AdditionalInformationForTest()), "2", ZDateTime.Today, Factory));
				addInfos.Add(new AdditionalInformationDocWrapperWithLineNumber(new AdditionalInformationDocWrapper(new AdditionalInformationForTest()), "3", ZDateTime.Today, Factory));
				tester = new SAD507DocumentPageWrapper(containers, addInfos);
				AssertEquals(2, tester.Containers.Count);
				AssertEquals(3, tester.AdditionalInformations.Count);
				AssertEquals(1, tester.AdditionalInformations.ToArray<AdditionalInformationDocWrapperWithLineNumber>().Count(x => x.LineNumber == "1"));
				AssertEquals(1, tester.AdditionalInformations.ToArray<AdditionalInformationDocWrapperWithLineNumber>().Count(x => x.LineNumber == "2"));
				AssertEquals(1, tester.AdditionalInformations.ToArray<AdditionalInformationDocWrapperWithLineNumber>().Count(x => x.LineNumber == "3"));
			});
		}

		protected override BusinessObject GetNewBusinessObject() => new SAD507DocumentPageWrapper(null, null);
	}
}
