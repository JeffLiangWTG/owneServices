using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class DpsResultsManagerTest : TestCaseWithFactory
	{
		public void TestSkipDpsLogsForHVLVConsignment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			var resultItems = new List<IDeniedPartyResultItemV4>();
			resultItems.Add(new DummyResultItem(ScreeningStatusesList.Codes.Unknown,
				ScreeningStatusesList.Codes.Clear,
				"",
				consignmentHeader.Consignments.AddNew(),
				new[] { shipment }));
			DpsResultsManager.CreateResultStatusAndRecordDecisions(Factory,
				resultItems,
				new List<ScreeningParty>(),
				new List<DpsSourceWithParties>() { new DpsSourceWithParties(shipment, Array.Empty<ScreeningParty>()) },
				false,
				"");

			var dpsLogCount = Factory.Load<StmEntityScreeningLog>(new ZQuery()).Length;

			AssertEquals("Should not create any StmEntityScreeningLog", 0, dpsLogCount);
		}

		public void TestCancelledPartiesNotIncludedForHVLVConsignment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignmentCancelled = consignmentHeader.Consignments.AddNew();
			var consignmentCancelledWithChildCleared = consignmentHeader.Consignments.AddNew();

			var dummyOrg = Factory.New<OrgHeader>();

			var resultItems = new List<IDeniedPartyResultItemV4>()
			{
				new DummyResultItem(ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Canceled, string.Empty, consignmentCancelled, Array.Empty<BusinessObject>()),
				new DummyResultItem(ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Clear, string.Empty, consignmentCancelled, Array.Empty<BusinessObject>()),
				new DummyResultItem(ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Canceled, string.Empty, consignmentCancelledWithChildCleared, Array.Empty<BusinessObject>()),
				new DummyResultItem(ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Clear, string.Empty, dummyOrg, new[] { consignmentCancelledWithChildCleared }),
			};

			var (resultStatuses, processedBizos) = DpsResultsManager.CreateResultStatusAndRecordDecisions(Factory,
				resultItems,
				new List<ScreeningParty>(),
				new List<DpsSourceWithParties>() { new DpsSourceWithParties(shipment, Array.Empty<ScreeningParty>()) },
				false,
				"");

			CombineAssertions(() =>
			{
				AssertEquals("No status changes on consignment", 0, resultStatuses.Count(s => s.ScreeningEntity.TablePrefix == HVLVConsignmentSchema.Constants.Prefix));
				AssertEquals("Consignment should not be processed", 0, processedBizos.Count(b => b.TablePrefix == HVLVConsignmentSchema.Constants.Prefix));
			});
		}

		public void TestProcessResults_NoFactory()
		{
			AssertExceptionThrown<ArgumentNullException>(() => DpsResultsManager.ProcessResults(null, null, false, null, false));
		}

		public void TestProcessResultsSaveLogsIntoDB()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var dpsResponse = new DpsResponse
			{
				ResponseCode = DpsResponseCode.Successful,
				ExtraMessage = "",
				AddressMatches = Enumerable.Empty<AddressMatchInfo>(),
				NameMatches = Enumerable.Empty<NameMatchInfo>(),
				RegistrationCodeMatches = Enumerable.Empty<RegistrationCodeMatchInfo>(),
				Profiles = Enumerable.Empty<ProfileHeaderInfo>(),
			};
			var response = new DpsResponseWithScreeningParty(new ScreeningParty(header, "test", header), dpsResponse, new DpsRequestHeaderWithAddressMatching());

			var statusCollection = new StmEntityScreeningLogCollection(header);
			AssertEquals("Precondition: ", false, statusCollection.Any());

			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(header, new[] { new ScreeningParty(header, header.HumanReadableName, header) })
			};

			DpsResultsManager.ProcessResults(new List<DpsResponseWithScreeningParty>() { response }, sourceBizOs, false, Factory, false);
			DpsLog.SaveWithExceptionHandler(Factory);

			AssertEquals(1, statusCollection.Count);
			AssertEquals(true, statusCollection[0].IsInDatabase);
		}

		public void TestProcessResultsDoNotSaveLogsIntoDB()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var dpsResponse = new DpsResponse
			{
				ResponseCode = DpsResponseCode.Successful,
				ExtraMessage = "",
				AddressMatches = Enumerable.Empty<AddressMatchInfo>(),
				NameMatches = Enumerable.Empty<NameMatchInfo>(),
				RegistrationCodeMatches = Enumerable.Empty<RegistrationCodeMatchInfo>(),
				Profiles = Enumerable.Empty<ProfileHeaderInfo>(),
			};
			var response = new DpsResponseWithScreeningParty(new ScreeningParty(header, "test", header), dpsResponse, new DpsRequestHeaderWithAddressMatching());

			var statusCollection = new StmEntityScreeningLogCollection(header);
			AssertEquals("Precondition: ", false, statusCollection.Any());

			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(header, new[] { new ScreeningParty(header, header.HumanReadableName, header) })
			};

			DpsResultsManager.ProcessResults(new List<DpsResponseWithScreeningParty>() { response }, sourceBizOs, false, Factory, false);

			AssertEquals(1, statusCollection.Count);
			AssertEquals(false, statusCollection[0].IsInDatabase);
		}

		public void TestCreateResultStatusAndRecordDecisionsDoNotSaveLogsIntoDB()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Test Org";

			var statusCollection = new StmEntityScreeningLogCollection(header);

			var resultItems = new List<IDeniedPartyResultItemV4>()
			{
				new DummyResultItem(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.PermanentClear, string.Empty, header, Array.Empty<BusinessObject>()),
			};

			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(header, new[] { new ScreeningParty(header, header.HumanReadableName, header) })
			};

			AssertEquals("Precondition: ", false, statusCollection.Any());
			DpsResultsManager.CreateResultStatusAndRecordDecisions(Factory, resultItems, new List<ScreeningParty>(), sourceBizOs, false, null);

			AssertEquals(1, statusCollection.Count);
			AssertEquals(false, statusCollection[0].IsInDatabase);
		}

		public void TestProcessResults_ResponseHasError()
		{
			var response = new DpsResponseWithScreeningParty(null, new DpsResponse { ResponseCode = DpsResponseCode.Exception, ExtraMessage = "Error1" }, new DpsRequestHeaderWithAddressMatching());
			var response2 = new DpsResponseWithScreeningParty(null, new DpsResponse { ResponseCode = DpsResponseCode.Failed, ExtraMessage = "Error2" }, new DpsRequestHeaderWithAddressMatching());
			DpsResultsManager.ProcessResults(new List<DpsResponseWithScreeningParty>() { response, response2 }, null, true, Factory, false);

			Assert(UnitTestUserNotification.Instance.LastMessage.Contains("Error1"));
			Assert(UnitTestUserNotification.Instance.LastMessage.Contains("Error2"));
		}

		public void TestProcessResults_ShowDpsResultForm()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();

			var profilePK = Guid.NewGuid();
			var nameMatches = new List<NameMatchInfo>
			{
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = DeniedPartyConstants.ScreeningNameTypes.Person, FullName = "ABC" }, MatchingNameID = Guid.NewGuid(), MatchingNameScore = 100, SourceProfileID = profilePK },
			};

			var profileHeader = new List<ProfileHeaderInfo>
			{
				new ProfileHeaderInfo
				{
					SourceProfileID = profilePK,
					ProfileNotes = Array.Empty<byte>(),
					ProfileNames = new List<ProfileNameInfo>
					{
						new ProfileNameInfo { ID = nameMatches[0].MatchingNameID, FullName = "ABC", Language = "EN", IsPrimaryName = true, SourceProfileID = profilePK },
					},
					SourceListCodes = new[] { "TESTDUMMY1" },
					TypeOfEntity = "PER"
				}
			};

			var dpsResponse = new DpsResponse { NameMatches = nameMatches, Profiles = profileHeader };
			var response = new DpsResponseWithScreeningParty(new ScreeningParty(header, "test", header), dpsResponse, new DpsRequestHeaderWithAddressMatching());

			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(header, new[] { new ScreeningParty(header, header.HumanReadableName, header) })
			};

			DpsResultsManager.ProcessResults(new List<DpsResponseWithScreeningParty>() { response }, sourceBizOs, true, Factory, false);
			AssertEquals("Show winform when disable WPF form is true", typeof(DpsResultWinform), ZFormModaliser.LastFormShownDialogForTest?.GetType());
		}

		public void TestProcessResults_ShowHVLVDpsResultForm_WhenScreeningPartyParentIsConsignment()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var response = CreateDpsResponseWithScreeningParty(consignment, header);

			var sourceBizOs = new List<DpsSourceWithParties>
			{
				new DpsSourceWithParties(consignment, new[] { new ScreeningParty(consignment, consignment.HumanReadableName, header) })
			};

			ProcessResultsAndAssert(response, sourceBizOs);
		}

		public void TestProcessResults_ShowHVLVDpsResultForm_WhenScreeningPartyParentIsBookingHeader()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var response = CreateDpsResponseWithScreeningParty(bookingHeader, header);

			var sourceBizOs = new List<DpsSourceWithParties>
			{
				new DpsSourceWithParties(bookingHeader, new[] { new ScreeningParty(bookingHeader, bookingHeader.HumanReadableName, header) })
			};

			ProcessResultsAndAssert(response, sourceBizOs);
		}

		public void TestProcessResults_ShowHVLVDpsResultForm_WhenScreeningPartyParentIsConsolContainsHVL()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;

			var header = Factory.NewWithValidTestData<OrgHeader>();
			var response = CreateDpsResponseWithScreeningParty(consol, header);

			var sourceBizOs = new List<DpsSourceWithParties>
			{
				new DpsSourceWithParties(consol, new[] { new ScreeningParty(consol, consol.HumanReadableName, header) })
			};

			ProcessResultsAndAssert(response, sourceBizOs);
		}

		DpsResponseWithScreeningParty CreateDpsResponseWithScreeningParty(BusinessObject entity, OrgHeader header)
		{
			var profilePK = Guid.NewGuid();
			var nameMatches = new List<NameMatchInfo>
			{
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = DeniedPartyConstants.ScreeningNameTypes.Person, FullName = "ABC" }, MatchingNameID = Guid.NewGuid(), MatchingNameScore = 100, SourceProfileID = profilePK },
			};

			var profileHeader = new List<ProfileHeaderInfo>
			{
				new ProfileHeaderInfo
				{
					SourceProfileID = profilePK,
					ProfileNotes = Array.Empty<byte>(),
					ProfileNames = new List<ProfileNameInfo>
					{
						new ProfileNameInfo { ID = nameMatches[0].MatchingNameID, FullName = "ABC", Language = "EN", IsPrimaryName = true, SourceProfileID = profilePK },
					},
					SourceListCodes = new[] { "TESTDUMMY1" },
					TypeOfEntity = "PER"
				}
			};

			var dpsResponse = new DpsResponse { NameMatches = nameMatches, Profiles = profileHeader };
			return new DpsResponseWithScreeningParty(new ScreeningParty(entity, "test", header), dpsResponse, new DpsRequestHeaderWithAddressMatching());
		}

		void ProcessResultsAndAssert(DpsResponseWithScreeningParty response, List<DpsSourceWithParties> sourceBizOs)
		{
			using (HVLVDataRegistry.Instance.HVLVEnablePartyScreening.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new HVLVEnablePartyScreening() { EnableNewDPSResultForm = true }))
			{
				DpsResultsManager.ProcessResults(new List<DpsResponseWithScreeningParty> { response }, sourceBizOs, true, Factory, false);
				AssertType(ObjectFactory.GetType<IHVLVDpsResultForm>(), ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestCreateStatusResultsAndRecordDecisions_DoNotUpdateParentsJCLStatusIfChildrenAreNotScreened()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_IsCreditor = true;
			header.OH_IsConsignee = true;
			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

			var shipment1 = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			shipment1[JobShipmentSchema.JS_UniqueConsignRef] = "S00001234";
			shipment1[JobShipmentSchema.JS_HouseBill] = "HOUSE1";
			shipment1[JobShipmentSchema.JS_OH_ExportBroker] = header.PK;
			shipment1[JobShipmentSchema.JS_ScreeningStatus] = ScreeningStatusesList.Codes.JobCleared;

			var shipment2 = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			shipment2[JobShipmentSchema.JS_UniqueConsignRef] = "S00001235";
			shipment2[JobShipmentSchema.JS_HouseBill] = "HOUSE2";
			shipment2[JobShipmentSchema.JS_OH_ExportBroker] = header.PK;
			shipment2[JobShipmentSchema.JS_ScreeningStatus] = ScreeningStatusesList.Codes.NotScreened;

			var consol1 = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			consol1[JobConsolSchema.JK_OH_Creditor] = header.PK;
			consol1[JobConsolSchema.JK_ScreeningStatus] = ScreeningStatusesList.Codes.JobCleared;

			var consol2 = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			consol2[JobConsolSchema.JK_OH_Creditor] = header.PK;
			consol2[JobConsolSchema.JK_ScreeningStatus] = ScreeningStatusesList.Codes.Clear;

			var declaration1 = (BusinessObject)Factory.New<Customs.IBaseJobDeclaration>();
			declaration1[JobDeclarationSchema.JE_OH_Consignee] = header.PK;
			declaration1[JobDeclarationSchema.JE_ScreeningStatus] = ScreeningStatusesList.Codes.JobCleared;

			var declaration2 = (BusinessObject)Factory.New<Customs.IBaseJobDeclaration>();
			declaration2[JobDeclarationSchema.JE_OH_Consignee] = header.PK;
			declaration2[JobDeclarationSchema.JE_ScreeningStatus] = ScreeningStatusesList.Codes.Unknown;

			var otherItems = new List<ScreeningParty>()
			{
				new ScreeningParty(shipment1, string.Empty, header),
				new ScreeningParty(shipment2, string.Empty, header),
				new ScreeningParty(consol1, string.Empty, header),
				new ScreeningParty(consol2, string.Empty, header),
				new ScreeningParty(declaration1, string.Empty, header),
				new ScreeningParty(declaration2, string.Empty, header),
			};

			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(header, new[] { new ScreeningParty(header, header.HumanReadableName, header) })
			};

			var resultStatuses = DpsResultsManager.CreateResultStatusAndRecordDecisions(Factory, new List<IDeniedPartyResultItemV4>(), otherItems, sourceBizOs, false, null).ResultStatuses;

			CombineAssertions(() =>
			{
				AssertEquals(3, resultStatuses.Count);
				AssertEquals(true, resultStatuses.Any(x => x.ScreeningEntity == shipment2 && x.Status == ScreeningStatusesList.Codes.Matched));
				AssertEquals(true, resultStatuses.Any(x => x.ScreeningEntity == consol2 && x.Status == ScreeningStatusesList.Codes.Matched));
				AssertEquals(true, resultStatuses.Any(x => x.ScreeningEntity == declaration2 && x.Status == ScreeningStatusesList.Codes.Matched));
			});
		}

		public void TestCreateStatusResultsAndRecordDecisions()
		{
			var header1 = Factory.NewWithValidTestData<OrgHeader>();
			var header2 = Factory.NewWithValidTestData<OrgHeader>();
			var header3 = Factory.NewWithValidTestData<OrgHeader>();
			var shipment3 = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			shipment3[JobShipmentSchema.JS_UniqueConsignRef] = "S00001285";
			shipment3[JobShipmentSchema.JS_HouseBill] = "HOUSE1";
			shipment3[JobShipmentSchema.JS_OH_ExportBroker] = header3.PK;

			var header4 = Factory.NewWithValidTestData<OrgHeader>();
			header4.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			var shipment4 = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			shipment4[JobShipmentSchema.JS_UniqueConsignRef] = "S00001286";
			shipment4[JobShipmentSchema.JS_HouseBill] = "HOUSE2";
			shipment4[JobShipmentSchema.JS_OH_ExportBroker] = header4.PK;

			var resultItems = new List<IDeniedPartyResultItemV4>()
			{
				new DummyResultItem(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Canceled, string.Empty, header1, Array.Empty<BusinessObject>()),
				new DummyResultItem(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Clear, string.Empty, header2, Array.Empty<BusinessObject>()),
				new DummyResultItem(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Clear, string.Empty, header3, new [] { shipment3 }),
			};

			var otherItems = new List<ScreeningParty>() { new ScreeningParty(shipment4, string.Empty, header4) };

			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(shipment4, new[] { new ScreeningParty(shipment4, header1.HumanReadableName, header1) })
			};

			var result = DpsResultsManager.CreateResultStatusAndRecordDecisions(Factory, resultItems, otherItems, sourceBizOs, false, null);
			var bizObjects = result.BizObjects;
			var resultStatuses = result.ResultStatuses;
			CombineAssertions(() =>
			{
				AssertEquals(header2.PK, bizObjects.Single().PK);
				AssertEquals(4, resultStatuses.Count);
				AssertEquals(0, resultStatuses.Count(x => x.ScreeningEntity == header1 && x.Status == ScreeningStatusesList.Codes.Matched));
				AssertEquals(1, resultStatuses.Count(x => x.ScreeningEntity == header2 && x.Status == ScreeningStatusesList.Codes.Clear));
				AssertEquals(1, resultStatuses.Count(x => x.ScreeningEntity == header3 && x.Status == ScreeningStatusesList.Codes.Clear));
				AssertEquals(1, resultStatuses.Count(x => x.ScreeningEntity == shipment3 && x.Status == ScreeningStatusesList.Codes.Clear));
				AssertEquals(1, resultStatuses.Count(x => x.ScreeningEntity == shipment4 && x.Status == ScreeningStatusesList.Codes.PermanentClear));
			});
		}

		public void TestCreateStatusResultsAndRecordDecisions_WhenEntityIsNotOrgOrVessel()
		{
			var header1 = Factory.NewWithValidTestData<OrgHeader>();
			var header2 = Factory.NewWithValidTestData<RefVessel>();
			var header3 = Factory.NewWithValidTestData<AccTransactionHeader>();
			var header4 = Factory.NewWithValidTestData<OrgHeader>();

			header4.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			var shipment4 = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			shipment4[JobShipmentSchema.JS_UniqueConsignRef] = "S00001286";
			shipment4[JobShipmentSchema.JS_HouseBill] = "HOUSE2";
			shipment4[JobShipmentSchema.JS_OH_ExportBroker] = header4.PK;

			var resultItems = new List<IDeniedPartyResultItemV4>()
			{
				new DummyResultItem(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Clear,string.Empty,header1, Array.Empty<BusinessObject>()),
				new DummyResultItem(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Clear,string.Empty,header2, Array.Empty<BusinessObject>()),
				new DummyResultItem(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Clear,string.Empty,header3, Array.Empty<BusinessObject>())
			};

			var otherItems = new List<ScreeningParty>() { new ScreeningParty(shipment4, string.Empty, header4) };

			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(shipment4, new[] { new ScreeningParty(shipment4, header1.HumanReadableName, header1) })
			};

			var result = DpsResultsManager.CreateResultStatusAndRecordDecisions(Factory, resultItems, otherItems, sourceBizOs, false, null);
			var resultStatuses = result.ResultStatuses;

			var ediMessages = Factory.Load<DpsEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.DPSRequestMessage) { FetchOnlyFromLocalCache = true });
			AssertEquals(2, ediMessages.Length);

			var content1 = JsonConvert.DeserializeObject<DpsEDIMessageCandidatesContent>(ediMessages[0].EM_MessageData.ToUTF8());
			AssertNotNull(content1);
			AssertEquals(header1.PK, content1.ClientSpecifiedIdentifier);
			AssertEquals(OrgHeaderSchema.Constants.Prefix, content1.EntityType);

			var content2 = JsonConvert.DeserializeObject<DpsEDIMessageCandidatesContent>(ediMessages[1].EM_MessageData.ToUTF8());
			AssertNotNull(content2);
			AssertEquals(header2.PK, content2.ClientSpecifiedIdentifier);
			AssertEquals(RefVesselSchema.Constants.Prefix, content2.EntityType);

			CombineAssertions(() =>
			{
				AssertEquals(4, resultStatuses.Count);
				AssertEquals(1, resultStatuses.Count(x => x.ScreeningEntity == header1 && x.Status == ScreeningStatusesList.Codes.Clear));
				AssertEquals(1, resultStatuses.Count(x => x.ScreeningEntity == header2 && x.Status == ScreeningStatusesList.Codes.Clear));
				AssertEquals(1, resultStatuses.Count(x => x.ScreeningEntity == header3 && x.Status == ScreeningStatusesList.Codes.Clear));
				AssertEquals(1, resultStatuses.Count(x => x.ScreeningEntity == shipment4 && x.Status == ScreeningStatusesList.Codes.PermanentClear));
			});
		}

		public void TestCreateStatusResultsAndRecordDecisions_WhenScreeningResultIsCancelled()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var logStatus = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, header.PK)).FirstOrDefault();
			AssertEquals("Pre-Condition", null, logStatus);

			var resultItems = new List<IDeniedPartyResultItemV4>()
			{
				new DummyResultItem(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Canceled, string.Empty, header, Array.Empty<BusinessObject>()),
			};

			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(header, new[] { new ScreeningParty(header, header.HumanReadableName, header) })
			};

			var resultStatusAndDecisions = DpsResultsManager.CreateResultStatusAndRecordDecisions(Factory, resultItems, new List<ScreeningParty>(), sourceBizOs, false, string.Empty);
			AssertEquals(0, resultStatusAndDecisions.ResultStatuses.Count);

			logStatus = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, header.PK)).First();
			AssertEquals(DeniedPartyConstants.LogsScreeningStatus.ScreenedCanceled, logStatus.PJ_Status);
		}

		public void TestShowSummaryFormWhenAllPartiesClear_WhenHasResponse()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var dpsResponse = new DpsResponse
			{
				ResponseCode = DpsResponseCode.Successful,
				ExtraMessage = "",
				AddressMatches = Enumerable.Empty<AddressMatchInfo>(),
				NameMatches = Enumerable.Empty<NameMatchInfo>(),
				RegistrationCodeMatches = Enumerable.Empty<RegistrationCodeMatchInfo>(),
				Profiles = Enumerable.Empty<ProfileHeaderInfo>(),
			};
			var response = new DpsResponseWithScreeningParty(new ScreeningParty(header, "test", header), dpsResponse, new DpsRequestHeaderWithAddressMatching());

			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(header, new[] { new ScreeningParty(header, header.HumanReadableName, header) })
			};

			DpsResultsManager.ProcessResults(new List<DpsResponseWithScreeningParty>() { response }, sourceBizOs, true, Factory, false);
			Assert("When all parties clear, show message when standalone mode", UnitTestUserNotification.Instance.LastMessage.Contains("No denied party matching info."));

			DpsResultsManager.ProcessResults(new List<DpsResponseWithScreeningParty>() { response }, sourceBizOs, false, Factory, false);
			using (var lastShownForm = (ZForm)ZFormModaliser.LastFormShownDialogForTest)
			{
				AssertEquals("When all parties clear, show summary form when normal mode", lastShownForm.GetType(), typeof(SummaryForm));
			}
		}

		public void TestShowSummaryFormWhenAllPartiesClear_WhenNoResponse()
		{
			DpsResultsManager.ProcessResults(new List<DpsResponseWithScreeningParty>(), null, false, Factory, false);
			AssertNull("No message info when there is no response and normal mode", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNull("No form shown when there is no response and normal mode", (ZForm)ZFormModaliser.LastFormShownDialogForTest);

			DpsResultsManager.ProcessResults(new List<DpsResponseWithScreeningParty>(), null, true, Factory, false);
			Assert("Still show message info when there is no response and stand alone mode", UnitTestUserNotification.Instance.LastMessage.Contains("No denied party matching info."));
			AssertNull("No form shown when there is no response and stand alone mode", (ZForm)ZFormModaliser.LastFormShownDialogForTest);
		}

		public void TestShouldSaveEDIMessageAndDPSLogToDatabase()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var dpsResponse = new DpsResponse
			{
				ResponseCode = DpsResponseCode.Successful,
				ExtraMessage = "",
				AddressMatches = Enumerable.Empty<AddressMatchInfo>(),
				NameMatches = Enumerable.Empty<NameMatchInfo>(),
				RegistrationCodeMatches = Enumerable.Empty<RegistrationCodeMatchInfo>(),
				Profiles = Enumerable.Empty<ProfileHeaderInfo>(),
			};
			var response = new DpsResponseWithScreeningParty(new ScreeningParty(organization, "test", organization), dpsResponse, new DpsRequestHeaderWithAddressMatching());

			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(organization, new[] { new ScreeningParty(organization, organization.HumanReadableName, organization) })
			};

			var processResult = DpsResultsManager.ProcessResults(new List<DpsResponseWithScreeningParty>() { response }, sourceBizOs, false, Factory, false);
			Factory.Save();

			using (var lastShownForm = (ZForm)ZFormModaliser.LastFormShownDialogForTest)
			{
				AssertEquals("When all parties clear, show summary form when normal mode", lastShownForm.GetType(), typeof(SummaryForm));
			}

			var filter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.DPSRequestMessage);
			filter.AddToFilter(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.JDC);
			filter.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.ScreeningRequest);

			var ediMessages = Factory.Load<DpsEDIMessage>(filter);
			AssertEquals(1, ediMessages.Length);
			Assert(ediMessages[0].IsInDatabase);

			var content = JsonConvert.DeserializeObject<DpsEDIMessageCandidatesContent>(ediMessages[0].EM_MessageData.ToUTF8());
			AssertNotNull(content);
			AssertEquals(organization.PK, content.ClientSpecifiedIdentifier);
			AssertEquals(organization.TablePrefix, content.EntityType);
			AssertEquals("CLR", content.PersistentStatus);

			var logStatus = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, organization.PK)).FirstOrDefault();
			AssertEquals("DPC", logStatus.PJ_Status);
			Assert(logStatus.IsInDatabase);

			AssertEquals("Organization expected screening status is CLR", true, processResult.ResultStatuses.Any(x => x.ScreeningEntity == organization && x.Status == "CLR"));
		}

		public void TestLogsCredentialsOverrideForOrg()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Test Org";

			var statusCollection = new StmEntityScreeningLogCollection(header);

			var resultItems = new List<IDeniedPartyResultItemV4>()
			{
				new DummyResultItem(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.PermanentClear, string.Empty, header, Array.Empty<BusinessObject>()),
				new DummyResultItem(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Clear, string.Empty, header, Array.Empty<BusinessObject>()),
				new DummyResultItem(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Matched, string.Empty, header, Array.Empty<BusinessObject>()),
			};

			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(header, new[] { new ScreeningParty(header, header.HumanReadableName, header) })
			};

			AssertEquals(false, statusCollection.Any());
			DpsResultsManager.CreateResultStatusAndRecordDecisions(Factory, resultItems, new List<ScreeningParty>(), sourceBizOs, false, "TestUser");

			CombineAssertions(() =>
			{
				AssertEquals(3, statusCollection.Count);
				AssertEquals("Override Status to Permanent Clear", true, statusCollection.Any(s => s.PJ_MatchingData.Contains($"The Screening Status of Test Org was set to Permanent Clear by user {EnvProxy.Instance.CurrentUser.FullName} using override credentials TestUser on the")
																								&& s.PJ_MatchingData.Contains("The user confirmed that they understood the impact of this by entering 'I understand the Impact'.\r\n\r\nThis record will remain clear unless manually changed.")));
				AssertEquals("Override Status to Clear", true, statusCollection.Any(s => s.PJ_MatchingData.Contains($"The Screening Status of Test Org was set to Clear by user {EnvProxy.Instance.CurrentUser.FullName} using override credentials TestUser on the")
																								&& !s.PJ_MatchingData.Contains("The user confirmed that they understood")));
				AssertEquals("Override Status to Matched", true, statusCollection.Any(s => s.PJ_MatchingData.Contains($"The Screening Status of Test Org was set to Matched by user {EnvProxy.Instance.CurrentUser.FullName} using override credentials TestUser on the")
																								&& !s.PJ_MatchingData.Contains("The user confirmed that they understood")));
			});
		}

		public void TestLogsCredentialsOverrideForVessel()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Test Vessel";

			var statusCollection = new StmEntityScreeningLogCollection(vessel);

			var resultItems = new List<IDeniedPartyResultItemV4>()
			{
				new DummyResultItem(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Clear, string.Empty, vessel, Array.Empty<BusinessObject>()),
				new DummyResultItem(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Matched, string.Empty, vessel, Array.Empty<BusinessObject>()),
			};

			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(vessel, new[] { new ScreeningParty(vessel, vessel.HumanReadableName, vessel) })
			};

			AssertEquals(false, statusCollection.Any());
			DpsResultsManager.CreateResultStatusAndRecordDecisions(Factory, resultItems, new List<ScreeningParty>(), sourceBizOs, false, "TestUser");

			CombineAssertions(() =>
			{
				AssertEquals(2, statusCollection.Count);
				AssertEquals("Override Status to Clear", true, statusCollection.Any(s => s.PJ_MatchingData.Contains($"The Screening Status of Test Vessel was set to Clear by user {EnvProxy.Instance.CurrentUser.FullName} using override credentials TestUser on the")));
				AssertEquals("Override Status to Matched", true, statusCollection.Any(s => s.PJ_MatchingData.Contains($"The Screening Status of Test Vessel was set to Matched by user {EnvProxy.Instance.CurrentUser.FullName} using override credentials TestUser on the")));
			});
		}

		public void TestCreateResultStatusAndRecordDecisions_WhenScreenStatusIsMarkAsSanctioned_ExpectedLogStatusIsDMS()
		{
			TestCreateResultStatusAndRecordDecisionsHelper(ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Matched, DeniedPartyConstants.LogsScreeningStatus.ScreenedMarkAsSanctioned);
		}

		public void TestCreateResultStatusAndRecordDecisions_WhenScreenStatusIsCanceled_ExpectedLogStatusIsDPD()
		{
			TestCreateResultStatusAndRecordDecisionsHelper(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Canceled, DeniedPartyConstants.LogsScreeningStatus.ScreenedCanceled);
		}

		public void TestCreateResultStatusAndRecordDecisions_WhenScreenStatusIsClear_ExpectedLogStatusIsDPC()
		{
			TestCreateResultStatusAndRecordDecisionsHelper(ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Clear, DeniedPartyConstants.LogsScreeningStatus.ScreenedClear);
		}

		void TestCreateResultStatusAndRecordDecisionsHelper(string fromStatus, string toStatus, string exptectedStatus)
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "AU";
			var logCollection = new StmEntityScreeningLogCollection(country);
			AssertEquals("Precondition screening log collection should be empty", false, logCollection.Any());

			DpsResultsManager.CreateResultStatusAndRecordDecisions(Factory, new List<IDeniedPartyResultItemV4>()
			{
				new DummyResultItem(fromStatus, toStatus, string.Empty, country, Array.Empty<BusinessObject>())
			},
			new List<ScreeningParty>(),
			new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(country, new[] { new ScreeningParty(country, country.HumanReadableName, country) })
			}, false, "TestUser");

			CombineAssertions(() =>
			{
				AssertEquals(1, logCollection.Count);
				AssertEquals(exptectedStatus, logCollection[0].PJ_Status);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestConnection.ExecuteNonQuery("delete from dbo.RefComplianceList");
		}

		class DummyResultItem : IDeniedPartyResultItemV4
		{
			public DummyResultItem(string currentScreeningStatus, string newScreeningStatus, string fullClearingReason, BusinessObject screenedEntity, BusinessObject[] parents)
			{
				CurrentScreeningStatus = currentScreeningStatus;
				NewScreeningStatus = newScreeningStatus;
				FullClearingReason = fullClearingReason;
				ScreenedEntity = screenedEntity;
				Parents = parents;
			}

			public string FullClearingReason { get; }

			public BusinessObject ScreenedEntity { get; }

			public BusinessObject[] Parents { get; }

			public DpsRequestHeaderWithAddressMatching RequestHeaderWithAddressMatching => new DpsRequestHeaderWithAddressMatching { DpsNameCandidates = new List<DpsNameCandidate> { new DpsNameCandidate { NameType = "PER", FullName = "ABC" } } };

			public DpsResponse Response { get; set; } = new DpsResponse { NameMatches = new List<NameMatchInfo>(), AddressMatches = new List<AddressMatchInfo>(), RegistrationCodeMatches = new List<RegistrationCodeMatchInfo>(), CountryMatches = new List<CountryMatchInfo>(), Profiles = new List<ProfileHeaderInfo>() };

			public string NewScreeningStatus { get; }

			public string HighConfidenceResults => string.Empty;

			public string MediumConfidenceResults => string.Empty;

			public int LowConfidenceResultsCount => 0;

			public string CurrentScreeningStatus { get; }
		}
	}
}
