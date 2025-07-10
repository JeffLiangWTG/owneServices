using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public interface IAttachedOrder
	{
		#region Common Properties

		ZString JobNo { get; }
		ZString JobDescription { get; }
		ZString JobStatus { get; }
		ZString JobType { get; }
		ZDateTime JobDate { get; }
		ZString GoodsDescription { get; }
		ModuleIdentifier ModuleID { get; }
		ControllerID ControllerID { get; }
		bool ShouldSkipAllValidations { get; set; }

		#endregion

		#region Orders

		ZString TransportMode { get; }
		ZDateTime ETD { get; }
		ZDateTime ETA { get; }
		ZDateTime RequiredExWorks { get; }
		ZDateTime RequiredInStore { get; }
		ZDecimal TotalWeight { get; }
		ZString WeightUnit { get; }
		ZDecimal TotalVolume { get; }
		ZString VolumeUnit { get; }
		ZDecimal QuantityRemaining { get; }
		ZDecimal QuantityInvoiced { get; }
		ZDecimal QuantityOrdered { get; }
		ZDecimal QuantityReceived { get; }
		ZString BuyerOrgCode { get; }
		ZString SupplierOrgCode { get; }
		ZInt TotalPacks { get; }
		ZString PacksType { get; }

		#endregion
	}
}
