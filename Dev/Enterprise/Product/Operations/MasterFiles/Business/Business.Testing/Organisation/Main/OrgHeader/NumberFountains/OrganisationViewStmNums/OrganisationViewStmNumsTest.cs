using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrganisationViewStmNums))]
	sealed class OrganisationViewStmNumsTest : EnterpriseBusinessObjectTestCase
	{
		#region TestSN_ValueForDisplay

		public void TestSN_ValueForDisplay_ForwardAirBillNumbers()
		{
			SN_ValueForDisplayCore(OrgConstants.NumberFountains.Code.ForwardAirBillNumbers);
		}

		public void TestSN_ValueForDisplay_TransportReferenceNumbers()
		{
			SN_ValueForDisplayCore(OrgConstants.NumberFountains.Code.TransportReferenceNumbers);
		}

		void SN_ValueForDisplayCore(string type)
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				var header = Factory.NewWithValidTestData<OrgHeader>();
				var stmNum = Factory.NewWithValidTestData<OrganisationViewStmNums>();

				AssertEquals(stmNum.SN_Value, stmNum.SN_ValueForDisplay);

				stmNum.SN_Type = type;
				stmNum.SN_MinimumValue = 100;
				stmNum.SN_MaximumValue = 102;
				stmNum.SN_Count = 3;
				stmNum.SN_Owner = header.PK;

				Factory.Save();

				var query = new ZQuery(ViewStmNumsSchema.SN_Owner, header.PK);
				var newStmNum = NewFactory().LoadTop1<OrganisationViewStmNums>(query);

				AssertNotNull(newStmNum);
				AssertEquals("Should load from database", 100L, (long)newStmNum.SN_ValueForDisplay);
				AssertEquals("Should load from database", 100L, (long)newStmNum.SN_Value);

				var foutain = stmNum.TryGetNumberFountain();
				foutain.GetNextFormatted(Factory);
				foutain.GetNextFormatted(Factory);

				Factory.Save();

				newStmNum = NewFactory().LoadTop1<OrganisationViewStmNums>(query);
				AssertEquals("Should load the latest number from database", 102L, (long)newStmNum.SN_ValueForDisplay);
				AssertEquals("SN_Value is the a different number after this ViewStmNum do a cache", 103L, (long)newStmNum.SN_Value);

				foutain.GetNextFormatted(Factory);
				Factory.Save();

				newStmNum = NewFactory().LoadTop1<OrganisationViewStmNums>(query);
				AssertEquals("Should load the latest number from database", -1L, (long)newStmNum.SN_ValueForDisplay);
				AssertEquals("SN_Value is the a different number after this ViewStmNum do a cache", 103L, (long)newStmNum.SN_Value);
			}
		}

		#endregion

		public void TestIsFtzWhatever()
		{
			var stmNum = Factory.NewWithValidTestData<OrganisationViewStmNums>();
			Assert(!stmNum.IsFTZWarehouseType);
			Assert(!stmNum.IsFTZNonWarehouseType);
			Assert(!stmNum.IsFTZNumberType);
			stmNum.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumberForWarehouse;
			Assert(stmNum.IsFTZWarehouseType);
			Assert(!stmNum.IsFTZNonWarehouseType);
			Assert(stmNum.IsFTZNumberType);
			stmNum.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			Assert(!stmNum.IsFTZWarehouseType);
			Assert(stmNum.IsFTZNonWarehouseType);
			Assert(stmNum.IsFTZNumberType);
		}

		public void TestUpdateMaximumValueFromType()
		{
			var stmNum = Factory.NewWithValidTestData<OrganisationViewStmNums>();
			AssertEquals("Default to OrganisationViewStmNums.Schema.DefaultFountainMaximumValue", OrganisationViewStmNums.Schema.DefaultFountainMaximumValue, (long)stmNum.SN_MaximumValue);

			stmNum.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			AssertEquals("Should update to OrganisationViewStmNums.Schema.DefaultFTZMaximumValue_NonWarehouse", OrganisationViewStmNums.Schema.DefaultFTZMaximumValue_NonWarehouse, (long)stmNum.SN_MaximumValue);
			AssertEquals(99999, OrganisationViewStmNums.Schema.DefaultFTZMaximumValue_NonWarehouse);

			AssertEquals("Should update to OrganisationViewStmNums.Schema.DefaultFTZMaximumValue_NonWarehouse", OrganisationViewStmNums.Schema.DefaultFTZMaximumValue_NonWarehouse, (long)stmNum.SN_MaximumValue);

			stmNum.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumberForWarehouse;
			AssertEquals("Should update to OrganisationViewStmNums.Schema.DefaultFTZMaximumValue_Warehouse", OrganisationViewStmNums.Schema.DefaultFTZMaximumValue_Warehouse, (long)stmNum.SN_MaximumValue);
			AssertEquals(99999999, OrganisationViewStmNums.Schema.DefaultFTZMaximumValue_Warehouse);

			stmNum.SN_MaximumValue = OrganisationViewStmNums.Schema.DefaultFountainMaximumValue + 1;
			stmNum.SN_Type = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
			AssertEquals("Should update to OrganisationViewStmNums.Schema.DefaultFountainMaximumValue", OrganisationViewStmNums.Schema.DefaultFountainMaximumValue, (long)stmNum.SN_MaximumValue);

			stmNum.SN_MaximumValue = 9999;
			stmNum.SN_Count = 9999;
			stmNum.SN_Type = OrgConstants.NumberFountains.Code.ForwardAirBillNumbers;
			AssertEquals("Should not update", 9999, (long)stmNum.SN_MaximumValue);
		}

		public void TestUpdatePrefixFromType()
		{
			var stmNum = Factory.NewWithValidTestData<OrganisationViewStmNums>();
			stmNum.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			stmNum.SN_ClientPrefix = "AAA";
			stmNum.SN_ZoneIDPrefix = "BBB";
			stmNum.SN_Prefix = "CCC";

			stmNum.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			AssertEquals(true, stmNum.SN_ClientPrefix.IsEmpty);
			AssertEquals(true, stmNum.SN_ZoneIDPrefix.IsEmpty);
			AssertEquals(true, stmNum.SN_Prefix.IsEmpty);

			stmNum.SN_Prefix = "CCC";
			stmNum.SN_Type = OrgConstants.NumberFountains.Code.ForwardAirBillNumbers;

			AssertEquals(true, stmNum.SN_Prefix.IsEmpty);
		}

		public void TestSN_Prefix_ReadOnly()
		{
			var stmNum = Factory.NewWithValidTestData<OrganisationViewStmNums>();
			stmNum.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			AssertEquals(false, stmNum.SN_PrefixInfo.ReadOnly);

			stmNum.SN_Type = OrgConstants.NumberFountains.Code.ForwardAirBillNumbers;
			AssertEquals(true, stmNum.SN_PrefixInfo.ReadOnly);

			stmNum.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			AssertEquals(false, stmNum.SN_PrefixInfo.ReadOnly);

			Factory.Save();
			AssertEquals("Currently when we store StmNums in the database SN_Prefix is ReadOnly, if you change this please add validation to make sure this prefix is not linked to StmNumberRangeMatchingDetails.", true, stmNum.SN_PrefixInfo.ReadOnly);
		}

		public void TestSN_Type_ReadOnly()
		{
			var stmNum = Factory.NewWithValidTestData<OrganisationViewStmNums>();
			stmNum.SN_Type = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
			AssertEquals(false, stmNum.SN_TypeInfo.ReadOnly);

			Factory.Save();
			AssertEquals("Currently when we store StmNums in the database SN_Type is ReadOnly, if you change this please add validation to make sure this prefix is not linked to StmNumberRangeMatchingDetails.", true, stmNum.SN_TypeInfo.ReadOnly);
		}

		public void TestSN_TypeDescription()
		{
			var stmNum = Factory.New<OrganisationViewStmNums>();
			stmNum.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			AssertEquals(OrgConstants.NumberFountains.Description.SSCCBarCodeNumbers, stmNum.SN_TypeDescription);

			stmNum.SN_Type = "AAA";
			AssertEquals("", stmNum.SN_TypeDescription);
		}

		public void TestSN_Type_UpdatesSN_Name()
		{
			var stmNum = Factory.New<OrganisationViewStmNums>();
			stmNum.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			AssertEquals("OrgOwned_SSC", stmNum.SN_Name);

			stmNum.SN_Type = "AAA";
			AssertEquals("OrgOwned_AAA", stmNum.SN_Name);
		}

		public void TestDataIsPersistedCorrectly()
		{
			DataIsPersistedCorrectlyCore(OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers, "123", "OrgOwned_SSC_123", OrgConstants.NumberFountains.Description.SSCCBarCodeNumbers);
			DataIsPersistedCorrectlyCore(OrgConstants.NumberFountains.Code.TransportReferenceNumbers, "44444", "OrgOwned_TRF_44444", OrgConstants.NumberFountains.Description.TransportReferenceNumbers);
		}

		void DataIsPersistedCorrectlyCore(string type, string prefix, string sn_name, string typeDescription)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var stmNum = Factory.New<OrganisationViewStmNums>();
			stmNum.SN_Type = type;
			stmNum.SN_Prefix = prefix;
			stmNum.SN_Owner = org.PK;
			AssertEquals("Precondition", sn_name, stmNum.SN_Name);
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var stmNumsInOtherFactory = newFactory.Load<OrganisationViewStmNums>(new ZQuery(ViewStmNumsSchema.SN_Name, sn_name)).Single();
			AssertEquals(type, stmNumsInOtherFactory.SN_Type);
			AssertEquals(prefix, stmNumsInOtherFactory.SN_Prefix);
			AssertEquals(typeDescription, stmNumsInOtherFactory.SN_TypeDescription);
		}

		public void TestSN_Prefix_UpdatesSN_Name()
		{
			var stmNum = Factory.New<OrganisationViewStmNums>();
			stmNum.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			stmNum.SN_Prefix = "1234567";
			AssertEquals("OrgOwned_SSC_1234567", stmNum.SN_Name);

			stmNum.SN_Prefix = "20082015";
			AssertEquals("OrgOwned_SSC_20082015", stmNum.SN_Name);

			stmNum.SN_Prefix = "";
			AssertEquals("OrgOwned_SSC", stmNum.SN_Name);
		}

		public void TestSN_ZoneIDPrefixAndSN_ClientPrefixUpdatesSN_Prefix()
		{
			var stmNum = Factory.New<OrganisationViewStmNums>();
			stmNum.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			stmNum.SN_ZoneIDPrefix = "1234567";
			AssertEquals("1234567", stmNum.SN_Prefix);

			stmNum.SN_ClientPrefix = "AAA";
			AssertEquals("1234567AAA", stmNum.SN_Prefix);
		}

		public void TestSN_Name_InializeTypeAndPrefixOnLoaded()
		{
			var stmNum = Factory.New<OrganisationViewStmNums>();
			stmNum.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			stmNum.SN_Prefix = "1234567";
			AssertEquals("prerequisite", "OrgOwned_SSC_1234567", stmNum.SN_Name);

			var stmNum2 = Factory.New<OrganisationViewStmNums>();
			stmNum2.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			stmNum2.SN_Prefix = "1234567AAA";

			var stmNum3 = Factory.New<OrganisationViewStmNums>();
			stmNum3.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			stmNum3.SN_Prefix = "123456789ABC";

			var stmNum4 = Factory.New<OrganisationViewStmNums>();
			stmNum4.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			stmNum4.SN_Prefix = "2020:13149600150|1";

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var stmNumReloaded = anotherFactory.LoadTop1<OrganisationViewStmNums>(new ZQuery(ViewStmNumsSchema.SN_Name, stmNum.SN_Name));

			AssertEquals("prerequisite", "OrgOwned_SSC_1234567", stmNumReloaded.SN_Name);
			AssertEquals("prerequisite", false, stmNumReloaded.HasChanges);
			AssertEquals("SSC", stmNumReloaded.SN_Type);
			AssertEquals("1234567", stmNumReloaded.SN_Prefix);

			var stmNumReloaded2 = anotherFactory.LoadTop1<OrganisationViewStmNums>(new ZQuery(ViewStmNumsSchema.SN_Name, stmNum2.SN_Name));
			AssertEquals("prerequisite", "OrgOwned_FTZ_1234567AAA", stmNumReloaded2.SN_Name);
			AssertEquals("1234567", stmNumReloaded2.SN_ZoneIDPrefix);
			AssertEquals("AAA", stmNumReloaded2.SN_ClientPrefix);

			var stmNumReloaded3 = anotherFactory.LoadTop1<OrganisationViewStmNums>(new ZQuery(ViewStmNumsSchema.SN_Name, stmNum3.SN_Name));
			AssertEquals("prerequisite", "OrgOwned_FTZ_123456789ABC", stmNumReloaded3.SN_Name);
			AssertEquals("123456789", stmNumReloaded3.SN_ZoneIDPrefix);
			AssertEquals("ABC", stmNumReloaded3.SN_ClientPrefix);

			AssertNoExceptionThrown(() =>
			{
				anotherFactory.LoadTop1<OrganisationViewStmNums>(new ZQuery(ViewStmNumsSchema.SN_Name, stmNum4.SN_Name));
			});
		}

		public void TestDefaultValues()
		{
			var stmNum = Factory.New<OrganisationViewStmNums>();
			AssertEquals(OrganisationViewStmNums.Schema.MinimumValue, (long)stmNum.SN_MinimumValue);
			AssertEquals(OrganisationViewStmNums.Schema.DefaultFountainMaximumValue, (long)stmNum.SN_MaximumValue);

			AssertEquals(OrganisationViewStmNums.Schema.MinimumValue, (long)stmNum.SN_Value);
		}

		public void TestDefaultMaximumValueForType()
		{
			var stmNum = Factory.New<OrganisationViewStmNums>();
			AssertEquals(OrganisationViewStmNums.Schema.DefaultFountainMaximumValue, (long)stmNum.SN_MaximumValue);

			stmNum.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			AssertEquals(OrganisationViewStmNums.Schema.DefaultFTZMaximumValue_NonWarehouse, (long)stmNum.SN_MaximumValue);

			stmNum.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumberForWarehouse;
			AssertEquals(OrganisationViewStmNums.Schema.DefaultFTZMaximumValue_Warehouse, (long)stmNum.SN_MaximumValue);

			stmNum.SN_Type = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
			AssertEquals(OrganisationViewStmNums.Schema.DefaultFountainMaximumValue, (long)stmNum.SN_MaximumValue);
		}

		public void TestDefaultMaximumValueForSSCCNumberType_PrefixLength()
		{
			var stmNum = Factory.New<OrganisationViewStmNums>();
			stmNum.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			stmNum.SN_Prefix = "123456789";

			AssertEquals("Length of maximum value should be 8", 99999999, (long)stmNum.SN_MaximumValue);

			stmNum.SN_Prefix = "123";

			AssertEquals("Length of maximum value should be 14", 99999999999999, (long)stmNum.SN_MaximumValue);

			stmNum.SN_Prefix = "123456789012";

			AssertEquals("Length of maximum value should be 5", 99999, (long)stmNum.SN_MaximumValue);
		}

		public void TestSN_Count()
		{
			var stmNum = Factory.New<OrganisationViewStmNums>();
			stmNum.SN_MinimumValue = 1000;
			stmNum.SN_Count = 200;

			AssertEquals(200L, (long)stmNum.SN_Count);
			AssertEquals(1199L, (long)stmNum.SN_MaximumValue);

			stmNum.SN_Count = 300;
			AssertEquals(300L, (long)stmNum.SN_Count);
			AssertEquals(1299L, (long)stmNum.SN_MaximumValue);
		}

		public void TestSN_Value_IsReadonly()
		{
			var stmNum = Factory.New<OrganisationViewStmNums>();
			Assert(stmNum.SN_ValueInfo.ReadOnly);
		}

		public void TestSN_MaximumValue_IsReadonly()
		{
			var stmNum = Factory.New<OrganisationViewStmNums>();
			Assert(stmNum.SN_MaximumValueInfo.ReadOnly);
		}

		public void TestSN_Type_IsReadonlyAfterInitialSave()
		{
			var stmNum = Factory.New<OrganisationViewStmNums>();
			stmNum.SN_Type = "AAA";
			Assert("Not readonly before initial save", !stmNum.SN_TypeInfo.ReadOnly);

			Factory.Save();
			Assert("Readonly after initial save", stmNum.SN_TypeInfo.ReadOnly);

			var anotherFactory = new BusinessObjectFactory();
			var stmNumReloaded = anotherFactory.LoadTop1<OrganisationViewStmNums>(new ZQuery(ViewStmNumsSchema.SN_Name, stmNum.SN_Name));

			Assert("Readonly after initial save", stmNumReloaded.SN_TypeInfo.ReadOnly);
		}

		public void TestSN_Prefix_IsReadonlyAfterInitialSave()
		{
			var stmNum = Factory.New<OrganisationViewStmNums>();
			stmNum.SN_Type = "AAA";
			stmNum.SN_Prefix = "123";
			Assert("Not readonly before initial save", !stmNum.SN_PrefixInfo.ReadOnly);

			Factory.Save();
			Assert("Readonly after initial save", stmNum.SN_PrefixInfo.ReadOnly);

			var anotherFactory = new BusinessObjectFactory();
			var stmNumReloaded = anotherFactory.LoadTop1<OrganisationViewStmNums>(new ZQuery(ViewStmNumsSchema.SN_Name, stmNum.SN_Name));

			Assert("Readonly after initial save", stmNumReloaded.SN_PrefixInfo.ReadOnly);
		}

		public void TestTryGetNumberFountain_NullWhenNotSaved()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();

			var stmNum = Factory.New<OrganisationViewStmNums>();
			stmNum.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			stmNum.SN_Prefix = "1234567";
			stmNum.SN_Owner = header.PK;

			AssertNull("Null when not saved", stmNum.TryGetNumberFountain());

			Factory.Save();
			AssertNotNull(stmNum.TryGetNumberFountain());
		}

		public void TestTryGetNumberFountain_NullWhenHasChanges()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();

			var stmNum = Factory.New<OrganisationViewStmNums>();
			stmNum.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			stmNum.SN_Prefix = "1234567";
			stmNum.SN_Owner = header.PK;

			Factory.Save();
			AssertNotNull("prerequisite", stmNum.TryGetNumberFountain());

			var anotherFactory = new BusinessObjectFactory();
			var stmNumReloaded = anotherFactory.LoadTop1<OrganisationViewStmNums>(new ZQuery(ViewStmNumsSchema.SN_Owner, header.PK));

			stmNumReloaded.SN_MaximumValue = 9000;
			AssertNull(stmNumReloaded.TryGetNumberFountain());

			anotherFactory.Save();
			AssertNotNull(stmNumReloaded.TryGetNumberFountain());
		}

		public void TestTryGetNumberFountain_NullWhenNoHeader()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();

			var stmNum = Factory.New<OrganisationViewStmNums>();
			stmNum.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			stmNum.SN_Prefix = "1234567";
			stmNum.SN_Owner = header.PK;

			Factory.Save();
			AssertNotNull("prerequisite", stmNum.TryGetNumberFountain());
			var newFactory = new BusinessObjectFactory();
			stmNum = newFactory.LoadTop1<OrganisationViewStmNums>(new ZQuery(ViewStmNumsSchema.SN_Owner, header.PK));
			stmNum.SN_Owner = ZGuid.Empty;
			AssertNull("prerequisite", stmNum.Header);
			AssertNull(stmNum.TryGetNumberFountain());
		}

		public void TestTryGetNumberFountain_SSCCBarCodeNumbers()
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				var header = Factory.NewWithValidTestData<OrgHeader>();
				var stmNum = Factory.New<OrganisationViewStmNums>();
				stmNum.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
				stmNum.SN_Prefix = "1234567";
				stmNum.SN_Owner = header.PK;
				Factory.Save();

				var fountain = stmNum.TryGetNumberFountain();
				AssertNotNull("prerequisite", fountain);

				AssertEquals("012345670000000015", fountain.GetNextFormatted(Factory));
				AssertEquals("012345670000000022", fountain.GetNextFormatted(Factory));

				Factory.Save();
				AssertEquals("012345670000000039", fountain.GetNextFormatted(Factory));
			}
		}

		public void TestTryGetNumberFountain_TransportReferenceNumbers()
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				var header = Factory.NewWithValidTestData<OrgHeader>();
				var stmNum = Factory.New<OrganisationViewStmNums>();
				stmNum.SN_Type = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
				stmNum.SN_Prefix = "1234567";
				stmNum.SN_MaximumValue = 99999999;
				stmNum.SN_Owner = header.PK;
				Factory.Save();

				var fountain = stmNum.TryGetNumberFountain();
				AssertNotNull("prerequisite", fountain);

				AssertEquals("123456700000001", fountain.GetNextFormatted(Factory));
				AssertEquals("123456700000002", fountain.GetNextFormatted(Factory));

				Factory.Save();
				AssertEquals("123456700000003", fountain.GetNextFormatted(Factory));
			}
		}

		public void TestCanNotDeleteTypeOrPrefixIfUsedInMatchingDetail()
		{
			var prefix = "1";
			var type = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
			var stmNums = Factory.NewWithValidTestData<OrganisationViewStmNums>();
			var header = Factory.NewWithValidTestData<OrgHeader>();
			stmNums.SN_Type = type;
			stmNums.SN_Prefix = prefix;
			stmNums.SN_Owner = header.PK;

			AssertEquals(true, stmNums.CanDelete);
			AssertEquals("Range with prefix '1' has Matching Details setup against it.", stmNums.ReasonForNotAbleToDelete);

			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var matchingDetails = newFactory.NewWithValidTestData<StmNumberRangeMatchingDetail>();
			matchingDetails.NRM_RangeType = type;
			matchingDetails.NRM_Prefix = prefix;
			matchingDetails.NRM_OwnerId = header.PK;
			newFactory.Save();

			newFactory = new BusinessObjectFactory();
			var query = new ZQuery(ViewStmNumsSchema.SN_Name, stmNums.SN_Name);
			query.AddToFilter(ViewStmNumsSchema.SN_Owner, header.PK);
			stmNums = newFactory.LoadTop1<OrganisationViewStmNums>(query);
			var stmNumsCanDelete = (ICanDelete)stmNums;

			AssertEquals(false, stmNumsCanDelete.CanDelete);
			AssertEquals("Range with prefix '1' has Matching Details setup against it.", stmNumsCanDelete.ReasonForNotAbleToDelete);

			stmNums.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			AssertEquals(true, stmNumsCanDelete.CanDelete);

			stmNums.SN_Type = type;
			AssertEquals("Precondition", false, stmNumsCanDelete.CanDelete);

			stmNums.SN_Prefix = "ABCD";
			AssertEquals(true, stmNumsCanDelete.CanDelete);
		}

		public void TestCanRolloverSetToTrueIfTypeSetToSSCCBarCodeNumbers()
		{
			var stmNum = Factory.New<OrganisationViewStmNums>();
			AssertEquals("SN_CanRollover (deafult value)", false, stmNum.SN_CanRollover);

			stmNum.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			AssertEquals("SN_CanRollover (after changing type to SSCCBarCodeNumbers)", true, stmNum.SN_CanRollover);

			stmNum.SN_Type = "~1~";
			AssertEquals("SN_CanRollover (after changing type to ~1~)", false, stmNum.SN_CanRollover);
		}

		public void TestFTZControlNumberLength()
		{
			AssertEquals(8, OrganisationViewStmNums.Schema.FTZControlNumberLength);
			AssertEquals(5, OrganisationViewStmNums.Schema.FTZFormatDigits);
		}

		#region TestCalRequiredDigitForTransportReferenceNumbers

		public void TestCalRequiredDigitForTransportReferenceNumbers_OneDigit()
		{
			CalRequiredDigitForTransportReferenceNumbersCore(maxNumber: 9, expectedPadding: 0);
		}

		public void TestCalRequiredDigitForTransportReferenceNumbers_FiveDigit()
		{
			CalRequiredDigitForTransportReferenceNumbersCore(maxNumber: 12345, expectedPadding: 5 - 1);
		}

		public void TestCalRequiredDigitForTransportReferenceNumbers_MoreThanEight_Ten()
		{
			CalRequiredDigitForTransportReferenceNumbersCore(maxNumber: 1234567890, expectedPadding: 10 - 1);
		}

		void CalRequiredDigitForTransportReferenceNumbersCore(int maxNumber, int expectedPadding)
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				var header = Factory.NewWithValidTestData<OrgHeader>();
				var stmNum = Factory.New<OrganisationViewStmNums>();
				stmNum.SN_Type = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
				stmNum.SN_Prefix = "1234567";
				stmNum.SN_MaximumValue = maxNumber;
				stmNum.SN_Owner = header.PK;
				Factory.Save();

				var fountain = stmNum.TryGetNumberFountain();
				AssertNotNull("prerequisite", fountain);
				var expectedResultStartWith = $"1234567{new string('0', expectedPadding)}";
				AssertEquals($"{expectedResultStartWith}1", fountain.GetNextFormatted(Factory));
				AssertEquals($"{expectedResultStartWith}2", fountain.GetNextFormatted(Factory));

				Factory.Save();
				AssertEquals($"{expectedResultStartWith}3", fountain.GetNextFormatted(Factory));
			}
		}

		#endregion

		#region Implementation

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("StmNums cannot be loaded by PK", true);
		}

		#endregion
	}
}
