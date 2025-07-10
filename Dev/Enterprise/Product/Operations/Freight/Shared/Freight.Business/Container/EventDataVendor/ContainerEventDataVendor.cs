using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public abstract class ContainerEventDataVendor
	{
		#region Factory Method

		public static ContainerEventDataVendor Instance
		{
			get
			{
				ContainerEventDataVendor result;
				var overridden = OverridableNewDelegate.Value;
				if (overridden != null)
				{
					result = overridden();
				}
				else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia || GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.NewZealand)
				{
					result = OneStopVendor;
				}
				else
				{
					result = new NoActionContainerEventDataVendor();
				}
				return result;
			}
		}

		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();
		protected delegate ContainerEventDataVendor NewDelegate();

		#endregion

		#region NotifyContainerCreated

		public void NotifyContainerCreated(CommonContainer container)
		{
			NotifyContainerCreatedCore(container);
		}

		protected virtual void NotifyContainerCreatedCore(CommonContainer container)
		{
		}

		#endregion

		#region NotifyContainerNumberChanged

		public void NotifyContainerNumberChanged(CommonContainer container)
		{
			NotifyContainerNumberChangedCore(container);
		}

		protected virtual void NotifyContainerNumberChangedCore(CommonContainer container)
		{
		}

		#endregion

		#region NotifyETDChanged / NotifyETAChanged

		public void NotifyETDChanged(CommonContainer container, ZPropertyInfo eTDProperty)
		{
			NotifyETDChangedCore(container, eTDProperty);
		}

		protected virtual void NotifyETDChangedCore(CommonContainer container, ZPropertyInfo eTDProperty)
		{
		}

		public void NotifyETAChanged(CommonContainer container, ZPropertyInfo eTAProperty)
		{
			NotifyETAChangedCore(container, eTAProperty);
		}

		protected virtual void NotifyETAChangedCore(CommonContainer container, ZPropertyInfo eTAProperty)
		{
		}

		#endregion

		#region NotifyVesselChanged

		public void NotifyVesselChanged(CommonContainer container)
		{
			NotifyVesselChangedCore(container);
		}

		protected virtual void NotifyVesselChangedCore(CommonContainer container)
		{
		}

		#endregion

		#region NotifyVoyageChanged

		public void NotifyVoyageChanged(CommonContainer container)
		{
			NotifyVoyageChangedCore(container);
		}

		protected virtual void NotifyVoyageChangedCore(CommonContainer container)
		{
		}

		#endregion

		#region NotifyDischargePortChanged

		public void NotifyDischargePortChanged(CommonContainer container)
		{
			NotifyDischargePortChangedCore(container);
		}

		protected virtual void NotifyDischargePortChangedCore(CommonContainer container)
		{
		}

		#endregion

		#region Implementation

		static ContainerEventDataVendor OneStopVendor
		{
			get
			{
				if (fOneStopVendor == null)
				{
					fOneStopVendor = (ContainerEventDataVendor)Activator.CreateInstance(ObjectFactory.GetType<Integration.SailingDataVendor.IOneStopContainerEventDataVendor>());
				}
				return fOneStopVendor;
			}
		}
		[ThreadStatic]
		static ContainerEventDataVendor fOneStopVendor;

		#endregion
	}
}
