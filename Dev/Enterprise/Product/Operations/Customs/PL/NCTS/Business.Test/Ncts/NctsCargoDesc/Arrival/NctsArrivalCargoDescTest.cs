using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(NctsArrivalCargoDesc))]
class NctsArrivalCargoDescTest : EU.NCTS.Business.Testing.NctsArrivalCargoDescAbstractTest<NctsHeader>
{
	protected override ZString CountryCode => Core.Constants.CountryCodes.Poland;

	public void TestAdditionalInfos() => AssertType<NctsAdditionalInfoCollection<NctsAdditionalInfo>>(GetNewBusinessObject(Factory).AdditionalInfos);

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad()
	{
		var arrivalCargoDesc = GetNewBusinessObject(Factory);
		arrivalCargoDesc.FillWithValidTestData(TestBusinessObjectKind.MinimumRequiredToSave | TestBusinessObjectKind.PopulateDependentCollections, Array.Empty<PropertyDescriptor>());
		return arrivalCargoDesc;
	}

	NctsArrivalCargoDesc GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		var bill = header.Bills.AddNew();
		var arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
		return arrivalCargoDesc;
	}
}
