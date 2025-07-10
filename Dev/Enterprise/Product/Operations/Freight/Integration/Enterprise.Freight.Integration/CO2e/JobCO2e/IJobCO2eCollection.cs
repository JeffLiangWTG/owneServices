using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public interface IJobCO2eCollection : IEnumerable<IJobCO2e>, ICollection
	{
		IJobCO2e Get(ZString type);
		IJobCO2e GetOrCreate(ZString type);

		public event EventHandler JobCO2e_StatusChanged;
		public event EventHandler JobCO2e_UpdatedByDataRefresh;
		public event EventHandler JobCO2e_OnSaving;
	}
}
