using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class LineReleaseMessageProcessorTest : ABIProcessorTest<LineReleaseMessageProcessor, APLA, APLB, APLY>
	{
		protected override void EndToEndCore()
		{
			using (Culture.SetTemporarily(System.Globalization.CultureInfo.InvariantCulture))
			{
				JobDeclaration dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.JE_DeclarationReference = "B00012345";
				CusEntryHeader entry = dec.CustomsEntryHeaders.AddNew();
				entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
				entry.EntryNumber = "16523736";

				LineReleaseMQEDIMessage message = Factory.New<LineReleaseMQEDIMessage>();
				message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				message.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
				message.EM_MessageNum = "~15000";
				message.EM_MessageText =
"B018888XJ5XR                                                                    " +
"X10A95-2041006008888XJ5 165237361206051019L89                                   " +
"X20870421    870490    HON2AMED112TRK01        00000008NUMXOHONCAN715SCA        " +
"X20870323              HON2AMED112AUU01        00000007NO XOHONCAN715SCA        " +
"X25CPRS053390088346                00000015                                     " +
"X400001000100000002                                                             " +
"Y018888XJ5XR00005";
				Factory.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIIncomingMessageProcessor().ExecuteBatch();
				message.Reload();
				AssertEquals("IsComplete", true, message.IsComplete);
				AssertEquals("RCV", message.EM_Status);
				AssertEquals("16523736", message.EM_ApplicationReference);
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals(2, email.Recipients.Count);
				Assert(email.Recipients.Cast<RecipientDef>().Any(x => x.Email == staffZ1.GS_EmailAddress));
				Assert(email.Recipients.Cast<RecipientDef>().Any(x => x.Email == staffZ2.GS_EmailAddress));
				string expectedBodyMessage = @"<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Status</th><th>Importer Of Record</th><th>District/Port Of Entry</th><th>Entry Filer Code</th><th>Entry Number</th><th>Release Date</th><th>FIRMS Code</th></tr></thead><tr><td>Accepted</td><td>95-204100600</td><td>8888</td><td>XJ5</td><td>16523736</td><td>06-Dec-05 10:19:00</td><td>L89</td></tr></table><BR /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>1st Tarriff</th><th>2nd Tariff</th><th>Common Commondity Classification Code</th><th>1st Country Of Origin</th><th>2nd Country Of Origin</th><th>3rd Country Of Origin</th><th>4th Country Of Origin</th><th>5th Country Of Origin</th><th>Quantity</th><th>Unit Of Measure</th><th>Manufacturer/Supplier Code</th></tr></thead><tr><td>870421</td><td>870490</td><td>HON2AMED112TRK</td><td>01</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>8</td><td>NUM</td><td>XOHONCAN715SCA</td></tr><tr><td>870323</td><td>&nbsp;</td><td>HON2AMED112AUU</td><td>01</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>7</td><td>NO</td><td>XOHONCAN715SCA</td></tr></table><BR /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill of Lading</th><th>House Bill</th><th>Sub-House Bill</th><th>Quantity</th><th>Transport Mode</th></tr></thead><tr><td>CPRS053390088346</td><td>&nbsp;</td><td>&nbsp;</td><td>15</td><td>&nbsp;</td></tr></table><BR /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Error Message</th></tr></thead><tr><td>1</td><td>0</td><td>1</td><td>2</td></tr></table><BR />
<br />
";
				AssertEmailResponse(Env.OutgoingCustomsMailManager.EmailsCreated[0], "Border Line Release Response for B00012345 / XJ5-1652373-6",
					expectedBodyMessage, ControllerIDs.Customs.JobDeclaration, dec.PK, new string[] { staffZ1.GS_EmailAddress, staffZ2.GS_EmailAddress });

				entry.Messages.Load();
				AssertEquals("Don't link to existing entry", false, entry.Messages.Contains(message));
			}
		}

		public void TestProcessingNewBorderLineRelease()
		{
			using (Culture.SetTemporarily(System.Globalization.CultureInfo.InvariantCulture))
			{
				LineReleaseMQEDIMessage message = Factory.New<LineReleaseMQEDIMessage>();
				message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				message.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
				message.EM_MessageNum = "~15000";
				message.EM_MessageText =
"B018888XJ5XR                                                                    " +
"X10A95-2041006008888XJ5 165237361206051019L89                                   " +
"X20870421    870490    HON2AMED112TRK01        00000008NUMXOHONCAN715SCA        " +
"X20870323              HON2AMED112AUU01        00000007NO XOHONCAN715SCA        " +
"X25CPRS053390088346                00000015                                     " +
"X400001000100000002                                                             " +
"Y018888XJ5XR00005";
				Factory.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIIncomingMessageProcessor().ExecuteBatch();
				message.Reload();
				AssertEquals("IsComplete", false, message.IsComplete);
				AssertEquals("RCV", message.EM_Status);
				AssertEquals("16523736", message.EM_ApplicationReference);
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				string expectedBodyMessage = @"<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Status</th><th>Importer Of Record</th><th>District/Port Of Entry</th><th>Entry Filer Code</th><th>Entry Number</th><th>Release Date</th><th>FIRMS Code</th></tr></thead><tr><td>Accepted</td><td>95-204100600</td><td>8888</td><td>XJ5</td><td>16523736</td><td>06-Dec-05 10:19:00</td><td>L89</td></tr></table><BR /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>1st Tarriff</th><th>2nd Tariff</th><th>Common Commondity Classification Code</th><th>1st Country Of Origin</th><th>2nd Country Of Origin</th><th>3rd Country Of Origin</th><th>4th Country Of Origin</th><th>5th Country Of Origin</th><th>Quantity</th><th>Unit Of Measure</th><th>Manufacturer/Supplier Code</th></tr></thead><tr><td>870421</td><td>870490</td><td>HON2AMED112TRK</td><td>01</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>8</td><td>NUM</td><td>XOHONCAN715SCA</td></tr><tr><td>870323</td><td>&nbsp;</td><td>HON2AMED112AUU</td><td>01</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>7</td><td>NO</td><td>XOHONCAN715SCA</td></tr></table><BR /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill of Lading</th><th>House Bill</th><th>Sub-House Bill</th><th>Quantity</th><th>Transport Mode</th></tr></thead><tr><td>CPRS053390088346</td><td>&nbsp;</td><td>&nbsp;</td><td>15</td><td>&nbsp;</td></tr></table><BR /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Error Message</th></tr></thead><tr><td>1</td><td>0</td><td>1</td><td>2</td></tr></table><BR />
<br />
";
				AssertEquals("US_ImporterNumber", "95-204100600", message.US_ImporterNumber);
				AssertEquals("US_ReleaseDateTime", new ZDateTime(2005, 12, 6, 10, 19, 0), message.US_ReleaseDateTime);
				AssertEquals("US_BillOfLading", "CPRS053390088346", message.US_BillOfLading);
				AssertEquals("US_PortCode", "8888", message.US_PortCode);
				AssertEquals("US_SupplierCode", "XOHONCAN715SCA", message.US_SupplierCode);
				AssertEmailResponse(Env.OutgoingCustomsMailManager.EmailsCreated[0], "Border Line Release Response for XJ5-1652373-6", expectedBodyMessage,
					ControllerIDs.Customs.US.BorderLineReleaseMessage, message.PK, new string[] { staffZ1.GS_EmailAddress, staffZ2.GS_EmailAddress });
			}
		}

		public void TestProcessingNewBorderLineRelease_AutoDecCreation_RegistryNotEnabled()
		{
			DataRegistry.Business.USCustomsDataRegistry.Instance.AutoCreateDeclarationFromLineRelease.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			using (Culture.SetTemporarily(System.Globalization.CultureInfo.InvariantCulture))
			{
				LineReleaseMQEDIMessage message = Factory.New<LineReleaseMQEDIMessage>();
				message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				message.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
				message.EM_MessageNum = "~15000";
				message.EM_MessageText =
					"B018888XJ5XR                                                                    " +
					"X10A95-2041006008888XJ5 165237361206051019L89                                   " +
					"X20870421    870490    HON2AMED112TRK01        00000008NUMXOHONCAN715SCA        " +
					"X20870323              HON2AMED112AUU01        00000007NO XOHONCAN715SCA        " +
					"X25CPRS053390088346                00000015                                     " +
					"X400001000100000002                                                             " +
					"Y018888XJ5XR00005";
				Factory.Save();

				new ABIIncomingMessageProcessor().ExecuteBatch();
				message.Reload();

				var query = new ZQuery(JobDeclarationSchema.JE_MasterBill, "053390088346");
				var declaration = Factory.LoadTop1<JobDeclaration>(query);

				AssertNull("There should be no declaration created when the registry is set to false", declaration);
			}
		}

		public void TestProcessingNewBorderLineRelease_AutoDecCreation_NoPortMatch()
		{
			DataRegistry.Business.USCustomsDataRegistry.Instance.AutoCreateDeclarationFromLineRelease.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			using (Culture.SetTemporarily(System.Globalization.CultureInfo.InvariantCulture))
			{
				LineReleaseMQEDIMessage message = Factory.New<LineReleaseMQEDIMessage>();
				message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				message.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
				message.EM_MessageNum = "~15000";
				message.EM_MessageText =
					"B018888XJ5XR                                                                    " +
					"X10A95-2041006008888XJ5 165237361206051019L89                                   " +
					"X20870421    870490    HON2AMED112TRK01        00000008NUMXOHONCAN715SCA        " +
					"X20870323              HON2AMED112AUU01        00000007NO XOHONCAN715SCA        " +
					"X25CPRS053390088346                00000015                                     " +
					"X400001000100000002                                                             " +
					"Y018888XJ5XR00005";
				Factory.Save();

				new ABIIncomingMessageProcessor().ExecuteBatch();
				message.Reload();

				var query = new ZQuery(JobDeclarationSchema.JE_MasterBill, "053390088346");
				var declaration = Factory.LoadTop1<JobDeclaration>(query);

				AssertNotNull("There should be a declaration created when the registry is set to true", declaration);
				AssertEquals("Without port match, the declaration should be created against the current branch", GlbBranch.CurrentBranch.PK, declaration.JE_GB);
			}
		}

		public void TestProcessingNewBorderLineRelease_AutoDecCreation_PortMatch()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1113", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			DataRegistry.Business.USCustomsDataRegistry.Instance.AutoCreateDeclarationFromLineRelease.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;

			var coll = new BorderCargoPortCollection();
			var port = coll.AddNew();
			port.PortCode = "1113";
			port.CRProcess = "1-Step";
			port.Location = "S";
			DataRegistry.Business.USCustomsDataRegistry.Instance.BorderCargoReleasePorts.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, coll);

			using (Culture.SetTemporarily(System.Globalization.CultureInfo.InvariantCulture))
			{
				LineReleaseMQEDIMessage message = Factory.New<LineReleaseMQEDIMessage>();
				message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				message.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
				message.EM_MessageNum = "~15000";
				message.EM_MessageText =
					"B018888XJ5XR                                                                    " +
					"X10A95-2041006001113XJ5 165237361206051019L89                                   " +
					"X20870421    870490    HON2AMED112TRK01        00000008NUMXOHONCAN715SCA        " +
					"X20870323              HON2AMED112AUU01        00000007NO XOHONCAN715SCA        " +
					"X25CPRS053390088346                00000015                                     " +
					"X400001000100000002                                                             " +
					"Y018888XJ5XR00005";
				Factory.Save();

				new ABIIncomingMessageProcessor().ExecuteBatch();
				message.Reload();

				var query = new ZQuery(JobDeclarationSchema.JE_MasterBill, "053390088346");
				var declaration = Factory.LoadTop1<JobDeclaration>(query);

				AssertNotNull("There should be a declaration created when the registry is set to true", declaration);
				AssertEquals("With port match, the declaration should be created against the matching branch", branch.PK, declaration.JE_GB);
			}
		}

		void AssertEmailResponse(EmailDef email, string expectedSubject, string expectedHtml, ControllerID expectedUrlControllerID, ZGuid expectedUrlPK, string[] expectedRecipients)
		{
			AssertEquals("Subject", expectedSubject, email.Subject);
			foreach (var receipient in expectedRecipients)
			{
				AssertCollectionContains(receipient, email.Recipients.ToStringCollection());
			}
			AssertContains(expectedHtml, email.Body);
			AssertContains(ObjectFactory.Get<IShowEditFormUrlCreator>().Create(expectedUrlControllerID, expectedUrlPK.ToGuid()), email.Body);
		}

		protected override void SetUp()
		{
			base.SetUp();
			USCustomsDataRegistry.Instance.BorderLineReleaseMessagesGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, groupZZ1.PK.ToGuid());
		}
	}
}
