using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportInstructionPkgDivotCollection<T> : ActiveBusinessObjectCollection<T>, IDtbTransportInstructionPkgDivotCollection
		where T : DtbTransportInstructionPkgDivot
	{
		protected DtbTransportInstructionPkgDivotCollection(DtbTransportInstruction instruction)
			: base(instruction.Factory, new InstructionDivotRelationship(instruction))
		{
		}

		protected DtbTransportInstructionPkgDivotCollection(DtbTransport transport)
			: base(transport.Factory, new DtbTransportPackageDivotRelationship<T>(transport))
		{
		}

		protected DtbTransportInstructionPkgDivotCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected DtbTransportInstructionPkgDivotCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		protected DtbTransportInstructionPkgDivotCollection(Package_PackageView package_PackageView)
			: base(package_PackageView.Factory, package_PackageView.Package, null, DtbBookingInstructionPkgDivotSchema.KD_KP_Package)
		{
			Package_PackageView = package_PackageView;
		}
		protected readonly Package_PackageView Package_PackageView;

		#region IDtbTransportInstructionPkgDivotCollection Members

		DtbTransportInstructionPkgDivot IDtbTransportInstructionPkgDivotCollection.this[int index] => this[index];

		#endregion

		class InstructionDivotRelationship : DependentRelationship
		{
			public InstructionDivotRelationship(DtbTransportInstruction instruction)
				: base(instruction, typeof(T), null, DtbBookingInstructionPkgDivotSchema.KD_KN_BookingInstruction)
			{
			}

			protected override void AddToRelationship(BusinessObject businessObject)
			{
				base.AddToRelationship(businessObject);

				var divot = (DtbTransportInstructionPkgDivot)businessObject;
				foreach (DtbTransportConfirmation confirmation in divot.ConfirmationsDivotOnly)
				{
					confirmation.KK_KN_BookingInstruction = Master.PK;
				}
			}
		}
	}
}
