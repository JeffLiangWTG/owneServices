using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusContainer))]
	sealed class CusContainerTest : Customs.Business.Testing.BaseCusContainerTest<CusContainer, JobDeclaration>
	{
		public void TestIControllerIDProviderMembers()
		{
			var dec = Factory.New<JobDeclaration>();
			var container = dec.CusContainers.AddNew();
			IControllerIDProvider provider = container;
			AssertEquals("ControllerID", ControllerIDs.Customs.JobDeclaration, provider.ControllerID);
			AssertEquals("BusinessObjectPK", dec.PK.ToGuid(), provider.BusinessObjectPK);
			var shipment = Factory.New<ForwardingShipment>();
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

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Factory.Save();

			using (Env.SetTemporaryUserContext(company1.PK.ToGuid(), branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				declaration.JE_GB = branch1.PK;
				var container = declaration.CusContainers.AddNew();

				var localTimeBranch1 = ZDateTime.Now;
				var utcTimeNow = DateTime.UtcNow;
				var centralimeInfo = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
				var centralTimeZone = TimeZoneInfo.ConvertTimeFromUtc(utcTimeNow, centralimeInfo);
				AssertEquals("Branch1 Time Zone", centralTimeZone.TimeOfDay.Hours, localTimeBranch1.TimeOfDay.Hours);

				using (Env.SetTemporaryUserContext(company1.PK.ToGuid(), branch2.PK.ToGuid(), department.PK.ToGuid()))
				{
					var localTimeBranch2 = ZDateTime.Now;
					var utcTimeNow2 = DateTime.UtcNow;

					var log = container.LogManager.AddALogIfNecessary("", ImportMessageStatusList.Codes.ClearDepartureOriginal, new ImportMessageStatusList());
					AssertEquals(ImportMessageStatusList.Codes.ClearDepartureOriginal, log.SL_Reference);

					var pacificTimeInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
					var pacificTimeZone = TimeZoneInfo.ConvertTimeFromUtc(utcTimeNow2, pacificTimeInfo);
					AssertEquals("Branch2 Time Zone", pacificTimeZone.TimeOfDay.Hours, localTimeBranch2.TimeOfDay.Hours);

					AssertEquals("Wrote by Job Branch Time", localTimeBranch1.TimeOfDay.Hours, log.SL_EventTime.TimeOfDay.Hours);
				}
			}
		}

		public void TestIContainerDetailMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			var container = declaration.CusContainers.AddNew();
			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "40@#";
			containerType.RC_Length = 40m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.Refrigerated;
			container.CO_ContainerNumber = "CONT32423";
			container.CO_RC = containerType.PK;
			IContainerDetail containerDetail = container;
			AssertEquals("ContainerEquipmentID", "CONT32423", containerDetail.ContainerEquipmentID);
			AssertEquals("ContainerLength", (short)40, containerDetail.ContainerLength);
			AssertEquals("IsRefrigerated", true, containerDetail.IsRefrigerated);
		}

		public void TestICusAddInfoTypeSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var container = declaration.CusContainers.AddNew();
			ICusAddInfoTypeSupporter supporter = container;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USDisposition, out var type);
			AssertEquals(typeof(DispositionData), type);
			supporter.GetCusAddInfoTypes().TryGetValue("ZZ!", out type);
			AssertNull(type);
			var dispositionData = container.DispositionCodes.AddNew();
			dispositionData.US_Code = "1";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var addInfo = newFactory.Load<CusAddInfo>(dispositionData.PK);
			AssertEquals(typeof(DispositionData), addInfo.GetType());
		}

		public void TestAdditionalReferenceNumbers()
		{
			var jobContainer = Factory.New<ForwardingContainer>();
			var cusContainer = Factory.New<CusContainer>();
			cusContainer.CO_JC = jobContainer.PK;
			var itNum = jobContainer.AdditionalReferenceNumbers.AddNew();
			itNum.CE_EntryType = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;
			itNum.CE_EntryNum = "V1";
			var carrierRef = jobContainer.AdditionalReferenceNumbers.AddNew();
			carrierRef.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			carrierRef.CE_EntryNum = "MB1";
			AssertEquals("No of code", 2, cusContainer.AdditionalReferenceNumbers.Count);
			AssertEquals("IT num", "V1", cusContainer.ITReferenceNumber);
			AssertEquals("Carrier Ref", "MB1", cusContainer.AMSNumber);
		}

		public void TestIContainer()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRUX432890";
			AssertEquals("ContainerNumber", "CRUX432890", ((IContainer)container).ContainerNumber);
			AssertEquals("ContainerType", "", ((IContainer)container).ContainerType);
			RefContainer refContainer = Factory.New<RefContainer>();
			refContainer.SetCountrySpecificContainerCode("20", Enterprise.Core.Constants.CountryCodes.UnitedStates);
			container.CO_RC = refContainer.PK;
			AssertEquals("ContainerType", "20", ((IContainer)container).ContainerType);
		}

		public void TestIAESTIRTransportationDetailMembers()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			CusContainer container = declaration.CusContainers.AddNew();
			IAESTIRTransportationDetail detail = container;
			AssertEquals(ZString.Empty, detail.EquipmentNumber);
			AssertEquals(ZString.Empty, detail.SealNumber);
			AssertEquals(ZString.Empty, detail.TransportationReferenceNumber);
			container.CO_ContainerNumber = "TURE2134232";
			container.CO_Seal = "SL3243";
			AssertEquals("TURE2134232", detail.EquipmentNumber);
			AssertEquals("SL3243", detail.SealNumber);
			AssertEquals(ZString.Empty, detail.TransportationReferenceNumber);
			container.CO_ContainerNumber = "TURE-21 342";
			container.CO_Seal = "SL-32 43";
			AssertEquals("TURE21342", detail.EquipmentNumber);
			AssertEquals("SL3243", detail.SealNumber);
			AssertEquals(ZString.Empty, detail.TransportationReferenceNumber);
		}

		public void TestIMessageResponseNotificatorMembers()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "Z8";
			staff.GS_EmailAddress = "dummy@email.com";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var container = declaration.CusContainers.AddNew();
			IMessageResponseNotificator notificator = container;
			AssertEquals(ZString.Empty, notificator.GetFallbackEmailAddressRecipient());
			declaration.JE_GS_NKCusAgent = "Z8";
			AssertEquals("dummy@email.com", notificator.GetFallbackEmailAddressRecipient());
			var declaration2 = Factory.New<JobDeclaration>();
			container.CO_JE = declaration2.PK;
			AssertEquals(ZString.Empty, notificator.GetFallbackEmailAddressRecipient());
		}

		public void TestMessageStatusIsClearedAfterCloned()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_MessageStatus = "AAA";
			JobDeclaration clonedDec = (JobDeclaration)declaration.TemplateCopy();
			AssertEquals("Containers are not cloned on normal template copy", 0, clonedDec.CusContainers.Count);
			clonedDec = (JobDeclaration)declaration.GetNewRelatedDeclaration(Factory);
			AssertEquals("Status is cleared", ZString.Empty, clonedDec.CusContainers[0].CO_MessageStatus);
		}

		public void TestAddAClearLog()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_MessageStatus = ImportMessageStatusList.Codes.AwaitingArrival;
			container.CO_MessageStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			AssertEquals("Add a clear log", true, container.LogManager.HasAClearInBondDeparture);
			container.CO_MessageStatus = ImportMessageStatusList.Codes.AwaitingDepartureWithdraw;
			container.CO_MessageStatus = ImportMessageStatusList.Codes.ClearDepartureWithdraw;
			AssertEquals("HasAWithdrawnLog", true, container.LogManager.HasAWithdrawnLog);
		}

		public void TestIMessageAttachee()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Bill bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill1.CU_BillNum = "M1";
			PackingGroup packGroup1 = bill1.PackingGroups.AddNew();
			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "M2";
			PackingGroup packGroup2 = bill2.PackingGroups.AddNew();
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRUX1234562";
			packGroup1.CR_CO_Container = container.PK;
			packGroup2.CR_CO_Container = container.PK;
			IMessageAttacheeInDeclaration msgAttachee = container;
			AssertEquals("Reference Num", "CRUX1234562", msgAttachee.HumanFriendlyReference);
			AssertEquals("Messages", container.Messages, msgAttachee.Messages);
			AssertEquals("RecordType", MessageAttacheeRecordType.Container, msgAttachee.RecordType);
			AssertEquals(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK), msgAttachee.Branch);
			AssertEquals(Core.Constants.TransportModes.Sea, msgAttachee.TransportMode);
			container.CO_MessageStatus = "AAA";
			AssertEquals("Status", "AAA", msgAttachee.MessageStatus);
			IIMessageAttacheeWithDisposition dispositionData = container;
			dispositionData.UpdateDispositionInformation("06", ZDateTime.Today.AddHours(-5));
			AssertEquals(1, container.DispositionCodes.Count);
			AssertEquals("06", container.DispositionCodes[0].US_Code);
			AssertEquals(ZDateTime.Today.AddHours(-5), container.DispositionCodes[0].US_DispositionDate);
		}

		public void TestCodeDescription()
		{
			CusContainer container = Factory.New<CusContainer>();
			container.CO_ContainerNumber = "CRUX123456";
			AssertEquals("Code", "CRUX123456", CodePropertyAttribute.CodeFromBusinessObject(container));
			AssertEquals("Desc", "CRUX123456", DescriptionPropertyAttribute.DescriptionFromBusinessObject(container));
		}

		public void TestTypeDecider()
		{
			Assert("Update BaseCusContainerTypeDecider to include a decider for this class", Factory.New(typeof(BaseCusContainer)).GetType() == typeof(CusContainer));
		}

		public void TestDeclaration()
		{
			JobDeclaration declaration = (JobDeclaration)GetJobDeclaration();
			CusContainer container = declaration.CusContainers.AddNew();
			AssertEquals(declaration, container.Declaration);
		}

		public void TestLookupsCachesInstance()
		{
			CusContainer container = (CusContainer)GetNewBusinessObject();
			CusContainerLookups lookup1 = container.Lookups;
			CusContainerLookups lookup2 = container.Lookups;
			AssertEquals(lookup2, lookup1);
		}

		public void TestRecordTypeDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			IMessageAttacheeInDeclaration container = declaration.CusContainers.AddNew();
			AssertEquals(MessageAttacheeRecordTypeDescriptions.Container, container.RecordTypeDescription);
		}

		public void TestIsRailCar()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusContainer container = declaration.CusContainers.AddNew();
			AssertEquals(false, container.IsRailCar(USContainerCodeList.Codes.AC));
			AssertEquals(true, container.IsRailCar(USContainerCodeList.Codes.BE));
		}

		protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bO) => new BaseCusContainer.CustomLabelsProvider(((CusContainer)bO).Declaration);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = (CusContainer)base.GetNewBusinessObjectForDeleteTest(factory);
			result.DispositionCodes.AddNew();
			return result;
		}
	}
}
