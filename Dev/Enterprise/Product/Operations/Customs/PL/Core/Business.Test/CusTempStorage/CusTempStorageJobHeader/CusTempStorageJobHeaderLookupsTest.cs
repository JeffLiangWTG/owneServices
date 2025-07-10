using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.CusTempStorage.Testing;

class CusTempStorageJobHeaderLookupsTest : BusinessObjectLookupsTestCase
{
	[TestDate(2019, 9, 5)]
	public void TestGuaranteeList()
	{
		var list = jobHeader.Lookups.GuaranteeList;

		CombineAssertions(() =>
		{
			AssertEquals("No Guarantees", 0, list.Count);
			AddGuarantees();
			AssertEquals("2 Guarantees exist", 2, list.Count);
		});
	}

	void AddGuarantees()
	{
		var organization1 = Factory.NewWithValidTestData<OrgHeader>();
		organization1.OH_Code = "Org1";
		var organization2 = Factory.NewWithValidTestData<OrgHeader>();
		organization2.OH_Code = "Org2";

		var permitHeader3 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
		permitHeader3.CPH_Number = "PERMIT3";
		permitHeader3.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		permitHeader3.CPH_OH_PermitHolder = organization1.PK;
		AddPermitLine(permitHeader3, "ARS", new ZDate(2019, 6, 4));

		var permitHeader4 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
		permitHeader4.CPH_Number = "PERMIT4";
		permitHeader4.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		permitHeader3.CPH_OH_PermitHolder = organization2.PK;
		AddPermitLine(permitHeader4, "PAA", new ZDate(2019, 6, 4));
		AddPermitLine(permitHeader4, "PEE", new ZDate(2019, 6, 5));
		AddPermitLine(permitHeader4, "POO", new ZDate(2019, 9, 5));
		AddPermitLine(permitHeader4, "PZZ", new ZDate(2019, 9, 6));
	}

	void AddPermitLine(CusGuaranteeHeader header, ZString reference, ZDate transactionDate)
	{
		var line = header.CusGuaranteeLineTransactions.AddNew();
		line.FillWithValidTestData();
		line.CPL_Reference = reference;
		line.CPL_TransactionDate = transactionDate;
	}

	protected override void SetUp()
	{
		base.SetUp();
		jobHeader = CusTempStorageJobHeader.New(Factory);
	}

	CusTempStorageJobHeader jobHeader;
}
