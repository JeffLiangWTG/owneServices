using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterpise.Customs.TW.DataTransfer.Testing
{
	public sealed class NCATKUniversalMessagingHelperTest : JobDeclarationUniversalMessagingHelperTest
	{
		public void TestEventReference()
		{
			using (Factory.AddDisposableService())
			{
				CombineAssertions(() =>
				{
					AssertEventReference(ControllingMessageTypeList.Codes.X101);
					AssertEventReference("X102");
					AssertEventReference(ControllingMessageTypeList.Codes.NX401);
					AssertEventReference(ControllingMessageTypeList.Codes.NX601);
				});
			}
		}

		void AssertEventReference(ZString expectedMST)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Enterprise.Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var messageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = expectedMST;
			Factory.Save();
			var wrapper = new NCATKMessageSendingObjectParent(declaration, expectedMST);
			var helper = new NCATKUniversalMessagingHelper(wrapper);
			foreach (NCATKMessageSendingObject sendingObject in wrapper.SendingObjectsCollection)
			{
				sendingObject.ShouldSend = true;
			}

			helper.SendUniversalMessage();
			Factory.Save();
			var createdMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, messageHeader.PK));
			var interchange = createdMessage.Interchange;
			var expectedEventReference = $"<EventReference>MST={expectedMST}|RFN={createdMessage.EM_MessageNum}</EventReference>";
			AssertTextContainsDispiteBlanks(expectedEventReference, interchange.EI_BodyText);
		}

		[ExpectNoExceptions]
		public void TestSelectedSendingObjectsHeaders()
		{
			var controllingMessageType = ControllingMessageTypeList.Codes.X101;
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = controllingMessageType;
			var wrapper = new NCATKMessageSendingObjectParent(declaration, controllingMessageType);
			var helper = new NCATKUniversalMessagingHelperForTest(wrapper);
			foreach (NCATKMessageSendingObject sendingObject in wrapper.SendingObjectsCollection)
			{
				sendingObject.ShouldSend = false;
			}

			NUnit.Framework.Assert.That(!helper.SelectedMessageSendingObjects.Any(), Is.True);
			foreach (NCATKMessageSendingObject sendingObject in wrapper.SendingObjectsCollection)
			{
				sendingObject.ShouldSend = true;
			}

			NUnit.Framework.Assert.That(helper.SelectedMessageSendingObjects.Any(x => x == messageHeader), Is.True);
			NUnit.Framework.Assert.That(helper.SelectedMessageSendingObjects.Any(x => x == messageHeader), Is.True);
		}

		[ExpectNoExceptions]
		public void TestGetPKsToPopulate()
		{
			var controllingMessageType = ControllingMessageTypeList.Codes.X101;
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = controllingMessageType;
			var messageHeader1 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader1.TW1_ControllingMessageType = "X102";
			var wrapper = new NCATKMessageSendingObjectParent(declaration, controllingMessageType);
			var helper = new NCATKUniversalMessagingHelperForTest(wrapper);
			var configuraion = helper.GetDeclarationDataObjectWriterConfiguraionForTest(messageHeader);
			NUnit.Framework.Assert.That(configuraion, Is.TypeOf<NCATKDataObjectWriterConfiguration>());
			var twConfiguraion = (NCATKDataObjectWriterConfiguration)configuraion;
			NUnit.Framework.Assert.That(twConfiguraion.CAHeaderPKsToPopulate.Any(x => x == messageHeader.PK), Is.True);
			NUnit.Framework.Assert.That(!twConfiguraion.CAHeaderPKsToPopulate.Any(x => x == messageHeader1.PK), Is.True);
			NUnit.Framework.Assert.That(twConfiguraion.EntryHeaderPKsToPopulate.Any(x => x == entryHeader.PK), Is.True);
		}

		[TestDate(2019, 7, 23, 5, 6, 2)]
		[ExpectNoExceptions]
		public void TestMessageAndInterchangeForX101()
		{
			using (Factory.AddDisposableService())
			{
				var controllingMessageType = ControllingMessageTypeList.Codes.X101;
				var messageHeader0 = entryInstruction.ControllingMessageHeaders.AddNew();
				messageHeader0.TW1_ControllingMessageType = "X102";
				messageHeader0.TW1_FunctionalReferenceId = "CAH000";
				messageHeader0.TW1_CertificateType = "01";
				var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
				messageHeader.TW1_ControllingMessageType = controllingMessageType;
				Factory.Save();
				var wrapper = new NCATKMessageSendingObjectParent(declaration, controllingMessageType);
				var helper = new NCATKUniversalMessagingHelper(wrapper);
				wrapper.SendingObjectsCollection.Cast<NCATKMessageSendingObject>().First(x => x.MessageType == controllingMessageType).ShouldSend = true;
				messageHeader.TW1_FunctionalReferenceId = "CAH001";
				messageHeader.TW1_CertificateType = "01";
				helper.SendUniversalMessage();
				Factory.Save();
				CombineAssertions(() =>
				{
					var createdMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, messageHeader.PK));
					NUnit.Framework.Assert.That(createdMessage.EM_ApplicationCode, Is.EqualTo("UDM").Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(createdMessage.EM_MessageType, Is.EqualTo("XUS").Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(createdMessage.EM_MessageSubType, Is.EqualTo("XUS").Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(createdMessage.EM_ReceiveTransmit, Is.EqualTo("TRX").Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(createdMessage.EM_Status, Is.EqualTo("SNT").Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(createdMessage.EM_MessageNum, Is.Not.EqualTo(default(ZString)));
					NUnit.Framework.Assert.That(createdMessage.EM_LinkUniqueID, Is.EqualTo(messageHeader.PK));
					NUnit.Framework.Assert.That(createdMessage.EM_LinkTable, Is.EqualTo("CusTWControllingMessageHeader").Using(CustomComparers.TypeComparison));
					var interchange = createdMessage.Interchange;
					NUnit.Framework.Assert.That(interchange.EI_ApplicationCode, Is.EqualTo("UDM").Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(interchange.EI_InterchangeType, Is.EqualTo("XUS").Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(interchange.EI_ReceiveTransmit, Is.EqualTo("TRX").Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(interchange.EI_To, Is.EqualTo("TWCustoms").Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(interchange.EI_InterchangeNum, Is.Not.EqualTo(default(ZString)), "createdMessage.EM_InterchangeNumber - should not be [null]");
					NUnit.Framework.Assert.That(interchange.EI_Status, Is.EqualTo("HQU").Using(CustomComparers.TypeComparison));
					var currentTime = ZDateTime.UtcNow.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
					var delivery = $"<EDIDelivery><FileName>X101.01.CAH001.{currentTime}.xml</FileName><EmailSubject></EmailSubject></EDIDelivery>";
					NUnit.Framework.Assert.That(interchange.EI_HeaderText, Is.EqualTo(delivery).Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(interchange.EI_BodyText.ToString(), Does.Contain("<Value>CAH001</Value>"), "has ControllingMessageHeader CAH001");
					NUnit.Framework.Assert.That(interchange.EI_BodyText.ToString(), Does.Not.Contain("<Value>CAH000</Value>"), "not has ControllingMessageHeader CAH000");
					NUnit.Framework.Assert.That(entryHeader.EntryNumber, Is.Not.EqualTo(default(ZString)));
					NUnit.Framework.Assert.That(interchange.EI_BodyText.ToString(), Does.Contain($"<Number>{entryHeader.EntryNumber}</Number>"), "has CusEntryHeader");
				});
			}
		}

		[TestDate(2019, 7, 23, 5, 6, 2)]
		[ExpectNoExceptions]
		public void TestMessageAndInterchangeForNX401()
		{
			using (Factory.AddDisposableService())
			{
				var controllingMessageType = ControllingMessageTypeList.Codes.NX401;
				var businessType = "40";
				var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
				messageHeader.TW1_ControllingAgency = "VP";
				messageHeader.TW1_ControllingMessageType = controllingMessageType;
				messageHeader.TW1_BusinessType = businessType;
				Factory.Save();
				messageHeader.TW1_FunctionalReferenceId = "69780234SW2005270131";
				AssertMessageAndInterchangeWithBusinessType(controllingMessageType, businessType, messageHeader);
			}
		}

		[TestDate(2019, 7, 23, 5, 6, 2)]
		[ExpectNoExceptions]
		public void TestMessageAndInterchangeForNX601()
		{
			using (Factory.AddDisposableService())
			{
				var controllingMessageType = ControllingMessageTypeList.Codes.NX601;
				var businessType = "01";
				var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
				messageHeader.TW1_ControllingAgency = "DH";
				messageHeader.TW1_ControllingMessageType = controllingMessageType;
				messageHeader.TW1_BusinessType = businessType;
				Factory.Save();
				messageHeader.TW1_FunctionalReferenceId = "69780234SW2005270131";
				AssertMessageAndInterchangeWithBusinessType(controllingMessageType, businessType, messageHeader);
			}
		}

		[ExpectNoExceptions]
		void AssertMessageAndInterchangeWithBusinessType(string controllingMessageType, string businessType, CusTWControllingMessageHeader messageHeader)
		{
			var wrapper = new NCATKMessageSendingObjectParent(declaration, controllingMessageType);
			var helper = new NCATKUniversalMessagingHelper(wrapper);
			wrapper.SendingObjectsCollection.Cast<NCATKMessageSendingObject>().First(x => x.MessageType == controllingMessageType).ShouldSend = true;
			helper.SendUniversalMessage();
			Factory.Save();
			CombineAssertions(() =>
			{
				var createdMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, messageHeader.PK));
				NUnit.Framework.Assert.That(createdMessage.EM_ApplicationCode, Is.EqualTo("UDM").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(createdMessage.EM_MessageType, Is.EqualTo("XUS").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(createdMessage.EM_MessageSubType, Is.EqualTo("XUS").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(createdMessage.EM_ReceiveTransmit, Is.EqualTo("TRX").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(createdMessage.EM_Status, Is.EqualTo("SNT").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(createdMessage.EM_MessageNum, Is.Not.EqualTo(default(ZString)));
				NUnit.Framework.Assert.That(createdMessage.EM_LinkUniqueID, Is.EqualTo(messageHeader.PK));
				NUnit.Framework.Assert.That(createdMessage.EM_LinkTable, Is.EqualTo("CusTWControllingMessageHeader").Using(CustomComparers.TypeComparison));
				var interchange = createdMessage.Interchange;
				NUnit.Framework.Assert.That(interchange.EI_ApplicationCode, Is.EqualTo("UDM").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(interchange.EI_InterchangeType, Is.EqualTo("XUS").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(interchange.EI_IsActive, Is.EqualTo(ZBool.True));
				NUnit.Framework.Assert.That(interchange.EI_ReceiveTransmit, Is.EqualTo("TRX").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(interchange.EI_TransportType, Is.EqualTo("HUB").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(interchange.EI_To, Is.EqualTo("TWCustoms").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(interchange.EI_InterchangeNum, Is.Not.EqualTo(default(ZString)), "createdMessage.EM_InterchangeNumber - should not be [null]");
				NUnit.Framework.Assert.That(interchange.EI_Status, Is.EqualTo("HQU").Using(CustomComparers.TypeComparison));
				var currentTime = ZDateTime.UtcNow.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
				var delivery = $"<EDIDelivery><FileName>{controllingMessageType}.{businessType}.69780234SW2005270131.{currentTime}.xml</FileName><EmailSubject></EmailSubject></EDIDelivery>";
				NUnit.Framework.Assert.That(interchange.EI_HeaderText, Is.EqualTo(delivery).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(interchange.EI_BodyText.ToString(), Does.Contain("<Value>69780234SW2005270131</Value>"), "has ControllingMessageHeader 69780234SW2005270131");
				NUnit.Framework.Assert.That(entryHeader.EntryNumber, Is.Not.EqualTo(default(ZString)));
				NUnit.Framework.Assert.That(interchange.EI_BodyText.ToString(), Does.Contain($"<Number>{entryHeader.EntryNumber}</Number>"), "has CusEntryHeader");
			}

			);
		}

		[ExpectNoExceptions]
		public void TestLogDeclarationEvent()
		{
			var controllingMessageType = ControllingMessageTypeList.Codes.X101;
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = controllingMessageType;
			var messageHeader1 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader1.TW1_ControllingMessageType = "X102";
			var wrapper = new NCATKMessageSendingObjectParent(declaration, controllingMessageType, "test menu");
			var helper = new NCATKUniversalMessagingHelperForTest(wrapper);
			helper.UpdateSendingObjectHeaderStatusAfterCreatingEDIEnterchageForTest(messageHeader, ZString.Empty);
			var log = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.MessageSent.Code)).Single();
			NUnit.Framework.Assert.That(log.SL_Reference, Is.EqualTo("test menu").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var org2Code = orgHeader.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "00612348", Enterprise.Core.Constants.CountryCodes.Taiwan);
			var warehouseAddress = orgHeader.Addresses.AddNew();
			warehouseAddress.Address1 = "Address1";
			warehouseAddress.Address2 = "Address2";
			org2Code.OK_OA_PremisesAddress = orgHeader.MainAddress.PK;
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Enterprise.Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_CustomsOffice = "AA";
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			declaration.JE_CustomsOffice = "BB";
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "BB";
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_BoxNumber = "123";
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 01, 01);
			entryInstruction.CEI_OA_Warehouse2 = orgHeader.MainAddress.PK;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_Status = "AWC";
			entryHeader.AllocateEntryNumber();
			this.declaration = declaration;
			this.entryHeader = entryHeader;
			this.entryInstruction = entryInstruction;
		}

		Enterprise.Customs.TW.Business.CusEntryHeader entryHeader;
		Enterprise.Customs.TW.Business.CusEntryInstruction entryInstruction;
		JobDeclaration declaration;
		class NCATKUniversalMessagingHelperForTest : NCATKUniversalMessagingHelper
		{
			public NCATKUniversalMessagingHelperForTest(IJobDeclarationMessageSendingObjectParent messageSendingObject) : base(messageSendingObject)
			{
			}

			public IEnumerable<BusinessObject> SelectedMessageSendingObjects => GetSelectedMessageSendingObjects();

			public DeclarationDataObjectWriterConfiguration GetDeclarationDataObjectWriterConfiguraionForTest(BusinessObject header) => base.GetDeclarationDataObjectWriterConfiguraion(header);

			public void UpdateSendingObjectHeaderStatusAfterCreatingEDIEnterchageForTest(BusinessObject businessObject, ZString status) => UpdateSendingObjectHeaderAfterCreatingEDIEnterchage(businessObject, status);
		}
	}
}
