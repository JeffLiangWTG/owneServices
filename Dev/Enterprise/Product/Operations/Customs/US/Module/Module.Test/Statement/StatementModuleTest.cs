using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.GUI;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(StatementModule))]
	sealed class StatementModuleTest : ZModuleBasherWithFetchHintsTest
	{
		public void TestActionMenuStatementACHRequest()
		{
			AssertClickCallZFormModaliser("Statement Re-Route Request", typeof(StatementAndACHRerouteForm));
		}

		public void TestActionMenuOperationalActions()
		{
			using (var module = new StatementModule())
			{
				Env.Security.FindOrCreateOperationalActionsCustomiseCheckpoint(Env.Security.USCustomsImportStatement).IsAllowed = false;
				var actionMenuItem = ((ZDisplayGrid)module.DisplayGrid).ContextMenu.MenuItems.FindByText("Actions");
				actionMenuItem.OnPopup(EventArgs.Empty);
				AssertNotNull(actionMenuItem.MenuItems.FindByText("Operational Actions"));
				Env.Security.FindOrCreateOperationalActionsCustomiseCheckpoint(Env.Security.USCustomsImportStatement).IsAllowed = true;
				actionMenuItem = ((ZDisplayGrid)module.DisplayGrid).ContextMenu.MenuItems.FindByText("Actions");
				actionMenuItem.OnPopup(EventArgs.Empty);
				AssertNotNull(actionMenuItem.MenuItems.FindByText("Operational Actions"));
			}
		}

		public void TestLicenceAndSecurityCheckPoint()
		{
			using (var module = new StatementModule())
			{
				AssertEquals("Licence Checkpoint", Env.Licence.ImportBroker, module.LicenceCheckPoint);
				AssertEquals("SecurityCheckpoint", Env.Security.USCustomsImportStatement, module.SecurityCheckpoint);
			}
		}

		public void TestStatementModuleAllows()
		{
			using (var module = new StatementModule())
			{
				AssertEquals("module.AllowNew", false, module.AllowNew);
				AssertEquals("module.AllowEdit", true, module.AllowEdit);
				AssertEquals("module.AllowDelete", false, module.AllowDelete);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.USCustomsStatement;

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;

		protected override bool HasController() => true;

		protected override ZFilterModule CreateModuleForFetchHintsTest() => new StatementModule();

		protected override void SetupDataForFetchHintsTest()
		{
			var newFactory = new BusinessObjectFactory();
			for (var i = 0; i < 20; i++)
			{
				CreateStatementForFetchHintTest(newFactory, i);
			}

			newFactory.Save();
		}

		protected override List<string> FetchHintIgnoreField
		{
			get
			{
				var result = base.FetchHintIgnoreField;
				result.Add("StatementType");
				return result;
			}
		}

		protected override bool HasFailedFetchHint(TableHitCount tableSelect)
		{
			if (tableSelect.Value == 10 && tableSelect.TableName == EDIMessageSchema.Constants.TableName) // EDIMessage.EM_MessageText will cause db hits to load blob
			{
				return false;
			}

			return base.HasFailedFetchHint(tableSelect);
		}

		void CreateStatementForFetchHintTest(BusinessObjectFactory factory, int i)
		{
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();

			var number = i.ToString();
			var mod6 = i % 6;
			var organisations = factory.GetCachedValue("OrganisationReconModuleTest", delegate
			{
				return factory.Load<OrgHeader>(new ZQuery()
				{ MaximumRows = 24 });
			});

			var statement = factory.New<CusStatementHeader>();
			var lookups = statement.Lookups;
			var statementHeaderStatusList = lookups.StatementHeaderStatusList;
			var paymentStatusList = lookups.PaymentStatusList;
			var paymentTypeList = lookups.PaymentTypeList;
			var accountingClassFeeCodeList = CusFeeCodeConstants.GetAccountingClassFeeCodeList(factory);
			statement.B2_StatementNumber = "ST" + number;
			if (i % 2 == 0)
			{
				statement.B2_IsMonthlyStatement = true;
			}
			else if (i % 3 == 0)
			{
				statement.B2_StatementNumber = ZString.Empty;
			}

			statement.B2_Status = statementHeaderStatusList[i % statementHeaderStatusList.Count].Code;
			statement.B2_PaymentStatus = paymentStatusList[i % paymentStatusList.Count].Code;
			statement.B2_ProcessPort = "PP" + number;
			statement.B2_ImporterCustomsID = "ICI" + number;
			statement.B2_ProcessDate = ZDateTime.Today.AddMinutes(i);
			statement.B2_PrintDate = ZDateTime.Today.AddMinutes(i + 1);
			statement.B2_DueDate = ZDateTime.Today.AddMinutes(i + 2);
			statement.B2_BranchDesignation = "B" + number;
			statement.B2_PaymentType = paymentTypeList[i % paymentTypeList.Count].Code;
			statement.B2_OH_Importer = organisations[(i + 1) % organisations.Length].PK;
			statement.B2_AccountNo = "AC" + number;
			if (!statement.B2_IsMonthlyStatement)
			{
				statement.B2_PaymentAuthorizationDate = ZDateTime.Today.AddMinutes(i + 3);
			}

			var messageText = "B011101XJ5MSF1234P   01051607691-013199000                                      " +
				"Q11234     11101XJ5            0516070516070000559488300000000000               " +
				"Q2000000203510000004263100005959169                                             " +
				"QA010560000000618410500000003530496000000005000540000000911205300000002159      " +
				"QA024990000024509331100000000800106000000024200550000001025650100000000316      " +
				"QA030570000000732109000000001172102000000059911030000000481010400000001640      " +
				"Q38804P04001061107061507XXX            0001118976600000000000                   " +
				"Q4000000407020000008526200000003000                                             " +
				"QE010560000001236810500000007060496000000010000540000001822405300000004318      " +
				"QE024990000049018631100000001600106000000048400550000002051250100000000632      " +
				"QE030570000001464209000000002344102000000119821030000000962010400000003280      " +
				"Q58804P04001061107061507XXX            0001118976600000000000                   " +
				"Q6000000407020000008526200011934338                                             " +
				"Y  1101XJ5MS00022";
			var message = factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement;
			message.EM_MessageText = messageText;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_LinkUniqueID = statement.PK;
			message.EM_LinkTable = statement.TableName;
			message.EM_MessageNum = "EM" + number;
			var daily = statement.IsMonthlyStatement ? statement.DailyStatements.AddNew() : statement;
			var line = daily.StatementLines.AddNew();
			var charge = line.Charges.AddNew();
			charge.B4_ChargeType = accountingClassFeeCodeList[i % accountingClassFeeCodeList.Count].Code;
			charge.B4_ChargeAmount = 10m * i;
			var charge2 = line.Charges.AddNew();
			charge2.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.ExciseTaxDeferred;
			charge2.B4_ChargeAmount = 11m * i;
		}

		void AssertClickCallZFormModaliser(string menuName, Type expectedFormType)
		{
			using (var module = new StatementModule())
			{
				var actionMenuItem = ((ZDisplayGrid)module.DisplayGrid).ContextMenu.MenuItems.FindByText("Actions");
				actionMenuItem.OnPopup(EventArgs.Empty);
				var menuItem = actionMenuItem.MenuItems.FindByText(menuName);
				ZFormModaliser.LastFormShownDialogForTest = null;
				menuItem.PerformClick();
				AssertEquals(expectedFormType, ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}
	}
}
