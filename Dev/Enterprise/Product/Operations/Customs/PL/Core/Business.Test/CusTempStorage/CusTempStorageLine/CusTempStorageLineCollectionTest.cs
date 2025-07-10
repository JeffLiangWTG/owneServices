using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.CusTempStorage.Testing;

[TestedType(typeof(CusTempStorageLineCollection))]
class CusTempStorageLineCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionTestCase
{
	protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CusTempStorageLine>();

	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var storageDec = Factory.New<CusTempStorageDec>();
		return storageDec.CusTempStorageLines;
	}
}
