using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Packing.Business;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportPackageDivotRelationshipTest<TInstructionPkgDivot, TDivotCollection> : ActiveBusinessObjectCollectionTestCase<TDivotCollection>
			where TInstructionPkgDivot : DtbTransportInstructionPkgDivot
			where TDivotCollection : DtbTransportInstructionPkgDivotCollection<TInstructionPkgDivot>
	{
		#region TestAddRemove

		public void TestAddRemove()
		{
			var transport = GetNewTransportBizO();
			var packageDivots = GetPackageDivotCollection(transport);
			AssertEquals("Precondition", 0, packageDivots.Count);

			var instruction = (DtbTransportInstruction)transport.Instructions.AddNew();
			AssertEquals("Precondition", 0, packageDivots.Count);

			var packageDivot = (DtbTransportInstructionPkgDivot)instruction.PackageDivots.AddNew();
			AssertContainsExactElementsInAnyOrder("Added", new DtbTransportInstructionPkgDivot[] { packageDivot }, packageDivots);

			var package = Factory.New<PkgPackage>();
			packageDivot.KD_KP_Package = package.PK;
			AssertContainsExactElementsInAnyOrder("Added", new DtbTransportInstructionPkgDivot[] { packageDivot }, packageDivots);

			var instruction2 = (DtbTransportInstruction)transport.Instructions.AddNew();
			var packageDivot2 = (DtbTransportInstructionPkgDivot)instruction2.PackageDivots.AddNew();
			AssertContainsExactElementsInAnyOrder("Added", new DtbTransportInstructionPkgDivot[] { packageDivot, packageDivot2 }, packageDivots);

			packageDivot2.KD_KN_BookingInstruction = ZGuid.Empty;
			AssertContainsExactElementsInAnyOrder("Removed", new DtbTransportInstructionPkgDivot[] { packageDivot }, packageDivots);
		}

		#endregion

		#region Implementation

		protected abstract DtbTransport GetNewTransportBizO();
		protected abstract IDtbTransportInstructionPkgDivotCollection GetPackageDivotCollection(DtbTransport transport);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			// Relationship doesn't support AddNew ... So setup packageDivot and return 

			return Instruction.PackageDivots.AddNew();
		}

		DtbTransport Transport
		{
			get { return transport ?? (transport = GetNewTransportBizO()); }
		}

		DtbTransportInstruction Instruction
		{
			get { return instruction ?? (instruction = (DtbTransportInstruction)Transport.Instructions.AddNew()); }
		}

		DtbTransport transport;
		DtbTransportInstruction instruction;

		#endregion
	}
}
