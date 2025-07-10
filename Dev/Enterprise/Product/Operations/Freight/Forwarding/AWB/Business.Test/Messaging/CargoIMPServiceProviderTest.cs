using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Messaging.Testing
{
	sealed class CargoIMPServiceProviderTest : TestCaseWithFactory
	{
		public void TestGetAirlineCodeBasedOnMAWBPrefix()
		{
			var awb = Factory.New<ExportAWBHeader>();
			awb.EH_AWBOriginCode = "SYD";
			awb.EH_By1st = "AA";
			awb.EH_WayBillNumber = "081-11111111";
			AssertEquals("AA", CargoIMPServiceProvider.GetAirlineCode(awb));

			using (ForwardingConfigurationRegistry.Instance.SendFWBOrFHLToAirlineBasedOnMAWBPrefix.SetTemporaryValue(GlbCompany.CurrentCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("QF", CargoIMPServiceProvider.GetAirlineCode(awb));
			}

			using (ForwardingConfigurationRegistry.Instance.SendFWBOrFHLToAirlineBasedOnMAWBPrefix.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("QF", CargoIMPServiceProvider.GetAirlineCode(awb));
			}
		}

		public void TestGetCargoIMPServiceProvider_CCN()
		{
			AssertEquals("CCN", CargoIMPServiceProvider.Get(CargoIMPServiceProvider.CCN.Code).Code);
			AssertEquals("QK", CargoIMPServiceProvider.Get(CargoIMPServiceProvider.CCN.Code).PriorityEnvelopeHeading);
			AssertEquals("CSGAGT85GHA", CargoIMPServiceProvider.Get(CargoIMPServiceProvider.CCN.Code).SystemAddress);
			AssertEquals(true, CargoIMPServiceProvider.Get(CargoIMPServiceProvider.CCN.Code).IsGoingViaCCN);
		}

		public void TestGetCargoIMPServiceProvider_Descartes()
		{
			AssertEquals("DESCARTES", CargoIMPServiceProvider.Get(CargoIMPServiceProvider.Descartes.Code).Code);
			AssertEquals("QK", CargoIMPServiceProvider.Get(CargoIMPServiceProvider.Descartes.Code).PriorityEnvelopeHeading);
			AssertEquals("", CargoIMPServiceProvider.Get(CargoIMPServiceProvider.Descartes.Code).SystemAddress);
			AssertEquals(false, CargoIMPServiceProvider.Get(CargoIMPServiceProvider.Descartes.Code).IsGoingViaCCN);
		}

		public void TestGetCargoIMPServiceProvider_BTviaCCN()
		{
			AssertEquals("BT via CCN", CargoIMPServiceProvider.Get(CargoIMPServiceProvider.BTviaCCN.Code).Code);
			AssertEquals("QP", CargoIMPServiceProvider.Get(CargoIMPServiceProvider.BTviaCCN.Code).PriorityEnvelopeHeading);
			AssertEquals("LONBCCR", CargoIMPServiceProvider.Get(CargoIMPServiceProvider.BTviaCCN.Code).SystemAddress);
			AssertEquals(true, CargoIMPServiceProvider.Get(CargoIMPServiceProvider.BTviaCCN.Code).IsGoingViaCCN);
		}

		public void TestGetCargoIMPServiceProvider_IATA()
		{
			AssertEquals("IATA", CargoIMPServiceProvider.Get(CargoIMPServiceProvider.IATA.Code).Code);
			AssertEquals("QP", CargoIMPServiceProvider.Get(CargoIMPServiceProvider.IATA.Code).PriorityEnvelopeHeading);
			AssertEquals("LONBCCR", CargoIMPServiceProvider.Get(CargoIMPServiceProvider.IATA.Code).SystemAddress);
			AssertEquals(false, CargoIMPServiceProvider.Get(CargoIMPServiceProvider.IATA.Code).IsGoingViaCCN);
		}

		public void TestGetCargoIMPServiceProvider_Other()
		{
			AssertEquals("CCN", CargoIMPServiceProvider.Get("XXX").Code);
		}

		[TestDate(2008, 12, 3, 2, 46, 13)]
		public void TestGetTransmissionMethodHeader()
		{
			var branchPK = Guid.NewGuid();
			DataRegistry.Instance.SetIssuingCarrierAgentIATACode(branchPK, "1234-45");

			ForwardingConfigurationRegistry.Instance.SenderPIMAAddress.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "XSPLXSQ");
			var awb = Factory.New<ExportAWBHeader>();
			awb.EH_AWBOriginCode = "SYD";
			awb.EH_By1st = "QF";

			var messageFWB = EDIMessageTestFactory.New(Factory);
			messageFWB.EM_MessageType = "FWB";
			messageFWB.EM_GB = branchPK;

			var messageFHL = EDIMessageTestFactory.New(Factory);
			messageFHL.EM_MessageType = "FHL";
			messageFHL.EM_GB = branchPK;

			string result = CargoIMPServiceProvider.CCN.GetTransmissionMethodHeader(awb, messageFWB, CargoIMPTransmissionMethod.SMTP);
			AssertEquals("FWB\r\nQFSYD\r\n", result);

			result = CargoIMPServiceProvider.CCN.GetTransmissionMethodHeader(awb, messageFWB, CargoIMPTransmissionMethod.eHub);
			AssertEquals(@"<CargoIMP xmlns=""http://cargowise.com/cargoimp/201108"">
<MessageType>FWB</MessageType>
<Priority>QK</Priority>
<Carrier>QF</Carrier>
<CreationDateTime>2008-12-03T02:46:13.0000000Z</CreationDateTime>
<HAWB></HAWB>
<MAWB></MAWB>
<IssuingCarrierAgentIATACode>1234-45</IssuingCarrierAgentIATACode>
<Body>
<![CDATA[", result);

			result = CargoIMPServiceProvider.CCN.GetTransmissionMethodHeader(awb, messageFWB, CargoIMPTransmissionMethod.eAdaptor);
			AssertEquals("FWB\r\nQFSYD\r\n", result);

			result = CargoIMPServiceProvider.Get("XYZ").GetTransmissionMethodHeader(awb, messageFWB, CargoIMPTransmissionMethod.FTP);
			AssertEquals("QK CSGAGT85GHAQFFWBSYD\r\n.XSPLXSQ 030246\r\n", result);

			try
			{
				result = CargoIMPServiceProvider.BTviaCCN.GetTransmissionMethodHeader(awb, messageFWB, CargoIMPTransmissionMethod.FTP);
				Fail("Exception should be raised");
			}
			catch (CargoIMPApplicationException ex)
			{
				AssertEquals("Carrier PIMA Address is not found.\r\nIt can be set up on 'Config' tab of organization record corresponding to this airline", ex.Message);
			}

			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "aaa";
			org1.MiscServ.OM_RM_Airline = Factory.LoadFromNaturalKey<RefAirline>(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, "081").PK;
			try
			{
				result = CargoIMPServiceProvider.BTviaCCN.GetTransmissionMethodHeader(awb, messageFWB, CargoIMPTransmissionMethod.FTP);
				Fail("Exception should be raised");
			}
			catch (CargoIMPApplicationException ex)
			{
				AssertEquals("Carrier PIMA Address is not found.\r\nIt can be set up on 'Config' tab of organization record corresponding to this airline", ex.Message);
			}

			org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PIMAAddress, "DDD111");
			result = CargoIMPServiceProvider.BTviaCCN.GetTransmissionMethodHeader(awb, messageFWB, CargoIMPTransmissionMethod.FTP);
			AssertEquals("\r\n\x01QP LONBCCR\r\n.XSPLXSQ 030246 DDD111\r\n\x02", result);

			ForwardingConfigurationRegistry.Instance.SenderPIMAAddress.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "");
			try
			{
				result = CargoIMPServiceProvider.Descartes.GetTransmissionMethodHeader(awb, messageFHL, CargoIMPTransmissionMethod.FTP);
				Fail("Exception should be raised");
			}
			catch (CargoIMPApplicationException ex)
			{
				AssertEquals("'Freight/AWB/CargoIMP/FWB Messaging/Sender Identification (PIMA)' is not set in registry", ex.Message);
			}

			try
			{
				result = CargoIMPServiceProvider.BTviaCCN.GetTransmissionMethodHeader(awb, messageFWB, CargoIMPTransmissionMethod.FTP);
				Fail("Exception should be raised");
			}
			catch (CargoIMPApplicationException ex)
			{
				AssertEquals("'Freight/AWB/CargoIMP/FWB Messaging/Sender Identification (PIMA)' is not set in registry", ex.Message);
			}

			ForwardingConfigurationRegistry.Instance.SenderPIMAAddress.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "XSPLXSQ");
			awb.EH_By1st = "~~";
			try
			{
				result = CargoIMPServiceProvider.Descartes.GetTransmissionMethodHeader(awb, messageFHL, CargoIMPTransmissionMethod.FTP);
				Fail("Exception should be raised");
			}
			catch (CargoIMPApplicationException ex)
			{
				AssertEquals("Airline code is not found", ex.Message);
			}

			try
			{
				result = CargoIMPServiceProvider.CCN.GetTransmissionMethodHeader(awb, messageFWB, CargoIMPTransmissionMethod.SMTP);
				Fail("Exception should be raised");
			}
			catch (CargoIMPApplicationException ex)
			{
				AssertEquals("Airline code is not found", ex.Message);
			}

			try
			{
				result = CargoIMPServiceProvider.CCN.GetTransmissionMethodHeader(awb, messageFWB, CargoIMPTransmissionMethod.eHub);
				Fail("Exception should be raised");
			}
			catch (CargoIMPApplicationException ex)
			{
				AssertEquals("Airline code is not found", ex.Message);
			}

			try
			{
				result = CargoIMPServiceProvider.CCN.GetTransmissionMethodHeader(awb, messageFWB, CargoIMPTransmissionMethod.eAdaptor);
				Fail("Exception should be raised");
			}
			catch (CargoIMPApplicationException ex)
			{
				AssertEquals("Airline code is not found", ex.Message);
			}
		}

		[TestDate(2008, 12, 3, 2, 46, 13)]
		public void TestInvalidCharactersInIssuingCarrierAgentIATACode()
		{
			var branchPK = Guid.NewGuid();
			DataRegistry.Instance.SetIssuingCarrierAgentIATACode(branchPK, "1234 & 45");

			ForwardingConfigurationRegistry.Instance.SenderPIMAAddress.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "XSPLXSQ");
			var awb = Factory.New<ExportAWBHeader>();
			awb.EH_AWBOriginCode = "SYD";
			awb.EH_By1st = "QF";

			var messageFWB = EDIMessageTestFactory.New(Factory);
			messageFWB.EM_MessageType = "FWB";
			messageFWB.EM_GB = branchPK;

			var messageFHL = EDIMessageTestFactory.New(Factory);
			messageFHL.EM_MessageType = "FHL";
			messageFHL.EM_GB = branchPK;

			string result = CargoIMPServiceProvider.CCN.GetTransmissionMethodHeader(awb, messageFWB, CargoIMPTransmissionMethod.SMTP);
			AssertEquals("FWB\r\nQFSYD\r\n", result);

			result = CargoIMPServiceProvider.CCN.GetTransmissionMethodHeader(awb, messageFWB, CargoIMPTransmissionMethod.eHub);
			AssertEquals(@"<CargoIMP xmlns=""http://cargowise.com/cargoimp/201108"">
<MessageType>FWB</MessageType>
<Priority>QK</Priority>
<Carrier>QF</Carrier>
<CreationDateTime>2008-12-03T02:46:13.0000000Z</CreationDateTime>
<HAWB></HAWB>
<MAWB></MAWB>
<IssuingCarrierAgentIATACode>123445</IssuingCarrierAgentIATACode>
<Body>
<![CDATA[", result);
		}

		public void TestOnlyAlphaNumericHWABForEHub()
		{
			var awb = Factory.New<ExportAWBHeader>();
			awb.EH_By1st = "QF";

			var message = EDIMessageTestFactory.New(Factory);
			message.EM_MessageType = "FHL";
			message.EM_ApplicationReference = "ABCDE abcde 1-2/3_4*5$6#7!8#9. ";

			var result = CargoIMPServiceProvider.IATA.GetTransmissionMethodHeader(awb, message, CargoIMPTransmissionMethod.eHub);
			Match m = new Regex(@"<HAWB>(.*)</HAWB>", RegexOptions.Multiline).Match(result);

			AssertEquals("ABCDEabcde123456789", m.Groups[1].Value);
		}

		[TestDate(2008, 12, 3, 2, 46, 13)]
		public void TestGetTransmissionMethodHeaderIATA()
		{
			var branchPK = Guid.NewGuid();
			DataRegistry.Instance.SetIssuingCarrierAgentIATACode(branchPK, "1234-45");

			var messageFWB = EDIMessageTestFactory.New(Factory);
			messageFWB.EM_MessageType = "FWB";
			messageFWB.EM_GB = branchPK;

			var messageFHL = EDIMessageTestFactory.New(Factory);
			messageFHL.EM_MessageType = "FHL";
			messageFHL.EM_GB = branchPK;

			ForwardingConfigurationRegistry.Instance.SenderPIMAAddress.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "XSPLXSQ");
			var awb = Factory.New<ExportAWBHeader>();
			awb.EH_AWBOriginCode = "SYD";
			awb.EH_By1st = "QF";

			string result;
			try
			{
				result = CargoIMPServiceProvider.IATA.GetTransmissionMethodHeader(awb, messageFWB, CargoIMPTransmissionMethod.FTP);
				Fail("Exception should be raised");
			}
			catch (CargoIMPApplicationException ex)
			{
				AssertEquals("Carrier PIMA Address is not found.\r\nIt can be set up on 'Config' tab of organization record corresponding to this airline", ex.Message);
			}

			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "aaa";
			org1.MiscServ.OM_RM_Airline = Factory.LoadFromNaturalKey<RefAirline>(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, "081").PK;
			try
			{
				result = CargoIMPServiceProvider.IATA.GetTransmissionMethodHeader(awb, messageFWB, CargoIMPTransmissionMethod.FTP);
				Fail("Exception should be raised");
			}
			catch (CargoIMPApplicationException ex)
			{
				AssertEquals("Carrier PIMA Address is not found.\r\nIt can be set up on 'Config' tab of organization record corresponding to this airline", ex.Message);
			}

			org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PIMAAddress, "DDD111");
			result = CargoIMPServiceProvider.IATA.GetTransmissionMethodHeader(awb, messageFWB, CargoIMPTransmissionMethod.FTP);
			AssertEquals("QP LONBCCR\r\n.XSPLXSQ 030246 DDD111\r\n", result);
			result = CargoIMPServiceProvider.IATA.GetTransmissionMethodHeader(awb, messageFWB, CargoIMPTransmissionMethod.SMTP);
			AssertEquals("QP LONBCCR\r\n.XSPLXSQ 030246 DDD111\r\n", result);
			result = CargoIMPServiceProvider.IATA.GetTransmissionMethodHeader(awb, messageFWB, CargoIMPTransmissionMethod.eHub);
			AssertEquals(@"<CargoIMP xmlns=""http://cargowise.com/cargoimp/201108"">
<MessageType>FWB</MessageType>
<Priority>QP</Priority>
<Carrier>QF</Carrier>
<CreationDateTime>2008-12-03T02:46:13.0000000Z</CreationDateTime>
<HAWB></HAWB>
<MAWB></MAWB>
<IssuingCarrierAgentIATACode>1234-45</IssuingCarrierAgentIATACode>
<Body>
<![CDATA[", result);

			result = CargoIMPServiceProvider.IATA.GetTransmissionMethodHeader(awb, messageFWB, CargoIMPTransmissionMethod.eAdaptor);
			AssertEquals("QP LONBCCR\r\n.XSPLXSQ 030246 DDD111\r\n", result);

			ForwardingConfigurationRegistry.Instance.SenderPIMAAddress.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "");
			try
			{
				result = CargoIMPServiceProvider.IATA.GetTransmissionMethodHeader(awb, messageFWB, CargoIMPTransmissionMethod.FTP);
				Fail("Exception should be raised");
			}
			catch (CargoIMPApplicationException ex)
			{
				AssertEquals("'Freight/AWB/CargoIMP/FWB Messaging/Sender Identification (PIMA)' is not set in registry", ex.Message);
			}

			try
			{
				result = CargoIMPServiceProvider.IATA.GetTransmissionMethodHeader(awb, messageFWB, CargoIMPTransmissionMethod.SMTP);
				Fail("Exception should be raised");
			}
			catch (CargoIMPApplicationException ex)
			{
				AssertEquals("'Freight/AWB/CargoIMP/FWB Messaging/Sender Identification (PIMA)' is not set in registry", ex.Message);
			}

			try
			{
				result = CargoIMPServiceProvider.IATA.GetTransmissionMethodHeader(awb, messageFWB, CargoIMPTransmissionMethod.eAdaptor);
				Fail("Exception should be raised");
			}
			catch (CargoIMPApplicationException ex)
			{
				AssertEquals("'Freight/AWB/CargoIMP/FWB Messaging/Sender Identification (PIMA)' is not set in registry", ex.Message);
			}
		}

		public void TestGetTransmissionMethodFooter()
		{
			var awb = Factory.New<ExportAWBHeader>();
			awb.EH_AWBOriginCode = "SYD";
			awb.EH_By1st = "QF";

			var messageFWB = EDIMessageTestFactory.New(Factory);
			messageFWB.EM_MessageType = "FWB";

			string result = CargoIMPServiceProvider.CCN.GetTransmissionMethodFooter(awb, messageFWB, CargoIMPTransmissionMethod.SMTP);
			AssertEquals("\x04", result);

			result = CargoIMPServiceProvider.Get("XYZ").GetTransmissionMethodFooter(awb, messageFWB, CargoIMPTransmissionMethod.FTP);
			AssertEquals("\x04", result);

			result = CargoIMPServiceProvider.BTviaCCN.GetTransmissionMethodFooter(awb, messageFWB, CargoIMPTransmissionMethod.FTP);
			AssertEquals("\x03\r\n\n\n\x04\n", result);

			result = CargoIMPServiceProvider.BTviaCCN.GetTransmissionMethodFooter(awb, messageFWB, CargoIMPTransmissionMethod.SMTP);
			AssertEquals("\x04", result);

			result = CargoIMPServiceProvider.IATA.GetTransmissionMethodFooter(awb, messageFWB, CargoIMPTransmissionMethod.SMTP);
			AssertEquals("\x04", result);

			result = CargoIMPServiceProvider.IATA.GetTransmissionMethodFooter(awb, messageFWB, CargoIMPTransmissionMethod.FTP);
			AssertEquals("\x04", result);
		}
	}
}
