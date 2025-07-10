using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSService : JobService
	{
		public CFSService(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region BusinessObject Overrides

		protected override void OnFactorySaving()
		{
			if (Parent != null && Parent.DependentServiceParents != null)
			{
				foreach (var dependent in Parent.DependentServiceParents)
				{
					dependent.Services.SetServicesOfThisToMatchOther(Parent.Services);
				}
			}

			base.OnFactorySaving();
		}

		#endregion

		#region Property Overrides

		#region ES_ServiceCode

		[List("Lookups.JobServiceType_List")]
		public override ZString ES_ServiceCode
		{
			get { return base.ES_ServiceCode; }
			set
			{
				if (!ES_ServiceCode.IsEmpty)
				{
					UpdateParentDependentsRequiredServices(service => service.ES_ServiceCode = value);
				}
				base.ES_ServiceCode = value;
			}
		}

		#endregion

		#region ES_Booked

		protected override ZDateTime ES_BookedCore
		{
			get => base.ES_BookedCore;
			set
			{
				base.ES_BookedCore = value;
				UpdateParentDependentsRequiredServices(service => service.ES_Booked = value);
			}
		}

		#endregion

		#region ES_Calc_LocationCode

		public override ZString ES_Calc_LocationCode
		{
			get { return base.ES_Calc_LocationCode; }
			set
			{
				base.ES_Calc_LocationCode = value;
				UpdateParentDependentsRequiredServices(service => service.ES_Calc_LocationCode = value);
			}
		}

		#endregion

		#region ES_Completed

		protected override ZDateTime ES_CompletedCore
		{
			get => base.ES_CompletedCore;
			set
			{
				base.ES_CompletedCore = value;
				UpdateParentDependentsRequiredServices(service => service.ES_Completed = value);
			}
		}

		#endregion

		#region ES_Duration

		public override ZDateTime ES_Duration
		{
			get { return base.ES_Duration; }
			set
			{
				base.ES_Duration = value;
				UpdateParentDependentsRequiredServices(service => service.ES_Duration = value);
			}
		}

		#endregion

		#region ES_OA_Location

		public override ZGuid ES_OA_Location
		{
			get { return base.ES_OA_Location; }
			set
			{
				base.ES_OA_Location = value;
				UpdateParentDependentsRequiredServices(service => service.ES_OA_Location = value);
			}
		}

		#endregion

		#region ES_OH_Contractor

		public override ZGuid ES_OH_Contractor
		{
			get { return base.ES_OH_Contractor; }
			set
			{
				base.ES_OH_Contractor = value;
				UpdateParentDependentsRequiredServices(service => service.ES_OH_Contractor = value);
			}
		}

		#endregion

		#region ES_References

		public override ZString ES_References
		{
			get { return base.ES_References; }
			set
			{
				base.ES_References = value;
				UpdateParentDependentsRequiredServices(service => service.ES_References = value);
			}
		}

		#endregion

		#region ES_ServiceCount

		public override ZDecimal ES_ServiceCount
		{
			get { return base.ES_ServiceCount; }
			set
			{
				base.ES_ServiceCount = value;
				UpdateParentDependentsRequiredServices(service => service.ES_ServiceCount = value);
			}
		}

		#endregion

		#endregion

		#region Implementation

		protected override Type GetTypeFromPrefix(string prefix)
		{
			if (PrefixToTypeHash == null)
			{
				PrefixToTypeHash = new Dictionary<string, Type>();
				PrefixToTypeHash["JP"] = typeof(CFSDocsAndCartage);
				PrefixToTypeHash["JC"] = typeof(CFSContainer);
			}

			return PrefixToTypeHash[prefix];
		}

		#endregion

		void UpdateParentDependentsRequiredServices(Action<JobService> action)
		{
			if (Parent != null && Parent.DependentServiceParents != null)
			{
				foreach (var dependent in Parent.DependentServiceParents)
				{
					var service = dependent.Services.GetRequiredService(this);
					if (service != null)
					{
						action.Invoke(service);
					}
				}
			}
		}
	}
}
