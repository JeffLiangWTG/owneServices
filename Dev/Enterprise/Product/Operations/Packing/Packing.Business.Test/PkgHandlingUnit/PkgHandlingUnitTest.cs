using System;
using System.Linq;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.Packing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgHandlingUnit))]
	[TestsSubclassesOf(typeof(PackingParentTestCase.TestExcludePackingParentHasTestCase))]
	class PkgHandlingUnitTest : PackingParentTestCase<PkgHandlingUnit>
	{
		public void TestHandlingUnitID()
		{
			var handlingUnitWithID = Factory.New<PkgHandlingUnit>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = handlingUnitWithID.PK;
			packageJob.KJ_ParentTableCode = PkgHandlingUnitSchema.Constants.Prefix;

			var packageHeader = Factory.New<PkgPackageHeader>();
			packageHeader.KPH_PackageID = "HU001";
			var package = packageJob.Packages.AddNew();
			package.KP_KPH_PackageHeader = packageHeader.PK;

			AssertEquals("Has Handling Unit ID", "HU001", handlingUnitWithID.HandlingUnitID);

			var handlingUnitWithoutID = Factory.New<PkgHandlingUnit>();
			var packageJob2 = Factory.New<PkgPackageJob>();
			packageJob2.KJ_ParentID = handlingUnitWithID.PK;
			packageJob2.KJ_ParentTableCode = PkgHandlingUnitSchema.Constants.Prefix;
			packageJob2.Packages.AddNew();

			AssertEquals("No Handling Unit ID", "", handlingUnitWithoutID.HandlingUnitID);
		}

		public void TestDocumentSupporter()
		{
			var handlingUnit = Factory.New<PkgHandlingUnit>();
			var documentSupporter = (IDocumentSupportable)handlingUnit;
			AssertEquals(typeof(PkgHandlingUnitDocumentSupporter), documentSupporter.DocumentSupporter.GetType());
		}

		public void TestHunmanReadableName()
		{
			var handlingunit = Factory.New<PkgHandlingUnit>();
			AssertEquals("Package Handling Unit", handlingunit.HumanReadableName);
		}

		#region IPackingParent

		public void TestIPackingParent_GetPackageActionStrategy()
		{
			var package = Factory.New<PkgPackage>();
			var handlingUnit = Factory.New<PkgHandlingUnit>();
			var packingParent = (IPackingParent)handlingUnit;

			var actualStrategy = packingParent.GetPackageActionStrategy(package);
			var expectedStrategy = new PackageActionStrategy(package);

			AssertEquals(expectedStrategy.Package, actualStrategy.Package);
			AssertEquals(expectedStrategy.ReasonForNotAllowingAction, actualStrategy.ReasonForNotAllowingAction);
			foreach (PackageAction action in Enum.GetValues(typeof(PackageAction)))
			{
				AssertEquals(expectedStrategy.IsActionAllowed(action), actualStrategy.IsActionAllowed(action));
			}
		}

		public void TestIPackingParent_ControllerID()
		{
			var handlingUnit = Factory.New<PkgHandlingUnit>();
			var packingParent = (IPackingParent)handlingUnit;
			AssertNull(packingParent.ControllerID);
		}

		public void TestIPackingParent_GetSSCCPrefix()
		{
			var handlingUnit = Factory.New<PkgHandlingUnit>();
			var packingParent = (IPackingParent)handlingUnit;
			AssertEquals("", packingParent.GetSSCCPrefix(null, SSCCGenerationContext.GeneratingIDsViaUser));
		}

		public void TestIPackingParent_DocumentOptions()
		{
			var handlingUnit = Factory.New<PkgHandlingUnit>();
			var packingParent = (IPackingParent)handlingUnit;
			AssertEquals(DocumentOptions.ShowBasicLabelOnly, packingParent.DocumentOptions);
		}

		public void TestIPackingParent_IsAutoPrintAllowed()
		{
			var handlingUnit = Factory.New<PkgHandlingUnit>();
			var packingParent = (IPackingParent)handlingUnit;
			AssertEquals(false, packingParent.IsAutoPrintAllowed);
		}

		public void TestIPackingParent_IsParentJobFinalised()
		{
			var handlingUnit = Factory.New<PkgHandlingUnit>();
			var packingParent = (IPackingParent)handlingUnit;
			AssertEquals(false, packingParent.IsParentJobFinalised);
		}

		public void TestIPackingParent_IsScanEventsVisible()
		{
			var handlingUnit = Factory.New<PkgHandlingUnit>();
			var packingParent = (IPackingParent)handlingUnit;
			AssertEquals(false, packingParent.IsScanEventsVisible);
		}

		public void TestIPackingParent_JobDescription()
		{
			var handlingUnit = Factory.New<PkgHandlingUnit>();
			var packingParent = (IPackingParent)handlingUnit;
			AssertEquals("Handling Unit", packingParent.JobDescription);
		}

		public void TestIPackingParent_ConnoteNo()
		{
			var handlingUnit = Factory.New<PkgHandlingUnit>();
			var packingParent = (IPackingParent)handlingUnit;
			AssertEquals("", packingParent.ConnoteNo);
		}
		public void TestIPackingParent_JobNo()
		{
			var handlingUnit = Factory.New<PkgHandlingUnit>();
			var packingParent = (IPackingParent)handlingUnit;
			AssertEquals("HU", packingParent.JobNo);
		}

		public void TestIPackingParent_CarrierBookingAgent()
		{
			var handlingUnit = Factory.New<PkgHandlingUnit>();
			var packingParent = (IPackingParent)handlingUnit;
			AssertNull(packingParent.CarrierBookingAgent);
		}

		public void TestIPackingParent_GetCarrier()
		{
			var package = Factory.New<PkgPackage>();
			var handlingUnit = Factory.New<PkgHandlingUnit>();
			var packingParent = (IPackingParent)handlingUnit;
			AssertNull(packingParent.GetCarrier(package));
		}

		public void TestIPackingParent_TransportReference()
		{
			var handlingUnit = Factory.New<PkgHandlingUnit>();
			var packingParent = (IPackingParent)handlingUnit;
			AssertEquals("", packingParent.TransportReference);

			packingParent.TransportReference = "AA";
			AssertEquals("", packingParent.TransportReference);
		}

		public void TestIPackingParent_IsLoosePackageIDsSupported()
		{
			var handlingUnit = Factory.New<PkgHandlingUnit>();
			var packingParent = (IPackingParent)handlingUnit;
			AssertEquals(false, packingParent.IsLoosePackageIDsSupported);
		}

		public void TestIPackingParent_IsUXMLEventParent()
		{
			var handlingUnit = Factory.New<PkgHandlingUnit>();
			var packingParent = (IPackingParent)handlingUnit;
			AssertEquals(false, packingParent.IsUXMLEventParent(null));
		}

		public void TestIPackingParent_GetAdditionalEventContextValuesFromParent()
		{
			var handlingUnit = Factory.New<PkgHandlingUnit>();
			var packingParent = (IPackingParent)handlingUnit;
			var additionalEventContext = packingParent.GetAdditionalEventContextValuesFromParent();
			AssertEquals(0, additionalEventContext.Count());
		}

		public void TestIPackingParent_ParentJobType()
		{
			var handlingUnit = Factory.New<PkgHandlingUnit>();
			var packingParent = (IPackingParent)handlingUnit;
			AssertEquals(ParentJobType.None, packingParent.ParentJobType);
		}

		public void TestIPackingParent_PackageSequenceType()
		{
			var handlingUnit = Factory.New<PkgHandlingUnit>();
			var packingParent = (IPackingParent)handlingUnit;
			AssertEquals(PackageSequenceType.Outer, packingParent.PackageSequenceType);
		}

		public void TestIPackingParent_IsPackingJobReadOnly()
		{
			var handlingUnit = Factory.New<PkgHandlingUnit>();
			var packingParent = (IPackingParent)handlingUnit;
			AssertEquals(true, packingParent.IsPackingJobReadOnly);
		}

		#endregion

		#region Implementation

		protected override PkgHandlingUnit GetNewParent()
		{
			return Factory.NewWithValidTestData<PkgHandlingUnit>();
		}

		public override void TestJobNoIsNotEmptyAfterSave()
		{
			Assert("Handling Unit Package Job does not have Job ID", true);
		}

		#endregion
	}
}
