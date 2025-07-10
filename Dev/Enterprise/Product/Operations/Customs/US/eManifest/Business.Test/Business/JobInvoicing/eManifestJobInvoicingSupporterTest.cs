using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(eManifestJobInvoicingSupporter))]
	sealed class eManifestJobInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestOverrides()
		{
			var trip = Factory.New<Trip>();
			trip.BH_ETA = new ZDateTime(2012, 05, 16);
			var equipment1 = trip.Equipment.AddNew();
			var equipment2 = trip.Equipment.AddNew();
			equipment1.BJ_RegistrationNumber = "equipment001";
			equipment2.BJ_RegistrationNumber = "equipment002";
			trip.BH_GB = Factory.NewWithValidTestData<GlbBranch>().PK;
			trip.BH_OA_Importer = Factory.NewWithValidTestData<OrgAddress>().PK;
			var shipment1 = trip.Shipments.AddNew();
			shipment1.Commodities.DeleteAll();
			shipment1.B0_Weight = 100;
			shipment1.B0_WeightUQ = Constants.Weight.Kilograms;
			shipment1.B0_Volume = 100;
			shipment1.B0_VolumeUQ = Constants.Volume.CubicMetres;
			var consignee1 = Factory.NewWithValidTestData<OrgHeader>();
			shipment1.Consignee.OrganisationPK = consignee1.PK;
			var shipment2 = trip.Shipments.AddNew();
			shipment2.Commodities.DeleteAll();
			shipment2.B0_Weight = 2;
			shipment2.B0_WeightUQ = Constants.Weight.Tonnes;
			shipment2.B0_Volume = 100;
			shipment2.B0_VolumeUQ = Constants.Volume.Litre;
			var consignee2 = Factory.NewWithValidTestData<OrgHeader>();
			shipment2.Consignee.OrganisationPK = consignee2.PK;
			var supporter = ((IJobInvoicingPlugIn)trip).InvoicingSupporter;
			AssertEquals("Consignee", trip.Importer.OA_OH, supporter.Consignee.PK);
			trip.BH_OA_Importer = ZGuid.Empty;
			AssertNull("Consignee", supporter.Consignee);
			shipment2.Consignee.OrganisationPK = consignee1.PK;
			AssertEquals("Consignee", consignee1.PK, supporter.Consignee.PK);
			AssertEquals("ETA", new ZDateTime(2012, 05, 16), supporter.ETA);
			AssertEquals("ActualChargeable", 2100m, supporter.ActualChargeable);
			AssertEquals("ActualChargeableUnit", Constants.Weight.Kilograms, supporter.ActualChargeableUnit);
			AssertEquals("TransportMode", Constants.TransportModes.Road, supporter.TransportMode);
			AssertEquals("IsImport", true, supporter.IsImport);
			AssertEquals("ConsumerType", JobInvoicingConsumerTypes.eManifest, supporter.ConsumerType);
			AssertEquals("ActualWeight", 2100m, supporter.ActualWeight);
			AssertEquals("ActualWeightUnit", Constants.Weight.Kilograms, supporter.ActualWeightUnit);
			shipment1.B0_WeightUQ = Constants.Weight.Tonnes;
			AssertEquals("ActualWeight", 102m, supporter.ActualWeight);
			AssertEquals("ActualWeightUnit", Constants.Weight.Tonnes, supporter.ActualWeightUnit);
			AssertEquals("ActualVolume", 100.1m, supporter.ActualVolume);
			AssertEquals("ActualVolumeUnit", Constants.Volume.CubicMetres, supporter.ActualVolumeUnit);
			shipment1.B0_VolumeUQ = Constants.Volume.Litre;
			AssertEquals("ActualVolume", 200m, supporter.ActualVolume);
			AssertEquals("ActualVolumeUnit", Constants.Volume.Litre, supporter.ActualVolumeUnit);
			AssertEquals("OperationsBranch", trip.Branch.PK, supporter.OperationsBranch.PK);
			AssertEquals("AuditSecurity", Env.Security.USeManifestAuditBilling, supporter.AuditSecurity);
			AssertEquals("JobInvoicingSecurity", Env.Security.USeManifestJobInvoicing, supporter.JobInvoicingSecurity);
			AssertEquals("CreateAccountingJobOnSavingOfOperationsJob", true, supporter.CreateAccountingJobOnSavingOfOperationsJob);
			Factory.Save();
			AssertEquals("CreateAccountingJobOnSavingOfOperationsJob", false, supporter.CreateAccountingJobOnSavingOfOperationsJob);
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject() => Factory.NewWithValidTestData<Trip>();

		protected override ZString TestingCountry => Constants.CountryCodes.UnitedStates;
	}
}
