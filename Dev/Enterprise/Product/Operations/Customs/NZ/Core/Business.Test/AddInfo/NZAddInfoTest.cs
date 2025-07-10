using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MasterFiles;

namespace Enterprise.Customs.NZ.Business.Testing
{
	using NUnit.Framework;

	[TestedType(typeof(NZAddInfo))]
	public class NZAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLineLevelOtherInfos()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = dec.Invoices.AddNew();

			AssertEquals(false, ((IHaveNZAddInfo)dec).AddInfo.IsLineLevel);

			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			AssertEquals(true, pivot.AddInfo.IsLineLevel);
		}

		public void TestBoolWorksTheWayWeExpectWhenTypeChangedToString()
		{
			var value = new ZBool("Y");
			AssertEquals(true, value);

			value = new ZBool("N");
			AssertEquals(false, value);
			//Cannot Initialise a ZBool with an empty string.
			/*			value = new ZBool(ZString.Empty);
						AssertEquals(false, value);	*/
		}

		public void TestSerialiseCollectionsOnFactorySaving()
		{
			TestAddInfo.HeaderOtherInfos.RemoveAll();
			Factory.Save();
			AssertNotContains("Other info collection is empty", "OtherInfos=", TestBizO.JE_AddInfo);
			OtherInfo otherInfo = TestAddInfo.HeaderOtherInfos.AddNew();
			otherInfo.ZO_Code = "ZZZ";
			otherInfo.ZO_Data = "111";
			Factory.Save();

			AssertEquals("Other info collection serialised", "ZZZ=111", TestAddInfo.ZN_OtherInfos);
			AssertContains("Class Addinfo", "OtherInfos=ZZZ=111", TestBizO.JE_AddInfo);
		}

		public void TestAddInfoWithOtherInfo()
		{
			TestBizO.JE_AddInfo = "OtherInfos=APE^APD=ZZZZ*ConcessionCode=123";
			((IHaveNZAddInfo)TestBizO).AddInfo.LoadPropertiesFromString(TestBizO.JE_AddInfo);
			AssertEquals("Concession", "123", TestAddInfo.ZN_ConcessionCode);
			AssertEquals("OtherInfo", "APE^APD=ZZZZ", TestAddInfo.ZN_OtherInfos);
		}

		public void TestOtherInfosCollection()
		{
			TestAddInfo.ZN_OtherInfos = "APE^APD=ZZZZ";
			TestAddInfo.OtherInfos.LoadFromString(TestAddInfo.ZN_OtherInfos);
			AssertEquals("Otherinfos count", 2, TestAddInfo.LineOtherInfos.Count);

			AssertEquals("APE", TestAddInfo.LineOtherInfos[0].ZO_Code);
			AssertEquals(ZString.Empty, TestAddInfo.LineOtherInfos[0].ZO_Data);
			AssertEquals("APD", TestAddInfo.LineOtherInfos[1].ZO_Code);
			AssertEquals("ZZZZ", TestAddInfo.LineOtherInfos[1].ZO_Data);
		}

		public void TestToString()
		{
			var startAddInfo = TestAddInfo.ToString();
			TestAddInfo.ZN_ConcessionCode = "123";
			Assert("ToString() of AddInfo", TestAddInfo.ToString().IndexOf("ConcessionCode=123") >= 0);

			TestAddInfo.ZN_ConcessionCode = ZString.Empty;
			AssertEquals("ToString() of AddInfo", startAddInfo, TestAddInfo.ToString());
		}

		public void TestLoadPropertiesFromAddInfoString()
		{
			ZString addInfoString = "ConcessionCode=123*PermitCodes=AF1=111";
			TestAddInfo.LoadPropertiesFromString(addInfoString);
			AssertEquals("Concession", "123", TestAddInfo.ZN_ConcessionCode);
			AssertEquals("Prohibit code", "AF1=111", TestAddInfo.ZN_PermitCodes);

			addInfoString = "ConcessionCode=123";
			TestAddInfo.LoadPropertiesFromString(addInfoString);
			AssertEquals("Concession", "123", TestAddInfo.ZN_ConcessionCode);
			AssertEquals("Permit code", ZString.Empty, TestAddInfo.ZN_PermitCodes);

			addInfoString = "PermitCodes=AF1=111";
			TestAddInfo.LoadPropertiesFromString(addInfoString);
			AssertEquals("Concession", ZString.Empty, TestAddInfo.ZN_ConcessionCode);
			AssertEquals("Permit code", "AF1=111", TestAddInfo.ZN_PermitCodes);

			addInfoString = "";
			TestAddInfo.LoadPropertiesFromString(addInfoString);
			AssertEquals("Concession", ZString.Empty, TestAddInfo.ZN_ConcessionCode);
			AssertEquals("Permit code", ZString.Empty, TestAddInfo.ZN_PermitCodes);
		}

		public void TestLoadPropertiesFromAddInfoWithClearExstingFalse()
		{
			ZString addInfoString = "ConcessionCode=123";
			TestAddInfo.LoadPropertiesFromString(addInfoString, clearExisting: false);
			AssertEquals("Concession Code", "123", TestAddInfo.ZN_ConcessionCode);

			addInfoString = "PermitCodes=AF1=111";
			TestAddInfo.LoadPropertiesFromString(addInfoString, clearExisting: false);
			AssertEquals("ProhibitedCode1", "123", TestAddInfo.ZN_ConcessionCode);
			AssertEquals("ProhibitedCode2", "AF1=111", TestAddInfo.ZN_PermitCodes);
		}

		public void TestAddInfoPropertyUpdate()
		{
			var startLength = TestBizO.JE_AddInfo.Length;
			TestBizO.JE_SoldOrConsigned = "NMB";
			Factory.Save();
			AssertContains("JE_AddInfo", "SoldOrConsigned=NMB", TestBizO.JE_AddInfo);
			AssertEquals("JE_AddInfo.Length", startLength + 20, TestBizO.JE_AddInfo.Length);
		}

		public void TestCodePropertiesUpdatedOnCollectionChanges()
		{
			PermitCode code = TestBizO.PermitCodes.AddNew();
			code.ZO_Code = "ZZZ";

			Factory.Save();
			AssertContains("AddInfo ZN_PermitCode is updated", "PermitCodes=ZZZ", TestBizO.JE_AddInfo);
		}

		#region Implementation

		#region TestAddInfo
		NZAddInfo TestAddInfo
		{
			get
			{
				if (fTestAddInfo == null)
				{
					fTestAddInfo = ((IHaveNZAddInfo)TestBizO).AddInfo;
				}
				return fTestAddInfo;
			}
		}
		NZAddInfo fTestAddInfo;
		#endregion

		#region TestBizO
		JobDeclaration TestBizO => fTestBizO ??= Factory.New<JobDeclaration>();
		JobDeclaration fTestBizO;
		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			CusClassification classification = Factory.New<CusClassification>();
			return new NZAddInfo(classification);
		}

		#endregion
	}
}
