using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Packing.Business;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class PackageCollectionForTransportTest<T> : ActiveBusinessObjectCollectionTestCase<PackageCollectionForTransport>
			where T : DtbTransport
	{
		#region TestAllowNew_Booking

		public void TestAllowNew_Booking()
		{
			var transport = GetNewTransportBizO();
			var packages = new PackageCollectionForTransport(transport);
			AssertEquals(false, ((IBindingList)packages).AllowNew);
		}

		#endregion

		#region TestAddRemove_Movement

		public void TestAddRemove_Movement()
		{
			var transport = GetNewTransportBizO();
			var packages = new PackageCollectionForTransport(transport);
			AssertEquals("Precondition", 0, packages.Count);

			var instruction = (DtbTransportInstruction)transport.Instructions.AddNew();
			AssertEquals("Precondition", 0, packages.Count);

			var packageDivot = (DtbTransportInstructionPkgDivot)instruction.PackageDivots.AddNew();
			AssertEquals("divot doesn't have a package yet, so should still be 0", 0, packages.Count);

			var package = Factory.New<PkgPackage>();
			packageDivot.KD_KP_Package = package.PK;
			AssertContainsExactElementsInAnyOrder("after setting the package it should have found it, if not, check to see if listening onto kd_kp", new PkgPackage[] { package }, packages);

			var packageDivot2 = (DtbTransportInstructionPkgDivot)instruction.PackageDivots.AddNew();
			var package2 = Factory.New<PkgPackage>();
			packageDivot2.KD_KP_Package = package2.PK;
			AssertContainsExactElementsInAnyOrder("should now have 2", new PkgPackage[] { package, package2 }, packages);

			packageDivot2.KD_KP_Package = ZGuid.Empty;
			AssertContainsExactElementsInAnyOrder("should now only have the first one", new PkgPackage[] { package }, packages);

			packageDivot.KD_KN_BookingInstruction = ZGuid.Empty;
			AssertEquals("Disconnect from instruction instead, should now have none", 0, packages.Count);
		}

		#endregion

		#region Implementation

		protected override PackageCollectionForTransport GetCollectionToTest()
		{
			return new PackageCollectionForTransport(Transport);
		}

		T Transport
		{
			get { return transport ?? (transport = GetNewTransportBizO()); }
		}
		T transport;

		DtbTransportInstruction Instruction
		{
			get { return instruction ?? (instruction = (DtbTransportInstruction)Transport.Instructions.AddNew()); }
		}
		DtbTransportInstruction instruction;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			// Relationship doesn't support AddNew ... So setup package and return 
			var packageDivot = (DtbTransportInstructionPkgDivot)Instruction.PackageDivots.AddNew();
			var package = Factory.New<PkgPackage>();
			packageDivot.KD_KP_Package = package.PK;
			return package;
		}

		protected abstract T GetNewTransportBizO();

		#endregion
	}
}
