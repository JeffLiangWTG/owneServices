using System;
using System.Collections;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	class ALPOXMLDataTransferExporterTest : TestCaseWithFactory
	{
		class AlpoxmlDataTransferExporterForTest : ALPOXMLDataTransferExporter
		{
			public AlpoxmlDataTransferExporterForTest(IValueObjectDataAdapter adapter, bool checkForLicence)
				: base(adapter, checkForLicence)
			{
				ExportStarted = false;
			}

			public bool ExportStarted { get; set; }
			public override void RunExport(IList selectedElements)
			{
				ExportStarted = true;
			}
		}

		public void TestSerialiserUsed()
		{
			var adapter = new ALPOConsolWithShipmentValueObjectDataAdapter();
			var exportor = new ALPOXMLDataTransferExporter(adapter, true);

			Assert("ALPOXMLDataTransferDirector should use ALPOXmlValueObjectSerializer", exportor.GetSerialiser() is ALPOXmlValueObjectSerializer);
		}

		[TestDate(2006, 9, 28, 12, 0, 0)]
		public void TestExportToXml_ALPO_wrongMode()
		{
			TestALPO("USORD", "RUMOW", Core.Constants.TransportModes.Air, false);
		}

		[TestDate(2006, 9, 28, 12, 0, 0)]
		public void TestExportToXml_ALPO_wrongPort()
		{
			TestALPO("USORD", "AUSYD", Core.Constants.TransportModes.Sea, false);
			TestALPO("USORD", "DEBRE", Core.Constants.TransportModes.Sea, true);
		}

		[TestDate(2006, 9, 28, 12, 0, 0)]
		public void TestExportToXml_ALPO()
		{
			TestALPO("DEBRE", "USORD", Core.Constants.TransportModes.Sea, true);
		}

		void TestALPO(string load, string discharge, string transportMode, bool expectedResult)
		{
			Shipment.JS_TransportMode = transportMode;
			Shipment.Transports[0].JW_RL_NKLoadPort = load;
			Shipment.Transports[0].JW_RL_NKDiscPort = discharge;
			SystemDataRegistry.Instance.ALPOExportDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, EnvProxy.Instance.TempPath);
			string expectedFileName = Shipment.JS_UniqueConsignRef + "_" + ZDateTime.Today.ToString("yyyyMMddhhmmss");
			try
			{
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
				ALPOConsolWithShipmentValueObjectDataAdapter adapter = new ALPOConsolWithShipmentValueObjectDataAdapter();
				var director = new AlpoxmlDataTransferExporterForTest(adapter, true);
				director.PromptUserAndExport(new CommonShipment[] { Shipment });
				AssertEquals("File Should have been created", expectedResult, director.ExportStarted);
			}
			finally
			{
				DeleteIfExists(Path.Combine(EnvProxy.Instance.TempPath, expectedFileName + ".xml"));
			}
		}

		public void TestExportFilename()
		{
			ZString timeStamp = ZDateTime.Today.ToString("yyyyMMddhhmmss");
			ZString expectedFilename = "ORDER_ALPO_9999_EXP_S0000001_" + timeStamp;
			FreightDataRegistry.Instance.ALPOExportFileName.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "ORDER_ALPO_9999_");
			ZString generatedFilename = GetFilename(timeStamp);
			AssertEquals(expectedFilename, generatedFilename);

			FreightDataRegistry.Instance.ALPOExportFileName.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "");
			expectedFilename = "ALPO_EXP_S0000001_" + timeStamp;
			generatedFilename = GetFilename(timeStamp);
			AssertEquals(expectedFilename, generatedFilename);
		}

		ZString GetFilename(ZString timeStamp)
		{
			ZString result = "";
			ZString generatedFileName = "";
			ZString fileNamePrefix = Shipment.IsImport() ? "IMP_" : "EXP_";

			result = fileNamePrefix + Shipment.JS_UniqueConsignRef + "_" + timeStamp;

			if (new ZString(FreightDataRegistry.Instance.ALPOExportFileName.Value).IsEmpty)
			{
				result = "ALPO_" + result;
			}
			else
			{
				result = FreightDataRegistry.Instance.ALPOExportFileName.Value + result;
			}

			return result;
		}

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();
			Consol = Factory.New<ForwardingConsol>();
			Consol.JK_BookingReference = "BookingReference";
			Consol.JK_RL_NKDischargePort = "USORD";

			ShippingLine = Factory.New<OrgHeader>();
			ShippingLine.OH_FullName = "ShippingLine";
			ShippingLine.OH_Code = "SHIPLINE";
			Consol.JK_OA_ShippingLineAddress = ShippingLine.MainAddress.PK;

			SendingForwarder = Factory.New<OrgHeader>();
			SendingForwarder.OH_FullName = "Sending Forwarder";
			SendingForwarder.OH_Code = "SFRWDR";

			Consol.JK_OA_SendingForwarderAddress = SendingForwarder.MainAddress.PK;

			CommonShipment someShipment = Consol.Shipments.AddNew();
			Shipment = Consol.Shipments.AddNew();
			Shipment.JS_HouseBill = "HAWB1234321";
			Shipment.JS_UniqueConsignRef = "UniqueConsRef";
			Shipment.JS_PackingMode = "FCL";
			Shipment.JS_MarksAndNumbers = "MarksAndNums";
			Shipment.JS_UniqueConsignRef = "S0000001";
			Shipment.JS_RL_NKOrigin = "DEBRE";
			Shipment.JS_RL_NKDestination = "USLAX";

			ALPOTransport = Shipment.Transports.AddNew();
			ALPOTransport.JW_Vessel = "Transport Name";
			ALPOTransport.JW_TransportType = "PRE";
			ALPOTransport.JW_TransportMode = "SEA";

			CFSOrg = Factory.LoadTop1<OrgHeader>(new ZQuery());
			RefCountry deCountry = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "DE"));
			CFSOrg.CustomsCodes.AddNew("EID", "ADZB", deCountry);
			Shipment.JS_OA_ImportReleaseDepot_ZAddress.SetOrgWithoutSettingDefaultAddress(CFSOrg.PK);
			Factory.Save();

			PackLine innerLine1 = Shipment.InnerPackLines.AddNew();
			innerLine1.JL_RefNumber = "InnerRefN1";
			PackLine innerLine2 = Shipment.InnerPackLines.AddNew();
			innerLine2.JL_RefNumber = "InnerRefN2";
			PackLine innerLine3 = Shipment.InnerPackLines.AddNew();
			innerLine3.JL_RefNumber = "InnerRefN1";

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			UNDG = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").FirstOrDefault();
			UNDG.DG_PG = "III";

			#region RefContainers

			RefContainer1 = Factory.New<RefContainer>();
			RefContainer1.RC_Code = "40GP";
			RefContainer1.RC_ISOType = "1234";

			RefContainer2 = Factory.New<RefContainer>();
			RefContainer2.RC_Code = "20GP";
			RefContainer2.RC_ISOType = "1235";

			#endregion

			#region Containers

			Container1 = Consol.Containers.AddNew();
			Container1.JC_ContainerNum = "CNTR000001";
			Container1.JC_ContainerMode = "LCL";
			Container1.JC_RC = RefContainer1.PK;
			Container1.JC_IsEmptyContainer = false;

			Container1.JC_TareWeight = 1.1;
			Container1.JC_GrossWeight = 1.2;

			PackLine11 = Shipment.OuterPackLines.AddNew();
			UNDGDataItem dGItem = PackLine11.UNDGs.AddNew();
			dGItem.LinkDefault(subs);
			dGItem.DI_DGFlashPoint = 1.11;

			PackLine11.JL_JS = Shipment.PK;
			PackLine11.JL_JC = Container1.PK;
			PackLine11.JL_RH_NKCommodityCode = "CCd";
			PackLine11.JL_ActualWeight = 11.1;
			PackLine11.JL_OutturnComment = "Outturn Comment";
			PackLine11.JL_MarksAndNumbers = "MarksAndNumbers";
			PackLine11.JL_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Carton;
			PackLine11.JL_Description = "Description";
			PackLine11.JL_RefNumber = "RefNum";

			PackLine12 = Shipment.OuterPackLines.AddNew();
			PackLine12.JL_JS = Shipment.PK;
			PackLine12.JL_JC = Container1.PK;
			PackLine12.JL_ActualWeight = 12.1;
			PackLine12.JL_OutturnComment = "Outturn Comment2";
			PackLine12.JL_MarksAndNumbers = "MarksAndNumbers2";
			PackLine12.JL_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Drum;
			PackLine12.JL_RefNumber = "RefNum";

			Container2 = Consol.Containers.AddNew();
			Container2.JC_ContainerNum = "CNTR000002";
			Container2.JC_ContainerMode = "BLK";

			PackLine21 = Shipment.OuterPackLines.AddNew();
			PackLine21.JL_JS = Shipment.PK;
			PackLine21.JL_JC = Container2.PK;
			PackLine21.JL_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Piece;
			PackLine21.JL_RefNumber = "RefNum21";

			PackLine22 = Shipment.OuterPackLines.AddNew();
			PackLine22.JL_JS = Shipment.PK;
			PackLine22.JL_JC = Container2.PK;
			PackLine22.JL_RefNumber = "RefNum22";

			Container3 = Consol.Containers.AddNew();
			Container3.JC_ContainerNum = "CNTR000003";
			Container3.JC_RC = RefContainer2.PK;
			Container3.JC_ContainerMode = "FCL";
			Container3.JC_IsEmptyContainer = true;
			PackLine31 = Shipment.OuterPackLines.AddNew();
			PackLine31.JL_JS = Shipment.PK;
			PackLine31.JL_JC = Container3.PK;
			PackLine31.JL_RefNumber = "RefNum31";

			#endregion
		}

		#region Implementation

		OrgHeader CFSOrg;
		OrgHeader SendingForwarder;
		OrgHeader ShippingLine;
		Transport ALPOTransport;
		ForwardingShipment Shipment;
		ForwardingConsol Consol;
		RefContainer RefContainer1;
		RefContainer RefContainer2;

		CommonContainer Container1;
		PackLine PackLine11;
		PackLine PackLine12;
		CommonContainer Container2;
		PackLine PackLine21;
		PackLine PackLine22;
		CommonContainer Container3;
		PackLine PackLine31;
		UNDGSubstance UNDG;

		#endregion

		#endregion
	}
}
