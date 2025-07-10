using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(EmmaMessageEntryHeaderWrapper))]
sealed class EmmaMessageEntryHeaderWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new EmmaMessageEntryHeaderWrapper(null));
	}

	public void TestEntryHeaders() => CombineAssertions(() =>
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		var wrapper = new EmmaMessageEntryHeaderWrapper(entryHeader);
		var headers = wrapper.EntryHeaders?.ToArray() ?? [];
		AssertEquals("Entry Header Count", 1, headers.Length);
		AssertSame("EntryHeader Object", entryHeader, headers.FirstOrDefault());
	});
}
