using System.Collections;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CusTempStorageRegHeader))]
sealed class CusTempStorageRegHeaderTest : EnterpriseBusinessObjectTestCase
{
	public void TestGetStorageRegLineTypeCore()
		=> AssertEquals(typeof(CusTempStorageRegLine), header.GetStorageRegLineType());

	public void TestSRH_InternalReference_Caption()
	{
		AssertEquals("Customer Reference", DataBoundResourceStrings.GetDataForProperty(header.SRH_InternalReferenceInfo).Caption);
	}

	public void TestSRH_InternalReference_ReadOnly()
	{
		AssertEquals(true, header.SRH_InternalReferenceInfo.ReadOnly);
	}

	public void TestUpdateSRH_Reference_ThroughGoodsRegistrationNumberManager()
	{
		var goodsRegistrationNumberManager = header as IGoodsRegistrationNumberManager;
		AssertNotNull("[Pre-condition]: Header should be a type of IGoodsRegistrationNumberManager", goodsRegistrationNumberManager);
		goodsRegistrationNumberManager.SetNextGoodsRegistrationNumber("20240823N1234001");
		AssertEquals("SRH_Reference", "20240823N1234001", header.SRH_Reference);
	}

	public void TestLoad()
	{
		var header1 = Factory.New<CusTempStorageRegHeader>();
		header1.SRH_Reference = "SBC1312";
		header1.SRH_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-2);
		var header2 = Factory.New<CusTempStorageRegHeader>();
		header2.SRH_Reference = "SBC1312";
		header2.SRH_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-3);
		var header3 = Factory.New<CusTempStorageRegHeader>();
		header3.SRH_Reference = "SBC1312";
		header3.SRH_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
		var header4 = Factory.New<CusTempStorageRegHeader>();
		header4.SRH_Reference = "SBC1312";
		header4.SRH_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-4);
		header4.SRH_AppCode = "TST";
		Factory.Save();
		var factory = new BusinessObjectFactory();
		var header = CusTempStorageRegHeader.Load(factory, "SBC1312");
		AssertEquals(header2.PK, header.PK);
	}

	public void TestLoad_MRN()
	{
		var header1 = Factory.New<CusTempStorageRegHeader>();
		header1.SRH_Reference = "23DE586601055987B7";
		header1.SRH_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-2);

		Factory.Save();
		var factory = new BusinessObjectFactory();
		var header = CusTempStorageRegHeader.Load(factory, null, "23DE586601055987B7");
		AssertEquals(header1.PK, header.PK);
	}

	public void TestDefaultAppCode()
	{
		AssertEquals("SRH_AppCode", "SBW", header.SRH_AppCode);
	}

	public void TestGetEDocsProviderSupporter()
	{
		AssertType<EDocsProviderSupporter>(header.GetEDocsProviderSupporter());
	}

	public void TestDocumentSupporter()
	{
		var supporter = header.DocumentSupporter;
		CombineAssertions(() =>
		{
			AssertType<CusTempStorageRegHeaderDocumentSupporter>("Type", supporter);
			AssertSame("Cached", supporter, header.DocumentSupporter);
		});
	}

	public void TestLookupsType() => AssertType<CusTempStorageRegHeaderLookups>(header.Lookups);

	public void TestSRH_Reference_Attributes() => CombineAssertions(() =>
		AssertEntity<CusTempStorageRegHeader>()
			.HasProperty(x => x.SRH_Reference)
			.WithCaption("Goods Number"));

	public void TestImportProcedure_Attributes() => CombineAssertions(() =>
		AssertEntity<CusTempStorageRegHeader>()
			.HasProperty(x => x.SRH_PreviousReferenceType)
			.WithList($"{nameof(CusTempStorageRegHeader.Lookups)}.{nameof(CusTempStorageRegHeaderLookups.PreviousReferenceTypeList)}"));

	public void TestSRH_Status_Attributes() => CombineAssertions(() =>
		AssertEntity<CusTempStorageRegHeader>()
			.HasProperty(x => x.SRH_Status)
			.WithList($"{nameof(CusTempStorageRegHeader.Lookups)}.{nameof(CusTempStorageRegHeaderLookups.StatusList)}"));

	public void TestUnloadingRemarks_Attributes() => CombineAssertions(() =>
		AssertEntity<CusTempStorageRegHeader>()
			.HasProperty(x => x.UnloadingRemarks)
			.WithCaption("Unloading Remarks")
			.WithFullDescription("Unloading Remarks when not NCTS. Note: Full formal remarks to be entered in portal Altlnn.")
			.WithAttribute<MaxLengthAttribute>(x => x.MaxLength == 100));

	public void TestUnloadingRemarks()
	{
		const string unloadingRemarks = "Sample Unloading Remarks";
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, header.UnloadingRemarks);
			UpdateUnloadingRemarksAddOnColumn();
			AssertEquals("After GenAddOnColumn", unloadingRemarks, header.UnloadingRemarks);
		});

		void UpdateUnloadingRemarksAddOnColumn()
		{
			var query = new ZQuery(GenAddOnColumnSchema.XA_ParentID, header.PK);
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, CusTempStorageRegHeaderSchema.Constants.Prefix);
			query.AddToFilter(GenAddOnColumnSchema.XA_Name, "NO_GoodsReg_UnloadingRemarks");
			var addOnColumn = Factory.LoadTop1<GenAddOnColumn>(query);
			addOnColumn.XA_Data = unloadingRemarks;
			Factory.Save();
		}
	}

	public void TestTransportMeans()
	{
		var transportMeans = header.TransportMeans;
		AssertNotNull("Calling first time", transportMeans);
		AssertEquals("When created, it should return same obj", transportMeans.PK, header.TransportMeans.PK);
	}

	public void TestCusTempStorageRegHeaderApplicationCodeTypes()
	{
		var cusTempStorageRegHeaderApplicationCodeTypes = (Hashtable)ObjectFactory.Get("CusTempStorageRegHeaderApplicationCodeTypes");
		AssertEquals("NO SBW code should be present", true, cusTempStorageRegHeaderApplicationCodeTypes.Contains(TemporaryStorageApplicationCodeList.Codes.SBW));
		var noType = (ObjectHandle)cusTempStorageRegHeaderApplicationCodeTypes[TemporaryStorageApplicationCodeList.Codes.SBW];
		AssertEquals("Type", typeof(CusTempStorageRegHeader), noType.GetObjectType());
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_Reference = "TEST";
	}
	CusTempStorageRegHeader header;

	protected override BusinessObject GetNewBusinessObject() => header;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => header;

	protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => header;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var header = factory.New<CusTempStorageRegHeader>();
		header.SRH_Reference = "TEST";
		return header;
	}
}
