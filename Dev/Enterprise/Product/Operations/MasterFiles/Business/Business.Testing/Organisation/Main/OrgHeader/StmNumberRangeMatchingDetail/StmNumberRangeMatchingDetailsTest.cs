using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static CargoWise.EntityFramework.ColumnValueRanker;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StmNumberRangeMatchingDetail))]
	sealed class StmNumberRangeMatchingDetailsTest : EnterpriseBusinessObjectTestCase
	{
		#region TestNRM_MatchingKey

		public void TestNRM_MatchingKey()
		{
			var staff = Factory.New<GlbStaff>();
			var details = StmNumberRangeMatchingDetailsTest.CreateStmNumberRangeMatchingDetails(staff.Factory, staff.PK, OrgConstants.NumberFountains.Code.PatentNumber, "1111111", ownerTable: GlbStaffSchema.Constants.Prefix);
			details.NRM_MatchingKey = ZString.Empty;

			CombineAssertions(() =>
			{
				AssertEquals("NRM_MatchingKey should be Empty", true, details.NRM_MatchingKey.IsEmpty);
				AssertEquals("CustomsArea should be Empty", true, details.CustomsArea.IsEmpty);
				AssertEquals("PatentNumber should be Empty", true, details.PatentNumber.IsEmpty);
			});

			details.CustomsArea = "123";
			CombineAssertions(() =>
			{
				AssertEquals("NRM_MatchingKey should be", "|123", details.NRM_MatchingKey);
				AssertEquals("CustomsArea should be", "123", details.CustomsArea);
				AssertEquals("PatentNumber should be Empty", true, details.PatentNumber.IsEmpty);
			});

			details.PatentNumber = "ABC";
			CombineAssertions(() =>
			{
				AssertEquals("NRM_MatchingKey should be", "ABC|123", details.NRM_MatchingKey);
				AssertEquals("CustomsArea should be", "123", details.CustomsArea);
				AssertEquals("PatentNumber should be", "ABC", details.PatentNumber);
			});

			details.CustomsArea = ZString.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("NRM_MatchingKey should be", "ABC|", details.NRM_MatchingKey);
				AssertEquals("CustomsArea should be Empty", true, details.CustomsArea.IsEmpty);
				AssertEquals("PatentNumber should be", "ABC", details.PatentNumber);
			});
		}

		#endregion

		#region TestLinkedFountain

		public void TestLinkedFountain()
		{
			var stmNums = CreateViewStmNums(prefix: "1234567", min: 10L, max: 999L);

			AssertNotNull("should find the matched number fountain", CreateStmNumberRangeMatchingDetails(stmNums.Header, stmNums.SN_Type, stmNums.SN_Prefix).LinkedFountain);
			AssertNull("Prefix not match - should return null", CreateStmNumberRangeMatchingDetails(stmNums.Header, stmNums.SN_Type, "1111111").LinkedFountain);
			AssertNull("Type not match - should return null", CreateStmNumberRangeMatchingDetails(stmNums.Header, "AAA", stmNums.SN_Prefix).LinkedFountain);
			AssertNull("Owner not match - should return null", CreateStmNumberRangeMatchingDetails(ZGuid.NewZGuid(), stmNums.SN_Type, stmNums.SN_Prefix).LinkedFountain);
			AssertNull("Owner is empty - should return null", CreateStmNumberRangeMatchingDetails(ZGuid.Empty, stmNums.SN_Type, stmNums.SN_Prefix).LinkedFountain);
		}

		#endregion

		#region TestCurrentNumber

		public void TestCurrentNumber()
		{
			var stmNums = CreateViewStmNums(prefix: "1234567", min: 10L, max: 999L);

			AssertEquals("should find the matched number fountain", stmNums.SN_ValueForDisplay, CreateStmNumberRangeMatchingDetails(stmNums.Header, stmNums.SN_Type, stmNums.SN_Prefix).CurrentNumber);
			AssertEquals("Prefix not match - should return 0", ZLong.Zero, CreateStmNumberRangeMatchingDetails(stmNums.Header, stmNums.SN_Type, "1111111").CurrentNumber);
			AssertEquals("Type not match - should return 0", ZLong.Zero, CreateStmNumberRangeMatchingDetails(stmNums.Header, "AAA", stmNums.SN_Prefix).CurrentNumber);
			AssertEquals("Owner not match - should return 0", ZLong.Zero, CreateStmNumberRangeMatchingDetails(ZGuid.NewZGuid(), stmNums.SN_Type, stmNums.SN_Prefix).CurrentNumber);
			AssertEquals("Owner is empty - should return 0", ZLong.Zero, CreateStmNumberRangeMatchingDetails(ZGuid.Empty, stmNums.SN_Type, stmNums.SN_Prefix).CurrentNumber);

			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				stmNums.Header.OrgFountains.TryGetNumberFountain(stmNums.SN_Type, "1234567").GetNextFormatted(Factory); // just to make sure it's bind to ValueForDisplay
			}
			AssertEquals("should find the matched number fountain", stmNums.SN_ValueForDisplay, CreateStmNumberRangeMatchingDetails(stmNums.Header, stmNums.SN_Type, stmNums.SN_Prefix).CurrentNumber);
		}

		#endregion

		#region TestMinimumValue

		public void TestMinimumValue()
		{
			var stmNums = CreateViewStmNums(prefix: "1234567", min: 10L, max: 999L);

			AssertEquals("should find the matched number fountain", stmNums.SN_MinimumValue, CreateStmNumberRangeMatchingDetails(stmNums.Header, stmNums.SN_Type, stmNums.SN_Prefix).MinimumValue);
			AssertEquals("Prefix not match - should return 0", ZLong.Zero, CreateStmNumberRangeMatchingDetails(stmNums.Header, stmNums.SN_Type, "1111111").MinimumValue);
			AssertEquals("Type not match - should return 0", ZLong.Zero, CreateStmNumberRangeMatchingDetails(stmNums.Header, "AAA", stmNums.SN_Prefix).MinimumValue);
			AssertEquals("Owner not match - should return 0", ZLong.Zero, CreateStmNumberRangeMatchingDetails(ZGuid.NewZGuid(), stmNums.SN_Type, stmNums.SN_Prefix).MinimumValue);
			AssertEquals("Owner is empty - should return 0", ZLong.Zero, CreateStmNumberRangeMatchingDetails(ZGuid.Empty, stmNums.SN_Type, stmNums.SN_Prefix).MinimumValue);
		}

		#endregion

		#region TestMaximumValue

		public void TestMaximumValue()
		{
			var stmNums = CreateViewStmNums(prefix: "1234567", min: 10L, max: 999L);

			AssertEquals("should find the matched number fountain", stmNums.SN_MaximumValue, CreateStmNumberRangeMatchingDetails(stmNums.Header, stmNums.SN_Type, stmNums.SN_Prefix).MaximumValue);
			AssertEquals("Prefix not match - should return 0", ZLong.Zero, CreateStmNumberRangeMatchingDetails(stmNums.Header, stmNums.SN_Type, "1111111").MaximumValue);
			AssertEquals("Type not match - should return 0", ZLong.Zero, CreateStmNumberRangeMatchingDetails(stmNums.Header, "AAA", stmNums.SN_Prefix).MaximumValue);
			AssertEquals("Owner not match - should return 0", ZLong.Zero, CreateStmNumberRangeMatchingDetails(ZGuid.NewZGuid(), stmNums.SN_Type, stmNums.SN_Prefix).MaximumValue);
			AssertEquals("Owner is empty - should return 0", ZLong.Zero, CreateStmNumberRangeMatchingDetails(ZGuid.Empty, stmNums.SN_Type, stmNums.SN_Prefix).MaximumValue);
		}

		#endregion

		#region TestCanRollover

		public void TestCanRollover()
		{
			var stmNums = CreateViewStmNums(prefix: "1234567", min: 10L, max: 999L);

			AssertEquals("should find the matched number fountain", stmNums.SN_CanRollover, CreateStmNumberRangeMatchingDetails(stmNums.Header, stmNums.SN_Type, stmNums.SN_Prefix).CanRollover);
			AssertEquals("Prefix not match - should return 0", false, CreateStmNumberRangeMatchingDetails(stmNums.Header, stmNums.SN_Type, "1111111").CanRollover);
			AssertEquals("Type not match - should return 0", false, CreateStmNumberRangeMatchingDetails(stmNums.Header, "AAA", stmNums.SN_Prefix).CanRollover);
			AssertEquals("Owner not match - should return 0", false, CreateStmNumberRangeMatchingDetails(ZGuid.NewZGuid(), stmNums.SN_Type, stmNums.SN_Prefix).CanRollover);
			AssertEquals("Owner is empty - should return 0", false, CreateStmNumberRangeMatchingDetails(ZGuid.Empty, stmNums.SN_Type, stmNums.SN_Prefix).CanRollover);
		}

		#endregion

		#region TestOwner

		public void TestOwner()
		{
			var header = Factory.New<OrgHeader>();
			var collection = new StmNumberRangeMatchingDetailsCollection(header);
			var matchingDetails = collection.AddNew();
			AssertType<OrgHeader>(matchingDetails.Owner);
			AssertEquals(header.PK, matchingDetails.Owner.PK);

			var staff = Factory.New<GlbStaff>();
			collection = new StmNumberRangeMatchingDetailsCollection(staff);
			matchingDetails = collection.AddNew();
			AssertType<GlbStaff>(matchingDetails.Owner);
			AssertEquals(staff.PK, matchingDetails.Owner.PK);
		}

		#endregion

		#region IsTransportReferenceNumbers & IsPatentNumber

		public void TestIsTransportReferenceNumbersAndIsPatentNumber()
		{
			var matchingDetail = Factory.New<StmNumberRangeMatchingDetail>();
			matchingDetail.NRM_RangeType = OrgConstants.NumberFountains.Code.PatentNumber;
			AssertEquals("IsTransportReferenceNumbers should be FALSE", false, matchingDetail.IsTransportReferenceNumbers);
			AssertEquals("IsPatentNumber should be TRUE", true, matchingDetail.IsPatentNumber);

			matchingDetail.NRM_RangeType = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
			AssertEquals("IsTransportReferenceNumbers should be TRUE", true, matchingDetail.IsTransportReferenceNumbers);
			AssertEquals("IsPatentNumber should be FALSE", false, matchingDetail.IsPatentNumber);

			matchingDetail.NRM_RangeType = ZString.Empty;
			AssertEquals("IsTransportReferenceNumbers should be FALSE", false, matchingDetail.IsTransportReferenceNumbers);
			AssertEquals("IsPatentNumber should be FALSE", false, matchingDetail.IsPatentNumber);
		}

		#endregion

		#region TestGetMatchingNumberRange

		#region TestGetMatchingNumberRange_DifferentOrganization

		public void TestGetMatchingNumberRange_DifferentOrganization()
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				var org1 = Factory.New<OrgHeader>();
				org1.OH_Code = "Org1";
				var type = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
				AssertNull("Should not have any error if no match", StmNumberRangeMatchingDetail.GetMatchingNumberRange(org1, type));

				var stmNums1 = CreateViewStmNums<OrganisationViewStmNums>(org1, "1234567", min: 10L, max: 999L);
				AssertNull("Should not have any error if no match", StmNumberRangeMatchingDetail.GetMatchingNumberRange(org1, type));
				var matchingDetail1 = CreateStmNumberRangeMatchingDetails(stmNums1.Header, stmNums1.SN_Type, stmNums1.SN_Prefix);
				Factory.Save();

				AssertEquals("Should find existing match", matchingDetail1, StmNumberRangeMatchingDetail.GetMatchingNumberRange(org1, type));
				AssertEquals("1234567010", StmNumberRangeMatchingDetail.GetMatchingNumberRange(org1, type).LinkedFountain.TryGetNumberFountain().GetNextFormatted(Factory));

				var org2 = Factory.New<OrgHeader>();
				org2.OH_Code = "Org2";
				var stmNums2 = CreateViewStmNums<OrganisationViewStmNums>(org2, "8901234", min: 20L, max: 999L);
				AssertNull("Should not have any error if no match", StmNumberRangeMatchingDetail.GetMatchingNumberRange(org2, type));
				var matchingDetail2 = CreateStmNumberRangeMatchingDetails(stmNums2.Header, stmNums2.SN_Type, stmNums2.SN_Prefix);
				Factory.Save();

				AssertEquals("Should find existing match", matchingDetail2, StmNumberRangeMatchingDetail.GetMatchingNumberRange(org2, type));
				AssertEquals("8901234020", StmNumberRangeMatchingDetail.GetMatchingNumberRange(org2, type).LinkedFountain.TryGetNumberFountain().GetNextFormatted(Factory));

				AssertNull("Should not find match", StmNumberRangeMatchingDetail.GetMatchingNumberRange(org1, OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers));
				AssertNull("Should not find match", StmNumberRangeMatchingDetail.GetMatchingNumberRange(org2, OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers));
			}
		}
		#endregion

		#region TestGetMatchingNumberRange_Type

		public void TestGetMatchingNumberRange_TypeMatch()
		{
			GetMatchingNumberRange_TypeCore(OrgConstants.NumberFountains.Code.TransportReferenceNumbers, "1234567", expectedFindTheMatch: true);
		}

		public void TestGetMatchingNumberRange_TypeNotMatch()
		{
			Db.Connection.ExecuteNonQuery(@"ALTER Table StmNumberRangeMatchingDetail DROP CONSTRAINT Constraint_NRM_RangeType");
			GetMatchingNumberRange_TypeCore(OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers, "1234567", expectedFindTheMatch: false);
		}

		void GetMatchingNumberRange_TypeCore(string stmType, string prefix, bool expectedFindTheMatch)
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				var org = Factory.New<OrgHeader>();
				org.OH_Code = "Org1";

				var stmNums = org.Factory.New<OrganisationViewStmNums>();
				stmNums.SN_Type = stmType;
				stmNums.SN_Prefix = prefix;
				stmNums.SN_MinimumValue = 1L;
				stmNums.SN_MaximumValue = 999L;
				stmNums.SN_Owner = org.PK;
				stmNums.SN_Count = 998;
				Factory.Save();
				var matchingDetail = CreateStmNumberRangeMatchingDetails(org, stmType, prefix);
				Factory.Save();

				AssertEquals($"Should {(!expectedFindTheMatch ? "Not " : "")}find matching fountain.", expectedFindTheMatch, org.GetMatchingNumberRange(OrgConstants.NumberFountains.Code.TransportReferenceNumbers) != null);

				if (expectedFindTheMatch)
				{
					AssertEquals(prefix + "001", matchingDetail.LinkedFountain.TryGetNumberFountain().GetNextFormatted(Factory));
				}
			}
		}

		#endregion

		#region TestNoErrorWhenStmNumsNotExists

		public void TestNoErrorWhenStmNumsNotExists()
		{
			var type = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var fountain1 = org.GetMatchingNumberRange(type);
			AssertNull("Should not have error when not exists", fountain1);

			CreateViewStmNums<OrganisationViewStmNums>(org, "1111111"); // has StmNums but not match
			var fountain2 = org.GetMatchingNumberRange(type);
			AssertNull("Should not have error when not exists", fountain2);
		}

		#endregion

		#region TestGetMatchingNumberRange_DBHits

		public void TestGetMatchingNumberRange_DBHits()
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				var type = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
				var header = Factory.NewWithValidTestData<OrgHeader>();
				var stmNum = Factory.NewWithValidTestData<OrganisationViewStmNums>();
				stmNum.SN_Type = type;
				stmNum.SN_MinimumValue = 100;
				stmNum.SN_MaximumValue = 200;
				stmNum.SN_Count = 10;
				stmNum.SN_Owner = header.PK;
				Factory.Save();
				CreateStmNumberRangeMatchingDetails(stmNum.Header, stmNum.SN_Type, stmNum.SN_Prefix);
				Factory.Save();

				var query = new ZQuery(ViewStmNumsSchema.SN_Owner, header.PK);
				var newStmNum = NewFactory().LoadTop1<OrganisationViewStmNums>(query);

				AssertNotNull("Precondition", newStmNum);

				var foutain = stmNum.TryGetNumberFountain();
				AssertEquals("100", foutain.GetNextFormatted(Factory));
				AssertEquals("101", foutain.GetNextFormatted(Factory));

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var org = newFactory.Load<OrgHeader>(stmNum.SN_Owner);
				var matchingDetails = org.GetMatchingNumberRange(type);
				var foutainMatchingDetail = matchingDetails.LinkedFountain.TryGetNumberFountain();

				AssertEquals("102", matchingDetails.LinkedFountain.TryGetNumberFountain().GetNextFormatted(Factory));
				AssertEquals("103", foutain.GetNextFormatted(Factory));

				var expectedDBHits = new Dictionary<string, int>
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ ViewStmNumsSchema.Constants.TableName, 1 },
				{ StmNumberRangeMatchingDetailSchema.Constants.TableName, 1 }
			};

				AssertDbHits(expectedDBHits, newFactory);

				var matchingDetails2 = org.GetMatchingNumberRange(type);
				AssertEquals("104", matchingDetails2.LinkedFountain.TryGetNumberFountain().GetNextFormatted(Factory));

				// Should not have increased DbHits after calling TryGetNumberFountain a second time.
				AssertDbHits(expectedDBHits, newFactory);
			}
		}

		#endregion

		#region TestGetMatchingNumberRange

		public void TestGetMatchingNumberRange()
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				var type = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
				var client = helper.CreateClient("C1");
				var clientOther = helper.CreateClient("CO");
				var warehouse = helper.CreateWarehouse("whs", "A").PK;
				var warehouseOther = helper.CreateWarehouse("whso", "C").PK;

				var stmNums1 = CreateViewStmNums<OrganisationViewStmNums>(org, "1111111");
				var stmNums2 = CreateViewStmNums<OrganisationViewStmNums>(org, "2222222");
				var stmNums3 = CreateViewStmNums<OrganisationViewStmNums>(org, "3333333");
				var stmNums4 = CreateViewStmNums<OrganisationViewStmNums>(org, "4444444");
				CreateStmNumberRangeMatchingDetails(org, stmNums1.SN_Type, stmNums1.SN_Prefix, client: null, warehouse: null);
				CreateStmNumberRangeMatchingDetails(org, stmNums2.SN_Type, stmNums2.SN_Prefix, client: null, warehouse: warehouse);
				CreateStmNumberRangeMatchingDetails(org, stmNums3.SN_Type, stmNums3.SN_Prefix, client: client, warehouse: null);
				CreateStmNumberRangeMatchingDetails(org, stmNums4.SN_Type, stmNums4.SN_Prefix, client: client, warehouse: warehouse);
				Factory.Save();

				AssertEquals("Should find with no client and warehouse when no ranking condition",
					"111111100000001", StmNumberRangeMatchingDetail.GetMatchingNumberRange(org, type).LinkedFountain.TryGetNumberFountain().GetNextFormatted(Factory));

				var rankerMatchClient = new List<ColumnValuesPair>();
				rankerMatchClient.Add(StmNumberRangeMatchingDetail.MatchWithValueOrNull(StmNumberRangeMatchingDetailSchema.NRM_OH_Client, client));
				AssertEquals("333333300000001", StmNumberRangeMatchingDetail.GetMatchingNumberRange(org, type, rankerMatchClient).LinkedFountain.TryGetNumberFountain().GetNextFormatted(Factory));

				var rankerMatchWarehouse = new List<ColumnValuesPair>();
				rankerMatchWarehouse.Add(StmNumberRangeMatchingDetail.MatchWithValueOrNull(StmNumberRangeMatchingDetailSchema.NRM_WW_Whs, warehouse));
				AssertEquals("222222200000001", StmNumberRangeMatchingDetail.GetMatchingNumberRange(org, type, rankerMatchWarehouse).LinkedFountain.TryGetNumberFountain().GetNextFormatted(Factory));

				var rankerMatchClientWarehouse = new List<ColumnValuesPair>();
				rankerMatchClientWarehouse.Add(StmNumberRangeMatchingDetail.MatchWithValueOrNull(StmNumberRangeMatchingDetailSchema.NRM_OH_Client, client));
				rankerMatchClientWarehouse.Add(StmNumberRangeMatchingDetail.MatchWithValueOrNull(StmNumberRangeMatchingDetailSchema.NRM_WW_Whs, warehouse));
				AssertEquals("444444400000001", StmNumberRangeMatchingDetail.GetMatchingNumberRange(org, type, rankerMatchClientWarehouse).LinkedFountain.TryGetNumberFountain().GetNextFormatted(Factory));

				var rankerMatchClientNotMatchWarehouse = new List<ColumnValuesPair>();
				rankerMatchClientNotMatchWarehouse.Add(StmNumberRangeMatchingDetail.MatchWithValueOrNull(StmNumberRangeMatchingDetailSchema.NRM_OH_Client, client));
				rankerMatchClientNotMatchWarehouse.Add(StmNumberRangeMatchingDetail.MatchWithValueOrNull(StmNumberRangeMatchingDetailSchema.NRM_WW_Whs, warehouseOther));
				AssertEquals("333333300000002", StmNumberRangeMatchingDetail.GetMatchingNumberRange(org, type, rankerMatchClientNotMatchWarehouse).LinkedFountain.TryGetNumberFountain().GetNextFormatted(Factory));

				var rankerNotMatchClientMatchWarehouse = new List<ColumnValuesPair>();
				rankerNotMatchClientMatchWarehouse.Add(StmNumberRangeMatchingDetail.MatchWithValueOrNull(StmNumberRangeMatchingDetailSchema.NRM_OH_Client, clientOther));
				rankerNotMatchClientMatchWarehouse.Add(StmNumberRangeMatchingDetail.MatchWithValueOrNull(StmNumberRangeMatchingDetailSchema.NRM_WW_Whs, warehouse));
				AssertEquals("222222200000002", StmNumberRangeMatchingDetail.GetMatchingNumberRange(org, type, rankerNotMatchClientMatchWarehouse).LinkedFountain.TryGetNumberFountain().GetNextFormatted(Factory));

				var rankerNotMatchClientNotMatchWarehouse = new List<ColumnValuesPair>();
				rankerNotMatchClientNotMatchWarehouse.Add(StmNumberRangeMatchingDetail.MatchWithValueOrNull(StmNumberRangeMatchingDetailSchema.NRM_OH_Client, clientOther));
				rankerNotMatchClientNotMatchWarehouse.Add(StmNumberRangeMatchingDetail.MatchWithValueOrNull(StmNumberRangeMatchingDetailSchema.NRM_WW_Whs, warehouseOther));
				AssertEquals("111111100000002", StmNumberRangeMatchingDetail.GetMatchingNumberRange(org, type, rankerNotMatchClientNotMatchWarehouse).LinkedFountain.TryGetNumberFountain().GetNextFormatted(Factory));
			}
		}

		#endregion

		#region TestGetMatchingNumberRange_WithNoMatches

		public void TestGetMatchingNumberRange_WithNoMatches()
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				var type = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
				var client = helper.CreateClient("C1");
				var warehouse = helper.CreateWarehouse("whs", "A").PK;

				var stmNums1 = CreateViewStmNums<OrganisationViewStmNums>(org, "1111111");
				var stmNums2 = CreateViewStmNums<OrganisationViewStmNums>(org, "2222222");
				var matchingWarehouseNoClient = CreateStmNumberRangeMatchingDetails(org, stmNums1.SN_Type, stmNums1.SN_Prefix, client: null, warehouse: warehouse);
				var matchingClientNoWarehouse = CreateStmNumberRangeMatchingDetails(org, stmNums2.SN_Type, stmNums2.SN_Prefix, client: client, warehouse: null);
				Factory.Save();

				AssertNull("Should not return any of the MatchingDetails that do not match.", StmNumberRangeMatchingDetail.GetMatchingNumberRange(org, type));

				var stmNums3 = CreateViewStmNums<OrganisationViewStmNums>(org, "3333333");
				var matching = CreateStmNumberRangeMatchingDetails(org, stmNums3.SN_Type, stmNums3.SN_Prefix);
				Factory.Save();

				AssertEquals("333333300000001", StmNumberRangeMatchingDetail.GetMatchingNumberRange(org, type).LinkedFountain.TryGetNumberFountain().GetNextFormatted(Factory));
			}
		}

		#endregion

		#endregion

		#region TestGetRankColumnAndDefaultValues_ShouldSpecifyColumns

		public void TestGetRankColumnAndDefaultValues_ShouldSpecifyColumns()
		{
			var nonRankingFields = new List<string>
			{
					StmNumberRangeMatchingDetailSchema.Constants.NRM_OwnerId,
					StmNumberRangeMatchingDetailSchema.Constants.NRM_OwnerTableCode,
					StmNumberRangeMatchingDetailSchema.Constants.NRM_Prefix,
					StmNumberRangeMatchingDetailSchema.Constants.NRM_RangeType,
					StmNumberRangeMatchingDetailSchema.Constants.NRM_MatchingKey,
					StmNumberRangeMatchingDetailSchema.Constants.NRM_SystemCreateTimeUtc,
					StmNumberRangeMatchingDetailSchema.Constants.NRM_SystemCreateUser,
					StmNumberRangeMatchingDetailSchema.Constants.NRM_SystemLastEditTimeUtc,
					StmNumberRangeMatchingDetailSchema.Constants.NRM_SystemLastEditUser
			};

			var dummy = Factory.New<StmNumberRangeMatchingDetailsForTest>();
			var nonRankingFieldsResult = dummy.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(i => i.IsPersistent)
				.Select(propertyInfo => propertyInfo.Name)
				.Where(name => !dummy.GetRankColumnAndDefaultValues_Expose.Any(c => c.Key.Name == name));

			// NOTE: When adding to the ranking dictionary, please ensure TestGetMatchingNumberRange is extended to use your new column
			AssertContainsExactElementsInAnyOrder("Please specify new column should be in ranking dictionary or not.", nonRankingFields, nonRankingFieldsResult);
		}

		#endregion

		#region ReadOnly

		#region TestCurrentNumber_ReadOnly

		public void TestCurrentNumber_ReadOnly()
		{
			AssertHasCustomAttribute<ReadOnlyAttribute>(typeof(StmNumberRangeMatchingDetail), "CurrentNumber", false, readOnlyAtt => readOnlyAtt.IsReadOnly);
		}

		#endregion

		#region TestMinimumValue_ReadOnly

		public void TestMinimumValue_ReadOnly()
		{
			AssertHasCustomAttribute<ReadOnlyAttribute>(typeof(StmNumberRangeMatchingDetail), "MinimumValue", false, readOnlyAtt => readOnlyAtt.IsReadOnly);
		}

		#endregion

		#region TestMaximumValue_ReadOnly

		public void TestMaximumValue_ReadOnly()
		{
			AssertHasCustomAttribute<ReadOnlyAttribute>(typeof(StmNumberRangeMatchingDetail), "MaximumValue", false, readOnlyAtt => readOnlyAtt.IsReadOnly);
		}

		#endregion

		#region TestCanRollover_ReadOnly

		public void TestCanRollover_ReadOnly()
		{
			AssertHasCustomAttribute<ReadOnlyAttribute>(typeof(StmNumberRangeMatchingDetail), "CanRollover", false, readOnlyAtt => readOnlyAtt.IsReadOnly);
		}

		#endregion

		#region TestPrefixIsReadOnly

		public void TestPrefixIsReadOnly()
		{
			var matchingDetails = Factory.New<StmNumberRangeMatchingDetail>();
			AssertEquals(true, matchingDetails.NRM_PrefixInfo.ReadOnly);
			matchingDetails.NRM_RangeType = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
			AssertEquals(false, matchingDetails.NRM_PrefixInfo.ReadOnly);
		}

		#endregion

		#endregion

		#region ConcurrentSaves

		#region TestChangeOnOtherFactory

		[UseSnapshotProtection]
		public void TestChangeOnOtherFactory()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var stmNums1 = CreateViewStmNums<OrganisationViewStmNums>(org, "PREFIX_1");
			var stmNums2 = CreateViewStmNums<OrganisationViewStmNums>(org, "PREFIX_2");
			Factory.Save();

			var matchingDetails = Factory.NewWithValidTestData<StmNumberRangeMatchingDetail>();
			matchingDetails.NRM_OwnerId = org.PK;
			matchingDetails.NRM_RangeType = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
			matchingDetails.NRM_Prefix = "PREFIX_2";

			var lookups = new StmNumberRangeMatchingDetailLookups(matchingDetails);
			AssertContainsExactElementsInAnyOrder("Precondition", new ZString[] { "PREFIX_1", "PREFIX_2" }, org.Fountains.Select(f => f.SN_Prefix).Distinct());

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var stmNums2InNewFactory = newFactory.Load<OrganisationViewStmNums>(new ZQuery(ViewStmNumsSchema.SN_Name, stmNums2.SN_Name)).Single();
			stmNums2InNewFactory.Delete();
			newFactory.Save();

			var ex = AssertExceptionThrown<ZSaveException>(Factory.Save);
			AssertEquals("Type and Prefix does not exists in stmNums table.", ex.InnerException.InnerException.Message);
		}

		#endregion

		#region TestAnotherUserDeletesStmNums

		[UseSnapshotProtection]
		public void TestAnotherUserDeletesStmNums()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var stmNums = CreateViewStmNums<OrganisationViewStmNums>(org, "AAA");
			Factory.Save();

			var matchingDetails = Factory.NewWithValidTestData<StmNumberRangeMatchingDetail>();
			matchingDetails.NRM_OwnerId = org.PK;
			matchingDetails.NRM_RangeType = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
			matchingDetails.NRM_Prefix = "AAA";

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var stmNums2InNewFactory = newFactory.Load<OrganisationViewStmNums>(new ZQuery(ViewStmNumsSchema.SN_Name, stmNums.SN_Name)).Single();
			stmNums2InNewFactory.Delete();
			newFactory.Save();

			var ex = AssertExceptionThrown<ZSaveException>(Factory.Save);
			AssertEquals("Type and Prefix does not exists in stmNums table.", ex.InnerException.InnerException.Message);
		}

		#endregion

		#region TestUseCacheToValidateMatchingDetails

		public void TestUseCacheToValidateMatchingDetails()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var stmNumsA = CreateViewStmNums<OrganisationViewStmNums>(org, "AAA");
			var stmNumsB = CreateViewStmNums<OrganisationViewStmNums>(org, "BBB");
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("C1");
			Factory.Save();

			var matchingDetailsA = CreateStmNumberRangeMatchingDetails(org, stmNumsA.SN_Type, stmNumsA.SN_Prefix);
			var matchingDetailsB = CreateStmNumberRangeMatchingDetails(org, stmNumsB.SN_Type, stmNumsB.SN_Prefix);
			matchingDetailsB.NRM_OH_Client = client;
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var matchingDetailsInNewFactoryA = newFactory.Load<StmNumberRangeMatchingDetail>(matchingDetailsA.PK);
			var matchingDetailsInNewFactoryB = newFactory.Load<StmNumberRangeMatchingDetail>(matchingDetailsB.PK);
			matchingDetailsInNewFactoryA.RunPreSaveValidation();

			AssertEquals("no cache for first validation, it's hits DB once", 1, newFactory.GetTableHitCount(ViewStmNumsSchema.Constants.TableName));
			matchingDetailsInNewFactoryB.RunPreSaveValidation();
			AssertEquals("should use cache to validate , no more DB hits", 1, newFactory.GetTableHitCount(ViewStmNumsSchema.Constants.TableName));
		}

		#endregion

		#endregion

		#region Helper

		#region CreateStmNumberRangeMatchingDetails

		static StmNumberRangeMatchingDetail CreateStmNumberRangeMatchingDetails(OrgHeader org, ZString type, ZString prefix)
		{
			return CreateStmNumberRangeMatchingDetails(org.Factory, org.PK, type, prefix);
		}

		StmNumberRangeMatchingDetail CreateStmNumberRangeMatchingDetails(ZGuid orgPK, ZString type, ZString prefix)
		{
			return CreateStmNumberRangeMatchingDetails(Factory, orgPK, type, prefix);
		}

		public static StmNumberRangeMatchingDetail CreateStmNumberRangeMatchingDetails(BusinessObjectFactory factory, ZGuid orgPK, ZString type, ZString prefix, string ownerTable = OrgHeaderSchema.Constants.Prefix)
		{
			var matchingDetails = factory.NewWithValidTestData<StmNumberRangeMatchingDetail>();
			// populate with valid data
			matchingDetails.NRM_RangeType = type;
			matchingDetails.NRM_Prefix = prefix;
			matchingDetails.NRM_OwnerId = orgPK;
			matchingDetails.NRM_OwnerTableCode = ownerTable;
			return matchingDetails;
		}

		public static StmNumberRangeMatchingDetail CreateStmNumberRangeMatchingDetails(OrgHeader org, ZString type, ZString prefix, ZGuid? client, ZGuid? warehouse)
		{
			var matchingDetails = CreateStmNumberRangeMatchingDetails(org, type, prefix);
			if (client.HasValue)
			{
				matchingDetails.NRM_OH_Client = client.Value;
			}
			if (warehouse.HasValue)
			{
				matchingDetails.NRM_WW_Whs = warehouse.Value;
			}
			return matchingDetails;
		}

		#endregion

		#region CreateViewStmNums

		OrganisationViewStmNums CreateViewStmNums(string prefix, long min, long max)
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			return CreateViewStmNums<OrganisationViewStmNums>(header, prefix, min, max);
		}

		public static T CreateViewStmNums<T>(IViewStmNumsOwner businessObject, string prefix, long min = 1L, long max = 99999999L, string type = OrgConstants.NumberFountains.Code.TransportReferenceNumbers) where T : ViewStmNums
		{
			var stmNums = businessObject.Factory.New<T>();
			stmNums.SN_Type = type;
			stmNums.SN_Prefix = prefix;
			stmNums.SN_MinimumValue = min;
			stmNums.SN_MaximumValue = max;
			stmNums.SN_Owner = businessObject.PK;
			stmNums.SN_Count = max - min;

			businessObject.Factory.Save();

			AssertNotNull("Precondition", businessObject.Fountains.TryGetNumberFountain(type, prefix));
			return stmNums;
		}

		#endregion

		#endregion

		#region Implementation

		#region GetNewBusinessObjectForDeleteTest

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var prefix = "DeleteTest";
			var header = factory.NewWithValidTestData<OrgHeader>();
			var stmNums = factory.New<OrganisationViewStmNums>();
			stmNums.SN_Owner = header.PK;
			stmNums.SN_Type = "TRF";
			stmNums.SN_Prefix = prefix;
			factory.Save();

			var bizo = factory.NewWithValidTestData<StmNumberRangeMatchingDetail>();
			bizo.NRM_OwnerId = header.PK;
			bizo.NRM_Prefix = prefix;

			return bizo;
		}

		#endregion

		#region StmNumberRangeMatchingDetailsForTest

		public class StmNumberRangeMatchingDetailsForTest : StmNumberRangeMatchingDetail
		{
			public StmNumberRangeMatchingDetailsForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public Dictionary<SchemaGuidColumn, object> GetRankColumnAndDefaultValues_Expose
			{
				get { return GetRankColumnAndDefaultValues; }
			}
		}
		#endregion

		#endregion
	}
}
