using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Output.Testing
{
	sealed class CMQR0Test : TestCaseWithFactory
	{
		public void TestCMQR0()
		{
			CMQR0 cmq = new CMQR0();
			cmq.EntryFilerCode = "XJ5";
			cmq.EntryNumber = "123456";
			AssertEquals("XJ5123456", ((IStatusesAndErrors)cmq).ReferenceNumber);
		}

		public void TestCMQR5()
		{
			CMQR5 cmq = new CMQR5();
			cmq.DispositionActionCode = "22";
			cmq.NarrativeMessage = "Entry Document Required";
			cmq.DispositionActionDate = ZDateTime.BrettsBirthday.Date;
			cmq.DispositionActionTime = " 123";

			AssertEquals("22", ((IDispositionDetailProvider)cmq).DispositionCode);
			AssertEquals("Entry Document Required", ((IDispositionDetailProvider)cmq).NarrativeMessage);
			AssertEquals(ZDateTime.BrettsBirthday.Date.AddHours(1).AddMinutes(23), ((IDispositionDetailProvider)cmq).DispositionDateTime);

			cmq.ReleaseOrigin = ReleaseOriginCodeList.Codes.SelectivityProcessingDate;
			cmq.ReleaseDate = ZDateTime.BrettsBirthday.Date.AddDays(1);
			cmq.Sequence = 12;
			cmq.Quantity = 20;
			AssertEquals("01", ((IReleaseDetailProvider)cmq).ReleaseOrigin);
			AssertEquals(ZDateTime.BrettsBirthday.Date.AddDays(1), ((IReleaseDetailProvider)cmq).ReleaseDateTime);
			AssertEquals(12, ((IReleaseDetailProvider)cmq).Sequence);
			AssertEquals(20, ((IReleaseDetailProvider)cmq).Quantity);
		}

		public void TestCMQSD()
		{
			CMQSD cmq = new CMQSD();
			cmq.DispositionCode = "22";
			cmq.NarrativeMessage = "Entry Document Required";
			cmq.DispositionActionDate = ZDateTime.BrettsBirthday.Date;
			cmq.DispositionActionTime = " 123";

			AssertEquals("22", ((IDispositionDetailProvider)cmq).DispositionCode);
			AssertEquals("Entry Document Required", ((IDispositionDetailProvider)cmq).NarrativeMessage);
			AssertEquals(ZDateTime.BrettsBirthday.Date.AddHours(1).AddMinutes(23), ((IDispositionDetailProvider)cmq).DispositionDateTime);
		}

		public void TestCMQS4()
		{
			CMQS4 cmq = new CMQS4();
			cmq.InbondArrivalDate = ZDateTime.BrettsBirthday.Date;
			AssertEquals(ZDateTime.BrettsBirthday.Date, ((IInBondStatusProvider)cmq).InBondArrivalDate);

			cmq.InbondExportDate = ZDateTime.BrettsBirthday.Date.AddDays(1);
			AssertEquals(ZDateTime.BrettsBirthday.Date.AddDays(1), ((IInBondStatusProvider)cmq).InBondExportDate);

			cmq.InbondStatus = 1;
			AssertEquals("1", ((IInBondStatusProvider)cmq).InBondStatus);
		}

		public void TestCMQS5()
		{
			CMQS5 cmq = new CMQS5();
			cmq.InbondArrivalDate = ZDateTime.BrettsBirthday.Date;
			AssertEquals(ZDateTime.BrettsBirthday.Date, ((IInBondStatusProvider)cmq).InBondArrivalDate);

			cmq.InbondExportDate = ZDateTime.BrettsBirthday.Date.AddDays(1);
			AssertEquals(ZDateTime.BrettsBirthday.Date.AddDays(1), ((IInBondStatusProvider)cmq).InBondExportDate);

			cmq.InbondStatus = 1;
			AssertEquals("1", ((IInBondStatusProvider)cmq).InBondStatus);
		}

		public void TestCMQSA()
		{
			CMQSA cmq = new CMQSA();
			cmq.InbondNumber = "1";
			AssertEquals("IT Number: 1", ((IStatusesAndErrors)cmq).ReferenceNumber);

			cmq.MasterBillNumber = "2";
			AssertEquals("IT Number: 1, Master Bill Number: 2", ((IStatusesAndErrors)cmq).ReferenceNumber);

			cmq.IssuerCodeOfMasterBillNumber = "ABCD";
			AssertEquals("IT Number: 1, Master Bill Number: 2, Issued By: ABCD", ((IStatusesAndErrors)cmq).ReferenceNumber);
		}

		public void TestCMQSB()
		{
			CMQSB cmq = new CMQSB();
			cmq.AirWaybillNumber = "1";
			AssertEquals("AWB: 1", ((IStatusesAndErrors)cmq).ReferenceNumber);

			cmq.HouseAirWaybillNumber = "2";
			AssertEquals("AWB: 1, HAWB: 2", ((IStatusesAndErrors)cmq).ReferenceNumber);
		}

		public void TestRR_R5Blocks()
		{
			var testMessage = Factory.New<MQEDIMessage>();
			testMessage.EM_Status = EDIMessage.Status.Received;
			testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			testMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			testMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults;
			testMessage.EM_MessageText = "B012704OHLRR                                                                    R18888XJ5 010000480191-013199000B00001004FTRDADMIRALENGRACHT     1356 061307    R4            13465865424                         00001000KG   AAAD             R5042409144222RELEASE DATE UPDATE                     04280902                  R5081011144416PENDING CUSTOMS REVIEW                                            R6FDA    0810111444  FDA MAY PROCEED                  07001 001THRU001 001      Y  2704XJ5RR00004";

			var r5Blocks = testMessage.MessageBlock.MessageBlocks.OfType<CMQR5>();
			foreach (var r5 in r5Blocks)
			{
				if (r5.NarrativeMessage.Contains("PENDING CUSTOMS REVIEW"))
				{
					AssertEquals("10-Aug-11 14:44", ((IDispositionDetailProvider)r5).DispositionDateTime.ToLongTimeString());
				}
				if (r5.NarrativeMessage.Contains("RELEASE DATE UPDATE"))
				{
					AssertEquals("24-Apr-09 14:42", ((IDispositionDetailProvider)r5).DispositionDateTime.ToLongTimeString());
				}
			}
		}
	}
}
