using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsJobInvoicingAdditionalDataPropertyProviderTest : TransactionedTestCase
	{
		#region Properties

		#region TestGetDocketID

		public void TestGetDocketID()
		{
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.DocketID];

			var charge1 = GetChargeWithData(Docket.WD_ExternalReference, Part.OP_PartNum, Units);
			AssertEquals(Docket.WD_DocketID, customProperty.GetValue(charge1));

			AssertNotNull("DocketID should be included in the CustomProperty collection.", customProperty);
			AssertEquals("Type should be ZString.", typeof(ZString), customProperty.Info.Type);
			Assert("Property should not be visible by default.", !customProperty.Info.Visible);
			AssertEquals("Value.", Docket.WD_DocketID, customProperty.GetValue(charge1));

			var client = Helper.CreateClient("Client2");
			var whs = Helper.CreateWarehouse("WH2");
			var otherDocket = Helper.CreateWhsReceive(client, whs);
			otherDocket.WD_ExternalReference = "OTHERREF";
			var charge2 = GetChargeWithData(otherDocket.WD_ExternalReference, Part.OP_PartNum, Units);

			AssertEquals("Value.", otherDocket.WD_DocketID, customProperty.GetValue(charge2));
		}

		#endregion

		#region TestGetCustomerReference

		public void TestGetCustomerReference()
		{
			var charge = GetChargeWithData(Docket.WD_ExternalReference, Part.OP_PartNum, Units);
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.CustomerReference];
			AssertNotNull("CustomerReference should be included in the CustomProperty collection.", customProperty);
			AssertEquals("Type should be ZString.", typeof(ZString), customProperty.Info.Type);
			Assert("Property should not be visible by default.", !customProperty.Info.Visible);
			AssertEquals("Value.", Docket.WD_CustomerReference, customProperty.GetValue(charge));
		}

		#endregion

		#region TestTransportCo

		public void TestTransportCo()
		{
			var charge = GetChargeWithData(Docket.WD_ExternalReference, Part.OP_PartNum, Units);
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.TransportCo];
			AssertNotNull("TransportCo should be included in the CustomProperty collection.", customProperty);
			AssertEquals("Type should be ZString.", typeof(ZString), customProperty.Info.Type);
			Assert("Property should not be visible by default.", !customProperty.Info.Visible);

			var job = (IJobWithTransportCompany)Docket;
			AssertEquals("Value.", job.TransportCoDocAddress.Organisation.OH_Code, customProperty.GetValue(charge));

			job.TransportCoDocAddress.E2_AddressOverride = true;
			job.TransportCoDocAddress.E2_CompanyName = "Test123";
			AssertEquals("Value.", "Test123", customProperty.GetValue(charge));
		}

		#endregion

		#region TestTransportCo_NoOrganisationEntered

		public void TestTransportCo_NoOrganisationEntered()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewDocket(data.Org1, data.Whs1);
			var job = Helper.CreateRatingJob(docket);

			var jobWithTransportCo = (IJobWithTransportCompany)docket;
			AssertEquals("Precondition.", false, jobWithTransportCo.TransportCoDocAddress.E2_AddressOverride);
			AssertNull("Precondition.", docket.TransportCo);

			var charge = GetChargeWithData(job, docket.WD_ExternalReference, data.Part1.OP_PartNum, 5m);
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.TransportCo];
			AssertEquals("Should return empty.", string.Empty, customProperty.GetValue(charge));
		}

		#endregion

		#region TestGetServiceLevel

		public void TestGetServiceLevel()
		{
			var charge = GetChargeWithData(Docket.WD_ExternalReference, Part.OP_PartNum, Units);
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ServiceLevel];
			AssertNotNull("ServiceLevel should be included in the CustomProperty collection.", customProperty);
			AssertEquals("Type should be ZString.", typeof(ZString), customProperty.Info.Type);
			Assert("Property should not be visible by default.", !customProperty.Info.Visible);
			AssertEquals("Value.", Docket.WD_PL_NKCarrierServiceLevel, customProperty.GetValue(charge));
		}

		#endregion

		#region TestGetWeight

		public void TestGetWeight()
		{
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.Weight];
			AssertNotNull("Weight should be included in the CustomProperty collection.", customProperty);
			AssertEquals("Type should be ZDecimal.", typeof(ZDecimal), customProperty.Info.Type);
			Assert("Property should not be visible by default.", !customProperty.Info.Visible);

			// test with no product
			Docket.WD_TotalWeight = 123m;
			var charge1 = GetChargeWithData(Docket.WD_ExternalReference, "", Units);
			AssertEquals("Should be Docket.WD_TotalWeight", 123m, customProperty.GetValue(charge1));

			// test with product and no rating unit
			var charge2 = GetChargeWithData(Docket.WD_ExternalReference, Part.OP_PartNum, Units);
			AssertEquals("Should be Part.OP_Weight (2) * Units (50)", 100m, customProperty.GetValue(charge2));

			// test with product and rating by weight
			var charge3 = GetChargeWithData(Docket.WD_ExternalReference, Part.OP_PartNum, 10, 9.5, "KG");
			AssertEquals("Rating by weight so should be same as Rating Units (10)", 10m, customProperty.GetValue(charge3));

			// test with product and rating by volume
			var charge4 = GetChargeWithData(Docket.WD_ExternalReference, Part.OP_PartNum, 30, 29.2, "M3");
			AssertEquals("Should be 30 m3 converted to weight (29.2 m3 /3 m3 per unit * 2 kg per unit)", 19.47m, customProperty.GetValue(charge4));

			// test with product and rating by pallet
			var charge5 = GetChargeWithData(Docket.WD_ExternalReference, Part.OP_PartNum, 5, 4.6, "PL");
			AssertEquals("Should be 5 pallets converted to weight (4.6 pallets * 40 units per pallet * 2 kg per unit", 368m, customProperty.GetValue(charge5));

			// test with product and no conversion to rating unit
			var charge6 = GetChargeWithData(Docket.WD_ExternalReference, Part.OP_PartNum, 50, 50, "XX");
			AssertEquals("There is no conversion on product to a unit of XX", 0m, customProperty.GetValue(charge6));
		}

		#endregion

		#region WeightUQ

		public void TestGetWeightUQ()
		{
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.WeightUQ];
			AssertNotNull("WeightUQ should be included in the CustomProperty collection.", customProperty);
			AssertEquals("Type should be ZString.", typeof(ZString), customProperty.Info.Type);
			Assert("Property should not be visible by default.", !customProperty.Info.Visible);

			var charge1 = GetChargeWithData(Docket.WD_ExternalReference, Part.OP_PartNum, Units);
			AssertEquals("KG", customProperty.GetValue(charge1));

			Docket.WD_TotalWeightUnit = "G";
			var charge2 = GetChargeWithData(Docket.WD_ExternalReference, "", Units);
			AssertEquals("G", customProperty.GetValue(charge2));
		}

		#endregion

		#region TestGetVolume

		public void TestGetVolume()
		{
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.Volume];
			AssertNotNull("ConsigneeCode should be included in the CustomProperty collection.", customProperty);
			AssertEquals("Type should be ZDecimal.", typeof(ZDecimal), customProperty.Info.Type);
			Assert("Property should not be visible by default.", !customProperty.Info.Visible);

			// test with no product
			Docket.WD_TotalCubic = 123m;
			var charge1 = GetChargeWithData(Docket.WD_ExternalReference, "", Units);
			AssertEquals("Should be Docket.WD_TotalCubic", 123m, customProperty.GetValue(charge1));

			// test with product and no rating unit
			var charge2 = GetChargeWithData(Docket.WD_ExternalReference, Part.OP_PartNum, Units);
			AssertEquals("Should be Part.OP_Cubic (3) * Units (50)", 150m, customProperty.GetValue(charge2));

			// test with product and rating by volume
			var charge3 = GetChargeWithData(Docket.WD_ExternalReference, Part.OP_PartNum, 10, 9.5, "M3");
			AssertEquals("Rating by volume so should be same as Rating Units (10) ", 10m, customProperty.GetValue(charge3));

			// test with product and rating by weight
			var charge4 = GetChargeWithData(Docket.WD_ExternalReference, Part.OP_PartNum, 50, 49.3, "KG");
			AssertEquals("Should be 50kg converted to volume (49.3kg /2kg per unit * 3 m3 per unit) ", 73.95m, customProperty.GetValue(charge4));

			// test with product and rating by palllet
			var charge5 = GetChargeWithData(Docket.WD_ExternalReference, Part.OP_PartNum, 5, 4.5, "PL");
			AssertEquals("Should be 5 pallets converted to volume (4.5 pallets * 40 units per pallet * 3 m3 per unit", 540m, customProperty.GetValue(charge5));

			// test with product and no conversion to rating unit
			var charge6 = GetChargeWithData(Docket.WD_ExternalReference, Part.OP_PartNum, 50, 50, "XX");
			AssertEquals("There is no conversion on product to a unit of XX", 0m, customProperty.GetValue(charge6));
		}

		#endregion

		#region TestGetVolumeUQ

		public void TestGetVolumeUQ()
		{
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.VolumeUQ];
			AssertNotNull("VolumeUQ should be included in the CustomProperty collection.", customProperty);
			AssertEquals("Type should be ZString.", typeof(ZString), customProperty.Info.Type);
			Assert("Property should not be visible by default.", !customProperty.Info.Visible);

			var charge1 = GetChargeWithData(Docket.WD_ExternalReference, Part.OP_PartNum, Units);
			AssertEquals("M3", customProperty.GetValue(charge1));

			Docket.WD_TotalCubicUnit = "D3";
			var charge2 = GetChargeWithData(Docket.WD_ExternalReference, "", Units);
			AssertEquals("D3", customProperty.GetValue(charge2));
		}

		#endregion

		#region TestPropertiesProxiedOffCharge

		public void TestPropertiesProxiedOffCharge_SerialNumber()
		{
			var charge = GetChargeWithData(Docket.WD_ExternalReference, Part.OP_PartNum, Units);
			AddJobChargeAttrib(charge, JobChargeAttribTypeList.Codes.LocationDesc, "LOC");
			AddJobChargeAttrib(charge, JobChargeAttribTypeList.Codes.LocationType, "LOCTYP");
			AddJobChargeAttrib(charge, JobChargeAttribTypeList.Codes.Attrib1, "PA1");
			AddJobChargeAttrib(charge, JobChargeAttribTypeList.Codes.Attrib2, "PA2");
			AddJobChargeAttrib(charge, JobChargeAttribTypeList.Codes.Attrib3, "PA3");
			AddJobChargeAttrib(charge, JobChargeAttribTypeList.Codes.SerialNumber, "SER");
			AddJobChargeAttrib(charge, JobChargeAttribTypeList.Codes.Commodity, "A");
			AddJobChargeAttrib(charge, JobChargeAttribTypeList.Codes.CartageZoneDescription, "ZONE");

			var additionalProperties = GetAdditionalDataProvider.GetAdditionalProperties();
			AssertCustomProperty(additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.Product], typeof(ZString), Part.OP_PartNum, charge, visible: true);
			AssertCustomProperty(additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.Location], typeof(ZString), "LOC", charge);
			AssertCustomProperty(additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.LocationType], typeof(ZString), "LOCTYP", charge);
			AssertCustomProperty(additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.PartAttrib1], typeof(ZString), "PA1", charge);
			AssertCustomProperty(additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.PartAttrib2], typeof(ZString), "PA2", charge);
			AssertCustomProperty(additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.PartAttrib3], typeof(ZString), "PA3", charge);
			AssertCustomProperty(additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.SerialNumber], typeof(ZString), "SER", charge);
			AssertCustomProperty(additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.Commodity], typeof(ZString), "A", charge);
			AssertCustomProperty(additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.CartageZone], typeof(ZString), "ZONE", charge);
			AssertCustomProperty(additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.DocketReference], typeof(ZString), "External Reference", charge);
		}

		void AssertCustomProperty(ICustomProperty customProperty, Type expectedType, string expectedValue, JobCharge charge, bool visible = false)
		{
			AssertNotNull("Property should be included in the CustomProperty collection.", customProperty);
			AssertEquals("Type.", expectedType, customProperty.Info.Type);
			AssertEquals("Visible.", visible, customProperty.Info.Visible);
			AssertEquals("Value.", expectedValue, customProperty.GetValue(charge));
		}

		#endregion

		#endregion

		#region TestDocket_LoadByClientAndReference

		public void TestDocket_LoadByClientAndReference()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client1 = data.Org1;
			var client2 = Helper.CreateClient("CLIENT2");
			var whs = data.Whs1;

			// create duplicate references for different clients
			var receive1 = Helper.CreateWhsReceive(client1, whs, "R1");
			var receive2 = Helper.CreateWhsReceive(client2, whs, "R1");
			receive1.WD_DocketID = "W00000123";
			receive2.WD_DocketID = "W00000456";

			var job1 = Helper.CreateRatingJob(receive1);
			var job2 = Helper.CreateRatingJob(receive2);

			var charge1 = Helper.CreateJobCharge(job1);
			var charge2 = Helper.CreateJobCharge(job2);
			Helper.CreateJobChargeAttrib(charge1, JobChargeAttribTypeList.Codes.DocketReference, "R1");
			Helper.CreateJobChargeAttrib(charge2, JobChargeAttribTypeList.Codes.DocketReference, "R1");

			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.DocketID];
			AssertNotNull("DocketID should be included in the CustomProperty collection.", customProperty);
			AssertEquals("Type should be ZString.", typeof(ZString), customProperty.Info.Type);
			AssertEquals("Even with multiple jobs with same reference, property provider should be able to find parent job.", "W00000123", customProperty.GetValue(charge1));
			AssertEquals("Even with multiple jobs with same reference, property provider should be able to find parent job.", "W00000456", customProperty.GetValue(charge2));
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			SetupDataInDB();
		}

		void SetupDataInDB()
		{
			var client = Helper.CreateClient("Client");
			var transportCo = Helper.CreateClient("TransportCo");
			var serviceLevel = transportCo.MiscServ.CarrierServiceLevels.AddNew();
			var whs = Helper.CreateWarehouse("WHS");
			client.MainAddress.FillWithValidTestData();
			serviceLevel.PL_Code = "AAA";

			Docket = GetNewDocket(client, whs);
			Job = Helper.CreateRatingJob(Docket);

			Part = Helper.CreateProduct(client, "P1");
			Part.OP_Weight = 2m;
			Part.OP_WeightUQ = "KG";
			Part.OP_Cubic = 3m;
			Part.OP_CubicUQ = "M3";
			Helper.CreateProductUnit(Part, "PLT", 40);

			Docket.WD_DocketID = "Docket ID";
			Docket.WD_ExternalReference = "External Reference";
			Docket.WD_CustomerReference = "Customer Reference";
			((IJobWithTransportCompany)Docket).TransportCoPK = transportCo.PK;
			Docket.WD_PL_NKCarrierServiceLevel = serviceLevel.PL_Code;

			Units = 50m;

			Factory.Save();
		}

		protected virtual WhsDocket GetNewDocket(OrgHeader client, WhsWarehouse whs)
		{
			return Helper.CreateWhsReceive(client, whs);
		}

		#region GetChargeWithData

		void AddJobChargeAttrib(JobCharge charge, string name, string value)
		{
			var attrib = charge.JobChargeAttributes.AddNew();
			attrib.EC_Name = name;
			attrib.EC_Value = value;
		}

		protected JobCharge GetChargeWithData(ZString docketReference, ZString partCode, ZDecimal units)
		{
			return GetChargeWithData(docketReference, partCode, units, units, "");
		}
		protected JobCharge GetChargeWithData(JobHeader job, ZString docketReference, ZString partCode, ZDecimal units)
		{
			return GetChargeWithData(job, docketReference, partCode, units, units, "");
		}

		protected JobCharge GetChargeWithData(ZString docketReference, ZString partCode, ZDecimal units, ZDecimal unRoundedUnits, ZString ratingUnit)
		{
			return GetChargeWithData(Job, docketReference, partCode, units, unRoundedUnits, ratingUnit);
		}

		protected JobCharge GetChargeWithData(JobHeader job, ZString docketReference, ZString partCode, ZDecimal units, ZDecimal unRoundedUnits, ZString ratingUnit)
		{
			var result = Factory.New<JobCharge>();
			result.JR_JH = job.PK;

			result.JobChargeAttributes.RemoveAndDeleteAll();
			Helper.CreateJobChargeAttrib(result, JobChargeAttribTypeList.Codes.DocketReference, docketReference);
			Helper.CreateJobChargeAttrib(result, JobChargeAttribTypeList.Codes.Product, partCode);
			Helper.CreateJobChargeAttrib(result, JobChargeAttribTypeList.Codes.ItemsToRate, units.ToString());
			Helper.CreateJobChargeAttrib(result, JobChargeAttribTypeList.Codes.UnroundedItemsToRate, unRoundedUnits.ToString());
			if (!ratingUnit.IsEmpty)
			{
				Helper.CreateJobChargeAttrib(result, JobChargeAttribTypeList.Codes.ItemsToRateUnit, ratingUnit);
			}
			return result;
		}

		#endregion

		protected virtual WhsJobInvoicingAdditionalDataPropertyProvider GetAdditionalDataProvider
		{
			get { return new WhsJobInvoicingAdditionalDataPropertyProvider(); }
		}

		protected BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		protected WhsDocket Docket;
		protected JobHeader Job;
		protected OrgSupplierPart Part;
		protected ZDecimal Units;

		#endregion
	}
}
