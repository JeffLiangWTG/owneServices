using CargoWise.EntityFramework;

namespace Enterprise.Freight.Agency.Business
{
	public class BulkMovementsChildCollection : NonPersistentBusinessObjectCollection<BulkMovementsChild>
	{
		public BulkMovementsChildCollection(BulkMovementsHeader header)
			: base(header.Factory)
		{
			this.header = header;
		}

		#region Implementation

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			BulkMovementsChild newChild = (BulkMovementsChild)child;
			base.SetDefaultsForNewChild(newChild);

			using (newChild.GetValidationSuspender())
			{
				newChild.DepotAddressPK = header.DepotAddressPK;
				newChild.MovementType = header.MovementType;
				newChild.MovementDate = header.MovementDate;
				newChild.PrincipalPK = header.PrincipalPK;
				newChild.ResponsiblePartyPK = header.ResponsiblePartyPK;
				newChild.ContainerIsEmpty = header.ContainerIsEmpty;
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BulkMovementsChild(header);
		}

		readonly BulkMovementsHeader header;

		#endregion
	}
}
