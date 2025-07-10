using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(NctsPLOfficeCode))]
sealed class NctsPLOfficeCodeTest : Customs.Business.Testing.CusCodeDataTest<NctsPLOfficeCode>
{
	public void TestValidation_Departure()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var officeCode = Factory.New<NctsPLOfficeCode>();
		officeCode.CY_ParentTableCode = header.TablePrefix;
		officeCode.CY_ParentID = header.PK;

		CombineAssertions(() =>
		{
			AssertType("NCTS4", typeof(EU.NCTS.Business.NctsEuOfficeCodeValidation), officeCode.Validation);

			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			officeCode.CY_ParentTableCode = header.MovementHeader.TablePrefix;
			officeCode.CY_ParentID = header.MovementHeader.PK;
			AssertType("NCTS5", typeof(NctsPLOfficeCodePhase5DepartureValidation), officeCode.Validation);
		});
	}

	protected override IEnumerable<NctsPLOfficeCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var header = factory.New<NctsHeader>();
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var customsOffice = header.MovementHeader.CustomsOffices.AddNew();
		customsOffice.CY_Data = "D";
		yield return customsOffice;
	}

	protected override BusinessObject GetNewBusinessObject() => departureMovement.CustomsOffices.AddNew();

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var header = factory.New<NctsHeader>();
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		return header.MovementHeader.CustomsOffices.AddNew();
	}

	protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest) => new LightValidationTesterNctsEuOfficeCode(bizObjToTest);

	protected override void SetUp()
	{
		base.SetUp();

		header = Factory.New<NctsHeader>();
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		departureMovement = header.MovementHeader;
		departureMovement.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed;
	}

	NctsHeader header;
	NctsDepartureMovementHeader departureMovement;
}

class LightValidationTesterNctsEuOfficeCode(BusinessObject bo) : LightValidationTester(bo)
{
	protected override bool ShouldTestProperty(ZPropertyInfo info)
	{
		// The validation do have a relationship with JobDocAddress, see NctsEuOfficeCodeValidation.CheckCY_Data, here we do reference header.DestinationTrader/SecurityConsignor/Principal...
		// But it can be overweight if we mark NctsEuOfficeCode as needing validation when we set properties in JobDocAddress,
		// So I suppressed the JobDocAddress here.
		if (info.BizObj.GetType() == typeof(JobDocAddress) || info.BizObj.GetType() == typeof(NctsPLOfficeCode))
		{
			return false;
		}
		return base.ShouldTestProperty(info);
	}
}
