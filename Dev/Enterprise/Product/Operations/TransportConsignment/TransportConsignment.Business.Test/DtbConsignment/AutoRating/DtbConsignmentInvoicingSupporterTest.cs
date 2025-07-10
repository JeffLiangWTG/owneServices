using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentInvoicingSupporter))]
	sealed class DtbConsignmentInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		#region Packages

		public void TestContainerMode()
		{
			AssertEquals(Constants.ContainerModes.LCL, InvoicingSupporter.ContainerMode);
			var package1 = Consignment.PackageJob.Packages.AddNew(Constants.PkgUnit.Container);
			AssertEquals(Constants.ContainerModes.LCL, InvoicingSupporter.ContainerMode);

			var package2 = Consignment.PackageJob.Packages.AddNew(Constants.PkgUnit.Package);
			AssertEquals(Constants.ContainerModes.LCL, InvoicingSupporter.ContainerMode);
		}

		public void TestActualVolume()
		{
			AssertEquals(0m, InvoicingSupporter.ActualVolume);
			AssertEquals(Constants.Volume.CubicMetres, InvoicingSupporter.ActualVolumeUnit);

			var package10 = Helper.CreatePackage("", null, 10, 11, 12);
			var package20 = Helper.CreatePackage("", null, 20, 21, 22);
			var package30 = Helper.CreatePackage("", null, 30, 31, 32);

			package10.KP_VolumeUQ = Constants.Volume.CubicMetres;
			package20.KP_VolumeUQ = Constants.Volume.CubicFeet;

			Consignment.PackageJob.Packages.Add(package10);
			Consignment.PackageJob.Packages.Add(package20);

			AssertEquals(12.622971m, InvoicingSupporter.ActualVolume);
			AssertEquals(Constants.Volume.CubicMetres, InvoicingSupporter.ActualVolumeUnit);

			var cont1 = Helper.CreatePackageContainer("CONT1", null, 1, 40, 26);
			Consignment.PackageJob.Packages.Add(cont1);

			AssertEquals(12.622971m, InvoicingSupporter.ActualVolume); //Consider only loosePackage
			AssertEquals(Constants.Volume.CubicMetres, InvoicingSupporter.ActualVolumeUnit);
		}

		public void TestActualWeight()
		{
			AssertEquals(0m, InvoicingSupporter.ActualWeight);
			AssertEquals(Constants.Weight.Kilograms, InvoicingSupporter.ActualWeightUnit);

			var package10 = Helper.CreatePackage("", null, 10, 11, 12);
			var package20 = Helper.CreatePackage("", null, 20, 21, 22);
			var package30 = Helper.CreatePackage("", null, 30, 31, 32);

			package10.KP_WeightUQ = Constants.Weight.Kilograms;
			package20.KP_WeightUQ = Constants.Weight.Pounds;

			Consignment.PackageJob.Packages.Add(package10);
			Consignment.PackageJob.Packages.Add(package20);

			AssertEquals(20.52544m, InvoicingSupporter.ActualWeight);
			AssertEquals(Constants.Weight.Kilograms, InvoicingSupporter.ActualWeightUnit);

			var cont1 = Helper.CreatePackageContainer("CONT1", null, 1, 400);
			Consignment.PackageJob.Packages.Add(cont1);

			AssertEquals(20.52544m, InvoicingSupporter.ActualWeight); //Consider only loosePackage
			AssertEquals(Constants.Weight.Kilograms, InvoicingSupporter.ActualWeightUnit);
		}

		public void TestContainerCount()
		{
			AssertEquals(0, InvoicingSupporter.ContainerCount);
			var package1 = Consignment.PackageJob.Packages.AddNew(Constants.PkgUnit.Container);
			AssertEquals(0, InvoicingSupporter.ContainerCount);
		}

		public void TestTEUCount()
		{
			AssertEquals(0m, InvoicingSupporter.TEUCount);
			var package1 = Consignment.PackageJob.Packages.AddNew(Constants.PkgUnit.Container);
			AssertEquals(0m, InvoicingSupporter.TEUCount);

			var package2 = Consignment.PackageJob.Packages.AddNew(Constants.PkgUnit.Container);
			package2.Container.K0_RC_ContainerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			AssertEquals(0m, InvoicingSupporter.TEUCount);
		}

		#endregion

		#region Other

		#region TestConsolType

		public void TestConsolType()
		{
			AssertEquals(Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol, InvoicingSupporter.ConsolType);
		}

		#endregion

		#region TestCreateAccountingJobOnSavingOfOperationsJob

		public void TestCreateAccountingJobOnSavingOfOperationsJob()
		{
			AssertEquals(true, InvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob);

			Factory.Save();
			AssertEquals(false, InvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob);
		}

		#endregion

		#region TestTransportMode

		public void TestTransportMode()
		{
			AssertEquals(Constants.TransportModes.Road, InvoicingSupporter.TransportMode);
		}

		#endregion

		#region TestConsignor

		public void TestConsignor()
		{
			AssertNull(InvoicingSupporter.Consignor);
		}

		#endregion

		#region TestOverriddenDefaultLocalClient

		public void TestOverriddenDefaultLocalClient()
		{
			AssertNull(InvoicingSupporter.OverriddenDefaultLocalClient);
		}

		#endregion

		#region TestDefaultChargeCostReference

		public void TestDefaultChargeCostReference()
		{
			Consignment.LTC_JobID = "Test";
			AssertEquals("Test", InvoicingSupporter.OperationalJobRef);
		}

		#endregion

		#region TestDefaultChargeCostReference

		public void TestGetDefaultCreditor()
		{
			AssertEquals(null, InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(null, null)));
		}

		#endregion

		#endregion

		#region ConsumerType

		public void TestConsumerType()
		{
			AssertEquals(JobInvoicingConsumerTypes.TransportConsignment, InvoicingSupporter.ConsumerType);
		}

		#endregion

		#region Security

		public void TestAuditSecurity()
		{
			AssertEquals(Env.Security.DtbConsignmentJobAuditBilling, InvoicingSupporter.AuditSecurity);
		}

		public void TestJobInvoicingSecurity_Consignment()
		{
			AssertEquals(Env.Security.DtbConsignmentJobInvoicing, InvoicingSupporter.JobInvoicingSecurity);
		}

		#endregion

		#region TestIncludeInConsolCostingCore

		public void TestIncludeInConsolCostingCore()
		{
			AssertEquals(true, InvoicingSupporter.IncludeInConsolCosting(true));
			AssertEquals(true, InvoicingSupporter.IncludeInConsolCosting(false));

			Consignment.LTC_IsActive = false;
			AssertEquals(false, InvoicingSupporter.IncludeInConsolCosting(true));
			AssertEquals(false, InvoicingSupporter.IncludeInConsolCosting(false));
		}

		#endregion

		#region TestOperationsBranch

		public void TestTransportOperationsBranch()
		{
			AssertEquals("Invoicing branch should be Env.CurrentBranchPK.", Env.CurrentBranch, InvoicingSupporter.OperationsBranch);
		}

		#endregion

		#region SetDefaultChargeCostReference

		protected override bool SetDefaultChargeCostReference(IJobInvoicingPlugIn parent, ZString costReference)
		{
			((DtbConsignment)parent).LTC_JobID = costReference;
			return true;
		}

		#endregion

		#region Implementation

		DtbConsignmentInvoicingSupporter InvoicingSupporter
		{
			get { return invoicingSupporter ?? (invoicingSupporter = new DtbConsignmentInvoicingSupporter(Consignment)); }
		}

		DtbConsignmentInvoicingSupporter invoicingSupporter;

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			consignment.LTC_JobType = "LTL";
			return consignment;
		}

		protected override bool ExcludeFromTestBecauseNoBillingTab => false;

		#region Consignment

		DtbConsignment Consignment
		{
			get { return consignment ?? (consignment = (DtbConsignment)GetNewBusinessObject()); }
		}

		DtbConsignment consignment;

		#endregion

		#region Helper

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper helper;

		#endregion

		#endregion
	}
}
