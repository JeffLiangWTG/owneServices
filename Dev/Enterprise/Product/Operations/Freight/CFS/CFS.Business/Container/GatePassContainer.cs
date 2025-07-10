
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class GatePassContainer : CFSContainer
	{
		public GatePassContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Related Business Objects

		#region PackLines

		[ChildEditable(true)]
		public new GatePassPackLineManyToManyCollection PackLines
		{
			get { return (GatePassPackLineManyToManyCollection)base.PackLines; }
		}

		protected override PackLineManyToManyCollection GetNewPackLineCollection()
		{
			return new GatePassPackLineManyToManyCollection(this);
		}

		#endregion

		#region Consol

		protected override CommonConsol LoadParentConsol()
		{
			return Factory.Load<GatePassLoadListConsol>(JC_JK);
		}

		#endregion

		#region Shipments

		protected override CFSShipmentDependentCollection GetNewPackUnpackShipmentDependentCollection()
		{
			return new GatePassShipmentDependentCollection(Factory, this);
		}

		#endregion

		#region Services

		[ChildEditable(true)]
		public new GatePassServiceDependentCollection Services
		{
			get { return (GatePassServiceDependentCollection)base.Services; }
		}

		protected override JobServiceDependentCollection GetNewServiceCollection()
		{
			return new GatePassServiceDependentCollection(this, Factory);
		}

		#endregion

		#endregion
	}
}
