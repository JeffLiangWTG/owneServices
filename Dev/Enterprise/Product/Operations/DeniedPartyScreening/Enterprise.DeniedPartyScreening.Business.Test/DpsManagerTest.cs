using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using CargoWise.Async;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	public class DpsManagerTest : TestCaseWithFactory
	{
		public void TestScreen()
		{
			using (var testServer = new DeniedPartyScreeningHttpServiceForTest(false))
			{
				var dpsManager = new DpsManager();
				var result = AsyncTaskSynchronizer.Run(() => dpsManager.Screen(new DpsRequestHeaderWithAddressMatching(), null));

				AssertEquals(DpsResponseCode.Successful, result.ResponseCode);

				var requestHeaders = testServer.service.Headers;
				var authorizationHeader = requestHeaders["Authorization"].Split(' ');
				var authorizationToken = new AuthenticationHeaderValue(authorizationHeader[0], authorizationHeader[1]);

				AssertEquals(HMACSHA256Helper.GetComputedLicenceCode(GlbCompany.CurrentCompany.GetLicenceKeyIdentifier("-")), requestHeaders["LicenceCode"]);
				AssertEquals(Convert.ToBase64String(Encoding.UTF8.GetBytes(EnvProxy.Instance.CurrentUser.LoginName)), requestHeaders["UserNameBase64Encoded"]);
				AssertEquals("Bearer", authorizationToken.Scheme);
				AssertNotNullOrEmpty(authorizationToken.Parameter);
			}
		}

		public void TestScreenFailedWithRetry()
		{
			using (new DeniedPartyScreeningHttpServiceForTest(true))
			{
				var dpsManager = new DpsManager();
				var exception = AssertExceptionThrown<AggregateException>(() => _ = AsyncTaskSynchronizer.Run(() => dpsManager.Screen(new DpsRequestHeaderWithAddressMatching(), null)));
				AssertEquals(1, ((exception.InnerException as DpsCommunicationException)?.InnerException as AggregateException)?.InnerExceptions.Count);
			}
		}

		public void TestCreateLogsMatchingStatus()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var statusCollection = new StmEntityScreeningLogCollection(header);
			var list1 = InitList("TESTDUMMY1", true);
			var list2 = InitList("TESTDUMMY2", false);
			var list3 = InitList("TESTDUMMY3", false);
			var list4 = InitList("TESTDUMMY4", false);
			var list5 = InitList("TESTDUMMY5", false);
			var list6 = InitList("TESTDUMMY6", true);
			Factory.Save();

			var listCodes = new[] { "TESTDUMMY1", "TESTDUMMY2", "TESTDUMMY3", "TESTDUMMY4" };

			var response = new DpsResponse
			{
				Profiles = new List<ProfileHeaderInfo>
				{
					new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Array.Empty<byte>(), SourceListCodes = listCodes, TypeOfEntity = "PER" },
					new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Array.Empty<byte>(), SourceListCodes = listCodes.Take(2).ToArray(), TypeOfEntity = "PER" }
				}
			};

			AssertEquals(0, statusCollection.Count);

			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(header, new[] { new ScreeningParty(header, header.HumanReadableName, header) })
			};

			DpsLog.AddNew(Factory, DeniedPartyConstants.LogsScreeningStatus.MatchedDeniedParty, new DeniedPartyResultItemV4ForTest(header, response), sourceBizOs, false, null);
			Factory.Save();

			AssertEquals(1, statusCollection.Count);

			CombineAssertions(() =>
			{
				AssertEquals(EnvProxy.Instance.CurrentUser.Initials.Trim(), statusCollection[0].PJ_SystemCreateUser.ToString());
				AssertEquals(DeniedPartyConstants.LogsScreeningStatus.MatchedDeniedParty, statusCollection[0].PJ_Status);
				AssertEquals("Test Clearing Reason", statusCollection[0].PJ_ClearedReason);
				AssertEquals(false, statusCollection[0].PJ_IsForcedRescreen);
				AssertEquals(true, statusCollection[0].IsInDatabase);
				AssertEquals($"High Confidence Result:\r\nABC\r\n{DpsLog.LineBreak}\r\nMedium Confidence Result:\r\nEFG", statusCollection[0].PJ_MatchingData);
				AssertEquals("ABC", statusCollection[0].PJ_HighConfidenceResults);
				AssertEquals("EFG", statusCollection[0].PJ_MediumConfidenceResults);
				AssertEquals(0, statusCollection[0].PJ_LowConfidenceResultsCount);
				Assert(statusCollection[0].PJ_IncludedLists.Contains("4 INCLUDED"));
				Assert(statusCollection[0].PJ_IncludedLists.Contains(list2.RCL_ListCode + " - " + list2.RCL_ListCode));
				Assert(statusCollection[0].PJ_IncludedLists.Contains(list3.RCL_ListCode + " - " + list3.RCL_ListCode));
				Assert(statusCollection[0].PJ_IncludedLists.Contains(list4.RCL_ListCode + " - " + list4.RCL_ListCode));
				Assert(statusCollection[0].PJ_IncludedLists.Contains(list5.RCL_ListCode + " - " + list5.RCL_ListCode));
				Assert(statusCollection[0].PJ_ExcludedLists.Contains("2 EXCLUDED"));
				Assert(statusCollection[0].PJ_ExcludedLists.Contains(list1.RCL_ListCode + " - " + list1.RCL_ListCode));
				Assert(statusCollection[0].PJ_ExcludedLists.Contains(list6.RCL_ListCode + " - " + list6.RCL_ListCode));
			});
		}

		public void TestCreateLogsMatchingStatus_PermanentlyClear()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var statusCollection = new StmEntityScreeningLogCollection(header);
			Factory.Save();

			var listCodes = Array.Empty<string>();
			var response = new DpsResponse
			{
				Profiles = new List<ProfileHeaderInfo>
				{
					new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Array.Empty<byte>(), SourceListCodes = listCodes, TypeOfEntity = "PER" },
					new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Array.Empty<byte>(), SourceListCodes = listCodes.Take(2).ToArray(), TypeOfEntity = "PER" }
				}
			};

			AssertEquals(0, statusCollection.Count);

			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(header, new[] { new ScreeningParty(header, header.HumanReadableName, header) })
			};

			DpsLog.AddNew(Factory, DeniedPartyConstants.LogsScreeningStatus.ScreenedPermanentClear, new DeniedPartyResultItemV4ForTest(header, response), sourceBizOs, false, null);

			AssertEquals(1, statusCollection.Count);
			Assert(statusCollection[0].PJ_MatchingData.Contains($"The Screening Status of {header.OH_FullName} was set to Permanent Clear by user {EnvProxy.Instance.CurrentUser.FullName} on the"));
		}

		public void TestCreateLogContainsSourceInfoForOrganisation()
		{
			var header1 = Factory.NewWithValidTestData<OrgHeader>();
			var header2 = Factory.NewWithValidTestData<OrgHeader>();
			var statusCollection = new StmEntityScreeningLogCollection(header1);

			Factory.Save();

			AssertEquals(0, statusCollection.Count);

			var bizOWithPartiesOneItem = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(header1, new[] { new ScreeningParty(header1, header1.HumanReadableName, header1) })
			};

			var bizOWithPartiesTwoItems = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(header2, new[] { new ScreeningParty(header1, header1.HumanReadableName, header1) }),
				new DpsSourceWithParties(header1, new[] { new ScreeningParty(header1, header1.HumanReadableName, header1) })
			};

			AssertCreateLogContainsSourceInfo(header1.PK, header1.TablePrefix, bizOWithPartiesOneItem, false, header1, statusCollection);
			AssertCreateLogContainsSourceInfo(header1.PK, header1.TablePrefix, bizOWithPartiesTwoItems, false, header1, statusCollection);
		}

		public void TestCreateLogContainsSourceInfoForVessel()
		{
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			var statusCollection = new StmEntityScreeningLogCollection(vessel1);

			Factory.Save();

			AssertEquals(0, statusCollection.Count);

			var bizOWithPartiesOneItem = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(vessel1, new[] { new ScreeningParty(vessel1, vessel1.HumanReadableName, vessel1) })
			};

			var bizOWithPartiesTwoItems = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(vessel2, new[] { new ScreeningParty(vessel1, vessel1.HumanReadableName, vessel1) }),
				new DpsSourceWithParties(vessel1, new[] { new ScreeningParty(vessel1, vessel1.HumanReadableName, vessel1) })
			};

			AssertCreateLogContainsSourceInfo(vessel1.PK, vessel1.TablePrefix, bizOWithPartiesOneItem, false, vessel1, statusCollection);
			AssertCreateLogContainsSourceInfo(vessel1.PK, vessel1.TablePrefix, bizOWithPartiesTwoItems, false, vessel1, statusCollection);
		}

		public void TestCreateLogContainsSourceInfoForShipment()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_OH_ExportBroker = header.PK;
			var statusCollection = new StmEntityScreeningLogCollection(header);

			Factory.Save();

			var bizOWithOneShipment = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(shipment, new[] { new ScreeningParty(header, header.HumanReadableName, header) })
			};

			AssertCreateLogContainsSourceInfo(shipment.PK, shipment.TablePrefix, bizOWithOneShipment, true, header, statusCollection);
		}

		public void TestCreateLogContainsSourceInfoForConsolidation()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var jobConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			jobConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			jobConsol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			jobConsol.JK_OA_ShippingLineAddress = header.MainAddress.PK;
			jobConsol.JK_OA_CreditorAddress = header.MainAddress.PK;
			var statusCollection = new StmEntityScreeningLogCollection(header);

			Factory.Save();

			var bizOWithOneConsol = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(jobConsol, new[] { new ScreeningParty(header, header.HumanReadableName, header) })
			};

			AssertCreateLogContainsSourceInfo(jobConsol.PK, jobConsol.TablePrefix, bizOWithOneConsol, true, header, statusCollection);
		}

		public void TestCreateLogContainsSourceInfoForDeclaration()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_OH_ExportBroker = header.PK;
			var jobDeclaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			jobDeclaration[JobDeclarationSchema.JE_JS] = shipment.PK;
			jobDeclaration[JobDeclarationSchema.JE_OH_Supplier] = header.PK;
			var statusCollection = new StmEntityScreeningLogCollection(header);

			Factory.Save();

			var bizOWithOneDeclaration = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(jobDeclaration, new[] { new ScreeningParty(header, header.HumanReadableName, header) })
			};

			AssertCreateLogContainsSourceInfo(jobDeclaration.PK, jobDeclaration.TablePrefix, bizOWithOneDeclaration, true, header, statusCollection);
		}

		public void TestCreateLogContainsSourceInfoForWarehouse()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var warehouse = WhsTransactionTestHelperCreator.GetNewHelper(Factory).CreateWarehouse("WHS", "A");
			warehouse[WhsWarehouseSchema.WW_OA_WarehouseAddress] = header.MainAddress.PK;

			var warehouseOrder = (BusinessObject)Factory.New<IWhsOrder>();
			warehouseOrder[WhsDocketSchema.WD_OH_Client] = header.PK;
			warehouseOrder[WhsDocketSchema.WD_WW_Whs] = warehouse.PK;

			var statusCollection = new StmEntityScreeningLogCollection(header);

			Factory.Save();

			var bizOWithOneWarehouse = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(warehouseOrder, new[] { new ScreeningParty(header, header.HumanReadableName, header) })
			};

			AssertCreateLogContainsSourceInfo(warehouseOrder.PK, warehouseOrder.TablePrefix, bizOWithOneWarehouse, true, header, statusCollection);
		}

		void AssertCreateLogContainsSourceInfo(ZGuid sourceID, ZString sourceTableCode, List<DpsSourceWithParties> dpsSourceWithParties, bool isRescreen, BusinessObject screenedEntity, StmEntityScreeningLogCollection statusCollection)
		{
			DpsLog.AddNew(Factory, DeniedPartyConstants.LogsScreeningStatus.MatchedDeniedParty, new DeniedPartyResultItemV4ForTest(screenedEntity), dpsSourceWithParties, isRescreen, null);
			CombineAssertions(() =>
			{
				AssertEquals(sourceID, statusCollection.Last().PJ_SourceID);
				AssertEquals(sourceTableCode, statusCollection.Last().PJ_SourceTableCode);
				AssertEquals(isRescreen, statusCollection.Last().PJ_IsForcedRescreen);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestConnection.ExecuteNonQuery("delete from dbo.RefComplianceList");
		}

		RefComplianceList InitList(string code, bool isExcluded)
		{
			var complianceList = Factory.New<RefComplianceList>();
			complianceList.RCL_ListCode = code;
			complianceList.RCL_ListPublisher = code + "Publisher";
			complianceList.RCL_ListDescription = code + "Description";
			complianceList.RCL_ListType = code + "Type";
			complianceList.RCL_ListName = code;
			complianceList.RCL_IsActive = true;
			complianceList.RCL_IsExcluded = isExcluded;
			complianceList.RCL_LastUpdatedDate = new ZDate(2021, 6, 30);

			return complianceList;
		}
	}

	class DeniedPartyResultItemV4ForTest : IDeniedPartyResultItemV4
	{
		public DeniedPartyResultItemV4ForTest(BusinessObject screenedEntity, DpsResponse response = null)
		{
			ScreenedEntity = screenedEntity;
			Response = response ?? new DpsResponse();
		}

		public string FullClearingReason => "Test Clearing Reason";
		public BusinessObject ScreenedEntity { get; }
		public BusinessObject[] Parents { get; }
		public DpsRequestHeaderWithAddressMatching RequestHeaderWithAddressMatching => new DpsRequestHeaderWithAddressMatching();
		public DpsResponse Response { get; }

		public string HighConfidenceResults => "ABC";

		public string MediumConfidenceResults => "EFG";

		public int LowConfidenceResultsCount => 0;

		public string CurrentScreeningStatus => throw new NotImplementedException();

		public string NewScreeningStatus => throw new NotImplementedException();
	}
}
