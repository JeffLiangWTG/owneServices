using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(AsycudaBillLookups))]
sealed class AsycudaBillLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestImportProcedureCodeList()
	{
		CombineAssertions(() =>
		{
			AssertCodeDescriptionPairList(lookups.ImportProcedureCodeList,
				("COLLECTIVE_RELEASE", "Collective clearance at unload"),
				("IMMEDIATE_RELEASE_IMPORT", "Into free circulation"),
				("IMMEDIATE_RELEASE_VOEC", "VOEC shipments"),
				("TRANSIT_IMPORT", "Transit from sender"),
				("TRANSIT_RELEASE", "Transit started at border"),
				("WAREHOUSE_RELEASE", "Entry into customs warehouse"),
				("DOCUMENTS_NOT_OBLIGED_RELEASE", "Documents exempt from the obligation to declare"),
				("ATA_CARNET", "Pre-cleared acc. to ATA convention"),
				("TIR_CARNET", "Sent according to TIR convention"));
		});
	}

	public void TestExportProcedureCodeList()
	{
		CombineAssertions(() =>
		{
			AssertCodeDescriptionPairList(lookups.ExportProcedureCodeList,
				("EXP", "Export"),
				("TRA", "Transit"));
		});
	}

	public void TestTransportDocumentTypeList() => CombineAssertions(() =>
	{
		var list = lookups.TransportDocumentTypeList;
		AssertType<ASYCUDA.Business.TransportDocumentTypes>(list);
		AssertSame("Cache", list, lookups.TransportDocumentTypeList);
	});

	public void TestPortOfLoadingCodes() => AssertUnlocoLookupCodes(bill.ABL_RL_NKPortOfLoadingInfo, nameof(lookups.PortOfLoadingCodes));

	public void TestPortOfDischargeCodes() => AssertUnlocoLookupCodes(bill.ABL_RL_NKPortOfDischargeInfo, nameof(lookups.PortOfDischargeCodes));

	public void TestFinalDestinationCodes() => AssertUnlocoLookupCodes(bill.ABL_RL_NKFinalDestinationInfo, nameof(lookups.FinalDestinationCodes));

	void AssertUnlocoLookupCodes(ZPropertyInfo propertyInfo, ZString codelist)
	{
		PropertyInfo codelistInfo = lookups.GetType().GetProperty(codelist);
		var propertyName = propertyInfo.Name;

		CombineAssertions(() =>
		{
			propertyInfo.Value = ZString.Empty;
			AssertType<RefUNLOCOCollection>($"{propertyName} is empty", codelistInfo.GetValue(lookups));

			propertyInfo.Value = (ZString)"DK";
			AssertType<RefCountryCollection>($"{propertyName} is 2 characters", codelistInfo.GetValue(lookups));

			propertyInfo.Value = (ZString)"ABCDE";
			AssertType<RefUNLOCOCollection>($"{propertyName} is more than 2 characters", codelistInfo.GetValue(lookups));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<AsycudaManifestHeader>();
		bill = header.Bills.AddNew();
		lookups = bill.Lookups;
	}

	AsycudaBill bill;
	AsycudaManifestHeader header;
	AsycudaBillLookups lookups;
}
