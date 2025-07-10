using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(NOEDIMessageCollection))]
sealed class NOEDIMessageCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		return new NOEDIMessageCollection(entryHeader);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var message = entryHeader.Messages.AddNew();
		return message;
	}

	protected override void SetUp()
	{
		base.SetUp();
		entryHeader = Factory.New<CusEntryHeader>();
	}

	CusEntryHeader entryHeader;
}
