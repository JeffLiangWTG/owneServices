using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class DummyAttachedOrderBusinessObject : DummyBusinessObject, IAttachedOrder
	{
		public DummyAttachedOrderBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region IAttachedOrder Members

		public ControllerID ControllerID
		{
			get { return DummyControllerIDs.Dummy; }
		}

		public ModuleIdentifier ModuleID
		{
			get { return DummyModuleIDs.Dummy; }
		}

		public ZString JobDescription
		{
			get { return "Dummy Job Description"; }
		}

		public ZString JobStatus
		{
			get { return "Dummy Job Status"; }
		}

		public ZString GoodsDescription
		{
			get { return "Chinese Tie Fighters"; }
		}

		public ZDateTime JobDate
		{
			get { return new ZDateTime(2012, 1, 1); }
		}

		public ZString JobNo
		{
			get { return "Dummy Job No."; }
		}

		public ZString JobType
		{
			get { return "DUM"; }
		}

		public bool ShouldSkipAllValidations
		{
			get; set;
		}

		public ZString TransportMode => Core.Constants.TransportModes.Sea;
		public ZDateTime ETD => new ZDateTime(2015, 01, 01);
		public ZDateTime ETA => new ZDateTime(2015, 02, 01);
		public ZDateTime RequiredExWorks => new ZDateTime(2015, 01, 01);
		public ZDateTime RequiredInStore => new ZDateTime(2015, 01, 01);
		public ZDecimal TotalWeight => 1;
		public ZString WeightUnit => ZString.Empty;
		public ZDecimal TotalVolume => 1;
		public ZString VolumeUnit => ZString.Empty;
		public ZDecimal QuantityRemaining => 1;
		public ZDecimal QuantityInvoiced => 1;
		public ZDecimal QuantityOrdered => 1;
		public ZDecimal QuantityReceived => 1;
		public ZString BuyerOrgCode => ZString.Empty;
		public ZString SupplierOrgCode => ZString.Empty;
		public ZInt TotalPacks => 2;
		public ZString PacksType => ZString.Empty;

		#endregion
	}
}
