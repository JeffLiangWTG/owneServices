using System;
using CargoWise.EntityFramework;

namespace Enterprise.Packing.GUI
{
	/// <summary>
	/// this class is used to suspend the Addition of Outer Package nodes to the Tree coming
	/// from Business changes. This is used when the Addition of nodes is handled by GUI,
	/// e.g when the user clicks 'Add Package'
	/// </summary>
	class AddingNodesSuspender
	{
		AddingNodesSuspender()
		{
		}

		public static IDisposable SuspendAddingNodes(BusinessObjectFactory factory)
		{
			return new SemaphoreManager(GetNodesSuspender(factory).AddingNodesSemaphore);
		}

		public static bool IsAddingNodesSuspended(BusinessObjectFactory factory)
		{
			return GetNodesSuspender(factory).AddingNodesSemaphore.IsSuspended;
		}

		static AddingNodesSuspender GetNodesSuspender(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("AddingNodesSuspender", () => new AddingNodesSuspender());
		}

		readonly Semaphore AddingNodesSemaphore = new Semaphore();
	}
}
