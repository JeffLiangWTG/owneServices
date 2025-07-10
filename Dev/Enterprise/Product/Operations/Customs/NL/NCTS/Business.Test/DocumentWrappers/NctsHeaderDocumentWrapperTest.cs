using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NL.Business.Testing;
using Enterprise.Customs.NL.NCTS.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderDocumentWrapper))]
sealed class NctsHeaderDocumentWrapperTest : NonPersistentBusinessObjectTestCase
{
	public void TestNew()
	{
		var header = Factory.New<NctsHeader>();

		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Expected exception when NctsHeader parameter is null", () => NctsHeaderDocumentWrapper.New(null, Factory));
			AssertExceptionThrown<ArgumentNullException>("Expected exception when Factory parameter is null", () => NctsHeaderDocumentWrapper.New(header, null));
			AssertNotNull("Instance of NctsHeaderDocumentWrapper expected", NctsHeaderDocumentWrapper.New(header, Factory));
		});
	}

	public void TestSHOWSTAMPONBOXC()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var wrapper = GetWrapper(header);

		CombineAssertions(() =>
		{
			AssertEquals("Inactive " + nameof(wrapper.SHOWSTAMPONBOXC), false, wrapper.SHOWSTAMPONBOXC);

			header.MovementHeader.IsFallbackProcedure = true;
			AssertEquals("Active " + nameof(wrapper.SHOWSTAMPONBOXC), true, wrapper.SHOWSTAMPONBOXC);
		});
	}

	public void TestBOXCDEPARTUREOFFICECOUNTRYCODE()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var wrapper = GetWrapper(header);
		AssertEquals(nameof(wrapper.BOXCDEPARTUREOFFICECOUNTRYCODE), "NL", wrapper.BOXCDEPARTUREOFFICECOUNTRYCODE);
	}

	public void TestBOXCOFFICEOFDEPARTURECODE()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var wrapper = GetWrapper(header);
		AssertEquals(nameof(wrapper.BOXCOFFICEOFDEPARTURECODE), "0074", wrapper.BOXCOFFICEOFDEPARTURECODE);
	}

	[TestDate(2025, 03, 17, 16, 00, 00)]
	public void TestBOXCUNIQUEREFERENCENUMBER()
	{
		FallbackConfigurationTestHelper.GetFallbackConfigurationWithDVASetValue(new ZDateTime(2025, 03, 17, 16, 00, 00));
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var wrapper = GetWrapper(header);

		CombineAssertions(() =>
		{
			AssertNullOrEmpty("Empty " + nameof(wrapper.BOXCUNIQUEREFERENCENUMBER), wrapper.BOXCUNIQUEREFERENCENUMBER);

			header.MovementHeader.IsFallbackProcedure = true;
			Factory.Save();
			AssertEquals(nameof(wrapper.BOXCUNIQUEREFERENCENUMBER), "2503000001", wrapper.BOXCUNIQUEREFERENCENUMBER);
		});
	}

	[TestDate(2025, 03, 17, 16, 00, 00)]
	public void TestBOXCDATE()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var wrapper = GetWrapper(header);

		CombineAssertions(() =>
		{
			AssertNullOrEmpty("Empty " + nameof(wrapper.BOXCDATE), wrapper.BOXCDATE);

			header.MovementHeader.FallbackTime = ZDateTime.Today;
			AssertEquals(nameof(wrapper.BOXCDATE), "17/03/2025", wrapper.BOXCDATE);
		});
	}

	public void TestBOXCAUTHORIZEDCONSIGNORNAME()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);

		CombineAssertions(() =>
		{
			var wrapper = GetWrapper(header);
			AssertNullOrEmpty("No usage, empty " + nameof(wrapper.BOXCAUTHORIZEDCONSIGNORNAME), wrapper.BOXCAUTHORIZEDCONSIGNORNAME);

			SetupAuthorizationUsage(header, "ACR", "11223344556677889900");
			wrapper = GetWrapper(header);
			AssertEquals(nameof(wrapper.BOXCAUTHORIZEDCONSIGNORNAME), "TestCompany", wrapper.BOXCAUTHORIZEDCONSIGNORNAME);

			SetupAuthorizationUsage(header, "ZZZ", "11223344556677889900");
			wrapper = GetWrapper(header);
			AssertNullOrEmpty("Wrong code, empty " + nameof(wrapper.BOXCAUTHORIZEDCONSIGNORNAME), wrapper.BOXCAUTHORIZEDCONSIGNORNAME);
		});
	}

	public void TestBOXCAUTHORISATIONNUMBER()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);

		CombineAssertions(() =>
		{
			var wrapper = GetWrapper(header);
			AssertNullOrEmpty("No usage, empty " + nameof(wrapper.BOXCAUTHORISATIONNUMBER), wrapper.BOXCAUTHORISATIONNUMBER);

			SetupAuthorizationUsage(header, "ACR", "1234");
			wrapper = GetWrapper(header);
			AssertEquals("Less that 8 characters " + nameof(wrapper.BOXCAUTHORISATIONNUMBER), "1234", wrapper.BOXCAUTHORISATIONNUMBER);

			SetupAuthorizationUsage(header, "ACR", "11223344556677889900");
			wrapper = GetWrapper(header);
			AssertEquals(nameof(wrapper.BOXCAUTHORISATIONNUMBER), "77889900", wrapper.BOXCAUTHORISATIONNUMBER);

			SetupAuthorizationUsage(header, "ZZZ", "11223344556677889900");
			wrapper = GetWrapper(header);
			AssertNullOrEmpty("Wrong code, empty " + nameof(wrapper.BOXCAUTHORISATIONNUMBER), wrapper.BOXCAUTHORISATIONNUMBER);
		});
	}

	public void TestNOTRELEASEDWATERMARK()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var wrapper = GetWrapper(header);
		AssertEquals(nameof(wrapper.NOTRELEASEDWATERMARK), string.Empty, wrapper.NOTRELEASEDWATERMARK);
	}

	void SetupAuthorizationUsage(NctsHeader header, ZString code, ZString number)
	{
		header.MovementHeader.CusAuthorizationUsages.RemoveAndDeleteAll();
		var authorizationUsage = header.MovementHeader.CusAuthorizationUsages.AddNew();
		authorizationUsage.AGC_Code = code;
		authorizationUsage.AGC_Number = number;
		authorizationUsage.AGC_OH_Owner = orgHeader.PK;
	}

	protected override void SetUp()
	{
		base.SetUp();
		orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "TestCompany";
	}
	OrgHeader orgHeader;

	protected override BusinessObject GetNewBusinessObject() => NctsHeaderDocumentWrapper.New(Factory.New<NctsHeader>(), Factory);

	NctsHeaderDocumentWrapper GetWrapper(NctsHeader header)
	{
		return (NctsHeaderDocumentWrapper)NctsHeaderDocumentWrapper.New(header, Factory);
	}
}
