using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Business.UniversalCopy;

namespace Enterprise.Freight.CarbonEmissions.Business
{
	public class JobCO2e : AutoJobCO2e, IJobCO2e
	{
		public JobCO2e(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		ICO2eParent Parent
		{
			get
			{
				if (parent == null)
				{
					parent = Factory.Load(JCO_ParentTableCode, JCO_ParentID) as ICO2eParent;
					if (parent == null)
					{
						ErrorReporter.ReportOnce("JobCO2eParentLoadError", "JobCO2e parent is not found or ICO2eProvider implementation is missing from the Parent BO class");
					}
				}
				return parent;
			}
		}
		ICO2eParent parent;

		#region Properties

		public override ZDecimal JCO_CO2ePerTonneInKg
		{
			get => base.JCO_CO2ePerTonneInKg;
			set
			{
				base.JCO_CO2ePerTonneInKg = value;
				if (!IsRunningCopy)
				{
					JCO_Status = CO2eStatusList.Codes.Current;
					Parent?.RefreshCO2e();
				}
			}
		}

		public override ZDecimal JCO_CO2ePerTEUInKg
		{
			get => base.JCO_CO2ePerTEUInKg;
			set
			{
				base.JCO_CO2ePerTEUInKg = value;
				if (!IsRunningCopy)
				{
					JCO_Status = CO2eStatusList.Codes.Current;
					Parent?.RefreshCO2e();
				}
			}
		}

		bool IsRunningCopy => IsCopying || BusinessObjectUniversalCopyFactoryService.IsRunningUniversalCopy(Factory);

		public override ZDecimal JCO_TotalCO2e
		{
			get => base.JCO_TotalCO2e;
			set
			{
				if (base.JCO_TotalCO2e != value)
				{
					base.JCO_TotalCO2e = value;
				}
			}
		}

		#endregion

		#region Overrides

		protected override bool SupportsCloneCore() => true;

		public override void OnSaving()
		{
			base.OnSaving();
			OnJobCO2eSaving?.Invoke(this, EventArgs.Empty);
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();
			Parent?.RefreshCO2e();
		}

		#endregion

		#region IJobCO2e interface members

		ZString IJobCO2e.Status
		{
			get => JCO_Status;
			set => JCO_Status = value;
		}

		ZDecimal IJobCO2e.TotalCO2e
		{
			get => JCO_TotalCO2e;
			set => JCO_TotalCO2e = value;
		}

		ZDecimal IJobCO2e.CO2ePerTonneInKg
		{
			get => JCO_CO2ePerTonneInKg;
			set => JCO_CO2ePerTonneInKg = value;
		}

		ZDecimal IJobCO2e.CO2ePerTEUInKg
		{
			get => JCO_CO2ePerTEUInKg;
			set => JCO_CO2ePerTEUInKg = value;
		}

		ZDecimal IJobCO2e.DistanceInKM
		{
			get => JCO_DistanceInKM;
			set => JCO_DistanceInKM = value;
		}

		event EventHandler IJobCO2e.StatusChanged
		{
			add { JCO_StatusInfo.ValueChanged += value; }
			remove { JCO_StatusInfo.ValueChanged -= value; }
		}

		event EventHandler IJobCO2e.JobCO2eOnSaving
		{
			add { OnJobCO2eSaving += value; }
			remove { OnJobCO2eSaving -= value; }
		}
		EventHandler OnJobCO2eSaving;

		#endregion
	}
}
