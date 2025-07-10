using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging.Testing
{
	[TestedType(typeof(GHACapture))]
	internal class GHACaptureTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidateGHA()
		{
			CombineAssertions(() =>
			{
				var capture = GetNewGHACapture();
				capture.ValidateGHA();
				AssertHasError("Empty GHA", capture.GHAInfo, "Please select a GHA.");
				capture.GHA = "Test";
				AssertHasError("Invalid GHA", capture.GHAInfo, "Please enter a valid GHA from the list.");
			}

			);
		}

		public void TestGHAMaxLength()
		{
			var capture = GetNewGHACapture();
			AssertExceptionThrown<MaxLengthExceededException>(() => capture.GHA = "Longer than 4 chars");
			ErrorReporter.Clear();
		}

		public void TestGHAInfo()
		{
			GHACapture capture = GetNewGHACapture();
			AssertEquals(4, capture.GHAInfo.MaxLength);
			AssertEquals("GHA", capture.GHAInfo.Name);
		}

		public void TestSubmittedTo()
		{
			GHACapture capture = GetNewGHACapture(CreateTestShipmentStatusArray1());
			AssertEquals("SATS", capture.SubmittedTo);
			capture = GetNewGHACapture(CreateTestShipmentStatusArray2());
			AssertEquals("CIAS", capture.SubmittedTo);
			capture = GetNewGHACapture(CreateTestShipmentStatusArray3());
			AssertEquals("-", capture.SubmittedTo);
			capture = GetNewGHACapture(CreateTestShipmentStatusArray4());
			AssertEquals("MIXED", capture.SubmittedTo);
			capture = GetNewGHACapture(CreateTestShipmentStatusArray5());
			AssertEquals("MIXED", capture.SubmittedTo);
		}

		public void TestSubmittedToInfo()
		{
			GHACapture capture = GetNewGHACapture();
			AssertEquals("SubmittedTo", capture.SubmittedToInfo.Name);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewGHACapture();
		}

		GHACapture GetNewGHACapture(params CMDShipmentWrapper[] cMDShipments)
		{
			return new GHACapture(new CMDWrapperBizOForTest(cMDShipments));
		}

		#region CreatingCMDShipmentStatusArray
		CMDShipmentWrapper[] CreateTestShipmentStatusArray1()
		{
			CMDShipmentWrapper[] result = new CMDShipmentWrapper[1];
			result[0] = new CMDShipmentWrapper(CreateTestShipment());
			AddMessage(result[0].Messages, "CMA\r\nSATS\r\nCMA");
			return result;
		}

		CMDShipmentWrapper[] CreateTestShipmentStatusArray2()
		{
			CMDShipmentWrapper[] result = new CMDShipmentWrapper[2];
			result[0] = new CMDShipmentWrapper(CreateTestShipment());
			AddMessage(result[0].Messages, "CMA\r\nCIAS\r\nCMA");
			result[1] = new CMDShipmentWrapper(CreateTestShipment());
			AddMessage(result[1].Messages, "CMA\r\nCIAS\r\nCMA");
			return result;
		}

		CMDShipmentWrapper[] CreateTestShipmentStatusArray3()
		{
			CMDShipmentWrapper[] result = new CMDShipmentWrapper[3];
			result[0] = new CMDShipmentWrapper(CreateTestShipment());
			AddMessage(result[0].Messages, "CMA\r\n\r\nCMA");
			result[1] = new CMDShipmentWrapper(CreateTestShipment());
			AddMessage(result[1].Messages, null);
			result[2] = new CMDShipmentWrapper(CreateTestShipment());
			return result;
		}

		CMDShipmentWrapper[] CreateTestShipmentStatusArray4()
		{
			CMDShipmentWrapper[] result = new CMDShipmentWrapper[2];
			result[0] = new CMDShipmentWrapper(CreateTestShipment());
			AddMessage(result[0].Messages, "CMA\r\nCIAS\r\nCMA");
			result[1] = new CMDShipmentWrapper(CreateTestShipment());
			AddMessage(result[1].Messages, "CMA\r\nSATS\r\nCMA");
			return result;
		}

		CMDShipmentWrapper[] CreateTestShipmentStatusArray5()
		{
			CMDShipmentWrapper[] result = new CMDShipmentWrapper[2];
			result[0] = new CMDShipmentWrapper(CreateTestShipment());
			AddMessage(result[0].Messages, "CMA\r\n\r\nCMA");
			result[1] = new CMDShipmentWrapper(CreateTestShipment());
			AddMessage(result[1].Messages, "CMA\r\nSATS\r\nCMA");
			return result;
		}

		ForwardingShipment CreateTestShipment()
		{
			return Factory.New<ForwardingShipment>();
		}

		void AddMessage(CMDEDIMessageCollection messages, ZString replyText)
		{
			CMDEDIMessage message = messages.AddNew();
			message.EM_IsActive = true;
			message.EM_MessageSubType = "GHA";
			if (!replyText.IsEmpty)
			{
				message.Reply = new CMDInbound(replyText);
			}
		}

		#endregion
		#endregion
		#region class CMDWrapperBizOForTest
		class CMDWrapperBizOForTest : CMDWrapperBase
		{
			public CMDWrapperBizOForTest(params CMDShipmentWrapper[] cMDShipments) : base(new BusinessObjectFactory())
			{
				fCMDShipments = cMDShipments;
			}

			public override CMDShipmentWrapper[] CMDShipments
			{
				get
				{
					return fCMDShipments;
				}
			}

			readonly CMDShipmentWrapper[] fCMDShipments;
			#region Not Implemented
			public override void SendMessage(INotifications notifications)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public override void DeleteExistingCMDMessages(INotifications notifications)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public override void SendMessage(string recipient, INotifications notifications)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public override void RunPreSendValidation(INotifications notifications)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public override void RunPreDeleteValidation(INotifications notifications)
			{
				throw new Exception("The method or operation is not implemented.");
			}
			#endregion
		}
		#endregion
	}
}
