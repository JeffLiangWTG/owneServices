using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class JobChargeAttribCollection : DependentBusinessObjectCollection<JobChargeAttrib, JobCharge>
	{
		public JobChargeAttribCollection(JobCharge jobCharge)
			: base(jobCharge)
		{
		}

		public new JobCharge Master
		{
			get { return base.Master; }
		}

		public ZString GetValueFromName(ZString name)
		{
			ZString result = "";
			foreach (JobChargeAttrib attrib in Elements)
			{
				if (attrib.EC_Name == name)
				{
					result = attrib.EC_Value;
					break;
				}
			}
			return result;
		}

		public IEnumerable<ZString> GetAllValuesFromName(ZString name)
		{
			return Elements
				.OfType<JobChargeAttrib>()
				.Where(attribute => attribute.EC_Name == name)
				.Select(attribute => attribute.EC_Value)
				.ToList();
		}
	}
}
