using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.CusTempStorage.Testing;

[TestedType(typeof(CusTempStorageDec))]
public class CusTempStorageDecTest : EnterpriseBusinessObjectTestCase
{
	public void TestNew()
	{
		var header = Factory.New<CusTempStorageJobHeader>();

		CombineAssertions(() =>
		{
			var storageDec = CusTempStorageDec.New(header);
			AssertNotNull("CusTempStorageDec", storageDec);
			AssertEquals("CusTempStorageDec parent", header.PK, storageDec.STH_SJH);
		});
	}

	public void TestDefaultValues()
	{
		var header = CusTempStorageJobHeader.New(Factory);
		var storageDec = header.CusTempStorageDec;

		CombineAssertions(() =>
		{
			AssertEquals("STH_DeclarationType", "IST", storageDec.STH_DeclarationType);
			AssertEquals("STH_SystemCreateTimeUtc", true, storageDec.STH_SystemCreateTimeUtc.IsValid);
		});
	}

	public void TestCusTempStorageLines() => AssertType<CusTempStorageLineCollection>(Factory.New<CusTempStorageDec>().CusTempStorageLines);

	public void TestStorageHeader() => AssertType<CusTempStorageJobHeader>(CusTempStorageJobHeader.New(Factory).CusTempStorageDec.StorageHeader);

	protected override BusinessObject GetNewBusinessObject() => CusTempStorageJobHeader.New(Factory).CusTempStorageDec;
}
