using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Documents.Testing.NL
{
	sealed class PortbaseExportNotificationBuilderTest : TestCaseWithFactory
	{
		#region Tests

		public void TestBuild()
		{
			var consol = Factory.New<ForwardingConsol>();

			consol.JK_UniqueConsignRef = "C00001000";

			var transportLeg1 = consol.Transports[0];
			transportLeg1.JW_LegOrder = 1;
			transportLeg1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transportLeg1.JW_RL_NKLoadPort = "NLRTM";
			transportLeg1.JW_RL_NKDiscPort = "AUSYD";

			var portbase = new PortbaseExportNotificationBuilder(consol).Build();

			AssertEquals("ConsolNumber", consol.JK_UniqueConsignRef, portbase.ConsolNumber);
		}

		public void TestBuildCollection()
		{
			var consol = Factory.New<ForwardingConsol>();

			consol.JK_UniqueConsignRef = "C00001001";

			var shipmentA = consol.Shipments.AddNew();
			PopulateShipment(shipmentA, "S0700001557", Customs.Common.EU.ExportCommunityTransitStatusList.Codes.C, "DOC0094877", 5000, 5, "Goods desc A");
			var packLineA = shipmentA.OuterPackLines.AddNew();
			PopulatePackLine(packLineA, "DOC0094877", 5, "PLT", 5000, "KG");

			var shipmentB = consol.Shipments.AddNew();
			PopulateShipment(shipmentB, "S00001590", Customs.Common.EU.ExportCommunityTransitStatusList.Codes.T1, "DOC0094878", 5000, 5, "Goods desc B");
			var packLineB = shipmentB.OuterPackLines.AddNew();
			PopulatePackLine(packLineB, ZString.Empty, 5, "PLT", 5000, "KG");

			var shipmentC = consol.Shipments.AddNew();
			PopulateShipment(shipmentC, "S00001591", ZString.Empty, ZString.Empty, 5000, 5, "Goods desc C");
			var packLineC = shipmentC.OuterPackLines.AddNew();
			PopulatePackLine(packLineC, ZString.Empty, 5, "PLT", 5000, "KG");

			var containerX = consol.Containers.AddNew();
			PopulateContainer(containerX, "TGMU3039480", 17280.00, "KG");
			var containerY = consol.Containers.AddNew();
			PopulateContainer(containerY, "TRLU2456893", 17280.00, "KG");

			containerX.PackLines.Add(packLineA);
			containerX.PackLines.Add(packLineB);
			containerX.PackLines.Add(packLineC);

			containerY.PackLines.Add(packLineA);
			containerY.PackLines.Add(packLineB);
			containerY.PackLines.Add(packLineC);

			var portbase = new PortbaseExportNotificationBuilder(consol).Build();

			AssertEquals("ConsolNumber", consol.JK_UniqueConsignRef, portbase.ConsolNumber);

			AssertEquals("Documents.Count", 2, portbase.Documents.Count);
			AssertEquals("Documents[0].ReferenceNumber", "DOC0094877", portbase.Documents.ElementAt(0).ReferenceNumber);
			AssertEquals("Documents[1].ReferenceNumber", "DOC0094878", portbase.Documents.ElementAt(1).ReferenceNumber);

			AssertEquals("Documents[0].Containers.Count", 2, portbase.Documents.ElementAt(0).Containers.Count);
			AssertEquals("Documents[0].Containers[0].Number", "TGMU3039480", portbase.Documents.ElementAt(0).Containers.ElementAt(0).Number);
			AssertEquals("Documents[0].Containers[0].IsNonOperativeReefer", ZBool.False, portbase.Documents.ElementAt(0).Containers.ElementAt(0).IsNonOperativeReefer);

			AssertEquals("Documents[0].Containers[0].Shipment.Count", 1, portbase.Documents.ElementAt(0).Containers.ElementAt(0).Shipments.Count);
			AssertEquals("Documents[0].Containers[0].Shipment[0].Number", "S0700001557", portbase.Documents.ElementAt(0).Containers.ElementAt(0).Shipments.ElementAt(0).Number);

			AssertEquals("Documents[1].Containers[1].Shipment.Count", 1, portbase.Documents.ElementAt(1).Containers.ElementAt(1).Shipments.Count);
			AssertEquals("Documents[1].Containers[1].Shipment[0].Number", "S00001590", portbase.Documents.ElementAt(1).Containers.ElementAt(1).Shipments.ElementAt(0).Number);
		}

		public void TestPopulateSendersCustomsNoFromDifferentCountries()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var builder = new PortbaseExportNotificationBuilder(consol);

			AssertNullOrEmpty(builder.Build().SendersCustomsNo);

			var customsEoriCodeFromNL = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "99988877", Core.Constants.CountryCodes.Netherlands);

			AssertEquals("SendersCustomsNo From NL customs Eori Code", "99988877", builder.Build().SendersCustomsNo);

			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.Remove(customsEoriCodeFromNL.PK);
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "11122233", Core.Constants.CountryCodes.Germany);

			AssertEquals("SendersCustomsNo From DE customs Eori Code", "11122233", builder.Build().SendersCustomsNo);
		}

		#endregion

		#region Implementation

		void PopulateShipment(ForwardingShipment shipment, ZString shipmentNumber, ZString cusEntryNumType, ZString mawb, ZDecimal weight, ZInt packs, ZString desc)
		{
			shipment.JS_UniqueConsignRef = shipmentNumber;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_ActualWeight = weight;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_GoodsDescription = desc;
			shipment.JS_OuterPacks = packs;

			var number = shipment.CusEntryNumbers.AddNew();
			number.CE_EntryType = cusEntryNumType;
			number.CE_EntryNum = mawb;
		}

		void PopulatePackLine(ForwardingPackLine packLine, ZString exportRef, ZInt packs, ZString packtype, ZDecimal weight, ZString weightUnit)
		{
			packLine.JL_ExportRefNumber = exportRef;
			packLine.JL_PackageCount = packs;
			packLine.JL_F3_NKPackType = packtype;
			packLine.JL_ActualWeight = weight;
			packLine.JL_ActualWeightUQ = weightUnit;
		}

		void PopulateContainer(ForwardingContainer container, ZString containerNum, ZDecimal weight, ZString weightUnit)
		{
			container.JC_ContainerNum = containerNum;
			container.JC_GrossWeight = weight;
			container.JC_GrossWeightUQ = weightUnit;
			container.JC_IsNonOperativeReefer = false;
		}

		#endregion

	}
}
