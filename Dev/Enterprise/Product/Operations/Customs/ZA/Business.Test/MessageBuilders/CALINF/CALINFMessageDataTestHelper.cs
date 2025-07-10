using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D16A.Elements;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CALINF.Testing
{
	sealed class CALINFMessageDataTestHelper
	{
		public static CALINFMessageDataForTest CreateCALINFMessageTestData_Sea_Schedule()
		{
			var data = new CALINFMessageDataForTest();
			data.Transport = CreateCALINFTransportTestData();
			data.CALINFMessageType = "SCH";
			data.DocumentIssueDateTime = new ZDateTime(2020, 06, 05, 11, 22, 0);
			data.DocumentToBeAmended = ZString.Empty;
			data.MessageSender = "ABC";
			data.Messages = null;
			data.Factory = null;
			data.MessageStatus = ZString.Empty;
			data.JobStatus = ZString.Empty;
			data.HasChanges = false;
			data.JobIdentification = ZString.Empty;
			data.TopLevelBusinessObject = null;
			return data;
		}

		public static CALINFMessageDataForTest CreateCALINFMessageTestData_Air_Schedule(BusinessObjectFactory factory)
		{
			var data = CreateCALINFMessageTestData_Sea_Schedule();
			data.CALINFMessageType = "ASC";
			var transport = (CALINFTransportDataForTest)data.Transport;
			transport.ConveyanceNumber = "BA5577";
			transport.TransportMode = "4";
			transport.CarrierCode = "BA";
			transport.CarrierName = "British Airways";
			transport.CarrierCodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation;
			transport.MeansOfTransportId = ZString.Empty;
			transport.MeansOfTransportName = ZString.Empty;
			transport.MeansOfTransportNationality = "GB";
			transport.MeansOfTransportCodeListIdentificationCode = "146";
			transport.PrincipalCarrierConveyanceNumber = "BA2233";
			Action<ICALINFCallInformation> convertCallInformationToAirFreightCodes = x =>
			{
				var callData = (CALINFCallDataForTest)x;
				callData.CallLocation = MessageBuilderHelper.UnlocoToIata(factory, isAir: ZBool.True, callData.CallLocation);
				callData.LocationCodeListIdentificationCode = "145";
				callData.LocationCodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation;
			};
			data.Transport.DepartureDetails.ForEach(convertCallInformationToAirFreightCodes);
			data.Transport.DischargeDetails.ForEach(convertCallInformationToAirFreightCodes);
			data.Transport.CallDetails.ForEach(convertCallInformationToAirFreightCodes);
			return data;
		}

		static CALINFTransportDataForTest CreateCALINFTransportTestData()
		{
			var transport = new CALINFTransportDataForTest();
			transport.DepartureDetails = new List<ICALINFCallInformation>()
			{ CreateCALINFCallTestData("DEHAM", new ZDateTime(2020, 06, 01, 13, 13, 0)) };
			transport.DischargeDetails = new List<ICALINFCallInformation>()
			{ CreateCALINFCallTestData("GBLON", new ZDateTime(2020, 06, 02, 14, 14, 0)) };
			transport.CallDetails = new List<ICALINFCallInformation>()
			{ CreateCALINFCallTestData("ZADUR", new ZDateTime(2020, 06, 03, 15, 15, 0)) };
			transport.ConveyanceNumber = "VOY456";
			transport.TransportMode = "1";
			transport.CarrierCode = "CarrierCode";
			transport.CarrierName = "CarrierName";
			transport.CarrierCodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.BicBureauInternationalDesContaineurs;
			transport.MeansOfTransportId = "RCS123";
			transport.MeansOfTransportName = "VesselName";
			transport.MeansOfTransportNationality = "GB";
			transport.MeansOfTransportCodeListIdentificationCode = "103";
			transport.PrincipalCarrierConveyanceNumber = "VOY123";
			return transport;
		}

		static CALINFCallDataForTest CreateCALINFCallTestData(ZString location, ZDateTime departureOrArrivalDate)
		{
			var call = new CALINFCallDataForTest();
			call.CallLocation = location;
			call.LocationCodeListIdentificationCode = "139";
			call.LocationCodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope;
			call.CallDateTime = departureOrArrivalDate;
			return call;
		}
	}
}
