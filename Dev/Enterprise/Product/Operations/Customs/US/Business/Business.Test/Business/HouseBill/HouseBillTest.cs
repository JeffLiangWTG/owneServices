using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using BillTypeList = Enterprise.Customs.Common.US.ISF.BillTypeList;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(Bill))]
	sealed class HouseBillTest : BaseHouseBillTest<Bill, JobDeclaration>
	{
		public void TestIControllerIDProviderMembers()
		{
			var dec = Factory.New<JobDeclaration>();
			var bill = dec.Bills.AddNew();
			IControllerIDProvider provider = bill;
			AssertEquals("ControllerID", ControllerIDs.Customs.JobDeclaration, provider.ControllerID);
			AssertEquals("BusinessObjectPK", dec.PK.ToGuid(), provider.BusinessObjectPK);
			var shipment = Factory.New<Freight.Forwarding.Business.ForwardingShipment>();
			dec.JE_JS = shipment.PK;
			AssertEquals("ControllerID", ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, provider.ControllerID);
			AssertEquals("BusinessObjectPK", dec.PK.ToGuid(), provider.BusinessObjectPK);
		}

		public void TestGetFromJobBranchCurrentTime()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "AAA";

			var branch1 = Factory.New<GlbBranch>();
			var branch2 = Factory.New<GlbBranch>();
			branch1.GB_Code = "CCC";
			branch1.GB_GC = company1.PK;
			branch1.GB_RL_NKHomePort = "USPHL";
			branch2.GB_Code = "DDD";
			branch2.GB_RL_NKHomePort = "USLAX";
			branch2.GB_GC = company1.PK;

			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "TTT";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			declaration.JE_MasterBill = "MB2424";

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Factory.Save();

			using (Env.SetTemporaryUserContext(company1.PK.ToGuid(), branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				declaration.JE_GB = branch1.PK;
				var bill = Declaration.Bills.AddNew();

				var localTimeBranch1 = ZDateTime.Now;
				var utcTimeNow = DateTime.UtcNow;
				var centralimeInfo = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
				var centralTimeZone = TimeZoneInfo.ConvertTimeFromUtc(utcTimeNow, centralimeInfo);
				AssertEquals("Branch1 Time Zone", centralTimeZone.TimeOfDay.Hours, localTimeBranch1.TimeOfDay.Hours);

				using (Env.SetTemporaryUserContext(company1.PK.ToGuid(), branch2.PK.ToGuid(), department.PK.ToGuid()))
				{
					var localTimeBranch2 = ZDateTime.Now;
					var utcTimeNow2 = DateTime.UtcNow;

					var log = bill.LogManager.AddALogIfNecessary("", ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal, new ImportMessageStatusList());
					AssertEquals(ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal, log.SL_Reference);

					var pacificTimeInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
					var pacificTimeZone = TimeZoneInfo.ConvertTimeFromUtc(utcTimeNow2, pacificTimeInfo);
					AssertEquals("Branch2 Time Zone", pacificTimeZone.TimeOfDay.Hours, localTimeBranch2.TimeOfDay.Hours);

					AssertEquals("Wrote by Job Branch Time", localTimeBranch1.TimeOfDay.Hours, log.SL_EventTime.TimeOfDay.Hours);
				}
			}
		}

		public void TestDeleteBillWillDeleteRelatedFDAPivots()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MasterBill = "MB3242";
			Bill bill = declaration.PrimaryMasterBill;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			FDA fda = invoiceLine.FDAs.AddNew();
			var fdaBillsAvailable = fda.BillsAvailable;
			AssertEquals(1, fdaBillsAvailable.Count);
			fdaBillsAvailable[0].IsForFDALine = true;
			AssertEquals(1, fda.BillsForFDALine.Count);
			FDARelatedBillsGenPivot relatedPivot = fda.BillsForFDALine[0];
			AssertEquals(bill, relatedPivot.Relation2Object);
			bill.Delete();
			AssertEquals(0, fda.BillsAvailable.Count);
			AssertEquals(0, fda.BillsForFDALine.Count);
			AssertEquals(true, relatedPivot.IsDeleted);
		}

		public void TestDefaultPackingInformation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_TotalNoOfPacks = 150;
			declaration.JE_TotalNoOfPacksPackType = "PK";
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill.CU_BillNum = "HB242";
			AssertEquals(150m, bill.CU_NoOfPacks);
			AssertEquals("PK", bill.CU_PackType);
		}

		public void TestRate()
		{
			AssertEquals("", Bill.Rate);
		}

		public void TestDuty()
		{
			AssertEquals("", Bill.Duty);
		}

		public void TestUSSeal()
		{
			AssertEquals("", Bill.USSeal);
		}

		public void TestDescriptionAndQtyOfMerchandise()
		{
			Bill.CU_NoOfPacks = 27.0000m;
			Bill.CU_PackType = "PCS";
			Declaration.JE_TotalNoOfPacks = 39;
			Declaration.JE_TotalNoOfPacksPackType = "CT";
			Declaration.JE_GoodsDescription = "GOODS DESCRIPTION";
			Bill childBill = Bill.ChildBills.AddNew();
			childBill.CU_BillNum = "HOUSE";
			childBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			childBill = Bill.ChildBills.AddNew();
			childBill.CU_BillNum = "HOUSE 1";
			childBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			AssertEquals("27 PCS\nGOODS DESCRIPTION\nHBL: HOUSE, HOUSE 1", Bill.DescriptionAndQtyOfMerchandise);
		}

		public void TestIMessageResponseNotificatorMembers()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "Z8";
			staff.GS_EmailAddress = "dummy@email.com";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var bill = declaration.Bills.AddNew();
			IMessageResponseNotificator notificator = bill;
			AssertEquals(ZString.Empty, notificator.GetFallbackEmailAddressRecipient());
			declaration.JE_GS_NKCusAgent = "Z8";
			AssertEquals("dummy@email.com", notificator.GetFallbackEmailAddressRecipient());
			var declaration2 = Factory.New<JobDeclaration>();
			bill.CU_JE = declaration2.PK;
			AssertEquals(ZString.Empty, notificator.GetFallbackEmailAddressRecipient());
		}

		public void TestICusAddInfoTypeSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			ICusAddInfoTypeSupporter supporter = bill;
			supporter.AssertType(typeof(ITDoc), CusAddInfoTypeAttribute.Codes.USITDoc);
			supporter.AssertType(typeof(ITAndSplitDetails), CusAddInfoTypeAttribute.Codes.USITNumber);
			supporter.AssertType(typeof(DispositionData), CusAddInfoTypeAttribute.Codes.USDisposition);
			supporter.AssertType(null, "ZZ!");
			bill.US_7512OpenArea = "HELLO";
			var itNumber = bill.ITAndSplitDetails.AddNew();
			itNumber.US_ITNumber = "IT234";
			var disposition = bill.DispositionCodes.AddNew();
			disposition.US_Code = "HD";
			Factory.Save();
			AssertEquals("HELLO", bill.US_7512OpenArea);
			var newFactory = new BusinessObjectFactory();
			var addInfo = newFactory.Load<CusAddInfo>(itNumber.PK);
			AssertEquals(typeof(ITAndSplitDetails), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(disposition.PK);
			AssertEquals(typeof(DispositionData), addInfo.GetType());
		}

		public void TestICusCodeDataTypeSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			ICusCodeDataTypeSupporter supporter = bill;
			supporter.AssertType(typeof(HouseBillRefNo), CusCodeDataTypeList.Codes.HouseBillRefNo);
			supporter.AssertType(null, "ZZ!");
			var refNo = bill.ReferenceNos.AddNew();
			refNo.CY_Code = "AD";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var codeData = Factory.Load<CusCodeData>(refNo.PK);
			AssertEquals(typeof(HouseBillRefNo), codeData.GetType());
		}

		public void TestDeleteHouseBillandEDIMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.US_EntryFilerCode = "ABC";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B89345789";
			EDIMessage message = Factory.New<EDIMessage>();
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.Messages.Add(message);
			houseBill.Delete();
			AssertEquals(EDIMessage.Status.Discarded, message.EM_Status);
		}

		public void TestICargoManifestStatusQueryData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.US_EntryFilerCode = "ABC";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B89345789";
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "M1";
			masterBill.US_UI_NKBillIssuerSCAC = "ABCD";
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "H1";
			ICargoManifestStatusQueryData queryData = masterBill;
			AssertEquals("ABCD", queryData.BillIssuerCode);
			AssertEquals("M1", queryData.BillNumber);
			queryData = houseBill;
			AssertEquals("Reference", "HB:H1 (MB:M1)", queryData.HumanFriendlyReference);
			AssertEquals("EntryOrInBondNumber", "", queryData.EntryOrInBondNumber);
			AssertEquals("ControllerID", ControllerIDs.Customs.JobDeclaration, queryData.ControllerID);
			AssertEquals("M1", queryData.MasterAirWayBillNumber);
			AssertEquals("H1", queryData.HouseAirWayBillNumber);
			AssertEquals("", queryData.BillIssuerCode);
			AssertEquals("H1", queryData.BillNumber);
			AssertEquals("JobReferenceNumber", "B89345789", queryData.JobReferenceNumber);
			AssertEquals("BusinessObjectPK", declaration.PK, queryData.BusinessObjectPK);
			var message = Factory.New<EDIMessage>();
			queryData.LinkToMessage(message);
			AssertEquals(true, declaration.Messages.Contains(message));
			AssertEquals(declaration, message.EM_LinkedObject);
			AssertEquals(CargoManifestQueryActionType.HAWB, queryData.QueryActionType);
			AssertEquals(JobDeclarationSchema.Constants.Prefix, queryData.TableCode);
			AssertEquals(CargoManifestQueryActionType.MAWB, ((ICargoManifestStatusQueryData)masterBill).QueryActionType);
			declaration.JE_TransportMode = declaration.TransportModeRoadCodeForTesting;
			AssertEquals(CargoManifestQueryActionType.BillOfLading, ((ICargoManifestStatusQueryData)masterBill).QueryActionType);
			AssertEquals(CargoManifestQueryActionType.HouseBill, queryData.QueryActionType);
		}

		public void TestTotalGoodsValueInLocalCurrency()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Bill masterBill = declaration.Bills.AddNew();
			masterBill.US_GoodsValueInLocalCurrency = 10000m;
			AssertEquals(10000m, masterBill.TotalGoodsValueInLocalCurrency);
			masterBill.US_GoodsValueInLocalCurrency = 0m;
			Bill houseBill = masterBill.ChildBills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.US_GoodsValueInLocalCurrency = 20000m;
			AssertEquals(20000m, masterBill.TotalGoodsValueInLocalCurrency);
			houseBill.US_GoodsValueInLocalCurrency = 0m;
			Bill subhouseBill = houseBill.ChildBills.AddNew();
			subhouseBill.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			subhouseBill.US_GoodsValueInLocalCurrency = 20000m;
			Bill subhouseBill2 = houseBill.ChildBills.AddNew();
			subhouseBill2.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			subhouseBill2.US_GoodsValueInLocalCurrency = 30000m;
			AssertEquals(50000m, masterBill.TotalGoodsValueInLocalCurrency);
		}

		public void TestMessageStatusForInBondIsClearedAfterCloned()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			Bill bill = declaration.Bills.AddNew();
			bill.CU_Status = "AAA";
			JobDeclaration clonedDec = (JobDeclaration)declaration.TemplateCopy();
			AssertEquals("Bills are not cloned on normal template copy", 0, clonedDec.Bills.Count);
			clonedDec = (JobDeclaration)declaration.GetNewRelatedDeclaration(Factory);
			AssertEquals("Status is cleared", ZString.Empty, clonedDec.Bills[0].CU_Status);
		}

		public void TestTotalManifestQuantity()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Bill masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			Bill houseBill = masterBill.ChildBills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			Bill subHouseBill1 = houseBill.ChildBills.AddNew();
			subHouseBill1.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouseBill1.CU_NoOfPacks = 2;
			subHouseBill1.CU_PackType = "AA";
			Bill subHouseBill2 = houseBill.ChildBills.AddNew();
			subHouseBill2.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouseBill2.CU_NoOfPacks = 4;
			AssertEquals("TotalManifestQuantity", 6, masterBill.TotalManifestQuantity);
			AssertEquals("UniqueManifestUQ", "AA", masterBill.UniqueManifestUQ);
			subHouseBill2.CU_PackType = "BB";
			AssertEquals("UniqueManifestUQ", AESUnitOfMeasureList.Codes.Pieces, masterBill.UniqueManifestUQ);
			subHouseBill2.CU_PackType = "";
			Bill houseBill2 = masterBill.ChildBills.AddNew();
			houseBill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			Bill subHouseBill3 = houseBill2.ChildBills.AddNew();
			subHouseBill3.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouseBill3.CU_PackType = "AA";
			Bill subHouseBill4 = houseBill.ChildBills.AddNew();
			subHouseBill4.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			AssertEquals("UniqueManifestUQ", "AA", masterBill.UniqueManifestUQ);
			subHouseBill4.CU_PackType = "BB";
			AssertEquals("UniqueManifestUQ", AESUnitOfMeasureList.Codes.Pieces, masterBill.UniqueManifestUQ);
			houseBill.CU_NoOfPacks = 9;
			houseBill.CU_PackType = "BB";
			AssertEquals("TotalManifestQuantity", 9, masterBill.TotalManifestQuantity);
			AssertEquals("UniqueManifestUQ", AESUnitOfMeasureList.Codes.Pieces, masterBill.UniqueManifestUQ);
			masterBill.CU_NoOfPacks = 10;
			masterBill.CU_PackType = "CC";
			AssertEquals("TotalManifestQuantity", 10, masterBill.TotalManifestQuantity);
			AssertEquals("UniqueManifestUQ", "CC", masterBill.UniqueManifestUQ);
		}

		public void TestIsThisChildTo()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Bill masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			Bill houseBill = masterBill.ChildBills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			Bill subHouseBill = houseBill.ChildBills.AddNew();
			subHouseBill.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			Bill masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			AssertEquals(false, masterBill.IsThisAChildOf(masterBill));
			AssertEquals(true, houseBill.IsThisAChildOf(masterBill));
			AssertEquals(true, subHouseBill.IsThisAChildOf(masterBill));
			AssertEquals(false, subHouseBill.IsThisAChildOf(masterBill2));
		}

		public void TestIMessageAttachee()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			Bill bill = declaration.Bills.AddNew();
			CusContainer container1 = declaration.CusContainers.AddNew();
			CusContainer container2 = declaration.CusContainers.AddNew();
			container1.PackingGroups.AddNew().CR_CU_HouseBill = bill.PK;
			container2.PackingGroups.AddNew().CR_CU_HouseBill = bill.PK;
			IMessageAttacheeInDeclaration msgAttachee = bill;
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_MasterBill = "08155555555";
			AssertEquals("HumanFriendlyReference", bill.CU_BillUniqueCode, msgAttachee.HumanFriendlyReference);
			AssertEquals(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK), msgAttachee.Branch);
			AssertEquals(bill.Messages, msgAttachee.Messages);
			AssertEquals(MessageAttacheeRecordType.MasterBillOfLading, msgAttachee.RecordType);
			bill.CU_Status = "BBB";
			AssertEquals("Status", "BBB", msgAttachee.MessageStatus);
			AssertEquals("Status cascaded from bill", "BBB", ((IMessageAttachee)container1).MessageStatus);
			AssertEquals("Status cascaded from bill", "BBB", ((IMessageAttachee)container2).MessageStatus);
			Bill bill2 = declaration.Bills.AddNew();
			Bill bill3 = declaration.Bills.AddNew();
			CusContainer container3 = declaration.CusContainers.AddNew();
			bill2.PackingGroups.AddNew().CR_CO_Container = container3.PK;
			bill3.PackingGroups.AddNew().CR_CO_Container = container3.PK;
			bill2.CU_Status = "CCC";
			AssertEquals("container status not affected by bill as the container3 associated with two bills", "", ((IMessageAttachee)container3).MessageStatus);
			IIMessageAttacheeWithDisposition dispositionData = bill;
			dispositionData.UpdateDispositionInformation("06", ZDateTime.Today.AddHours(-5));
			AssertEquals(1, bill.DispositionCodes.Count);
			AssertEquals("06", bill.DispositionCodes[0].US_Code);
			AssertEquals(ZDateTime.Today.AddHours(-5), bill.DispositionCodes[0].US_DispositionDate);
		}

		public void TestIInBondBillDetails()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			IInBondBillDetails billDetails = bill;
			AssertEquals(bill.ConsigneeAddress, billDetails.ConsigneeAddress);
			declaration.US_SchDLoading = "8888";
			AssertEquals("Foreign Lading Port", "8888", billDetails.ForeignLadingPortLocalCode);
			AssertEquals(bill.ForeignShipperAddress, billDetails.ForeignShipperAddress);
			bill.US_InBondQty = 20;
			AssertEquals(20, billDetails.InBondQuantity);
			bill.CU_NoOfPacks = 30;
			bill.CU_PackType = "CTN";
			AssertEquals(30, billDetails.ManifestQuantity);
			AssertEquals("CTN", billDetails.ManifestUQ);
			bill.US_UI_NKBillIssuerSCAC = "1";
			AssertEquals("1", billDetails.MasterBillIssuerSCAC);
			bill.CU_MasterBill = "56897845";
			AssertEquals(bill.CU_MasterBill, billDetails.MasterBillNumber);
			AssertEquals(bill.NotifyPartyAddress, billDetails.NotifyPartyAddress);
			bill.US_PreReceiptPlace = "Chicago";
			AssertEquals("Chicago", billDetails.PlaceOfPreReceipt);
			bill.US_PreviousITNo = "2356874";
			AssertEquals("2356874", billDetails.PreviousITNumber);
			bill.ReferenceNos.AddNew(ReferenceQualifierList.Codes.FEN, "BDH32423");
			List<IInBondBillReferenceNumber> refNumbers = new List<IInBondBillReferenceNumber>(billDetails.RefNumbers);
			AssertEquals(1, refNumbers.Count);
			AssertEquals(bill.ReferenceNos[0], refNumbers[0]);
			bill.US_SequenceNo = 2;
			AssertEquals("2", billDetails.SequenceNumber);
			bill.US_Volume = 80m;
			bill.US_VolumeUQ = Core.Constants.Volume.CubicFeet;
			AssertEquals(2m, billDetails.VolumeInWholeNumber);
			AssertEquals("CM", billDetails.VolumeUQ);
			bill.US_Weight = 0.030m;
			bill.US_WeightUQ = Core.Constants.Weight.Tonnes;
			AssertEquals(30m, billDetails.WeightInWholeNumber);
			AssertEquals(Core.Constants.Weight.Kilograms, billDetails.WeightUQ);
			bill.US_GoodsValueInLocalCurrency = 2000m;
			AssertEquals(2000m, billDetails.GoodsValueInLocalCurrency);
			bill.US_Weight = 46m;
			bill.US_WeightUQ = Core.Constants.Weight.Pounds;
			AssertEquals(21m, billDetails.WeightInWholeNumber);
			AssertEquals(Core.Constants.Weight.Kilograms, billDetails.WeightUQ);
			bill.US_Weight = 46m;
			bill.US_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals(46m, billDetails.WeightInWholeNumber);
			AssertEquals(Core.Constants.Weight.Kilograms, billDetails.WeightUQ);
		}

		public void TestIssuerCodesOfBills()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Bill masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.US_UI_NKBillIssuerSCAC = "AAAD";
			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.US_UI_NKBillIssuerSCAC = "AAAD";
			Bill subHouseBill = declaration.Bills.AddNew();
			subHouseBill.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouseBill.US_UI_NKBillIssuerSCAC = "AAAD";
			IBillDetails iBill = masterBill;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			Assert(!iBill.IsNonAMS);
			AssertEquals("", iBill.IssuerCodeOfMasterBillNumber);
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals("", iBill.IssuerCodeOfMasterBillNumber);
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			AssertEquals("AAAD", iBill.IssuerCodeOfMasterBillNumber);
			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			AssertEquals("", iBill.IssuerCodeOfMasterBillNumber);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("AAAD", iBill.IssuerCodeOfMasterBillNumber);
			declaration.JE_TransportMode = TransportModeCodes.Codes.AirContainer;
			AssertEquals("", iBill.IssuerCodeOfMasterBillNumber);
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertEquals("AAAD", iBill.IssuerCodeOfMasterBillNumber);
			iBill = houseBill;
			AssertEquals("AAAD", iBill.IssuerCodeOfHouseBillNumber);
			declaration.JE_TransportMode = TransportModeCodes.Codes.AirNonContainer;
			Assert(!iBill.IsNonAMS);
			AssertEquals("", iBill.IssuerCodeOfHouseBillNumber);
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals("", iBill.IssuerCodeOfHouseBillNumber);
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			AssertEquals("AAAD", iBill.IssuerCodeOfHouseBillNumber);
			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			AssertEquals("", iBill.IssuerCodeOfHouseBillNumber);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("AAAD", iBill.IssuerCodeOfHouseBillNumber);
			iBill = subHouseBill;
			AssertEquals("AAAD", iBill.IssuerCodeOfSubHouseBillNumber);
			declaration.JE_TransportMode = TransportModeCodes.Codes.AirNonContainer;
			AssertEquals("", iBill.IssuerCodeOfSubHouseBillNumber);
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals("", iBill.IssuerCodeOfSubHouseBillNumber);
			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			AssertEquals("", iBill.IssuerCodeOfSubHouseBillNumber);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("AAAD", iBill.IssuerCodeOfSubHouseBillNumber);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			Assert(iBill.IsNonAMS);
		}

		[TestDate(2015, 10, 30)]
		public void TestIConveyanceOrSplitDetails()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			Declaration.US_EnableCRL = true;
			Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			Declaration.US_UI_NKCarrierSCAC = "APLU";
			Declaration.JE_VoyageFlightNo = "AA12345";
			Declaration.JE_DateOfArrival = ZDateTime.Today;
			Declaration.US_PipelineName = "ABC12345";
			Bill.CU_NoOfPacks = 12345m;
			Bill.CU_PackType = "AA";
			IConveyanceOrSplitDetails conveyanceOrSplitDetails = Bill;
			AssertEquals("APLU", conveyanceOrSplitDetails.CarrierCode);
			AssertEquals("AA12345", conveyanceOrSplitDetails.FlightNumber);
			AssertEquals(ZDateTime.Today, conveyanceOrSplitDetails.ArrivalDate);
			AssertEquals(12345, conveyanceOrSplitDetails.Qty);
			AssertEquals("AA", conveyanceOrSplitDetails.UQ);
			AssertEquals("ABC12345", conveyanceOrSplitDetails.PipelineName);
			Declaration.US_UI_NKCarrierSCAC = "AA";
			Declaration.JE_VoyageFlightNo = "AA12345";
			Bill.CU_NoOfPacks = (ZDecimal)int.MaxValue + 10;
			AssertEquals("AA12345", conveyanceOrSplitDetails.FlightNumber);
			AssertEquals(0, conveyanceOrSplitDetails.Qty);
		}

		[TestDate(2015, 10, 30)]
		public void TestIConveyanceOrSplitDetailsCarrierCode()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			Declaration.US_EnableCRL = true;
			Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			Declaration.US_UI_NKCarrierSCAC = "APLU";
			Declaration.JE_VoyageFlightNo = "AA123";
			Declaration.JE_DateOfArrival = ZDateTime.Today;
			Declaration.US_PipelineName = "ABC123";
			Bill.CU_NoOfPacks = 12345m;
			Bill.CU_PackType = "AA";
			IConveyanceOrSplitDetails conveyanceOrSplitDetails = Bill;
			AssertEquals("APLU", conveyanceOrSplitDetails.CarrierCode);
			Declaration.US_UI_NKCarrierSCAC = "";
			AssertEquals(ZString.Empty, conveyanceOrSplitDetails.CarrierCode);
			Declaration.JE_TransportMode = TransportTypeList.Codes.Pedestrian;
			AssertEquals(Bill.Constants.N_A, conveyanceOrSplitDetails.CarrierCode);
		}

		public void TestIIssuerCodeOfBillNumberCode()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			Declaration.US_EnableCRL = true;
			Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			Declaration.JE_MasterBillIssuerSCAC = "APLU";
			Declaration.US_UI_NKCarrierSCAC = "APLU";
			Declaration.JE_VoyageFlightNo = "AA123";
			Declaration.JE_DateOfArrival = ZDateTime.Today;
			Declaration.US_PipelineName = "ABC123";
			Bill.CU_NoOfPacks = 12345m;
			Bill.CU_PackType = "AA";
			IBillDetails billDetails = Bill;
			AssertEquals("APLU", billDetails.IssuerCodeOfMasterBillNumber);
			Declaration.JE_MasterBillIssuerSCAC = "";
			AssertEquals(Bill.Constants.N_A, billDetails.IssuerCodeOfMasterBillNumber);
		}

		public void TestMasterBillNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.US_EntryFilerCode = "XJ5";
			var primaryMasterBill = declaration.Bills.AddNew();
			primaryMasterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			primaryMasterBill.US_UI_NKBillIssuerSCAC = "AAAD";
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.US_UI_NKBillIssuerSCAC = "AAAD";
			houseBill.CU_CU_ParentBill = primaryMasterBill.PK;
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.US_UI_NKBillIssuerSCAC = "AABC";
			masterBill.CU_BillNum = "master2";
			AssertEquals(primaryMasterBill, declaration.PrimaryMasterBill);
			IBillDetails iBill = primaryMasterBill;
			AssertEquals("", iBill.MasterBillNumber);
			declaration.AllocateEntryNumber("000001");
			AssertEquals("", iBill.MasterBillNumber);
			declaration.US_SchDEntry = "2704";
			AssertEquals("XJ5000001", iBill.MasterBillNumber);
			iBill = masterBill;
			AssertEquals("master2", iBill.MasterBillNumber);
		}

		public void TestMasterBillNumberPlaceholder()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.US_SchDEntry = "2704";
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			var primaryMasterBill = declaration.Bills.AddNew();
			primaryMasterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			primaryMasterBill.US_UI_NKBillIssuerSCAC = "AAAD";
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.US_UI_NKBillIssuerSCAC = "AABC";
			masterBill.CU_BillNum = "master2";
			AssertEquals(primaryMasterBill, declaration.PrimaryMasterBill);
			declaration.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Mexico;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var builder = new EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, true);
			var ensMessage = builder.PopulateMessage();
			AssertContains("22            MASTER2                             00000000           AABC       ", ensMessage.EM_MessageText);
			AssertContains("22            XJ5<E#PLCH>                         00000000           AAAD       ", ensMessage.EM_MessageText);
		}

		[TestDate(2018, 07, 10)]
		public void TestMasterBillNumberPlaceholderForStandaloneBCR()
		{
			USCustomsDataRegistry.Instance.EnableNCTAsDefaultContainerModeForAirOrTruckDeclaration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.US_SchDEntry = "2704";
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			declaration.JE_HouseBill = "HOUSETEST1";
			declaration.JE_TotalNoOfPacks = 12;
			declaration.JE_TotalNoOfPacksPackType = ABIUnitOfMeasureList.Codes.Pairs;
			declaration.JE_MasterBillIssuerSCAC = "APLU";
			AssertEquals("HOUSETEST1", declaration.PrimaryHouseBill.CU_BillNum);
			AssertEquals(declaration.PrimaryHouseBill.CU_CU_ParentBill, declaration.PrimaryMasterBill.PK);
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var builder = new BorderCargoReleaseMessageBuilder(declaration.ActiveEntryHeaders.CargoReleaseEntry, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains("B012704XJ5HN                                               <<MSGNO PLACEHOLDER>>"
				+ "01A2704XJ5<E#PLCH>              0               071018   APLU                   "
				+ "0M            XJ5<E#PLCH> HOUSETEST1              00000012PRS        APLU       "
				+ "02                                                                              "
				+ "Y  2704XJ5HN00003", message.EM_MessageText);
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration2.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration2.US_SchDEntry = "2704";
			declaration2.US_EntryFilerCode = "XJ5";
			declaration2.US_EnableCRL = true;
			declaration2.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			declaration2.JE_HouseBill = "HOUSETEST2";
			declaration2.JE_TotalNoOfPacks = 3;
			declaration2.JE_TotalNoOfPacksPackType = ABIUnitOfMeasureList.Codes.Pairs;
			declaration2.JE_MasterBillIssuerSCAC = "APLU";
			AssertEquals(declaration2.PrimaryHouseBill.CU_CU_ParentBill, declaration2.PrimaryMasterBill.PK);
			var houseBill2 = declaration2.Bills.AddNew();
			houseBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_BillNum = "house2fortest";
			declaration2.Invoices.AddNew();
			declaration2.InvoiceLines.AddNew();
			declaration2.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			builder = new BorderCargoReleaseMessageBuilder(declaration2.ActiveEntryHeaders.CargoReleaseEntry, UpdateActionCode.Add);
			message = builder.PopulateMessage();
			AssertContains("B012704XJ5HN                                               <<MSGNO PLACEHOLDER>>"
				+ "01A2704XJ5<E#PLCH>              0               071018   APLU                   "
				+ "0M            XJ5<E#PLCH> HOUSETEST2              00000003PRS        APLU       "
				+ "0M            XJ5<E#PLCH> HOUSE2FORTES            00000000           APLU       "
				+ "02                                                                              "
				+ "Y  2704XJ5HN00004", message.EM_MessageText);
		}

		public void TestISimplifiedEntryBillMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			var masterBill = declaration.Bills.CreatePrimaryBill(Customs.Business.BillTypeList.Codes.MasterBill);
			masterBill.CU_BillNum = "MB001";
			masterBill.US_UI_NKBillIssuerSCAC = "3C";
			var masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "MB002";
			var masterBill3 = declaration.Bills.AddNew();
			masterBill3.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill3.CU_BillNum = "MB003";
			var houseBill = masterBill3.ChildBills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "HB1";
			var subHouseBill = houseBill.ChildBills.AddNew();
			subHouseBill.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouseBill.CU_BillNum = "SubHB1";
			IBillDetails iBill = masterBill;
			AssertEquals("MB001", iBill.MasterBillNumber);
			iBill = masterBill2;
			AssertEquals("MB002", iBill.MasterBillNumber);
			iBill = masterBill3;
			AssertEquals("MB003", iBill.MasterBillNumber);
			iBill = houseBill;
			AssertEquals("HB1", iBill.HouseBillNumber);
			iBill = subHouseBill;
			AssertEquals("SubHB1", iBill.SubHouseBillNumber);
		}

		public void TestIDispositionCodeDateParentMembers()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSAirDispositionCode, "AMSAD", dataGrouping.ZZZ_DataGrouping);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Z1", "Z1 DESC", startDate, endDate);

			var codeType1 = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SO50RecordDispCode, "SO50RecordDispCode", dataGrouping.ZZZ_DataGrouping);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType1.ZZK_CodeType, "53", "CBP Hold", startDate, endDate);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var masterBill = declaration.Bills.CreatePrimaryBill(Customs.Business.BillTypeList.Codes.MasterBill);
			masterBill.CU_BillNum = "MB001";
			IDispositionCodeDateParent iDispositionParent = masterBill;
			AssertEquals(typeof(DispositionList), iDispositionParent.DispositionCodeDescriptionList.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			AssertEquals(typeof(DispositionList), iDispositionParent.DispositionCodeDescriptionList.GetType());
			AssertEquals("Export of in-bond - bill of lading/Manifest Hold CBP", iDispositionParent.DispositionCodeDescriptionList.GetDescriptionFromCode(DispositionList.Codes._51));
			var description = iDispositionParent.GetDispositionDescriptionBasedOnSource(BillDispositionSourceList.Codes.SO, SEBillProcessingResultList.CBPHold);
			AssertEquals("CBP Hold", description);
			description = iDispositionParent.GetDispositionDescriptionBasedOnSource(BillDispositionSourceList.Codes.CQ, "Z1");
			AssertEquals("Z1 DESC", description);
		}

		public void TestParentBillNumbersHouseBillNumbersContainerNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "0012365489";
			bill.US_UI_NKBillIssuerSCAC = "ABC";
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "HOUSEBILL1";
			houseBill.US_UI_NKBillIssuerSCAC = "ABC";
			houseBill.CU_CU_ParentBill = bill.PK;
			var subHouseBill = declaration.Bills.AddNew();
			subHouseBill.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouseBill.CU_BillNum = "SUBHOUSEBILL1";
			subHouseBill.US_UI_NKBillIssuerSCAC = "ABC";
			subHouseBill.CU_CU_ParentBill = houseBill.PK;
			AssertEquals("MB: ABC0012365489", bill.ParentBillNumbers);
			AssertEquals("MB: ABC0012365489", houseBill.ParentBillNumbers);
			AssertEquals("MB: ABC0012365489", subHouseBill.ParentBillNumbers);
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "OON1111111";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "OON2222222";
			houseBill.Containers.Add(container1);
			AssertEquals("HB: ABCHOUSEBILL1 SH: ABCSUBHOUSEBILL1", bill.HouseBillNumbers);
			AssertEquals("CNR: OON1111111", houseBill.ContainerNumbers);
			houseBill.Containers.Add(container2);
			AssertContains("OON1111111", houseBill.ContainerNumbers);
			AssertContains("OON2222222", houseBill.ContainerNumbers);
		}

		public void TestITNumberAndNoOfPacks()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_NoOfPacks = 12;
			AssertEquals(0, bill.ITAndSplitDetails.Count);
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.ITNumber = "1245789";
			AssertEquals(1, houseBill.ITAndSplitDetails.Count);
			AssertEquals("1245789", houseBill.ITNumber);
			AssertEquals("1245789", houseBill.ITAndSplitDetails[0].US_ITNumber);
			houseBill.CU_NoOfPacks = 46;
			AssertEquals(46, houseBill.ITAndSplitDetails[0].US_NoOfPacks);
			houseBill.ITAndSplitDetails.AddNew().US_ITNumber = "V1005006";
			AssertEquals(2, houseBill.ITAndSplitDetails.Count);
			AssertEquals(Bill.Constants.Multiple, houseBill.ITNumber);
		}

		public void TestEffectiveMasterBillIssuerSCAC()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			AssertEquals("Effective MasterBillIssuerSCAC", "", bill.EffectiveMasterBillIssuerSCAC);
			bill.US_UI_NKBillIssuerSCAC = "OTT1";
			AssertEquals("Effective MasterBillIssuerSCAC", "OTT1", bill.EffectiveMasterBillIssuerSCAC);
			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_CU_ParentBill = ZGuid.Empty;
			AssertEquals("Effective MasterBillIssuerSCAC", "", houseBill.EffectiveMasterBillIssuerSCAC);
			houseBill.CU_CU_ParentBill = bill.PK;
			AssertEquals("Effective MasterBillIssuerSCAC", "OTT1", houseBill.EffectiveMasterBillIssuerSCAC);
		}

		public void TestUS_UI_NKBillIssuerSCAC()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.US_UI_NKBillIssuerSCAC = "AAAD";
			AssertEquals("AAAD", declaration.US_UI_NKCarrierSCAC);
			bill.US_UI_NKBillIssuerSCAC = "FR";
			AssertEquals("FR", declaration.US_UI_NKCarrierSCAC);
		}

		public void TestJobDocAddresses()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Bill bill = declaration.Bills.AddNew();
			AssertEquals(DocAddressTypes.Codes.ImporterPickupDeliveryAddress, bill.ConsigneeAddress.E2_AddressType);
			AssertEquals(DocAddressTypes.Codes.ForeignShipperDocumentaryAddress, bill.ForeignShipperAddress.E2_AddressType);
			AssertEquals(DocAddressTypes.Codes.NotifyParty, bill.NotifyPartyAddress.E2_AddressType);
		}

		public void TestUS_SequenceNoReadOnly()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Bill bill = declaration.Bills.AddNew();
			AssertEquals(true, bill.US_SequenceNoInfo.ReadOnly);
		}

		public void TestWeightInKGAndVolumeInCM()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Bill bill = declaration.Bills.AddNew();
			bill.US_Weight = 1000m;
			bill.US_WeightUQ = Core.Constants.Weight.Tonnes;
			bill.US_Volume = 890m;
			bill.US_VolumeUQ = Core.Constants.Volume.CubicInches;
			AssertEquals(1000000m, bill.WeightInKG);
			AssertEquals(0.014584m, bill.VolumeInCM);
			bill.US_WeightUQ = Core.Constants.Weight.Tonnes.ToLower();
			bill.US_VolumeUQ = Core.Constants.Volume.CubicInches.ToLower();
			AssertEquals(1000000m, bill.WeightInKG);
			AssertEquals(0.014584m, bill.VolumeInCM);
		}

		[ExpectNoExceptions]
		public void TestConsigneeAddressNotThrowExceptionIfDeleted()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Bill houseBill = declaration.Bills.AddNew();
			AssertNotNull(houseBill.ConsigneeAddress);
			Factory.Save();
			houseBill.ConsigneeAddress.E2_City = "Hello City";
		}

		[ExpectNoExceptions]
		public void TestForeignShipperAddressNotThrowExceptionIfDeleted()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Bill houseBill = declaration.Bills.AddNew();
			AssertNotNull(houseBill.ForeignShipperAddress);
			Factory.Save();
			houseBill.ForeignShipperAddress.E2_City = "Hello City";
		}

		[ExpectNoExceptions]
		public void TestNotifyPartyAddressNotThrowExceptionIfDeleted()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Bill houseBill = declaration.Bills.AddNew();
			AssertNotNull(houseBill.NotifyPartyAddress);
			Factory.Save();
			houseBill.NotifyPartyAddress.E2_City = "Hello City";
		}

		public void TestSCACAndBillNumber()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "MB1111";
			AssertEquals("SCACAndBillNumber", "MB1111", bill.SCACAndBillNumber);
			bill.US_UI_NKBillIssuerSCAC = "OTT1";
			AssertEquals("SCACAndBillNumber", "OTT1MB1111", bill.SCACAndBillNumber);
		}

		public void TestSubHouseBillNumber()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			bill.CU_BillNum = "SubBill_1 234232";
			IBillDetails billDetails = bill;
			AssertEquals("Is SubHouse Bill", true, bill.IsSubHouseBill);
			AssertEquals("SubBill12342", billDetails.SubHouseBillNumber);
			declaration.Bills.RemoveAll();
			bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "MasterBill";
			billDetails = bill;
			AssertEquals("not SubHouse Bill", false, bill.IsSubHouseBill);
			AssertEquals(ZString.Empty, billDetails.SubHouseBillNumber);
		}

		public void TestEffectiveSubHouseBillIssuerSCAC()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			bill.CU_BillNum = "SHB_1";
			AssertEquals(ZString.Empty, bill.EffectiveSubHouseBillIssuerSCAC);
			bill.US_UI_NKBillIssuerSCAC = "OTT1";
			AssertEquals("EffectiveSubHouseBillIssuerSCAC", "OTT1", bill.EffectiveSubHouseBillIssuerSCAC);
		}

		public void TestAutoSendSCAC()
		{
			var scacCode = "XPPX";
			var carrierObj = Factory.New<USCarrierCombined>();
			carrierObj.UI_Code = scacCode;
			Factory.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Bill bill = declaration.Bills.AddNew();
			USCarrierCombined carrier = Factory.LoadTop1<USCarrierCombined>(new ZQuery());
			if (carrier != null)
			{
				bill.US_UI_NKBillIssuerSCAC = carrier.UI_Code;
				AssertEquals("Bill has no carrier code as candidate for update", false, bill.IsIssuerCarrierCodeCandidateForUpdate);
				AssertEquals("Bill has no carrier code marked for request", false, bill.IssuerCarrierCodeMarkedForRequest);
			}

			bill.US_UI_NKBillIssuerSCAC = "!";
			AssertEquals("Bill has no carrier code as candidate for update", false, bill.IsIssuerCarrierCodeCandidateForUpdate);
			AssertEquals("Bill has no carrier code marked for request", false, bill.IssuerCarrierCodeMarkedForRequest);

			bill.US_UI_NKBillIssuerSCAC = "XKY9";
			AssertEquals("Bill has carrier code as candidate for update", true, bill.IsIssuerCarrierCodeCandidateForUpdate);
			AssertEquals("Bill has carrier code marked for request", true, bill.IssuerCarrierCodeMarkedForRequest);

			bill.US_UI_NKBillIssuerSCAC = scacCode;
			AssertEquals("Bill has no carrier code as candidate for update", false, bill.IsIssuerCarrierCodeCandidateForUpdate);
			AssertEquals("Bill has no carrier code marked for request", false, bill.IssuerCarrierCodeMarkedForRequest);

			bill.US_UI_NKBillIssuerSCAC = "";
			AssertEquals("Bill has a blank carrier code - should not be marked as candidate for update", false, bill.IsIssuerCarrierCodeCandidateForUpdate);
			AssertEquals("Bill has a blank carrier code - should not be marked for request", false, bill.IssuerCarrierCodeMarkedForRequest);
		}

		public void TestRecordTypeDescription()
		{
			AssertEquals(MessageAttacheeRecordTypeDescriptions.Bill, ((IMessageAttacheeInDeclaration)Bill).RecordTypeDescription);
		}

		public void TestUS_7512OpenArea()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			AssertEquals("Non M/B should be read only", true, bill.US_7512OpenArea_ReadOnly);
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			AssertEquals("M/B should be enterable", false, bill.US_7512OpenArea_ReadOnly);
			bill.US_7512OpenArea = "Document value entered here";
			AssertEquals("Document value entered here", bill.US_7512OpenArea);
		}

		public void TestUS_7512OpenAreaBillValuesAreClearedOnBillTypeChange()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.US_7512OpenArea = "Document value entered here";
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			AssertEquals("Document value entered here", bill.US_7512OpenArea);
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			AssertEquals("Non M/B type should be cleared out", "", bill.US_7512OpenArea);
		}

		public void TestFTZLinesForSea()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var m1 = declaration.Bills.AddNew();
			m1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			var h1 = declaration.Bills.AddNew();
			h1.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			h1.CU_CU_ParentBill = m1.PK;
			var m2 = declaration.Bills.AddNew();
			m2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			var h2 = declaration.Bills.AddNew();
			h2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			h2.CU_CU_ParentBill = m2.PK;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_CU_RelatedHouseBill = h1.PK;
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_CU_RelatedHouseBill = h2.PK;
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Assert(((IFTZBill)m1).Lines.Contains(invoiceLine1.CusEntryLine));
			Assert(!((IFTZBill)m1).Lines.Contains(invoiceLine2.CusEntryLine));
			Assert(!((IFTZBill)m2).Lines.Contains(invoiceLine1.CusEntryLine));
			Assert(((IFTZBill)m2).Lines.Contains(invoiceLine2.CusEntryLine));
			var invoice3 = declaration.Invoices.AddNew();
			invoice3.JZ_CU_RelatedHouseBill = h2.PK;
			var invoiceLine3 = invoice3.JobComInvoiceLines.AddNew();
			invoice3.JZ_InvoiceNumber = "A";
			invoice2.JZ_InvoiceNumber = "B";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var lines = ((IFTZBill)m1).Lines.ToList();
			Assert(lines.Contains(invoiceLine1.CusEntryLine));
			Assert(!lines.Contains(invoiceLine2.CusEntryLine));
			Assert(!lines.Contains(invoiceLine3.CusEntryLine));
			lines = ((IFTZBill)m2).Lines.ToList();
			Assert(!lines.Contains(invoiceLine1.CusEntryLine));
			Assert(lines.Contains(invoiceLine2.CusEntryLine));
			Assert(lines.Contains(invoiceLine3.CusEntryLine));
			Assert(lines[0].LineNumber < lines[1].LineNumber);
		}

		public void TestFTZLinesForAIR()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MB123";
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_CU_ParentBill = masterBill.PK;
			houseBill.CU_BillNum = "HB123456789012345";
			AssertEquals("MB123", ((IFTZBill)masterBill).BillOfLading);
			AssertEquals(ZString.Empty, ((IFTZBill)masterBill).HouseBill);
			AssertEquals("MB123", ((IFTZBill)houseBill).BillOfLading);
			AssertEquals("HB1234567890", ((IFTZBill)houseBill).HouseBill);
		}

		public void TestFTZLinesHouseBillForSea()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MB123";
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_CU_ParentBill = masterBill.PK;
			houseBill.CU_BillNum = "HB123456789012345";
			houseBill.US_UI_NKBillIssuerSCAC = "AAAD";
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				AssertEquals("MB123", ((IFTZBill)masterBill).BillOfLading);
				AssertEquals(ZString.Empty, ((IFTZBill)masterBill).HouseBill);
				AssertEquals("MB123", ((IFTZBill)houseBill).BillOfLading);
				AssertEquals("AAADHB1234567890", ((IFTZBill)houseBill).HouseBill);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				AssertEquals("MB123", ((IFTZBill)masterBill).BillOfLading);
				AssertEquals(ZString.Empty, ((IFTZBill)masterBill).HouseBill);
				AssertEquals("MB123", ((IFTZBill)houseBill).BillOfLading);
				AssertEquals(ZString.Empty, ((IFTZBill)houseBill).HouseBill);
			}
		}

		[TestDate(2011, 01, 01)]
		public void TestISFBillStatusAndDescription()
		{
			var isfCreateTime = ZDateTime.Now;
			var isfJob1 = CreateISFJob(isfCreateTime.AddSeconds(-10), "XXXABill1", BillTypeList.Codes.HouseBillOfLading, DispositionCodeList.Codes.S2);
			var isfJob2 = CreateISFJob(isfCreateTime.AddSeconds(-9), "Bill1", BillTypeList.Codes.HouseBillOfLading, DispositionCodeList.Codes.S4);
			var isfJob3 = CreateISFJob(isfCreateTime.AddSeconds(-8), "XXXABill2", BillTypeList.Codes.OceanBillOfLading, DispositionCodeList.Codes.S5);
			var isfJob4 = CreateISFJob(isfCreateTime.AddSeconds(-7), "XXXABill2", BillTypeList.Codes.HouseBillOfLading, DispositionCodeList.Codes.S3);
			var isfJob5 = CreateISFJob(isfCreateTime.AddSeconds(-6), "XXXABill3", BillTypeList.Codes.OceanBillOfLading, DispositionCodeList.Codes.S6);
			var isfJob6 = CreateISFJob(isfCreateTime.AddSeconds(-5), "XXXABill3", BillTypeList.Codes.OceanBillOfLading, DispositionCodeList.Codes.S2);
			var isfJob7 = CreateISFJob(isfCreateTime.AddSeconds(-4), "Bill4", BillTypeList.Codes.HouseBillOfLading, DispositionCodeList.Codes.S1);
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_TransportMode = declaration1.TransportModeSeaCodeForTesting;
			declaration1.JE_DeclarationReference = "B00001";
			declaration1.JE_HouseBill = "BILL1";
			declaration1.JE_HouseBillIssuerSCAC = "XXXA";
			var billDeclaration1 = declaration1.PrimaryHouseBill;
			declaration1.JE_MasterBill = "BILL2";
			declaration1.JE_MasterBillIssuerSCAC = "XXXA";
			var masterBillDeclaration1 = declaration1.PrimaryMasterBill;
			var exportDeclaration = Factory.New<JobDeclaration>();
			exportDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			exportDeclaration.JE_TransportMode = exportDeclaration.TransportModeSeaCodeForTesting;
			exportDeclaration.JE_DeclarationReference = "B00002";
			exportDeclaration.JE_HouseBill = "BILL1";
			exportDeclaration.JE_HouseBillIssuerSCAC = "XXXA";
			var billExportDeclaration = exportDeclaration.PrimaryHouseBill;
			var airDeclaration = Factory.New<JobDeclaration>();
			airDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			airDeclaration.JE_TransportMode = airDeclaration.TransportModeAirCodeForTesting;
			airDeclaration.JE_DeclarationReference = "B00003";
			airDeclaration.JE_HouseBill = "BILL1";
			airDeclaration.JE_HouseBillIssuerSCAC = "XXXA";
			var billAirDeclaration = airDeclaration.PrimaryHouseBill;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_TransportMode = declaration1.TransportModeSeaCodeForTesting;
			declaration2.JE_DeclarationReference = "B00004";
			declaration2.JE_MasterBill = "BILL3";
			declaration2.JE_MasterBillIssuerSCAC = "XXXA";
			var masterBillDeclaration2 = declaration2.PrimaryMasterBill;
			Factory.Save();
			AssertEquals("Should match to HouseBill XXXABill1", DispositionCodeList.Codes.S2, billDeclaration1.ISFBillStatus);
			AssertEquals("Should match to HouseBill XXXABill1", DispositionCodeList.Descriptions.S2, billDeclaration1.ISFBillStatusDescription);
			AssertEquals("Should not match to OceanBill XXXABill2 as it's not lowest bill", ZString.Empty, masterBillDeclaration1.ISFBillStatus);
			AssertEquals("Should not match to OceanBill XXXABill2 as it's not lowest bill", ZString.Empty, masterBillDeclaration1.ISFBillStatusDescription);
			AssertEquals("Should not match as it's not Import", "", billExportDeclaration.ISFBillStatus);
			AssertEquals("Should not match as it's not Import", "", billExportDeclaration.ISFBillStatusDescription);
			AssertEquals("Should not match as it's not SEA", "", billAirDeclaration.ISFBillStatus);
			AssertEquals("Should not match as it's not SEA", "", billAirDeclaration.ISFBillStatusDescription);
			AssertEquals("Should match to multiple OceanBill XXXABill3", ISFStatusHelper.Multiple, masterBillDeclaration2.ISFBillStatus);
			AssertEquals("Should match to multiple OceanBill XXXABill3", ISFStatusHelper.BillFoundOnMultipleISF, masterBillDeclaration2.ISFBillStatusDescription);
			masterBillDeclaration1.CU_BillNum = "BILL3";
			AssertEquals("Should show status as it's not the lowest bill", ZString.Empty, masterBillDeclaration1.ISFBillStatus);
			AssertEquals("Should show status as it's not the lowest bill", ZString.Empty, masterBillDeclaration1.ISFBillStatusDescription);
			billDeclaration1.CU_BillNum = "BILL4";
			AssertEquals("Should not match as XXXABILL4 doesn't exist", "", billDeclaration1.ISFBillStatus);
			AssertEquals("Should not match as XXXABILL4 doesn't exist", "", billDeclaration1.ISFBillStatusDescription);
			billDeclaration1.Delete();
			masterBillDeclaration1.CU_BillNum = "BILL2";
			AssertEquals("Should match to OceanBill XXXABill2", DispositionCodeList.Codes.S5, masterBillDeclaration1.ISFBillStatus);
			AssertEquals("Should match to OceanBill XXXABill2", DispositionCodeList.Descriptions.S5, masterBillDeclaration1.ISFBillStatusDescription);
		}

		public void TestSetCu_NoOfPacks_WhenValueIsOutOfRange()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Bill bill = declaration.Bills.AddNew();
			var itnumber = bill.ITAndSplitDetails.AddNew();
			decimal outOfRangeValue = 3000000000;
			AssertNoExceptionThrown(() => bill.CU_NoOfPacks = outOfRangeValue);
		}

		public new void TestWillBeDeletedDuringSave()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			bill.CU_GUIPresentationRecord = false;
			bill.CU_BillNum = "12345";
			bill.OnSaving();
			Assert(!bill.IsDeleted);
			bill.CU_BillNum = ZString.Empty;
			var itNumber = bill.ITAndSplitDetails.AddNew();
			itNumber.US_ITNumber = "12345";
			bill.OnSaving();
			Assert(!bill.IsDeleted);
			bill.ITAndSplitDetails.RemoveAndDeleteAll();
			var cusAddInfo = Factory.New<ITAndSplitDetails>();
			cusAddInfo.B7_ParentID = bill.PK;
			cusAddInfo.B7_ParentTableCode = "CU";
			cusAddInfo.B7_Type = "ITN";
			bill.ITAndSplitDetails.Load();
			bill.OnSaving();
			Assert(!bill.IsDeleted);
			cusAddInfo.Delete();
			bill.OnSaving();
			Assert(bill.IsDeleted);
		}

		public void TestITAndSplitDetailsDeletedWithConcurrencyError()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			bill.CU_GUIPresentationRecord = true;
			bill.CU_BillType = "HB";
			bill.CU_BillNum = "HB0123";
			bill.ITNumber = "12345";
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var decCopy = factory2.Load<JobDeclaration>(declaration.PK);
			var billCopy = (Bill)decCopy.Bills.FindByPK(bill.PK);
			// concurrency resolver will try to update this value on billCopy
			bill.ITNumber = "98765";
			declaration.US_ITDate = ZDateTime.BrettsBirthday;
			Factory.Save();
			// deletes the IT details without first loading them leaving 'Data' null
			billCopy.ITAndSplitDetails.DeleteAll();
			// needed to get a concurrency error
			decCopy.US_ITDate = ZDateTime.Today;
			var notificationHandler = new NotificationHandlerForTest();
			try
			{
				factory2.Save();
			}
			catch (Exception ex)
			{
				ZExceptionReporting.HandleSaveException(ex, notificationHandler, null);
			}

			// Should not throw an exception when loading ITAndSplitDetails.Data to report the AddInfo name.
			AssertContains(@"The following objects have been deleted:
USITNumberAddInfo (pending delete)
GenAddOnColumn (pending delete)", notificationHandler.ReportInformationMessage);
		}

		public void TestUS_UI_NKBillIssuerSCAC_ReadOnly()
		{
			var consol = Factory.New<Freight.Forwarding.Business.ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUDYD";
			consol.JK_RL_NKDischargePort = "USCHI";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_HouseBill = "HB123456";
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "APLU";
			cusCode.OK_RN_NKCodeCountry = "US";
			shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(1, declaration.Bills.Count);
			AssertEquals("APLU", declaration.Bills[0].US_UI_NKBillIssuerSCAC);
			AssertEquals(true, declaration.Bills[0].US_UI_NKBillIssuerSCAC_ReadOnly);
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(1, declaration.Bills.Count);
			AssertEquals("APLU", declaration.Bills[0].US_UI_NKBillIssuerSCAC);
			AssertEquals(true, declaration.Bills[0].US_UI_NKBillIssuerSCAC_ReadOnly);
			shipment.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS, "AMS00001");
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(1, declaration.Bills.Count);
			AssertEquals("AMS0", declaration.Bills[0].US_UI_NKBillIssuerSCAC);
			AssertEquals(true, declaration.Bills[0].US_UI_NKBillIssuerSCAC_ReadOnly);
			shipment.Numbers.RemoveAndDeleteAll();
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(1, declaration.Bills.Count);
			AssertEquals("APLU", declaration.Bills[0].US_UI_NKBillIssuerSCAC);
			AssertEquals(true, declaration.Bills[0].US_UI_NKBillIssuerSCAC_ReadOnly);
		}

		public void TestFTZBill()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "M132890";
			var houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1.CU_BillNum = "H132890";
			houseBill1.CU_CU_ParentBill = masterBill.PK;
			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_BillNum = "H232890";
			houseBill2.CU_CU_ParentBill = masterBill.PK;
			AssertEquals(houseBill1, houseBill1.FTZBill);
			AssertEquals(houseBill2, houseBill2.FTZBill);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals(masterBill, houseBill1.FTZBill);
			AssertEquals(masterBill, houseBill2.FTZBill);
			declaration.AdmissionStatus = FTZMessageStatusList.Codes.ClearFTZAdmissionAdd;
			AssertEquals("Should be readonly", false, houseBill1.CU_BillNumInfo.ReadOnly);
			AssertEquals("Should be readonly", false, houseBill1.CU_BillTypeInfo.ReadOnly);
			AssertEquals("Should be readonly", false, declaration.JE_MasterBillInfo.ReadOnly);
			AssertEquals("Should be readonly", false, declaration.JE_HouseBillInfo.ReadOnly);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				AssertEquals(houseBill1, houseBill1.FTZBill);
				AssertEquals(houseBill2, houseBill2.FTZBill);
			}
		}

		public void TestIFTZBillMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MasterBill = "12345678901234567890123456789012345";
			var iFTZMaster = (IFTZBill)declaration.PrimaryMasterBill;
			AssertEquals("Bill of lading or Airway Bill", "12345678901234567890123456789012345", iFTZMaster.BillOfLading);
			var houseBill = declaration.PrimaryMasterBill.ChildBills.AddNew();
			IFTZBill iFTZHouseBill = houseBill;
			houseBill.CU_BillNum = "123456789012";
			AssertEquals("Master bill", "12345678901234567890123456789012345", iFTZHouseBill.BillOfLading);
			AssertEquals("House bill", "123456789012", iFTZHouseBill.HouseBill);
			houseBill.CU_NoOfPacks = 1000m;
			AssertEquals("Quantity", 1000m, iFTZHouseBill.Quantity);
			declaration.US_UC_NKCountryOfExport = "AU";
			AssertEquals("Country of Export", "AU", iFTZHouseBill.CountryOfExport);
			declaration.US_SchDLoading = "12345";
			AssertEquals("Foreign Load Port", "12345", iFTZHouseBill.ForeignLoadPort);
			declaration.US_US_NKLocationOfGoods = "1234";
			AssertEquals("FIRMS Identifer", "1234", iFTZHouseBill.FIRMSCode);
			var itNo = houseBill.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "123456789";
			AssertEquals("IT numbers", "123456789", iFTZHouseBill.ITNumbers.ElementAt(0).ITNumber);
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "32-12345434", Core.Constants.CountryCodes.UnitedStates);
			declaration.DeliveryOrPickupCartageCoPK = carrier.PK;
			AssertEquals("Fallback from declaration to bill Carrier", "32-12345434", iFTZHouseBill.IRSIdentifier);
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingLine = true;
			carrier2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.CBPAssignedNumber, "143802-03062", Core.Constants.CountryCodes.UnitedStates);
			declaration.DeliveryOrPickupCartageCoPK = carrier2.PK;
			AssertEquals("PTT Carrier changed to a different org", "143802-03062", iFTZHouseBill.IRSIdentifier);
			houseBill.Containers.AddNew();
			houseBill.Containers.AddNew();
			houseBill.Containers.AddNew();
			AssertEquals("No. of Containers", 3, iFTZHouseBill.Containers.Count());
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("Master bill", "12345678901234567890123456789012345", iFTZHouseBill.BillOfLading);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				AssertEquals("House bill", "123456789012", iFTZHouseBill.HouseBill);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				AssertEquals("House bill", ZString.Empty, iFTZHouseBill.HouseBill);
			}

			var houseBill2 = declaration.PrimaryMasterBill.ChildBills.AddNew();
			itNo = houseBill2.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "123456789";
			AssertEquals("should be unique IT number for master bill", 1, iFTZMaster.ITNumbers.Count());
			AssertEquals("should be unique IT number for master bill", "123456789", iFTZMaster.ITNumbers.ElementAt(0).ITNumber);
			declaration.JE_MasterBillIssuerSCAC = "XXXA";
			declaration.JE_MasterBill = "W004ADJ120101002";
			AssertEquals("Master bill", "W004ADJ120101002", iFTZHouseBill.BillOfLading);
			declaration.JE_MasterBill = "001560MB001";
			AssertEquals("Master bill", "XXXA001560MB001", iFTZHouseBill.BillOfLading);
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			AssertEquals("Master bill", "XXXA001560MB001", iFTZHouseBill.BillOfLading);
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertEquals("Master bill", "XXXA001560MB001", iFTZHouseBill.BillOfLading);
		}

		public void TestIFZEventBillMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_MasterBill = "342890342";
			var bill = declaration.PrimaryMasterBill;
			bill.CU_BillNum = "001M003";
			var houseBill = bill.ChildBills.AddNew();
			houseBill.CU_BillNum = "H001004";
			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.US_UI_NKBillIssuerSCAC = "ABCD";
			houseBill2.CU_BillNum = "HB00002";
			houseBill2.CU_CU_ParentBill = ZGuid.Empty;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				AssertEquals("Bill of Lading from house bill.", houseBill2.EffectiveBillNumber, ((IFZEventBill)houseBill2).BillOfLading);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				AssertEquals("Bill of Lading from house bill.", ZString.Empty, ((IFZEventBill)houseBill2).BillOfLading);
			}

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "123901-1234");
			bill.US_F_OH_PTTCarrier = orgHeader.PK;
			bill.US_F_Remarks = "Comments";
			bill.USB_PermitToTransferID = "00001";
			var iBill = (IFZEventBill)bill;
			AssertEquals("123901-1234", iBill.IRSIdentifier);
			AssertEquals("Remarks", "Comments", iBill.Remarks);
			AssertEquals("Bill Number", "001M003", iBill.BillOfLading);
			AssertEquals("PID Unique Identifier", "00001", iBill.PIDUniqueIdentifier);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "98-12345612");
			AssertEquals("Should be EIN from Carrier", "98-12345612", iBill.IRSIdentifier);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("Bill Number: MB num + HB num", "001M003H001004", ((IFZEventBill)houseBill).BillOfLading);
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			AssertEquals("Air shipment details should be empty", 0, iBill.AirShipmentDetails.Count());
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.JE_VoyageFlightNo = "0001";
			bill.US_SESplitShip = true;
			var splitDetal = bill.ITAndSplitDetails.AddNew();
			splitDetal.US_ArrivalDate = ZDateTime.Today;
			splitDetal.US_CarrierCode = "ABC";
			splitDetal.US_FlightNumber = "001";
			AssertEquals("Air shipment details should NOT be empty", 1, iBill.AirShipmentDetails.Count());
			var houseFTZBill = (IFZEventBill)houseBill;
			AssertEquals("Air shipment details should NOT be empty", 1, houseFTZBill.AirShipmentDetails.Count());
		}

		public void TestHasLinkedInvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			var bill = declaration.Bills.AddNew();
			bill.US_UI_NKBillIssuerSCAC = "AAAD";
			bill.CU_BillNum = "Bill1";
			AssertEquals("No linked lines", false, bill.HasLinkedInvoiceLines);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = bill.PK;
			invoice.JobComInvoiceLines.AddNew();
			AssertEquals("Has linked lines", true, bill.HasLinkedInvoiceLines);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			invoice.JZ_CU_RelatedHouseBill = Guid.Empty;
			var houseBill = bill.ChildBills.AddNew();
			AssertEquals("No linked lines", false, bill.HasLinkedInvoiceLines);
			AssertEquals("No linked lines", false, houseBill.HasLinkedInvoiceLines);
			invoice.JZ_CU_RelatedHouseBill = houseBill.PK;
			AssertEquals("Has linked lines", true, bill.HasLinkedInvoiceLines);
		}

		public void TestIBillOfLadingDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MasterBillIssuerSCAC = "APLU";
			declaration.JE_MasterBill = "MB11111111";
			declaration.JE_HouseBillIssuerSCAC = "SXXX";
			declaration.JE_HouseBill = "HB11111111";
			var masterBillOfLading = (IBillOfLadingDetail)declaration.PrimaryMasterBill;
			AssertEquals(Customs.Business.BillTypeList.Codes.MasterBill, masterBillOfLading.BillType);
			AssertEquals("APLU", masterBillOfLading.IssuerCodeOfBillOfLading);
			AssertEquals("MB11111111", masterBillOfLading.BillOfLadingNumber);
			var houseBillOfLading = (IBillOfLadingDetail)declaration.PrimaryHouseBill;
			AssertEquals(Customs.Business.BillTypeList.Codes.HouseBill, houseBillOfLading.BillType);
			AssertEquals("SXXX", houseBillOfLading.IssuerCodeOfBillOfLading);
			AssertEquals("HB11111111", houseBillOfLading.BillOfLadingNumber);
		}

		public void TestUS_ExpressTracking()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MasterBillExpressTracking = true;
			var masterBill = declaration.Bills[0];
			AssertEquals(true, masterBill.US_ExpressTracking);
			AssertEquals(false, masterBill.US_ExpressTrackingInfo.ReadOnly);
			var houseBill = masterBill.ChildBills.AddNew();
			AssertEquals(true, houseBill.US_ExpressTrackingInfo.ReadOnly);
			masterBill.US_ExpressTracking = false;
			AssertEquals(false, declaration.JE_MasterBillExpressTracking);
			masterBill.US_ExpressTracking = true;
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			AssertEquals(false, masterBill.US_ExpressTracking);
			AssertEquals(null, declaration.PrimaryMasterBill);
		}

		public void TestEffectiveBillNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MasterBillIssuerSCAC = "QF";
			declaration.JE_MasterBill = "08157824211";
			declaration.JE_HouseBill = "SHAE21100063";
			var masterBill = declaration.PrimaryMasterBill;
			AssertEquals("08157824211", masterBill.EffectiveBillNumber);
			var houseBill = declaration.PrimaryHouseBill;
			AssertEquals("SHAE21100063", houseBill.EffectiveBillNumber);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_MasterBillIssuerSCAC = "EGLV";
			declaration.JE_MasterBill = "149109767330";
			declaration.JE_HouseBillIssuerSCAC = "TVLC";
			declaration.JE_HouseBill = "YTNLAX951431";
			AssertEquals("EGLV149109767330", masterBill.EffectiveBillNumber);
			AssertEquals("TVLCYTNLAX951431", houseBill.EffectiveBillNumber);
			declaration.JE_HouseBill = "YADJTNLAX951431";
			AssertEquals("YADJTNLAX951431", houseBill.EffectiveBillNumber);
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertEquals("EGLV149109767330", masterBill.EffectiveBillNumber);
			AssertEquals("YADJTNLAX951431", houseBill.EffectiveBillNumber);
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			AssertEquals("EGLV149109767330", masterBill.EffectiveBillNumber);
			AssertEquals("YADJTNLAX951431", houseBill.EffectiveBillNumber);
		}

		public void TestIsLowestBill()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_EnableENS = true;
			declaration.JE_MasterBill = "MB111111";
			var masterBill = declaration.Bills[0];
			AssertEquals(true, masterBill.IsLowestBill);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals(true, masterBill.IsLowestBill);
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			AssertEquals(true, masterBill.IsLowestBill);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_HouseBill = "HB2222222";
			AssertEquals(false, masterBill.IsLowestBill);
			var houseBill = declaration.Bills[1];
			AssertEquals(true, houseBill.IsLowestBill);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals(false, masterBill.IsLowestBill);
			AssertEquals(true, houseBill.IsLowestBill);
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			AssertEquals(true, masterBill.IsLowestBill);
			AssertEquals(false, houseBill.IsLowestBill);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MasterBillExpressTracking = true;
			AssertEquals(true, masterBill.IsLowestBill);
			AssertEquals(false, houseBill.IsLowestBill);
		}

		public void TestUSB_PermitToTransferIDReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			AssertEquals(true, bill.USB_PermitToTransferIDInfo.ReadOnly);
		}

		[ExpectNoExceptions]
		public void TestNoExceptionWhenPTTCarrierIsNull()
		{
			var dec = Factory.New<JobDeclaration>();
			var bill = dec.Bills.AddNew();
			Assert(!bill.PTTCarrierHasPOA);
		}

		public void TestUS_SplitShipmentDetail_SplitShipHasChanges()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillNum = "HB";
			houseBill.US_SESplitShip = true;
			var billSplitDetail = houseBill.ITAndSplitDetails.AddNew();
			billSplitDetail.US_ArrivalDate = new ZDateTime(2021, 04, 28);
			billSplitDetail.US_CarrierCode = "A2";
			billSplitDetail.US_FlightNumber = "04A";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = houseBill.PK;
			invoice.US_SplitShipmentDetail = invoice.AddInfoLookups.SplitShipmentDetailsList[0].Code;
			AssertNotEquals(ZString.Empty, invoice.US_SplitShipmentDetail);
			houseBill.US_SESplitShip = false;
			AssertEquals(ZString.Empty, invoice.US_SplitShipmentDetail);
		}

		public void TestIFTZBillCommonCU_BillNum()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var bill = declaration.Bills.AddNew();
			bill.US_UI_NKBillIssuerSCAC = "AAAD";
			bill.CU_BillNum = "123456789";

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				AssertEquals("123456789", ((IFTZBillCommon)bill).CU_BillNum);
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals("123456789", ((IFTZBillCommon)bill).CU_BillNum);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				AssertEquals("AAAD123456789", ((IFTZBillCommon)bill).CU_BillNum);
				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals("123456789", ((IFTZBillCommon)bill).CU_BillNum);
			}
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = (Bill)base.GetNewBusinessObjectForDeleteTest(factory);
			result.DispositionCodes.AddNew();
			result.ReferenceNos.AddNew();
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2704", "Test Port", startDate, endDate);
			newFactory.Save();

			var coll = new BorderCargoPortCollection();
			var bcPort = coll.AddNew();
			bcPort.PortCode = "2704";
			bcPort.CRProcess = CRProcessList.Codes.OneStep;
			bcPort.Location = LocationList.Codes.South;
			USCustomsDataRegistry.Instance.BorderCargoReleasePorts.SetValue(Guid.Empty, Declaration.Branch.PK.ToGuid(), Guid.Empty, coll);
		}

		Integration.Customs.US.ISF.ICusISFHeader CreateISFJob(ZDateTime createTime, ZString billNumber, ZString billType, ZString customsStatus)
		{
			var header = Factory.New<Integration.Customs.US.ISF.ICusISFHeader>();
			header.BF_SystemCreateTimeUtc = createTime;
			CreateISFBill(header.PK, billNumber, billType, customsStatus);
			return header;
		}

		Integration.Customs.US.ISF.ICusISFBill CreateISFBill(ZGuid headerPK, ZString billNumber, ZString billType, ZString customsStatus)
		{
			var bill = Factory.New<Integration.Customs.US.ISF.ICusISFBill>();
			bill.BB_BF = headerPK;
			bill.BB_BillType = billType;
			bill.BB_BillNum = billNumber;
			bill.BB_CustomsStatus = customsStatus;
			return bill;
		}

		JobDeclaration declaration;
		new JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());

		Bill bill;
		new Bill Bill => bill ?? (bill = Declaration.Bills.AddNew());
	}
}
