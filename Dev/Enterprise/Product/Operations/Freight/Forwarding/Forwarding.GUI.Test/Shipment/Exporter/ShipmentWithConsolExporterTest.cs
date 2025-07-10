using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ShipmentWithConsolExporterTest : TestCaseWithFactory
	{
		[TestDate(2007, 6, 7, 12, 5, 1)]
		public void TestFileExportedIntoNominatedDirectory()
		{
			SystemDataRegistry.Instance.ShipmentAsCustomDeclarationExportDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, EnvProxy.Instance.TempPath);
			string expectedFileName = Path.Combine(EnvProxy.Instance.TempPath, Shipment.JS_UniqueConsignRef + "_20070607120501.xml");
			ZFormModaliser.FileNameToSelectInShowCommonDialog = expectedFileName;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			try
			{
				var exporter = new ShipmentWithConsolExporterForTest();
				Shipment.Consols.Add(GetConsol("DEFRA", "AUSYD"));
				Factory.Save();
				exporter.Export(Shipment);
				Assert("File should have been created", File.Exists(expectedFileName));
			}
			finally
			{
				DeleteIfExists(expectedFileName);
			}
		}

		public void TestExportShipmentWithoutConsol()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var shipment = Factory.New<ForwardingShipment>();
			var exporter = new ShipmentWithConsolExporter();
			exporter.Export(shipment);
			AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
			AssertEquals("Should show Error Message", true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("Link the Shipment to a Consol before running this data export"));
		}

		#region Implementation

		ForwardingShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = Factory.New<ForwardingShipment>();
					var consignee = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));
					fShipment.ConsigneePK = consignee.PK;
					Factory.Save();
				}
				return fShipment;
			}
		}
		ForwardingShipment fShipment;

		ForwardingConsol GetConsol(ZString loadPort, ZString dischargePort)
		{
			var result = Factory.New<ForwardingConsol>();
			result.JK_TransportMode = Core.Constants.TransportModes.Sea;
			result.JK_MasterBillNum = "masterbill";
			result.JK_RL_NKLoadPort = loadPort;
			result.JK_RL_NKDischargePort = dischargePort;

			var transport = result.Transports[0];
			transport.JW_Vessel = "ADMIRALENGRACHT";
			transport.JW_VoyageFlight = "voyageno";
			return result;
		}

		#region Test Classes

		class ShipmentWithConsolExporterForTest : ShipmentWithConsolExporter
		{
			protected override XmlDataTransferExporter GetXmlDirector(IValueObjectDataAdapter adapter)
			{
				return new XmlDataTransferExporter(adapter, false);
			}
		}

		#endregion

		#endregion
	}
}
