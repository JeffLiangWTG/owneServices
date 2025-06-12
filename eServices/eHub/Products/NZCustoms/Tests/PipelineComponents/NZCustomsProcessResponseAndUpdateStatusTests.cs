using System;
using System.IO;
using System.Text;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.Products.NZCustoms.PipelineComponents;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.Data.SqlClient;

namespace CargoWise.eHub.Products.NZCustoms.Tests
{
	[TestClass]
	public class NZCustomsProcessResponseAndUpdateStatusTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZCustomsProcessResponseAndUpdateStatu_Properties()
		{
			var component = new NZCustomsProcessResponseAndUpdateStatus();

			Assert.AreEqual("Process Response And Update Status", component.Description);
			Assert.AreEqual("Process Response And Update Status", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZCustomsProcessResponseAndUpdateStatus_Execute_Success()
		{
			var messageFactory = new MessageFactory();
			var message = messageFactory.CreateMessage();
			message.AddPart("body", messageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes("<SendLodgementResponse><SendLodgementResult><MessageTrackingID>9B1B97A4-EC1F-445B-A47B-970A02AA892D</MessageTrackingID><IsSuccess>true</IsSuccess><ErrorMessage></ErrorMessage></SendLodgementResult></SendLodgementResponse>"));
			message.Context = messageFactory.CreateMessageContext();

			var pipelineContext = new PipelineContext();

			var outboxAccessor = MockRepository.GenerateMock<IOutboxAccessor>();
			outboxAccessor.Expect(x => x.UpdateInboxMessageDistributionStatus("9B1B97A4-EC1F-445B-A47B-970A02AA892D")).Repeat.Once();

			var component = MockRepository.GeneratePartialMock<NZCustomsProcessResponseAndUpdateStatus>();
			component.Expect(x => x.GetOutboxAccessor()).Return(outboxAccessor);

			var outMsg = component.Execute(pipelineContext, message);

			Assert.IsNull(outMsg);
			component.VerifyAllExpectations();
			outboxAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZCustomsProcessResponseAndUpdateStatus_Execute_GeneralSecurityFault_Fail()
		{
			var messageFactory = new MessageFactory();
			var message = messageFactory.CreateMessage();
			message.Context = messageFactory.CreateMessageContext();
			message.AddPart("body", messageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes(@"<SendLodgementResponse><SendLodgementResult><MessageTrackingID>9B1B97A4-EC1F-445B-A47B-970A02AA892D</MessageTrackingID><IsSuccess>false</IsSuccess><ErrorMessage>General Security Fault 001
Please contact NZ Customs referencing message ID Id-db41305a0984d9723a7b70ae</ErrorMessage></SendLodgementResult></SendLodgementResponse>"));

			var pipelineContext = new PipelineContext();
			var origionalStream = message.BodyPart.Data;

			var exceptionsAccessor = MockRepository.GenerateMock<IExceptionsAccessor>();
			exceptionsAccessor.Expect(x => x.SubmitErrorAndUpdateStatus(Arg<Guid>.Is.Anything, Arg<string>.Is.Equal("NZC"), Arg<string>.Is.Equal("NZC"),
				Arg<string>.Is.Equal(@"General Security Fault 001
Please contact NZ Customs referencing message ID Id-db41305a0984d9723a7b70ae"), Arg<Guid>.Is.Equal(Guid.Empty), Arg<Guid>.Is.Equal(Guid.Empty),
				Arg<Guid>.Is.Equal(Guid.Parse("9B1B97A4-EC1F-445B-A47B-970A02AA892D")), Arg<Guid>.Is.Equal(Guid.Empty), Arg<SqlConnection>.Is.Null)).Repeat.Once();

			var component = MockRepository.GeneratePartialMock<NZCustomsProcessResponseAndUpdateStatus>();
			component.Expect(x => x.GetExceptionsAccessor()).Return(exceptionsAccessor);
			
			var outMsg = component.Execute(pipelineContext, message);

			Assert.IsNull(outMsg);
			exceptionsAccessor.VerifyAllExpectations();
			component.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void NZCustomsProcessResponseAndUpdateStatus_Execute_InvalidResponse_Fail()
		{
			var messageFactory = new MessageFactory();
			var message = messageFactory.CreateMessage();
			message.Context = messageFactory.CreateMessageContext();
			message.AddPart("body", messageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes("<WeirdFormat/>"));

			var pipelineContext = new PipelineContext();
			var origionalStream = message.BodyPart.Data;

			var component = MockRepository.GeneratePartialMock<NZCustomsProcessResponseAndUpdateStatus>();

			try
			{
				var outMsg = component.Execute(pipelineContext, message);
				Assert.Fail("Expect ArgumentException");
			}
			catch (ArgumentException ex)
			{
				Assert.AreEqual("SendLodgementResponse/SendLodgementResult/IsSuccess is expected in SOAP response but could not be found!", ex.Message);
			}
		}
	}

	public class NZCustomsProcessResponseAndUpdateStatusTest : NZCustomsProcessResponseAndUpdateStatus
	{
		public override IOutboxAccessor GetOutboxAccessor()
		{
			var stubOutboxAccessor = MockRepository.GenerateMock<IOutboxAccessor>();
			stubOutboxAccessor.Expect(x => x.UpdateInboxMessageDistributionStatus("9B1B97A4-EC1F-445B-A47B-970A02AA892D")).Repeat.Any();

			return stubOutboxAccessor;
		}
	}
}
