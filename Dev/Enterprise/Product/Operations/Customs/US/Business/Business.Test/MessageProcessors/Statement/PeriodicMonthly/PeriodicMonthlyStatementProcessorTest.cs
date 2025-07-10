using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors.Statement.Testing
{
	public class PeriodicMonthlyStatementProcessorTest : StatementProcessorTest<PeriodicMonthlyStatementProcessor, APLA, APLB, APLY>
	{
		protected override (EDIMessage message, ZGuid expectedBranchPK, ZGuid expectedLinkUniqueID, ZString expectedJobNumber, string[] expectedKeys) GenerateInfoToTestGetKeysForBlockingParallelProcessing()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "8804091001";
			statement.B2_EntryFilerCode = "SV9";
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement;
			message.EM_MessageText =
"B013901SV9MSP88040910010516076                1704     1                        " +
"Y  3901SV9MS00022                                                               ";
			message.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();
			return (message, GlbBranch.CurrentBranch.PK, statement.PK, "8804091001", new[] { $"Statement:SV9-8804091001|{GlbCompany.CurrentCompany.PK}" });
		}

		public void TestEmailWithLogo()
		{
			var newBranch = Factory.New<GlbBranch>();
			newBranch.FillWithValidTestData();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			var image1 = new System.Drawing.Bitmap(1, 2);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);

			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement,
							"A3901SV9FSTATE05160701",
							"B011704GFSMSF1709P06772061909711-358469900    1704GFS                           Q117091343381704GFS11-3584699000514090601090000000000000000000000               Q2000000000000000000000000000004719                                             QA014990000000295850100000001761                                                Q117091404231704GFS11-3584699000520090601090000000000000000000000               Q2000000000000000000000000000003239                                             QA024990000000250050100000000739                                                Q117091413431704GFS11-3584699000521090601090000000878500000000000               Q2000000000000000000000000000013885                                             QA034990000000319750100000001903                                                Q117091484151704GFS11-3584699000528090601090000004858400000000000               Q2000000000000000000000000000054196                                             QA044990000000351850100000002094                                                Q31709P06772061909061909GFS11-3584699000000005736900000000000                   Q4000000000000000000000000000076039                                             QE014990000001217350100000006497                                                Q51709P06772061909061909GFS11-3584699000000005736900000000000                   Q6000000000000000000000000000076039                                             QJ014990000001217350100000006497                                                Y  1704GFSMS00018", "Z3901SV9FSTATE05160701", "");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var processor = GetNewIncomingMessageProcessor();
			processor.ExecuteBatch();

			message.Reload();
			AssertEquals("Status: Processed", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("Email created", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var banner = email.Attachments.Cast<ZArchitecture.Environment.AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new System.Drawing.Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(1, image.Width);
		}

		protected override void EndToEndCore()
		{
			var dailyStatement1 = Factory.New<CusStatementHeader>();
			dailyStatement1.B2_StatementNumber = "8804091001";
			dailyStatement1.B2_EntryFilerCode = "XXX";
			dailyStatement1.B2_ProcessPort = "8888";

			var statementLine = dailyStatement1.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XXX";
			statementLine.B3_EntryNum = "10135797";
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine.B3_EntryProcessPort = dailyStatement1.B2_ProcessPort;

			statementLine = dailyStatement1.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XXX";
			statementLine.B3_EntryNum = "10135698";
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine.B3_EntryProcessPort = dailyStatement1.B2_ProcessPort;

			statementLine = dailyStatement1.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XXX";
			statementLine.B3_EntryNum = "10135699";
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine.B3_EntryProcessPort = dailyStatement1.B2_ProcessPort;

			var dailyStatement2 = Factory.New<CusStatementHeader>();
			dailyStatement2.B2_StatementNumber = "8804091002";
			dailyStatement2.B2_EntryFilerCode = "XXX";
			dailyStatement2.B2_ProcessPort = "8888";

			statementLine = dailyStatement2.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XXX";
			statementLine.B3_EntryNum = "20135795";
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine.B3_EntryProcessPort = dailyStatement2.B2_ProcessPort;

			statementLine = dailyStatement2.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XXX";
			statementLine.B3_EntryNum = "20135796";
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine.B3_EntryProcessPort = dailyStatement2.B2_ProcessPort;

			statementLine = dailyStatement2.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XXX";
			statementLine.B3_EntryNum = "20135797";
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine.B3_EntryProcessPort = dailyStatement2.B2_ProcessPort;

			var processor = GetNewIncomingMessageProcessor();
			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement, "A3901SV9FSTATE05160701", "B013901SV9MSP8804P040010516076                1704     1                        Q188040910018888XXX            0516070516070000559488300000000000               Q2000000203510000004263100005959169                                             QA010560000000618410500000003530496000000005000540000000911205300000002159      QA024990000024509331100000000800106000000024200550000001025650100000000316      QA030570000000732109000000001172102000000059911030000000481010400000001640      Q188040910028888XXX            0516070516070000559488300000000000               Q2000000203510000004263100005959169                                             QA010560000000618410500000003530496000000005000540000000911205300000002159      QA024990000024509331100000000800106000000024200550000001025650100000000316      QA030570000000732109000000001172102000000059911030000000481010400000001640      Q38804P04001061107061507XXX            0001118976600000000000                   Q4000000407020000008526200011918338                                             QE010560000001236810500000007060496000000010000540000001822405300000004318      QE024990000049018631100000001600106000000048400550000002051250100000000632      QE030570000001464209000000002344102000000119821030000000962010400000003280      Q58804P04001061107061507XXX            0001118976600000000000                   Q6000000407020000008526200011934338                                             QJ010560000001236810500000007060496000000010000540000001822405300000004318      QJ024990000050618631100000001600106000000048400550000002051250100000000632      QJ030570000001464209000000002344102000000119821030000000962010400000003280      Q78804091001XXX10135797ABIXXX10135698ABI                                        Q78804091002XXX20135795ABIXXX20135696ABI                                        Y  3901SV9MS00022", "Z3901SV9FSTATE05160701", "");
			processor.ExecuteBatch();
			Factory.Save();

			var factoryForLoad = new BusinessObjectFactory();

			var statementQuery = new ZQuery(CusStatementHeaderSchema.B2_EntryFilerCode, "SV9");
			statementQuery.AddToFilter(CusStatementHeaderSchema.B2_ProcessPort, "3901");
			statementQuery.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, "8804P04001");
			var statementHeaders = factoryForLoad.Load<CusStatementHeader>(statementQuery);
			AssertEquals("Is monthly statement", true, statementHeaders[0].B2_IsMonthlyStatement);
			AssertEquals("Preparer District Port", "1704", statementHeaders[0].B2_PreparerDistrictPort);
			AssertEquals("Statement Amount", 119183.38m, statementHeaders[0].B2_StatementAmount);
			AssertEquals("Due date", new ZDateTime(2007, 6, 15), statementHeaders[0].B2_DueDate);

			message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement, "A3901SV9FSTATE05160701", "B013901SV9MSF8804P040010516076                1704     1                        Q188040910018888XXX            0516070516070000559488300000000000               Q2000000203510000004263100005959169                                             QA010560000000618410500000003530496000000005000540000000911205300000002159      QA024990000024509331100000000800106000000024200550000001025650100000000316      QA030570000000732109000000001172102000000059911030000000481010400000001640      Q188040910028888XXX            0516070516070000559488300000000000               Q2000000203510000004263100005959169                                             QA010560000000618410500000003530496000000005000540000000911205300000002159      QA024990000024509331100000000800106000000024200550000001025650100000000316      QA030570000000732109000000001172102000000059911030000000481010400000001640      Q38804P04001061107061507XXX            0001118976600000000000                   Q4000000407020000008526200011918338                                             QE010560000001236810500000007060496000000010000540000001822405300000004318      QE024990000049018631100000001600106000000048400550000002051250100000000632      QE030570000001464209000000002344102000000119821030000000962010400000003280      Q58804P04001061107061507XXX            0001118976600000000000                   Q6000000407020000008526200011934338                                             QJ010560000001236810500000007060496000000010000540000001822405300000004318      QJ024990000050618631100000001600106000000048400550000002051250100000000632      QJ030570000001464209000000002344102000000119821030000000962010400000003280      Q78804091001XXX10135797ABIXXX10135698ABI                                        Q78804091002XXX20135795ABIXXX20135696ABI                                        Y  3901SV9MS00022", "Z3901SV9FSTATE05160701", "");

			processor.ExecuteBatch();

			Factory.Save();

			dailyStatement1.Reload();
			dailyStatement2.Reload();

			var factory2 = new BusinessObjectFactory();
			statementQuery = new ZQuery(CusStatementHeaderSchema.B2_EntryFilerCode, "XXX");
			statementQuery.AddToFilter(CusStatementHeaderSchema.B2_ProcessPort, "8888");
			statementQuery.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, "8804091001");
			AssertEquals("For monthly statement message processing, it should not create a new daily statement", 1, factory2.Load<CusStatementHeader>(statementQuery).Length);

			statementQuery = new ZQuery(CusStatementHeaderSchema.B2_EntryFilerCode, "XXX");
			statementQuery.AddToFilter(CusStatementHeaderSchema.B2_ProcessPort, "8888");
			statementQuery.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, "8804091002");
			AssertEquals(1, factory2.Load<CusStatementHeader>(statementQuery).Length);

			statementQuery = new ZQuery(CusStatementHeaderSchema.B2_EntryFilerCode, "SV9");
			statementQuery.AddToFilter(CusStatementHeaderSchema.B2_ProcessPort, "3901");
			statementQuery.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, "8804P04001");

			var dailyStatement1PK = dailyStatement1.PK;
			var dailyStatement2PK = dailyStatement2.PK;
			AssertForMonthlyStatement(dailyStatement1PK, dailyStatement2PK);

			message.EM_Status = "QUE";
			processor.ExecuteBatch();

			Factory.Save();
			AssertForMonthlyStatement(dailyStatement1PK, dailyStatement2PK);

			//broker has rerouted a preliminary statement 
			var message2 = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement, "A3901SV9FSTATE05160701", "B013901SV9MSP8804P040010516076                                                  Q188040910018888XXX            0516070516070000559488300000000000               Q2000000203510000004263100005959169                                             QA010560000000618410500000003530496000000005000540000000911205300000002159      QA024990000024509331100000000800106000000024200550000001025650100000000316      QA030570000000732109000000001172102000000059911030000000481010400000001640      Q188040910028888XXX            0516070516070000559488300000000000               Q2000000203510000004263100005959169                                             QA010560000000618410500000003530496000000005000540000000911205300000002159      QA024990000024509331100000000800106000000024200550000001025650100000000316      QA030570000000732109000000001172102000000059911030000000481010400000001640      Q38804P04001061107061507XXX            0001118976600000000000                   Q4000000407020000008526200011918338                                             QE010560000001236810500000007060496000000010000540000001822405300000004318      QE024990000049018631100000001600106000000048400550000002051250100000000632      QE030570000001464209000000002344102000000119821030000000962010400000003280      Q58804P04001061107061507XXX            0001118976600000000000                   Q6000000407020000008526200011934338                                             QJ010560000001236810500000007060496000000010000540000001822405300000004318      QJ024990000050618631100000001600106000000048400550000002051250100000000632      QJ030570000001464209000000002344102000000119821030000000962010400000003280      Q78804091001XXX10135797ABIXXX10135698ABI                                        Q78804091002XXX20135795ABIXXX20135696ABI                                        Y  3901SV9MS00022", "Z3901SV9FSTATE05160701", "");
			message2.EM_Status = "QUE";
			processor.ExecuteBatch();
			Factory.Save();

			var factory3 = new BusinessObjectFactory();
			var monthlyStatement = factory3.LoadTop1<CusStatementHeader>(new ZQuery(CusStatementHeaderSchema.B2_StatementNumber, "8804P04001"));
			AssertEquals("Status should still be Final", StatementHeaderStatusList.Codes.Final, monthlyStatement.B2_Status);
			AssertEquals("Importer Of Record should be empty, because ImporterOfRecordNumber in block B is empty",
						ZGuid.Empty, monthlyStatement.B2_OH_Importer);

			AssertEquals("dailyStatement1 - Individual statements should also be finalised", StatementHeaderStatusList.Codes.Final, dailyStatement1.B2_Status);
			AssertEquals("dailyStatement2 - Individual statements should also be finalised", StatementHeaderStatusList.Codes.Final, dailyStatement2.B2_Status);
			AssertEquals("dailyStatement1 - Individual statements payment status should be accepted", PaymentStatusList.Codes.PaymentAuthorizationAccepted, dailyStatement1.B2_PaymentStatus);
			AssertEquals("dailyStatement2 - Individual statements payment status should be accepted", PaymentStatusList.Codes.PaymentAuthorizationAccepted, dailyStatement2.B2_PaymentStatus);
		}

		public void TestProcessForSeveralDailyStatements()
		{
			var image1 = new System.Drawing.Bitmap(1, 2);
			var image2 = new System.Drawing.Bitmap(2, 1);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "11-358469900");

			var dailyStatement1 = Factory.New<CusStatementHeader>();
			dailyStatement1.B2_StatementNumber = "1709134338";
			dailyStatement1.B2_EntryFilerCode = "GFS";
			dailyStatement1.B2_ProcessPort = "1704";

			var dailyStatement2 = Factory.New<CusStatementHeader>();
			dailyStatement2.B2_StatementNumber = "1709140423";
			dailyStatement2.B2_EntryFilerCode = "GFS";
			dailyStatement2.B2_ProcessPort = "1704";

			var dailyStatement3 = Factory.New<CusStatementHeader>();
			dailyStatement3.B2_StatementNumber = "1709141343";
			dailyStatement3.B2_EntryFilerCode = "GFS";
			dailyStatement3.B2_ProcessPort = "1704";

			var dailyStatement4 = Factory.New<CusStatementHeader>();
			dailyStatement4.B2_StatementNumber = "1709148415";
			dailyStatement4.B2_EntryFilerCode = "GFS";
			dailyStatement4.B2_ProcessPort = "1714";//This is different to the PMS message below. Refer to CS00146914

			Factory.Save();

			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement,
							"A3901SV9FSTATE05160701",
							"B011704GFSMSF1709P06772061909711-358469900    1704GFS                           Q117091343381704GFS11-3584699000514090601090000000000000000000000               Q2000000000000000000000000000004719                                             QA014990000000295850100000001761                                                Q117091404231704GFS11-3584699000520090601090000000000000000000000               Q2000000000000000000000000000003239                                             QA024990000000250050100000000739                                                Q117091413431704GFS11-3584699000521090601090000000878500000000000               Q2000000000000000000000000000013885                                             QA034990000000319750100000001903                                                Q117091484151704GFS11-3584699000528090601090000004858400000000000               Q2000000000000000000000000000054196                                             QA044990000000351850100000002094                                                Q31709P06772061909061909GFS11-3584699000000005736900000000000                   Q4000000000000000000000000000076039                                             QE014990000001217350100000006497                                                Q51709P06772061909061909GFS11-3584699000000005736900000000000                   Q6000000000000000000000000000076039                                             QJ014990000001217350100000006497                                                Y  1704GFSMS00018", "Z3901SV9FSTATE05160701", "");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var processor = GetNewIncomingMessageProcessor();
			processor.ExecuteBatch();

			message.Reload();
			AssertEquals("Status: Processed", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("Email created", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var banner = email.Attachments.Cast<ZArchitecture.Environment.AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new System.Drawing.Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(1, image.Width);

			var newFactoryForLoading = new BusinessObjectFactory();
			var statementQuery = new ZQuery(CusStatementHeaderSchema.B2_StatementNumber, "1709P06772");

			var monthlyStatements = newFactoryForLoading.Load<CusStatementHeader>(statementQuery);
			AssertEquals("There should be one montly new statement generated", 1, monthlyStatements.Length);
			var monthlyStatement = monthlyStatements[0];

			AssertEquals("Importer Of Record should be specified, because ImporterOfRecordNumber is not empty in B block",
						organisation.PK, monthlyStatement.B2_OH_Importer);

			AssertEquals("Should be 4 Daily Statements in the message", 4, monthlyStatement.DailyStatements.Count);
			AssertEquals("Total Monthly Statement Amount", 760.39m, monthlyStatement.B2_StatementAmount);
			AssertEquals("Shoud be ACE", StatementTypeList.Codes.ACE, monthlyStatement.B2_StatementType);

			dailyStatement1.Reload();
			AssertEquals("Daily Statement belongs to Monthly Statement", monthlyStatement.PK, dailyStatement1.B2_B2_PeriodicStatement);
			dailyStatement2.Reload();
			AssertEquals("Daily Statement belongs to Monthly Statement", monthlyStatement.PK, dailyStatement2.B2_B2_PeriodicStatement);
			dailyStatement3.Reload();
			AssertEquals("Daily Statement belongs to Monthly Statement", monthlyStatement.PK, dailyStatement3.B2_B2_PeriodicStatement);
			dailyStatement4.Reload();
			AssertEquals("Daily Statement belongs to Monthly Statement", monthlyStatement.PK, dailyStatement4.B2_B2_PeriodicStatement);

			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = "B011704GFSMSF1709P06772061909711-358469900    1704GFS                           Q117091343381704GFS11-3584699000514090601090000000000000000000000               Q2000000000000000000000000000002719                                             QA014990000000295850100000001761                                                Q117091404231704GFS11-3584699000520090601090000000000000000000000               Q2000000000000000000000000000003239                                             QA024990000000250050100000000739                                                Q117091413431704GFS11-3584699000521090601090000000878500000000000               Q2000000000000000000000000000013885                                             QA034990000000319750100000001903                                                Q117091484151704GFS11-3584699000528090601090000004858400000000000               Q2000000000000000000000000000054196                                             QA044990000000351850100000002094                                                Q31709P06772061909061909GFS11-3584699000000005736900000000000                   Q4000000000000000000000000000076039                                             QE014990000001217350100000006497                                                Q51709P06772061909061909GFS11-3584699000000005736900000000000                   Q6000000000000000000000000000076039                                             QJ014990000001217350100000006497                                                Y  1704GFSMS00018";

			Factory.Save();
			processor.ExecuteBatch();

			newFactoryForLoading = new BusinessObjectFactory();
			monthlyStatements = newFactoryForLoading.Load<CusStatementHeader>(statementQuery);

			AssertEquals("Importer Of Record should be specified, because ImporterOfRecordNumber is not empty in B block",
				organisation.PK, monthlyStatement.B2_OH_Importer);

			AssertEquals("There should be one montly statement after reprocessing", 1, monthlyStatements.Length);

			monthlyStatement = monthlyStatements[0];
			AssertEquals("Should be 4 Daily Statements in the message", 4, monthlyStatement.DailyStatements.Count);
			AssertEquals("Total Monthly Statement Amount should be recalculated from second message", 740.39m, monthlyStatement.B2_StatementAmount);
			AssertEquals("Shoud be ACE", StatementTypeList.Codes.ACE, monthlyStatement.B2_StatementType);
		}

		public void TestReprocessing()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "11-358469900");

			var monthlyStatement = Factory.New<CusStatementHeader>();
			monthlyStatement.B2_StatementNumber = "1709P06772";
			monthlyStatement.B2_EntryFilerCode = "GFS";
			monthlyStatement.B2_ProcessPort = "1704";
			monthlyStatement.B2_StatementAmount = 50.42m;
			monthlyStatement.B2_OH_Importer = organisation.PK;

			var dailyStatement1 = Factory.New<CusStatementHeader>();
			dailyStatement1.B2_StatementNumber = "1709134338";
			dailyStatement1.B2_EntryFilerCode = "GFS";
			dailyStatement1.B2_ProcessPort = "1704";
			dailyStatement1.B2_B2_PeriodicStatement = monthlyStatement.PK;

			var dailyStatement2 = Factory.New<CusStatementHeader>();
			dailyStatement2.B2_StatementNumber = "1709140423";
			dailyStatement2.B2_EntryFilerCode = "GFS";
			dailyStatement2.B2_ProcessPort = "1704";
			dailyStatement2.B2_B2_PeriodicStatement = monthlyStatement.PK;

			var dailyStatement3 = Factory.New<CusStatementHeader>();
			dailyStatement3.B2_StatementNumber = "1709141343";
			dailyStatement3.B2_EntryFilerCode = "GFS";
			dailyStatement3.B2_ProcessPort = "1704";
			dailyStatement3.B2_B2_PeriodicStatement = monthlyStatement.PK;

			var dailyStatement4 = Factory.New<CusStatementHeader>();
			dailyStatement4.B2_StatementNumber = "1709148415";
			dailyStatement4.B2_EntryFilerCode = "GFS";
			dailyStatement4.B2_ProcessPort = "1704";
			dailyStatement4.B2_B2_PeriodicStatement = monthlyStatement.PK;

			Factory.Save();

			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement,
							"A3901SV9FSTATE05160701",
							"B011704GFSMSF1709P06772061909711-358469900    1704GFS                           Q117091343381704GFS11-3584699000514090601090000000000000000000000               Q2000000000000000000000000000004719                                             QA014990000000295850100000001761                                                Q117091404231704GFS11-3584699000520090601090000000000000000000000               Q2000000000000000000000000000003239                                             QA024990000000250050100000000739                                                Q117091413431704GFS11-3584699000521090601090000000878500000000000               Q2000000000000000000000000000013885                                             QA034990000000319750100000001903                                                Q117091484151704GFS11-3584699000528090601090000004858400000000000               Q2000000000000000000000000000054196                                             QA044990000000351850100000002094                                                Q31709P06772061909061909GFS11-3584699000000005736900000000000                   Q4000000000000000000000000000076039                                             QE014990000001217350100000006497                                                Q51709P06772061909061909GFS11-3584699000000005736900000000000                   Q6000000000000000000000000000076039                                             QJ014990000001217350100000006497                                                Y  1704GFSMS00018", "Z3901SV9FSTATE05160701", "");

			message.EM_MessageOwner = "Reprocessing";
			Factory.Save();
			AssertEquals("Precondition: Message Owner", "Reprocessing", message.EM_MessageOwner);
			AssertEquals("Precondition: Monthly Statement Amount", 50.42m, monthlyStatement.B2_StatementAmount);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var processor = GetNewIncomingMessageProcessor();
			processor.ExecuteBatch();

			message.Reload();
			AssertEquals("Status: Processed", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("Message Owner should be empty", ZString.Empty, message.EM_MessageOwner);
			AssertEquals("No email created", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			var newFactoryForLoading = new BusinessObjectFactory();
			var statementQuery = new ZQuery(CusStatementHeaderSchema.B2_StatementNumber, "1709P06772");

			var monthlyStatements = newFactoryForLoading.Load<CusStatementHeader>(statementQuery);
			AssertEquals("There should be one montly statement", 1, monthlyStatements.Length);
			monthlyStatement = monthlyStatements[0];

			AssertEquals("Should be 4 Daily Statements for this Monthly Statement", 4, monthlyStatement.DailyStatements.Count);
			AssertEquals("Total Monthly Statement Amount", 760.39m, monthlyStatement.B2_StatementAmount);
			AssertEquals("Importer Of Record should be specified", organisation.PK, monthlyStatement.B2_OH_Importer);
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 3; }
		}

		public void TestTriggerAccountingIntegration()
		{
			var testHelper = new Customs.Business.Testing.InvoicingTestHelper(Factory);
			testHelper.SetUpDisbursementCreditorAndChargeCode();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = testHelper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_SchDEntry = "8888";
			declaration.US_EntryFilerCode = "XXX";
			declaration.ImportEntryNumber = "10135797";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			Factory.Save();

			var dailyStatement1 = Factory.New<CusStatementHeader>();
			dailyStatement1.B2_StatementNumber = "8804091001";
			dailyStatement1.B2_EntryFilerCode = "XXX";
			dailyStatement1.B2_ProcessPort = "8888";
			dailyStatement1.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			dailyStatement1.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			dailyStatement1.B2_AccountNo = "111111";

			var statementLine1 = dailyStatement1.StatementLines.AddNew();
			statementLine1.B3_EntryFilerCode = "XXX";
			statementLine1.B3_EntryNum = "10135797";
			statementLine1.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine1.B3_EntryProcessPort = dailyStatement1.B2_ProcessPort;
			AssertNotNull(statementLine1.Declaration);

			var lineCharge = statementLine1.Charges.AddNew();
			lineCharge.B4_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;
			lineCharge.B4_ChargeAmount = 5261.36m;
			statementLine1.B3_CustomsFeesTotal = 5261.36m;
			dailyStatement1.B2_StatementAmount = 5261.36m;
			Factory.Save();

			var options = new AccountingIntegrationOptions();
			options.EnableAccountingIntegration = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, options);

			var creditor = testHelper.DisbursementCreditor;
			OrgHeaderWrapper.New(creditor).ZO_AccountNo = "111111";

			var group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "test@cargowise.com";
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = group.PK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);

			var brokerAccounts = new BrokersAccountCollection();
			var account = brokerAccounts.AddNew();
			account.PayerUnitNumber = "111111";
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			account.BankAccount = bankAccount.PK;
			Enterprise.Customs.US.DataRegistry.Business.USCustomsDataRegistry.Instance.BrokersAccounts.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, brokerAccounts);

			dailyStatement1.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
			Factory.Save();

			AssertNotNull("A job is created against dec", new JobHeader.Loader(declaration).Load());

			var apInvoice = Factory.LoadTop1<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "10135797").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertNull(apInvoice);

			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement, "A3901SV9FSTATE05160701", "B013901SV9MSF8804P040010516076                                                  Q188040910018888XXX            0516070516070000559488300000000000               Q2000000203510000004263100005959169                                             QA010560000000618410500000003530496000000005000540000000911205300000002159      QA024990000024509331100000000800106000000024200550000001025650100000000316      QA030570000000732109000000001172102000000059911030000000481010400000001640      Q188040910028888XXX            0516070516070000559488300000000000               Q2000000203510000004263100005959169                                             QA010560000000618410500000003530496000000005000540000000911205300000002159      QA024990000024509331100000000800106000000024200550000001025650100000000316      QA030570000000732109000000001172102000000059911030000000481010400000001640      Q38804P04001061107061507XXX            0001118976600000000000                   Q4000000407020000008526200011918338                                             QE010560000001236810500000007060496000000010000540000001822405300000004318      QE024990000049018631100000001600106000000048400550000002051250100000000632      QE030570000001464209000000002344102000000119821030000000962010400000003280      Q58804P04001061107061507XXX            0001118976600000000000                   Q6000000407020000008526200011934338                                             QJ010560000001236810500000007060496000000010000540000001822405300000004318      QJ024990000050618631100000001600106000000048400550000002051250100000000632      QJ030570000001464209000000002344102000000119821030000000962010400000003280      Q78804091001XXX10135797ABIXXX10135698ABI                                        Q78804091002XXX20135795ABIXXX20135696ABI                                        Y  3901SV9MS00022", "Z3901SV9FSTATE05160701", "");

			var processor = GetNewIncomingMessageProcessor();
			processor.ExecuteBatch();

			message.Reload();
			AssertEquals("PreCondition:Processed", EDIMessage.Status.Received, message.EM_Status);

			var factory2 = new BusinessObjectFactory();
			var query = new ZQuery(CusStatementHeaderSchema.B2_EntryFilerCode, "SV9");
			query.AddToFilter(CusStatementHeaderSchema.B2_ProcessPort, "3901");
			query.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, "8804P04001");

			var monthlyStatements = factory2.Load<CusStatementHeader>(query);
			AssertEquals("There should be one montly new statement generated", 1, monthlyStatements.Length);
			AssertEquals(true, monthlyStatements[0].DailyStatements.Count > 0);
			AssertEquals(true, monthlyStatements[0].DailyStatements.Contains(dailyStatement1));
			AssertEquals("B2_PaymentParty should have been set to a unique party", PaymentPartyList.Codes.Broker, monthlyStatements[0].B2_PaymentParty);
			AssertEquals("B2_AccountNo should have been set to a unique party", "111111", monthlyStatements[0].B2_AccountNo);

			var payment = Factory.LoadTop1<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, Enterprise.ZArchitecture.Core.TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertNull(payment);
		}

		public void TestWhenPaymentDetailsAreNotUnique()
		{
			var dailyStatement1 = Factory.New<CusStatementHeader>();
			dailyStatement1.B2_StatementNumber = "8804091001";
			dailyStatement1.B2_EntryFilerCode = "XXX";
			dailyStatement1.B2_ProcessPort = "8888";
			dailyStatement1.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			dailyStatement1.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			dailyStatement1.B2_AccountNo = "111111";
			dailyStatement1.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;

			var statementLine1 = dailyStatement1.StatementLines.AddNew();
			statementLine1.B3_EntryFilerCode = "XXX";
			statementLine1.B3_EntryNum = "10135797";
			statementLine1.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine1.B3_EntryProcessPort = dailyStatement1.B2_ProcessPort;

			var dailyStatement2 = Factory.New<CusStatementHeader>();
			dailyStatement2.B2_StatementNumber = "8804091002";
			dailyStatement2.B2_EntryFilerCode = "XXX";
			dailyStatement2.B2_ProcessPort = "8888";
			dailyStatement2.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			dailyStatement2.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			dailyStatement2.B2_AccountNo = "111112";
			dailyStatement2.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
			Factory.Save();

			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement, "A3901SV9FSTATE05160701", "B013901SV9MSF8804P040010516076                                                  Q188040910018888XXX            0516070516070000559488300000000000               Q2000000203510000004263100005959169                                             QA010560000000618410500000003530496000000005000540000000911205300000002159      QA024990000024509331100000000800106000000024200550000001025650100000000316      QA030570000000732109000000001172102000000059911030000000481010400000001640      Q188040910028888XXX            0516070516070000559488300000000000               Q2000000203510000004263100005959169                                             QA010560000000618410500000003530496000000005000540000000911205300000002159      QA024990000024509331100000000800106000000024200550000001025650100000000316      QA030570000000732109000000001172102000000059911030000000481010400000001640      Q38804P04001061107061507XXX            0001118976600000000000                   Q4000000407020000008526200011918338                                             QE010560000001236810500000007060496000000010000540000001822405300000004318      QE024990000049018631100000001600106000000048400550000002051250100000000632      QE030570000001464209000000002344102000000119821030000000962010400000003280      Q58804P04001061107061507XXX            0001118976600000000000                   Q6000000407020000008526200011934338                                             QJ010560000001236810500000007060496000000010000540000001822405300000004318      QJ024990000050618631100000001600106000000048400550000002051250100000000632      QJ030570000001464209000000002344102000000119821030000000962010400000003280      Q78804091001XXX10135797ABIXXX10135698ABI                                        Q78804091002XXX20135795ABIXXX20135696ABI                                        Y  3901SV9MS00022", "Z3901SV9FSTATE05160701", "");

			var processor = GetNewIncomingMessageProcessor();
			processor.ExecuteBatch();

			message.Reload();
			AssertEquals("PreCondition:Processed", EDIMessage.Status.Received, message.EM_Status);

			var newFactoryForLoad = new BusinessObjectFactory();
			var query = new ZQuery(CusStatementHeaderSchema.B2_EntryFilerCode, "SV9");
			query.AddToFilter(CusStatementHeaderSchema.B2_ProcessPort, "3901");
			query.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, "8804P04001");

			var monthlyStatements = newFactoryForLoad.Load<CusStatementHeader>(query);
			AssertEquals("There should be one montly new statement generated", 1, monthlyStatements.Length);
			AssertEquals(true, monthlyStatements[0].DailyStatements.Count > 0);
			AssertEquals(true, monthlyStatements[0].DailyStatements.Contains(dailyStatement1));
			AssertEquals(true, monthlyStatements[0].DailyStatements.Contains(dailyStatement2));

			AssertEquals("B2_PaymentParty should have been set to a unique party", PaymentPartyList.Codes.Broker, monthlyStatements[0].B2_PaymentParty);
			AssertEquals("B2_AccountNo should have been set to a unique accountNo", "", monthlyStatements[0].B2_AccountNo);
			AssertEquals("B2_PaymentParty should have been set to a unique party", PaymentPartyList.Codes.Broker, monthlyStatements[0].B2_PaymentParty);
		}

		public void TestDailyStatementsStatusUpdatedOnFinalStatementProcessing()
		{
			var dailyStatement1 = Factory.New<CusStatementHeader>();
			dailyStatement1.B2_StatementNumber = "8804091001";
			dailyStatement1.B2_EntryFilerCode = "XXX";
			dailyStatement1.B2_ProcessPort = "8888";
			dailyStatement1.B2_Status = StatementHeaderStatusList.Codes.Preliminary;

			var statementLine = dailyStatement1.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XXX";
			statementLine.B3_EntryNum = "10135797";
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine.B3_EntryProcessPort = dailyStatement1.B2_ProcessPort;

			statementLine = dailyStatement1.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XXX";
			statementLine.B3_EntryNum = "10135698";
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine.B3_EntryProcessPort = dailyStatement1.B2_ProcessPort;

			statementLine = dailyStatement1.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XXX";
			statementLine.B3_EntryNum = "10135699";
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine.B3_EntryProcessPort = dailyStatement1.B2_ProcessPort;

			var dailyStatement2 = Factory.New<CusStatementHeader>();
			dailyStatement2.B2_StatementNumber = "8804091002";
			dailyStatement2.B2_EntryFilerCode = "XXX";
			dailyStatement2.B2_ProcessPort = "8888";
			dailyStatement2.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			dailyStatement2.B2_PaymentStatus = PaymentStatusList.Codes.PaymentInProgress;

			statementLine = dailyStatement2.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XXX";
			statementLine.B3_EntryNum = "20135795";
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine.B3_EntryProcessPort = dailyStatement2.B2_ProcessPort;

			statementLine = dailyStatement2.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XXX";
			statementLine.B3_EntryNum = "20135796";
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine.B3_EntryProcessPort = dailyStatement2.B2_ProcessPort;

			statementLine = dailyStatement2.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XXX";
			statementLine.B3_EntryNum = "20135797";
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine.B3_EntryProcessPort = dailyStatement2.B2_ProcessPort;

			AssertEquals("dailyStatement1", StatementHeaderStatusList.Codes.Preliminary, dailyStatement1.B2_Status);
			AssertEquals("dailyStatement2", StatementHeaderStatusList.Codes.Preliminary, dailyStatement2.B2_Status);
			AssertEquals("dailyStatement1 - payment status", "", dailyStatement1.B2_PaymentStatus);
			AssertEquals("dailyStatement2 - payment status", PaymentStatusList.Codes.PaymentInProgress, dailyStatement2.B2_PaymentStatus);

			var processor = GetNewIncomingMessageProcessor();
			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement, "A3901SV9FSTATE05160701", "B013901SV9MSP8804P040010516076                1704     1                        Q188040910018888XXX            0516070516070000559488300000000000               Q2000000203510000004263100005959169                                             QA010560000000618410500000003530496000000005000540000000911205300000002159      QA024990000024509331100000000800106000000024200550000001025650100000000316      QA030570000000732109000000001172102000000059911030000000481010400000001640      Q188040910028888XXX            0516070516070000559488300000000000               Q2000000203510000004263100005959169                                             QA010560000000618410500000003530496000000005000540000000911205300000002159      QA024990000024509331100000000800106000000024200550000001025650100000000316      QA030570000000732109000000001172102000000059911030000000481010400000001640      Q38804P04001061107061507XXX            0001118976600000000000                   Q4000000407020000008526200011918338                                             QE010560000001236810500000007060496000000010000540000001822405300000004318      QE024990000049018631100000001600106000000048400550000002051250100000000632      QE030570000001464209000000002344102000000119821030000000962010400000003280      Q58804P04001061107061507XXX            0001118976600000000000                   Q6000000407020000008526200011934338                                             QJ010560000001236810500000007060496000000010000540000001822405300000004318      QJ024990000050618631100000001600106000000048400550000002051250100000000632      QJ030570000001464209000000002344102000000119821030000000962010400000003280      Q78804091001XXX10135797ABIXXX10135698ABI                                        Q78804091002XXX20135795ABIXXX20135696ABI                                        Y  3901SV9MS00022", "Z3901SV9FSTATE05160701", "");
			processor.ExecuteBatch();
			Factory.Save();

			var factoryForLoad = new BusinessObjectFactory();

			var query = new ZQuery(CusStatementHeaderSchema.B2_EntryFilerCode, "SV9");
			query.AddToFilter(CusStatementHeaderSchema.B2_ProcessPort, "3901");
			query.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, "8804P04001");
			var statementHeaders = factoryForLoad.Load<CusStatementHeader>(query);
			AssertEquals("Preparer District Port", "1704", statementHeaders[0].B2_PreparerDistrictPort);

			message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement, "A3901SV9FSTATE05160701", "B013901SV9MSF8804P040010516076                1704     1                        Q188040910018888XXX            0516070516070000559488300000000000               Q2000000203510000004263100005959169                                             QA010560000000618410500000003530496000000005000540000000911205300000002159      QA024990000024509331100000000800106000000024200550000001025650100000000316      QA030570000000732109000000001172102000000059911030000000481010400000001640      Q188040910028888XXX            0516070516070000559488300000000000               Q2000000203510000004263100005959169                                             QA010560000000618410500000003530496000000005000540000000911205300000002159      QA024990000024509331100000000800106000000024200550000001025650100000000316      QA030570000000732109000000001172102000000059911030000000481010400000001640      Q38804P04001061107061507XXX            0001118976600000000000                   Q4000000407020000008526200011918338                                             QE010560000001236810500000007060496000000010000540000001822405300000004318      QE024990000049018631100000001600106000000048400550000002051250100000000632      QE030570000001464209000000002344102000000119821030000000962010400000003280      Q58804P04001061107061507XXX            0001118976600000000000                   Q6000000407020000008526200011934338                                             QJ010560000001236810500000007060496000000010000540000001822405300000004318      QJ024990000050618631100000001600106000000048400550000002051250100000000632      QJ030570000001464209000000002344102000000119821030000000962010400000003280      Q78804091001XXX10135797ABIXXX10135698ABI                                        Q78804091002XXX20135795ABIXXX20135696ABI                                        Y  3901SV9MS00022", "Z3901SV9FSTATE05160701", "");

			processor.ExecuteBatch();
			Factory.Save();

			dailyStatement1.Reload();
			dailyStatement2.Reload();

			message.EM_Status = "QUE";
			processor.ExecuteBatch();
			Factory.Save();

			factoryForLoad = new BusinessObjectFactory();
			var monthlyStatement = factoryForLoad.LoadTop1<CusStatementHeader>(new ZQuery(CusStatementHeaderSchema.B2_StatementNumber, "8804P04001"));
			AssertEquals("Status should still be Final", StatementHeaderStatusList.Codes.Final, monthlyStatement.B2_Status);
			AssertEquals("dailyStatement1 - Individual statements should also be finalised", StatementHeaderStatusList.Codes.Final, dailyStatement1.B2_Status);
			AssertEquals("dailyStatement2 - Individual statements should also be finalised", StatementHeaderStatusList.Codes.Final, dailyStatement2.B2_Status);
			AssertEquals("dailyStatement1 - Individual statements payment status should be accepted", PaymentStatusList.Codes.PaymentAuthorizationAccepted, dailyStatement1.B2_PaymentStatus);
			AssertEquals("dailyStatement2 - Individual statements payment status should be accepted", PaymentStatusList.Codes.PaymentAuthorizationAccepted, dailyStatement2.B2_PaymentStatus);
		}

		void AssertForMonthlyStatement(ZGuid dailyStatement1PK, ZGuid dailyStatement2PK)
		{
			var factoryForLoading = new BusinessObjectFactory();
			var query = new ZQuery(CusStatementHeaderSchema.B2_EntryFilerCode, "SV9");
			query.AddToFilter(CusStatementHeaderSchema.B2_ProcessPort, "3901");
			query.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, "8804P04001");

			var monthlyStatements = factoryForLoading.Load<CusStatementHeader>(query);
			AssertEquals("There should be one montly new statement generated", 1, monthlyStatements.Length);

			var monthlyStatement = monthlyStatements[0];
			AssertEquals("Statement Status", StatementHeaderStatusList.Codes.Final, monthlyStatement.B2_Status);
			AssertEquals("Statement Print Date", new ZDateTime(2007, 5, 16), monthlyStatement.B2_PrintDate);
			AssertEquals("Statement Amount", 119183.38m, monthlyStatement.B2_StatementAmount);

			var dailyStatement1Loaded = factoryForLoading.Load<CusStatementHeader>(dailyStatement1PK);
			var dailyStatement2Loaded = factoryForLoading.Load<CusStatementHeader>(dailyStatement2PK);

			AssertEquals("FK to the monthly set", monthlyStatement.PK, dailyStatement1Loaded.B2_B2_PeriodicStatement);
			AssertEquals("FK to the monthly set", monthlyStatement.PK, dailyStatement2Loaded.B2_B2_PeriodicStatement);

			var statementLine = dailyStatement1Loaded.StatementLines.GetStatementLineFor("XXX", "10135797");
			AssertEquals("Status", StatementLineStatusList.Codes.Active, statementLine.B3_Status);

			statementLine = dailyStatement1Loaded.StatementLines.GetStatementLineFor("XXX", "10135698");
			AssertEquals("Status", StatementLineStatusList.Codes.Active, statementLine.B3_Status);

			statementLine = dailyStatement1Loaded.StatementLines.GetStatementLineFor("XXX", "10135699");
			AssertEquals("Status", StatementLineStatusList.Codes.Active, statementLine.B3_Status);

			statementLine = dailyStatement2Loaded.StatementLines.GetStatementLineFor("XXX", "20135795");
			AssertEquals("Status", StatementLineStatusList.Codes.Active, statementLine.B3_Status);

			statementLine = dailyStatement2Loaded.StatementLines.GetStatementLineFor("XXX", "20135796");
			AssertEquals("Status", StatementLineStatusList.Codes.Active, statementLine.B3_Status);

			statementLine = dailyStatement2Loaded.StatementLines.GetStatementLineFor("XXX", "20135797");
			AssertEquals("Status", StatementLineStatusList.Codes.Active, statementLine.B3_Status);
		}

		public override void TestCreateNewCusStatementHeaderFilter()
		{
			AssertNotNull(loader.CreateNewCusStatementHeaderFilter("XJ5", "8804P04001", GlbCompany.CurrentCompany.PK));
		}

		public override void TestGetCusStatementHeader()
		{
			var header = Factory.New<CusStatementHeader>();
			header.B2_EntryFilerCode = "XJ5";
			header.B2_ProcessPort = "8888";
			header.B2_StatementNumber = "8804P04001";
			Factory.Save();

			AssertEquals(header.B2_StatementNumber, loader.LoadWithStatementNumber("XJ5", "8804P04001", GlbCompany.CurrentCompany.PK).B2_StatementNumber);
		}

		protected override string ApplicationIdentifier
		{
			get { return ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement; }
		}

		protected override string MessageText
		{
			get { return "B011704GFSMSF1709P06772061909713-262203600    1704GFS                           Q117091343381704GFS11-3584699000514090601090000000000000000000000               Q2000000000000000000000000000004719                                             QA014990000000295850100000001761                                                Q117091404231704GFS11-3584699000520090601090000000000000000000000               Q2000000000000000000000000000003239                                             QA024990000000250050100000000739                                                Q117091413431704GFS11-3584699000521090601090000000878500000000000               Q2000000000000000000000000000013885                                             QA034990000000319750100000001903                                                Q117091484151704GFS11-3584699000528090601090000004858400000000000               Q2000000000000000000000000000054196                                             QA044990000000351850100000002094                                                Q31709P06772061909061909GFS11-3584699000000005736900000000000                   Q4000000000000000000000000000076039                                             QE014990000001217350100000006497                                                Q51709P06772061909061909GFS11-3584699000000005736900000000000                   Q6000000000000000000000000000076039                                             QJ014990000001217350100000006497                                                Y  1704GFSMS00018"; }
		}

		protected override string StatementNumber
		{
			get { return "1709P06772"; }
		}

		protected override IncomingMessageProcessor GetNewIncomingMessageProcessor() => new USRIncomingMessageProcessor();

		protected override void SetUpTestData()
		{
			base.SetUpTestData();

			var dailyStatement1 = Factory.New<CusStatementHeader>();
			dailyStatement1.B2_StatementNumber = "1709134338";
			dailyStatement1.B2_EntryFilerCode = "GFS";
			dailyStatement1.B2_ProcessPort = "1704";

			var statementLine = dailyStatement1.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XXX";
			statementLine.B3_EntryNum = "20135761";
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine.B3_EntryProcessPort = dailyStatement1.B2_ProcessPort;

			var dailyStatement2 = Factory.New<CusStatementHeader>();
			dailyStatement2.B2_StatementNumber = "1709140423";
			dailyStatement2.B2_EntryFilerCode = "GFS";
			dailyStatement2.B2_ProcessPort = "1704";

			var dailyStatement3 = Factory.New<CusStatementHeader>();
			dailyStatement3.B2_StatementNumber = "1709141343";
			dailyStatement3.B2_EntryFilerCode = "GFS";
			dailyStatement3.B2_ProcessPort = "1704";

			var dailyStatement4 = Factory.New<CusStatementHeader>();
			dailyStatement4.B2_StatementNumber = "1709148415";
			dailyStatement4.B2_EntryFilerCode = "GFS";
			dailyStatement4.B2_ProcessPort = "1704";
		}

		protected override void SetUp()
		{
			base.SetUp();
			USCustomsDataRegistry.Instance.PeriodicMonthlyStatementsMessagesGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new GroupNotification(GroupNotification.StaffMemberOrNominatedGroup, groupZZ1.PK));
			Factory.Save();

			loader = new CusStatementHeader.Loader(Factory);
		}

		CusStatementHeader.Loader loader;
	}
}
