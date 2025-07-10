using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class BillOfLadingPackLine : AgencyShipmentPackLine
	{
		public BillOfLadingPackLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			JL_UnitOfDimension = AgencyRegistry.Instance.DefaultBillDimensionUnit.Value;
		}

		#region Validation

		public new BillOfLadingPackLineValidation Validation
		{
			get { return (BillOfLadingPackLineValidation)base.Validation; }
		}

		protected override JobPackLinesValidation GetNewValidation()
		{
			return new BillOfLadingPackLineValidation(this);
		}

		#endregion

		#region Related Business Objects

		#region Shipment

		protected override CommonShipment GetParentShipment()
		{
			return Factory.Load<BillOfLading>(JL_JS);
		}

		public new BillOfLading Shipment
		{
			get { return (BillOfLading)base.Shipment; }
		}

		#endregion

		#region Containers

		public new BillOfLadingContainer Container
		{
			get { return (BillOfLadingContainer)LoadContainer(JL_JC); }
		}

		protected override CommonContainer LoadContainer(ZGuid containerPK)
		{
			return Factory.Load<BillOfLadingContainer>(containerPK);
		}

		protected override AgencyShipmentContainerManyToManyCollection GetNewContainersCollectionCore()
		{
			return new BillOfLadingContainerManyToManyCollection(this);
		}

		#endregion

		#endregion
	}
}


