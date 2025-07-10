using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DivotsWithPackagesCollectionTest<T> : ActiveBusinessObjectCollectionTestCase<DivotsWithPackagesCollection<T>>
			where T : DtbTransportInstructionPkgDivot
	{
		#region TestConstructorDoesNotAllowNullInstruction

		public void TestConstructorDoesNotAllowNullInstruction()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new DivotsWithPackagesCollection<T>(null));
		}

		#endregion

		#region TestAllowNew_Instruction

		public void TestAllowNew_Instruction()
		{
			var instruction = GetNewInstruction();
			var packages = new DivotsWithPackagesCollection<T>(instruction);
			AssertEquals(false, ((IBindingList)packages).AllowNew);
		}

		#endregion

		#region TestAddRemove_Instruction

		public void TestAddRemove_Instruction()
		{
			var instruction = GetNewInstruction();
			var packages = new DivotsWithPackagesCollection<T>(instruction);
			AssertEquals("Precondition", 0, packages.Count);

			var packageDivot = (T)instruction.PackageDivots.AddNew();
			AssertEquals("divot doesn't have a package yet, so should still be 0", 0, packages.Packages.Count());

			var package = Factory.New<PkgPackage>();
			packageDivot.KD_KP_Package = package.PK;
			AssertContainsExactElementsInAnyOrder("after setting the package it should have found it, if not, check to see if listening onto kd_kp", new[] { package }, packages.Packages);

			var packageDivot2 = (T)instruction.PackageDivots.AddNew();
			var package2 = Factory.New<PkgPackage>();
			packageDivot2.KD_KP_Package = package2.PK;
			AssertContainsExactElementsInAnyOrder("should now have 2", new[] { package, package2 }, packages.Packages);

			packageDivot2.KD_KP_Package = ZGuid.Empty;
			AssertContainsExactElementsInAnyOrder("should now only have the first one", new[] { package }, packages.Packages);

			packageDivot.KD_KN_BookingInstruction = ZGuid.Empty;
			AssertEquals("Disconnect from instruction instead, should now have none", 0, packages.Packages.Count());
		}

		#endregion

		#region TestAddPackage

		public void TestAddPackage()
		{
			var instruction = GetNewInstruction();
			var packages = new DivotsWithPackagesCollection<T>(instruction);
			var package = Factory.New<PkgPackage>();
			packages.AddPackage(null);
			AssertEquals(0, instruction.PackageDivots.Count);

			packages.AddPackage(package);
			AssertEquals(1, instruction.PackageDivots.Count);
			AssertEquals(package.PK, ((T)instruction.PackageDivots[0]).KD_KP_Package);

			packages.AddPackage(package);
			AssertEquals(1, instruction.PackageDivots.Count);
		}

		#endregion

		#region TestAddPackages

		public void TestAddPackages()
		{
			var address = Factory.New<OrgAddress>();
			address.OA_AIREquipmentNeeded = "DM1";
			address.OA_LCLEquipmentNeeded = "DM2";
			address.OA_FCLEquipmentNeeded = "DM3";

			var instruction = GetNewInstruction();
			instruction.Address.E2_OA_Address = address.PK;
			instruction.KN_DropMode = "";

			var dropModeChangedHitCount = 0;
			instruction.KN_DropModeInfo.ValueChanged += (sender, e) => dropModeChangedHitCount++;

			var packages = new DivotsWithPackagesCollection<T>(instruction);
			var package1 = Factory.New<PkgPackage>();
			var package2 = Factory.New<PkgPackage>();
			packages.AddPackages(new[] { package1, package2 });
			AssertContainsExactElementsInAnyOrder(new[] { package1, package2 }, packages.Packages);
			AssertEquals("Should have defaulted Drop Mode.", "DM2", instruction.KN_DropMode);
			AssertEquals("Should only have changed Drop Mode once.", 1, dropModeChangedHitCount);
		}

		public void TestAddPackages_UpdatesQuantityIfReAddingExistingPackage()
		{
			var instruction = GetNewInstruction();
			var package = Factory.New<PkgPackage>();
			package.KP_PackageQty = 10;

			var packages = new DivotsWithPackagesCollection<T>(instruction);
			packages.AddPackage(package);
			AssertContainsExactElementsInAnyOrder(new[] { package }, packages.Packages);
			AssertEquals("Quantity should be based on Package.", 10, packages.Single().KD_Quantity);

			package.KP_PackageQty = 1;
			packages.AddPackages(new[] { package });
			AssertContainsExactElementsInAnyOrder(new[] { package }, packages.Packages);
			AssertEquals("Quantity should be updated.", 1, packages.Single().KD_Quantity);
		}

		#endregion

		#region TestContains

		public void TestContains()
		{
			var instruction = GetNewInstruction();
			var packages = new DivotsWithPackagesCollection<T>(instruction);
			var package = Factory.New<PkgPackage>();
			AssertEquals(false, packages.Contains(package));

			var divot = packages.AddNew();
			AssertEquals(false, packages.Contains(package));

			divot.KD_KP_Package = package.PK;
			AssertEquals(true, packages.Contains(package));

			divot.Delete();
			AssertEquals(false, packages.Contains(package));
		}

		#endregion

		#region TestIndexer

		public void TestIndexer()
		{
			var instruction = GetNewInstruction();
			var packages = new DivotsWithPackagesCollection<T>(instruction);
			var divot = packages.AddNew();
			DtbTransportInstructionPkgDivot indexerDivot = ((IDivotsWithPackagesCollection)packages)[0];
			AssertEquals(divot, indexerDivot);
		}

		#endregion

		#region TestPackages

		public void TestPackages()
		{
			var instruction = GetNewInstruction();
			var packages = new DivotsWithPackagesCollection<T>(instruction);
			var package1 = Factory.New<PkgPackage>();
			var package2 = Factory.New<PkgPackage>();

			var divot1 = packages.AddNew();
			packages.AddNew();
			var divot3 = packages.AddNew();
			divot1.KD_KP_Package = package1.PK;
			divot3.KD_KP_Package = package2.PK;
			AssertContainsExactElementsInAnyOrder(new[] { package1, package2 }, packages.Packages);

			package2.Delete();
			AssertContainsExactElementsInAnyOrder(new[] { package1 }, packages.Packages);
		}

		#endregion

		#region TestRemovePackage

		public void TestRemovePackage()
		{
			var instruction = GetNewInstruction();
			var packages = new DivotsWithPackagesCollection<T>(instruction);
			var package1 = Factory.New<PkgPackage>();
			var package2 = Factory.New<PkgPackage>();
			var divot1 = (T)instruction.PackageDivots.AddNew();
			divot1.KD_KP_Package = package1.PK;
			var divot2 = (T)instruction.PackageDivots.AddNew();

			packages.RemovePackage(null);
			AssertContainsExactElementsInAnyOrder(new[] { divot1, divot2 }, instruction.PackageDivots);

			packages.RemovePackage(package2);
			AssertContainsExactElementsInAnyOrder(new[] { divot1, divot2 }, instruction.PackageDivots);

			packages.RemovePackage(package1);
			AssertEquals(1, instruction.PackageDivots.Count);
			AssertContainsExactElementsInAnyOrder(new[] { divot2 }, instruction.PackageDivots);
		}

		public void TestRemovePackage_PackageDetatchedFromRow()
		{
			var instruction = GetNewInstruction();
			var packages = new DivotsWithPackagesCollection<T>(instruction);
			var rowFactory = ((IBusinessObjectFactoryInternals)Factory).RowFactory;
			var package1 = new PkgPackage(Factory, rowFactory.New(PkgPackageSchema.Constants.TableName));
			var package2 = new PkgPackage(Factory, rowFactory.New(PkgPackageSchema.Constants.TableName));
			var divot1 = (T)instruction.PackageDivots.AddNew();
			divot1.KD_KP_Package = package1.PK;
			var divot2 = (T)instruction.PackageDivots.AddNew();

			packages.RemovePackage(null);
			AssertContainsExactElementsInAnyOrder(new[] { divot1, divot2 }, instruction.PackageDivots);

			packages.RemovePackage(package2);
			AssertContainsExactElementsInAnyOrder(new[] { divot1, divot2 }, instruction.PackageDivots);

			packages.RemovePackage(package1);
			AssertEquals(1, instruction.PackageDivots.Count);
			AssertContainsExactElementsInAnyOrder(new[] { divot2 }, instruction.PackageDivots);
		}

		#endregion

		#region TestTyped

		public void TestTyped()
		{
			var instruction = GetNewInstruction();
			var packages = new DivotsWithPackagesCollection<T>(instruction);
			var divot1 = packages.AddNew();
			var divot2 = packages.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { divot1, divot2 }, ((IDivotsWithPackagesCollection)packages).Typed);
		}

		#endregion

		#region Implementation

		protected override DivotsWithPackagesCollection<T> GetCollectionToTest()
		{
			return new DivotsWithPackagesCollection<T>(Instruction);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			// Relationship doesn't support AddNew ... So setup package and return 

			var packageDivot = Factory.New<T>();
			var package = Factory.New<PkgPackage>();
			packageDivot.KD_KP_Package = package.PK;
			return packageDivot;
		}

		DtbTransportInstruction Instruction
		{
			get { return instruction ?? (instruction = GetNewInstruction()); }
		}

		protected abstract DtbTransportInstruction GetNewInstruction();

		DtbTransportInstruction instruction;

		#endregion
	}
}
