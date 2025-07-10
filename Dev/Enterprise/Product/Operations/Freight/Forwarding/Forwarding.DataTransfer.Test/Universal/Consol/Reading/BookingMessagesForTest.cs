using System;
using System.IO;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	static class BookingMessagesForTest
	{
		public static ForwardingConsol CreateConsolMatchingDataTarget(BusinessObjectFactory factory, UniversalShipment shipment)
		{
			_ = factory ?? throw new ArgumentNullException(nameof(factory));
			_ = shipment ?? throw new ArgumentNullException(nameof(shipment));

			var dataTarget = shipment.GetMatchingDataTarget(DataContextType.ForwardingConsol);

			if (dataTarget == null
				|| !dataTarget.Key.HasValue)
			{
				return null;
			}

			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = dataTarget.Key.Value;
			consol.JK_RL_NKLoadPort = "CNDCB";
			consol.JK_RL_NKDischargePort = "SEGOT";

			var shippingLine = CreateShippingLine(factory);
			consol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;

			return consol;
		}

		public static ForwardingConsol CreateCoLoadConsolMatchingDataTarget(BusinessObjectFactory factory, UniversalShipment shipment)
		{
			_ = factory ?? throw new ArgumentNullException(nameof(factory));
			_ = shipment ?? throw new ArgumentNullException(nameof(shipment));

			var dataTarget = shipment.GetMatchingDataTarget(DataContextType.ForwardingConsol);

			if (dataTarget == null
				|| !dataTarget.Key.HasValue)
			{
				return null;
			}

			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_UniqueConsignRef = dataTarget.Key.Value;
			consol.JK_RL_NKLoadPort = "CNDCB";
			consol.JK_RL_NKDischargePort = "SEGOT";

			var shippingLine = CreateShippingLine(factory);
			consol.JK_OA_CreditorAddress = shippingLine.MainAddress.PK;

			return consol;
		}

		static OrgHeader CreateShippingLine(BusinessObjectFactory factory)
		{
			_ = factory ?? throw new ArgumentNullException(nameof(factory));

			var shippingLine = factory.NewWithValidTestData<OrgHeader>();

			var code = shippingLine.CustomsCodes.AddNew();
			code.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			code.OK_CustomsRegNo = "HLCU";

			return shippingLine;
		}

		public static UniversalShipment CreateBookingConfirmation() => CreateUniversalShipment("BookingConfirmation.xml");
		public static UniversalShipment CreateBookingRequestReply() => CreateUniversalShipment("BookingRequestReply.xml");

		static UniversalShipment CreateUniversalShipment(string fileName)
		{
			_ = fileName ?? throw new ArgumentNullException(nameof(fileName));

			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var resourceRetriever = new EmbeddedResourceRetriever(typeof(BookingMessagesForTest).Assembly);
			var testFilePath = GetEmbeddedResourceReadingTestFilePathFor(fileName);
			var buffer = resourceRetriever.GetBytes(testFilePath);
			var logger = new TestErrorLogger();

			using (var inputStream = (SubStreamableStream)new MemoryStream(buffer))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipmentDataObject, inputStream, logger);
			}

			return shipmentDataObject;
		}

		static string GetEmbeddedResourceReadingTestFilePathFor(string fileName) => $"Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.Consol.Reading.TestFiles.{fileName}";
	}
}
