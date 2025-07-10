using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsPickStrategy
	{
		void OnFinalised(BusinessObjectFactory factory, IWhsPick pick);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "It's the best way to also pass on an error notification, which the caller might need.")]
		bool IsOrderActionAllowed(IWhsOrder order, PickOrderAction action, out ErrorNotification reasonNotAllowed);
	}

	[Flags]
	public enum PickOrderAction
	{
		None = 0,
		AttachOrder = 1,
		DetachOrder = 2
		// Next should be 4, 8, 16...
	}
}
