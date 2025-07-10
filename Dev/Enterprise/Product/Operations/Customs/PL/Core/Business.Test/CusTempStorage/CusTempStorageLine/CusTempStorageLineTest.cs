using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.Business.CusTempStorage.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.CusTempStorage.Testing;

[TestedType(typeof(CusTempStorageLine))]
class CusTempStorageLineTest : CusTempStorageLineTestCase<CusTempStorageLine>
{
	protected override CusTempStorageLine GetNewCusTempStorageLine(BusinessObjectFactory factory)
	{
		var storageHeader = CusTempStorageJobHeader.New(factory);
		var storageDec = CusTempStorageDec.New(storageHeader);
		var storageLine = storageDec.CusTempStorageLines.AddNew();

		return storageLine;
	}

	protected override Type GetLookupType() => typeof(CusTempStorageLineLookups);

	protected override Type GetValidationType() => typeof(CusTempStorageLineValidation);

	protected override Type GetLineItemsType() => typeof(CusTempStorageLineItemCollection<CusTempStorageLineItem>);

	protected override Type GetDecType() => typeof(CusTempStorageDec);
}
