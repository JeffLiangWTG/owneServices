using System;
using System.Collections.Generic;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.Integration;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal.Testing
{
	sealed class ISFEventParentFinderTest : OrganizationAddressTestHelper
	{
		public void TestFindBasedOnReferenceNumber()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_JobReference = "ISF0000002";
			Factory.SaveForTesting();

			var finder = new ISFEventParentFinder(Factory.BOFactory, new ISFHeaderDataContextManager(), Logger);
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.USImporterSecurityFiling, null);
			var eventXml = new UniversalEvent()
			{
				DataContext = dataContext,
				ContextCollection = new List<Context>()
				{
					new Context() { Type = "DeclarationReference", Value = "ISF0000002" },
				}
			};

			var bizObjs = finder.GetLogParentsForEvent(eventXml);
			AssertNotNull(bizObjs);
			AssertEquals(1, bizObjs.Length);
			AssertCollectionContains(header, bizObjs);

			eventXml = new UniversalEvent()
			{
				DataContext = dataContext,
				ContextCollection = new List<Context>()
				{
					new Context() { Type = "DeclarationReference", Value = "ISF0000003" },
				}
			};

			bizObjs = finder.GetLogParentsForEvent(eventXml);
			AssertNull(bizObjs);
		}

		public void TestFindBasedOnCustomsReference()
		{
			var header0 = Factory.New<CusISFHeader>();
			header0.BF_JobReference = "ISF0000002";
			header0.BF_CustomsReference = "XJ5-20089367423";
			var header1 = Factory.New<CusISFHeader>();
			header1.BF_JobReference = "ISF0000003";
			header1.BF_CustomsReference = "XJ5-20089367423";
			Factory.SaveForTesting();

			var finder = new ISFEventParentFinder(Factory.BOFactory, new ISFHeaderDataContextManager(), Logger);
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.USImporterSecurityFiling, null);
			var eventXml = new UniversalEvent()
			{
				DataContext = dataContext,
				ContextCollection = new List<Context>()
				{
					new Context() { Type = "EntryNumber", Value = "XJ5-20089367423" },
					new Context() { Type = "EntryNumberType", Value = "ISF" },
					new Context() { Type = "EntryNumberCountryOfIssue", Value = "US" },
				}
			};

			var bizObjs = finder.GetLogParentsForEvent(eventXml);
			AssertNotNull(bizObjs);
			AssertEquals(2, bizObjs.Length);
			AssertCollectionContains(header0, bizObjs);
			AssertCollectionContains(header1, bizObjs);

			eventXml = new UniversalEvent()
			{
				DataContext = dataContext,
				ContextCollection = new List<Context>()
				{
					new Context() { Type = "EntryNumber", Value = "XJ5-20089366542" },
					new Context() { Type = "EntryNumberType", Value = "ISF" },
					new Context() { Type = "EntryNumberCountryOfIssue", Value = "US" },
				}
			};

			bizObjs = finder.GetLogParentsForEvent(eventXml);
			AssertNull(bizObjs);
		}

		public void TestFindForBondStatusNotificationMessage()
		{
			var groupZZ1 = Factory.New<GlbGroup>();
			groupZZ1.GG_Code = "ZZ1";
			var staffZ1 = groupZZ1.Staff.AddNew();
			staffZ1.GS_Code = "Z1";
			staffZ1.GS_LoginName = "z1";
			staffZ1.GS_EmailAddress = "jason@test.email.com";
			USCustomsDataRegistry.Instance.BondStatusNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupZZ1.PK.ToGuid());

			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates);

			var cusISFHeader = Factory.NewWithValidTestData<CusISFHeader>();
			cusISFHeader.BF_BondType = BondTypeList.Codes.SingleTransactionBond;
			cusISFHeader.BF_CustomsReference = "739-94506084190";
			cusISFHeader.BF_JobReference = "ISFT234322";

			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;

			message.EM_MessageText =
			"B004701739BS                                                                    " +
			"B116S006BLS ENB NEW BOND HAS BEEN ADDED IN ACE           1 1107161717           " +
			"10B916      10000110716000118005110716      16S006BLS                           " +
			"124701739                                                                       " +
			"202  73994506084190                                                             " +
			"30EI 90 - 107748100CADENCE INSOLES LLC                                          " +
			"40856143 - 48 - 7151LEXON INSURANCE COMPANY                      10000          " +
			"Y  4701739BS00006                                                               ";

			Factory.SaveForTesting();

			var eventDataObject = new UniversalEvent();
			eventDataObject.DataContext = new DataContext()
			{
				DataSourceCollection = new List<DataSource>(),
				DataTargetCollection = new List<DataTarget>(),
			};
			eventDataObject.AdditionalFieldsToUpdateCollection = new List<AdditionalFieldToUpdate>();

			var dataContext = (DataContext)eventDataObject.DataContext;
			eventDataObject.DataContext.AddDataTarget(DataContextType.USImporterSecurityFiling, null);
			dataContext.DataProvider = "USCATAIR";
			dataContext.ActionPurpose = new CodeDescriptionPair() { Code = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification, Description = "" };
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification,
				Department = "CBP"
			};

			eventDataObject.EventReference = "~15000";
			eventDataObject.EventType = Enterprise.ZArchitecture.Business.AutoEvents.MessageReceivedCode;
			eventDataObject.ContextCollection = new List<Context>();
			eventDataObject.ContextCollection.Add(new Context() { Type = nameof(Enterprise.UniversalDataBuss.DataObjects.Universal.Event.ContextTypes.EntryNumber), Value = "73994506084190" });

			var finder = new ISFEventParentFinder(Factory.BOFactory, new ISFHeaderDataContextManager(), Logger);
			var bizObjs = finder.GetLogParentsForEvent(eventDataObject);
			AssertNotNull(bizObjs);
		}
	}
}
