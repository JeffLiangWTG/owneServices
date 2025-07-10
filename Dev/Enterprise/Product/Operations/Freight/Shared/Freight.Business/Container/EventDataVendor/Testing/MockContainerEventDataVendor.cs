#if DEBUG
using System;
using System.Collections.Generic;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class MockContainerEventDataVendor : ContainerEventDataVendor, IDisposable
	{
		#region Instance

		protected MockContainerEventDataVendor()
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		public new static MockContainerEventDataVendor Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MockContainerEventDataVendor();
					OverridableNewDelegate.Value = delegate
					{ return instance; };
				}
				return instance;
			}
		}
		[ThreadStatic]
		static MockContainerEventDataVendor instance;

		#endregion

		public bool NotifyContainerCreatedCalled;
		protected override void NotifyContainerCreatedCore(CommonContainer container)
		{
			base.NotifyContainerCreatedCore(container);
			NotifyContainerCreatedCalled = true;
		}

		public bool NotifyContainerNumberChangedCalled;
		protected override void NotifyContainerNumberChangedCore(CommonContainer container)
		{
			base.NotifyContainerNumberChangedCore(container);
			NotifyContainerNumberChangedCalled = true;
		}

		public readonly List<CommonContainer> NotifyVesselChangedCalledForContainers = new List<CommonContainer>();
		protected override void NotifyVesselChangedCore(CommonContainer container)
		{
			base.NotifyVesselChangedCore(container);
			NotifyVesselChangedCalledForContainers.Add(container);
		}

		public readonly List<CommonContainer> NotifyVoyageChangedCalledForContainers = new List<CommonContainer>();
		protected override void NotifyVoyageChangedCore(CommonContainer container)
		{
			base.NotifyVoyageChangedCore(container);
			NotifyVoyageChangedCalledForContainers.Add(container);
		}

		public readonly List<CommonContainer> NotifyDischargePortChangedCalledForContainers = new List<CommonContainer>();
		protected override void NotifyDischargePortChangedCore(CommonContainer container)
		{
			base.NotifyDischargePortChangedCore(container);
			NotifyDischargePortChangedCalledForContainers.Add(container);
		}

		public readonly List<CommonContainer> NotifyETDChangedCalledForContainers = new List<CommonContainer>();
		protected override void NotifyETDChangedCore(CommonContainer container, ZPropertyInfo eTDProperty)
		{
			base.NotifyETDChangedCore(container, eTDProperty);
			NotifyETDChangedCalledForContainers.Add(container);
		}

		public readonly List<CommonContainer> NotifyETAChangedCalledForContainers = new List<CommonContainer>();
		protected override void NotifyETAChangedCore(CommonContainer container, ZPropertyInfo eTAProperty)
		{
			base.NotifyETAChangedCore(container, eTAProperty);
			NotifyETAChangedCalledForContainers.Add(container);
		}

		#region IDisposable Members

		void IDisposable.Dispose()
		{
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
			instance = null;
			OverridableNewDelegate.ResetValue();
		}

		#endregion
	}
}
#endif
