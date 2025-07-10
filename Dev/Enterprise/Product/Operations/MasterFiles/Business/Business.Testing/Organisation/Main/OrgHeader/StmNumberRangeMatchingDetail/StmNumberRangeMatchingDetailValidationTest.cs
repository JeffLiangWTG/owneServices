using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class StmNumberRangeMatchingDetailValidationTest : BusinessObjectValidationTestCase
	{
		#region TestValidateNRM_RangeType

		public void TestValidateNRM_RangeType()
		{
			//ADD CONSTRAINT [Constraint_NRM_RangeType] CHECK (NRM_RangeType = 'TRF')
			var stmNums = CreateStmNums(OrgConstants.NumberFountains.Code.TransportReferenceNumbers, "1234567");
			var matchingDetails = CreateMatchingDetailBasedOnStmNums(stmNums);

			matchingDetails.Validation.ValidateAll();
			AssertNoErrors("Precondition", matchingDetails.NRM_OwnerTableCodeInfo);
			AssertNoErrors("Precondition", matchingDetails.NRM_RangeTypeInfo);

			matchingDetails.NRM_RangeType = "HH";// invalid type
			AssertHasError(matchingDetails.NRM_RangeTypeInfo, "Enter a valid Range Type.");

			matchingDetails.NRM_RangeType = OrgConstants.NumberFountains.Code.TransportReferenceNumbers; // valid
			AssertNoErrors(matchingDetails.NRM_RangeTypeInfo);
		}

		#endregion

		#region TestValidateNRM_OwnerTableCode

		public void TestValidateNRM_OwnerTableCode()
		{
			//ADD CONSTRAINT [Constraint_NRM_OwnerTableCode] CHECK (NRM_OwnerTableCode = 'OH')
			var stmNums = CreateStmNums(OrgConstants.NumberFountains.Code.TransportReferenceNumbers, "1234567");
			var matchingDetails = CreateMatchingDetailBasedOnStmNums(stmNums);

			matchingDetails.Validation.ValidateAll();
			AssertNoErrors("Precondition", matchingDetails.NRM_OwnerTableCodeInfo);
			AssertNoErrors("Precondition", matchingDetails.NRM_RangeTypeInfo);

			matchingDetails.NRM_OwnerTableCode = "AAA"; // invalid code
			AssertHasError(matchingDetails.NRM_OwnerTableCodeInfo, "Please enter a valid owner table code.");

			matchingDetails.NRM_OwnerTableCode = "OH"; // valid
			AssertNoErrors(matchingDetails.NRM_OwnerTableCodeInfo);

			matchingDetails.NRM_OwnerTableCode = "GS"; // valid
			AssertNoErrors(matchingDetails.NRM_OwnerTableCodeInfo);
		}

		#endregion

		#region TestCheckCustomsArea

		public void TestCheckCustomsArea()
		{
			var helper = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.Universal.IUniversalReferenceTestDataHelper>("Universal.IUniversalReferenceTestDataHelper", Factory);
			helper.CreateNewOrGetExistingCusCodeType(Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Customs Facilities", Core.Constants.CountryCodes.Mexico);
			var item1 = helper.CreateNewOrGetExistingCusCodeList(Enterprise.Core.Constants.CountryCodes.Mexico, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "123", "Customs Facilities Test1", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			var item2 = helper.CreateNewOrGetExistingCusCodeList(Enterprise.Core.Constants.CountryCodes.Mexico, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "45", "Customs Facilities Test2", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			Factory.Save();

			var stmNumsStaff = CreateStmNums(OrgConstants.NumberFountains.Code.PatentNumber, "1234567");
			var matchingDetailsStaff = CreateMatchingDetailBasedOnStmNums(stmNumsStaff);

			matchingDetailsStaff.CustomsArea = "789";
			AssertHasErrorContaining(matchingDetailsStaff.CustomsAreaInfo, "Enter a valid Customs Area.");
			matchingDetailsStaff.CustomsArea = string.Empty;
			AssertHasErrorContaining(matchingDetailsStaff.CustomsAreaInfo, "Please enter a Customs Area.");
			matchingDetailsStaff.CustomsArea = "123";
			AssertNoNotifications(matchingDetailsStaff.CustomsAreaInfo);

			var stmNumsHeader = CreateStmNums(OrgConstants.NumberFountains.Code.TransportReferenceNumbers, "7654321");
			var matchingDetailsHeader = CreateMatchingDetailBasedOnStmNums(stmNumsHeader);

			matchingDetailsHeader.CustomsArea = "789";
			AssertNoNotifications(matchingDetailsHeader.CustomsAreaInfo);
			matchingDetailsHeader.CustomsArea = string.Empty;
			AssertNoNotifications(matchingDetailsHeader.CustomsAreaInfo);
		}

		#endregion

		#region TestRequiringALinkedFountain

		public void TestRequiringALinkedFountain()
		{
			var stmNums = CreateStmNums(OrgConstants.NumberFountains.Code.TransportReferenceNumbers, "1234567");

			AssertAllRelatedColumnsHasError(stmNums, false, (matchingDetails) => { }); // all links are valid (no error) 
			AssertAllRelatedColumnsHasError(stmNums, true, (matchingDetails) => { matchingDetails.NRM_Prefix = "PrefixNotMatch"; });
			AssertAllRelatedColumnsHasError(stmNums, true, (matchingDetails) => { matchingDetails.NRM_RangeType = "AAA"; });
			AssertAllRelatedColumnsHasError(stmNums, true, (matchingDetails) => { matchingDetails.NRM_OwnerId = ZGuid.NewZGuid(); });
			AssertAllRelatedColumnsHasError(stmNums, true, (matchingDetails) => { matchingDetails.NRM_OwnerId = ZGuid.Empty; });
			AssertAllRelatedColumnsHasError(stmNums, false, (matchingDetails) => { });
		}

		void AssertAllRelatedColumnsHasError(ViewStmNums stmNums, bool hasError, Action<StmNumberRangeMatchingDetail> changeLinkData)
		{
			var matchingDetails = CreateMatchingDetailBasedOnStmNums(stmNums);

			changeLinkData(matchingDetails);
			AssertContainError(matchingDetails.NRM_OwnerIdInfo, hasError);
			AssertContainError(matchingDetails.NRM_RangeTypeInfo, hasError);
			AssertContainError(matchingDetails.NRM_PrefixInfo, hasError);
			matchingDetails.Delete(); // clean up for next test
		}

		void AssertContainError(ZPropertyInfo column, bool hasError)
		{
			var expectedError = "Matching details have no number range.";
			if (hasError)
			{
				AssertHasError(column, expectedError);
			}
			else
			{
				AssertNoError(column, expectedError);
			}
		}
		#endregion

		#region TestCheckIsUniqueByMatchingDetails

		public void TestCheckIsUniqueByMatchingDetailsForPatentNumber()
		{
			//	NRM_OwnerId + NRM_RangeType + NRM_MatchingKey
			var stmNums1 = CreateStmNums(OrgConstants.NumberFountains.Code.PatentNumber, "1234567");
			var stmNums2 = CreateStmNums(OrgConstants.NumberFountains.Code.PatentNumber, "8901234");
			var matchingDetails = CreateMatchingDetailBasedOnStmNums(stmNums1);
			var otherMatchingDetailsPK = CreateMatchingDetailBasedOnStmNums(stmNums2).PK;

			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var owner1 = stmNums1.SN_Owner;
			var owner2 = stmNums2.SN_Owner;

			AssertEquals("Precondition", matchingDetails.NRM_OwnerId, owner1);
			AssertEquals("Precondition", matchingDetails.NRM_RangeType, OrgConstants.NumberFountains.Code.PatentNumber);
			matchingDetails.CustomsArea = "ABC";
			matchingDetails.PatentNumber = "123";

			Factory.Save();

			AssertHasUniqueValidationError(null, otherMatchingDetailsPK, owner1, OrgConstants.NumberFountains.Code.PatentNumber, null, null, "ABC", "123");
			AssertHasUniqueValidationError("no error - Patent Number is different", otherMatchingDetailsPK, owner1, OrgConstants.NumberFountains.Code.PatentNumber, null, null, "ABC", "456");
			AssertHasUniqueValidationError("no error - Customs Area is different", otherMatchingDetailsPK, owner1, OrgConstants.NumberFountains.Code.PatentNumber, null, null, "DEF", "123");
			AssertHasUniqueValidationError("no error - Range type is different", otherMatchingDetailsPK, owner1, "AAA", null, null, "ABC", "123");
			AssertHasUniqueValidationError("no error - Owner is different", otherMatchingDetailsPK, owner2, OrgConstants.NumberFountains.Code.PatentNumber, null, null, "ABC", "123");
		}

		public void TestCheckIsUniqueByMatchingDetailsForTransportReferenceNumbers()
		{
			//	NRM_OwnerId + NRM_RangeType + NRM_OH_Client + NRM_WW_Whs 
			var stmNums1 = CreateStmNums(OrgConstants.NumberFountains.Code.TransportReferenceNumbers, "1234567");
			var stmNums2 = CreateStmNums(OrgConstants.NumberFountains.Code.TransportReferenceNumbers, "8901234");
			var matchingDetails = CreateMatchingDetailBasedOnStmNums(stmNums1);
			var otherMatchingDetailsPK = CreateMatchingDetailBasedOnStmNums(stmNums2).PK;

			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var clientPK1 = helper.CreateClient("C1");
			var clientPK2 = helper.CreateClient("C2");
			var owner1 = stmNums1.SN_Owner;
			var owner2 = stmNums2.SN_Owner;
			var warehouse1 = helper.CreateWarehouse("Whs1", "A");
			var warehouse2 = helper.CreateWarehouse("Whs2", "A");
			matchingDetails.NRM_OH_Client = clientPK1;

			AssertEquals("Precondition", matchingDetails.NRM_OwnerId, owner1);
			AssertEquals("Precondition", matchingDetails.NRM_RangeType, OrgConstants.NumberFountains.Code.TransportReferenceNumbers);
			matchingDetails.NRM_OH_Client = clientPK1;
			matchingDetails.NRM_WW_Whs = warehouse1.PK;

			Factory.Save();

			AssertHasUniqueValidationError(null, otherMatchingDetailsPK, owner1, OrgConstants.NumberFountains.Code.TransportReferenceNumbers, clientPK1, warehouse1.PK, ZString.Empty, ZString.Empty);
			AssertHasUniqueValidationError("no error - warehouse is different", otherMatchingDetailsPK, owner1, OrgConstants.NumberFountains.Code.TransportReferenceNumbers, clientPK1, warehouse2.PK, ZString.Empty, ZString.Empty);
			AssertHasUniqueValidationError("no error - client is different", otherMatchingDetailsPK, owner1, OrgConstants.NumberFountains.Code.TransportReferenceNumbers, clientPK2, warehouse1.PK, ZString.Empty, ZString.Empty);
			AssertHasUniqueValidationError("no error - Range type is different", otherMatchingDetailsPK, owner1, "AAA", clientPK1, warehouse1.PK, ZString.Empty, ZString.Empty);
			AssertHasUniqueValidationError("no error - Owner is different", otherMatchingDetailsPK, owner2, OrgConstants.NumberFountains.Code.TransportReferenceNumbers, clientPK1, warehouse1.PK, ZString.Empty, ZString.Empty);
		}

		void AssertHasUniqueValidationError(string reasonWhyShouldNotHaveError, ZGuid otherMatchingDetailsPK, ZGuid owner, ZString rangeType, ZGuid? clientPK, ZGuid? warehousePK, ZString customsArea, ZString patentNumber)
		{
			AssertHasUniqueValidationErrorCore(reasonWhyShouldNotHaveError, otherMatchingDetailsPK, owner, rangeType, clientPK, warehousePK, customsArea, patentNumber, "Owner");
			AssertHasUniqueValidationErrorCore(reasonWhyShouldNotHaveError, otherMatchingDetailsPK, owner, rangeType, clientPK, warehousePK, customsArea, patentNumber, "RangeType");
			if (clientPK.HasValue && warehousePK.HasValue)
			{
				AssertHasUniqueValidationErrorCore(reasonWhyShouldNotHaveError, otherMatchingDetailsPK, owner, rangeType, clientPK, warehousePK, customsArea, patentNumber, "Warehouse");
				AssertHasUniqueValidationErrorCore(reasonWhyShouldNotHaveError, otherMatchingDetailsPK, owner, rangeType, clientPK, warehousePK, customsArea, patentNumber, "Client");
			}
			else
			{
				AssertHasUniqueValidationErrorCore(reasonWhyShouldNotHaveError, otherMatchingDetailsPK, owner, rangeType, clientPK, warehousePK, customsArea, patentNumber, "CustomsArea");
				AssertHasUniqueValidationErrorCore(reasonWhyShouldNotHaveError, otherMatchingDetailsPK, owner, rangeType, clientPK, warehousePK, customsArea, patentNumber, "PatentNumber");
			}
		}

		static void AssertHasUniqueValidationErrorCore(string reasonWhyShouldNotHaveError, ZGuid otherMatchingDetailsPK, ZGuid owner, ZString rangeType, ZGuid? clientPK, ZGuid? warehousePK, ZString customsArea, ZString patentNumber, string testPropertyName)
		{
			var newFactory = new BusinessObjectFactory { RefreshEnabled = true };

			var matchingDetails = newFactory.Load<StmNumberRangeMatchingDetail>(otherMatchingDetailsPK);
			newFactory.ResetDatabaseLoadCount();

			var expectedError = "The matching details have been duplicated and must be unique.";

			using (RowFactory.SetCachedTables())
			{
				matchingDetails.Factory.SuspendValidation();
				if (testPropertyName == "Owner")
				{ matchingDetails.NRM_OwnerId = ZGuid.Empty; }
				else
				{ matchingDetails.NRM_OwnerId = owner; }
				if (testPropertyName == "RangeType")
				{ matchingDetails.NRM_RangeType = string.Empty; }
				else
				{ matchingDetails.NRM_RangeType = rangeType; }
				if (clientPK.HasValue && warehousePK.HasValue)
				{
					if (testPropertyName == "Client")
					{ matchingDetails.NRM_OH_Client = ZGuid.Empty; }
					else
					{ matchingDetails.NRM_OH_Client = clientPK.Value; }
					if (testPropertyName == "Warehouse")
					{ matchingDetails.NRM_WW_Whs = ZGuid.Empty; }
					else
					{ matchingDetails.NRM_WW_Whs = warehousePK.Value; }
				}
				else
				{
					if (testPropertyName == "CustomsArea")
					{ matchingDetails.CustomsArea = ZString.Empty; }
					else
					{ matchingDetails.CustomsArea = customsArea; }
					if (testPropertyName == "PatentNumber")
					{ matchingDetails.PatentNumber = ZString.Empty; }
					else
					{ matchingDetails.PatentNumber = patentNumber; }
				}
				matchingDetails.Factory.ResumeValidation();

				if (testPropertyName == "Owner")
				{ matchingDetails.NRM_OwnerId = owner; }
				if (testPropertyName == "RangeType")
				{ matchingDetails.NRM_RangeType = rangeType; }

				if (clientPK.HasValue && warehousePK.HasValue)
				{
					if (testPropertyName == "Client")
					{ matchingDetails.NRM_OH_Client = clientPK.Value; }
					if (testPropertyName == "Warehouse")
					{ matchingDetails.NRM_WW_Whs = warehousePK.Value; }
				}
				else
				{
					if (testPropertyName == "CustomsArea")
					{ matchingDetails.CustomsArea = customsArea; }
					if (testPropertyName == "PatentNumber")
					{ matchingDetails.PatentNumber = patentNumber; }
				}
			}

			if (!string.IsNullOrEmpty(reasonWhyShouldNotHaveError))
			{
				CombineAssertions(reasonWhyShouldNotHaveError, () =>
				{
					AssertNoError("NRM_OwnerIdInfo", matchingDetails.NRM_OwnerIdInfo, expectedError);
					AssertNoError("NRM_RangeTypeInfo", matchingDetails.NRM_RangeTypeInfo, expectedError);
					AssertNoError("NRM_OH_ClientInfo", matchingDetails.NRM_OH_ClientInfo, expectedError);
					AssertNoError("NRM_WW_WhsInfo", matchingDetails.NRM_WW_WhsInfo, expectedError);
					AssertNoError("CustomsAreaInfo", matchingDetails.CustomsAreaInfo, expectedError);
					AssertNoError("PatentNumberInfo", matchingDetails.PatentNumberInfo, expectedError);
				});
			}
			else
			{
				CombineAssertions(() =>
				{
					AssertHasError("NRM_OwnerIdInfo", matchingDetails.NRM_OwnerIdInfo, expectedError);
					AssertHasError("NRM_RangeTypeInfo", matchingDetails.NRM_RangeTypeInfo, expectedError);
					if (clientPK.HasValue && warehousePK.HasValue)
					{
						AssertHasError("NRM_OH_ClientInfo", matchingDetails.NRM_OH_ClientInfo, expectedError);
						AssertHasError("NRM_WW_WhsInfo", matchingDetails.NRM_WW_WhsInfo, expectedError);
						AssertNoError("CustomsAreaInfo", matchingDetails.CustomsAreaInfo, expectedError);
						AssertNoError("PatentNumberInfo", matchingDetails.PatentNumberInfo, expectedError);
					}
					else
					{
						AssertNoError("NRM_OH_ClientInfo", matchingDetails.NRM_OH_ClientInfo, expectedError);
						AssertNoError("NRM_WW_WhsInfo", matchingDetails.NRM_WW_WhsInfo, expectedError);
						AssertHasError("CustomsAreaInfo", matchingDetails.CustomsAreaInfo, expectedError);
						AssertHasError("PatentNumberInfo", matchingDetails.PatentNumberInfo, expectedError);
					}
				});
			}
			if (clientPK.HasValue && warehousePK.HasValue)
			{
				var expectedDbHits = new Dictionary<string, int>
				{
					{ OrgHeaderSchema.Constants.TableName, 2 },
					{ StmNumberRangeMatchingDetailSchema.Constants.TableName, 1 },
					{ WhsWarehouseSchema.Constants.TableName, 1 },
					{ ViewStmNumsSchema.Constants.TableName, 1 }
				};
				AssertDbHits(expectedDbHits, newFactory);
			}
		}

		#endregion

		#region TestCheckPrefixIsUnique

		public void TestCheckPrefixIsUniqueForGlbStaff()
		{
			//	NRM_OwnerId+NRM_RangeType+NRM_Prefix
			var prefix1 = "1234567";
			var stmNums1 = CreateStmNums(OrgConstants.NumberFountains.Code.PatentNumber, prefix1);
			var stmNums2 = CreateStmNums(OrgConstants.NumberFountains.Code.PatentNumber, "8901234");
			var matchingDetails = CreateMatchingDetailBasedOnStmNums(stmNums1);
			var otherMatchingDetailsPK = CreateMatchingDetailBasedOnStmNums(stmNums2).PK;

			var owner1 = stmNums1.SN_Owner;
			var owner2 = stmNums2.SN_Owner;
			var rangeType1 = OrgConstants.NumberFountains.Code.PatentNumber;

			AssertEquals("Precondition", matchingDetails.NRM_OwnerId, owner1);
			AssertEquals("Precondition", matchingDetails.NRM_RangeType, rangeType1);
			matchingDetails.NRM_Prefix = prefix1;

			Factory.Save();

			AssertHasUniqueValidationError(null, otherMatchingDetailsPK, owner1, prefix1, rangeType1, isPatentNumber: true);
			AssertHasUniqueValidationError("no error - prefix is different", otherMatchingDetailsPK, owner1, "2222222", rangeType1, isPatentNumber: true);
			AssertHasUniqueValidationError("no error - Owner is different", otherMatchingDetailsPK, owner2, prefix1, rangeType1, isPatentNumber: true);
			AssertHasUniqueValidationError("no error - Range Type is different", otherMatchingDetailsPK, owner1, prefix1, OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers, isPatentNumber: true);
		}

		public void TestCheckPrefixIsUniqueForOrgHeader()
		{
			//	NRM_OwnerId+NRM_RangeType+NRM_Prefix
			var prefix1 = "1234567";
			var stmNums1 = CreateStmNums(OrgConstants.NumberFountains.Code.TransportReferenceNumbers, prefix1);
			var stmNums2 = CreateStmNums(OrgConstants.NumberFountains.Code.TransportReferenceNumbers, "8901234");
			var matchingDetails = CreateMatchingDetailBasedOnStmNums(stmNums1);
			var otherMatchingDetailsPK = CreateMatchingDetailBasedOnStmNums(stmNums2).PK;

			var owner1 = stmNums1.SN_Owner;
			var owner2 = stmNums2.SN_Owner;
			var rangeType1 = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;

			AssertEquals("Precondition", matchingDetails.NRM_OwnerId, owner1);
			AssertEquals("Precondition", matchingDetails.NRM_RangeType, rangeType1);
			matchingDetails.NRM_Prefix = prefix1;

			Factory.Save();

			AssertHasUniqueValidationError(null, otherMatchingDetailsPK, owner1, prefix1, rangeType1, isPatentNumber: false);
			AssertHasUniqueValidationError("no error - prefix is different", otherMatchingDetailsPK, owner1, "2222222", rangeType1, isPatentNumber: false);
			AssertHasUniqueValidationError("no error - Owner is different", otherMatchingDetailsPK, owner2, prefix1, rangeType1, isPatentNumber: false);
			AssertHasUniqueValidationError("no error - Range Type is different", otherMatchingDetailsPK, owner1, prefix1, OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers, isPatentNumber: false);
		}

		void AssertHasUniqueValidationError(string reasonWhyShouldNotHaveError, ZGuid otherMatchingDetailsPK, ZGuid owner, ZString prefix, ZString rangeType, bool isPatentNumber)
		{
			AssertHasUniqueValidationError(reasonWhyShouldNotHaveError, otherMatchingDetailsPK, owner, prefix, rangeType, "Owner", isPatentNumber);
			AssertHasUniqueValidationError(reasonWhyShouldNotHaveError, otherMatchingDetailsPK, owner, prefix, rangeType, "Prefix", isPatentNumber);
			AssertHasUniqueValidationError(reasonWhyShouldNotHaveError, otherMatchingDetailsPK, owner, prefix, rangeType, "RangeType", isPatentNumber);
		}
		void AssertHasUniqueValidationError(string reasonWhyShouldNotHaveError, ZGuid otherMatchingDetailsPK, ZGuid owner, ZString prefix, ZString rangeType, string testPropertyName, bool isPatentNumber)
		{
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var matchingDetails = newFactory.Load<StmNumberRangeMatchingDetail>(otherMatchingDetailsPK);
			newFactory.ResetDatabaseLoadCount();

			matchingDetails.Factory.SuspendValidation();
			if (testPropertyName == "Owner")
			{ matchingDetails.NRM_OwnerId = ZGuid.Empty; }
			else
			{ matchingDetails.NRM_OwnerId = owner; }
			if (testPropertyName == "Prefix")
			{ matchingDetails.NRM_Prefix = string.Empty; }
			else
			{ matchingDetails.NRM_Prefix = prefix; }
			if (testPropertyName == "RangeType")
			{ matchingDetails.NRM_RangeType = string.Empty; }
			else
			{ matchingDetails.NRM_RangeType = rangeType; }
			matchingDetails.Factory.ResumeValidation();

			if (testPropertyName == "Owner")
			{ matchingDetails.NRM_OwnerId = owner; }
			if (testPropertyName == "Prefix")
			{ matchingDetails.NRM_Prefix = prefix; }
			if (testPropertyName == "RangeType")
			{ matchingDetails.NRM_RangeType = rangeType; }

			var expectedError = "The Prefix must be unique for this Range Type.";
			matchingDetails.NRM_OwnerId = owner;
			matchingDetails.NRM_Prefix = prefix;
			if (!string.IsNullOrEmpty(reasonWhyShouldNotHaveError))
			{
				CombineAssertions(reasonWhyShouldNotHaveError, () =>
				{
					AssertNoError("NRM_OwnerIdInfo", matchingDetails.NRM_OwnerIdInfo, expectedError);
					AssertNoError("NRM_RangeTypeInfo", matchingDetails.NRM_RangeTypeInfo, expectedError);
					AssertNoError("NRM_PrefixInfo", matchingDetails.NRM_PrefixInfo, expectedError);
					AssertNoError("NRM_RangeTypeInfo", matchingDetails.NRM_RangeTypeInfo, expectedError);
				});
			}
			else
			{
				CombineAssertions(reasonWhyShouldNotHaveError, () =>
				{
					AssertHasError("NRM_OwnerIdInfo", matchingDetails.NRM_OwnerIdInfo, expectedError);
					AssertHasError("NRM_RangeTypeInfo", matchingDetails.NRM_RangeTypeInfo, expectedError);
					AssertHasError("NRM_PrefixInfo", matchingDetails.NRM_PrefixInfo, expectedError);
					AssertHasError("NRM_RangeTypeInfo", matchingDetails.NRM_RangeTypeInfo, expectedError);
				});
			}

			if (!isPatentNumber)
			{
				var expectedDbHits = new Dictionary<string, int>
				{
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ StmNumberRangeMatchingDetailSchema.Constants.TableName, 1 },
					{ ViewStmNumsSchema.Constants.TableName, 1 },
				};
				AssertDbHits(expectedDbHits, newFactory);
			}
		}

		#endregion

		#region TestValidateNRM_Prefix_ValidationBasedOnList

		public void TestValidateNRM_Prefix_ValidationBasedOnList()
		{
			var stmNums = CreateStmNums(OrgConstants.NumberFountains.Code.TransportReferenceNumbers, "1234567");
			var matchingDetails = CreateMatchingDetailBasedOnStmNums(stmNums);

			matchingDetails.NRM_Prefix = "1122334";// invalid Prefix
			AssertHasError(matchingDetails.NRM_PrefixInfo, "Enter a valid Prefix.");

			matchingDetails.NRM_Prefix = "1234567"; // valid
			AssertNoErrors(matchingDetails.NRM_PrefixInfo);
		}

		#endregion

		#region Helper

		#region CreateStmNums

		ViewStmNums CreateStmNums(ZString type, ZString prefix)
		{
			ViewStmNums stmNums;
			if (type == OrgConstants.NumberFountains.Code.TransportReferenceNumbers)
			{
				var header = Factory.NewWithValidTestData<OrgHeader>();
				stmNums = Factory.New<OrganisationViewStmNums>();
				stmNums.SN_Owner = header.PK;
			}
			else
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				stmNums = Factory.New<StaffViewStmNums>();
				stmNums.SN_Owner = staff.PK;
			}
			stmNums.SN_Type = type;
			stmNums.SN_Prefix = prefix;

			Factory.Save();

			return stmNums;
		}

		#endregion

		#region CreateMatchingDetailBasedOnStmNums

		StmNumberRangeMatchingDetail CreateMatchingDetailBasedOnStmNums(ViewStmNums stmNums)
		{
			var matchingDetails = Factory.New<StmNumberRangeMatchingDetail>();
			matchingDetails.NRM_RangeType = stmNums.SN_Type;
			matchingDetails.NRM_Prefix = stmNums.SN_Prefix;
			matchingDetails.NRM_OwnerTableCode = stmNums.Owner.TablePrefix;
			matchingDetails.NRM_OwnerId = stmNums.SN_Owner;
			return matchingDetails;
		}

		#endregion

		#region TestPrefixeIsMandatoryWhenThereIsNoEmptyPrefixInStmNums

		public void TestPrefixeIsMandatoryWhenThereIsNoEmptyPrefixInStmNums()
		{
			var newFactory = new BusinessObjectFactory { RefreshEnabled = true };
			var matchingDetails = newFactory.NewWithValidTestData<StmNumberRangeMatchingDetail>();
			matchingDetails.NRM_Prefix = "";
			matchingDetails.Validation.ValidateNRM_Prefix();
			AssertHasError("Prefix is Mandatory.", matchingDetails.NRM_PrefixInfo, "Please enter a Prefix.");

			var stmNumsFWA = CreateStmNums(OrgConstants.NumberFountains.Code.ForwardAirBillNumbers, "");
			matchingDetails.NRM_OwnerId = stmNumsFWA.SN_Owner;
			matchingDetails.Validation.ValidateNRM_Prefix();
			AssertHasError("Prefix is Mandatory.", matchingDetails.NRM_PrefixInfo, "Please enter a Prefix.");

			var stmNumsTRF = CreateStmNums(OrgConstants.NumberFountains.Code.TransportReferenceNumbers, "");
			matchingDetails.NRM_OwnerId = stmNumsTRF.SN_Owner;
			matchingDetails.Validation.ValidateNRM_Prefix();
			AssertNoError("stmNums has empty prefix and is valid.", matchingDetails.NRM_PrefixInfo, "Please enter a Prefix.");
		}

		#endregion

		#endregion
	}
}
