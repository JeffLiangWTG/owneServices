using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JobServiceDependentCollection : DependentBusinessObjectCollection<JobService, BusinessObject>
	{
		public JobServiceDependentCollection(BusinessObject parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
			this.Parent = (IHaveServices)parent;
		}

		#region Overrides

		protected override bool AllowNewCore
		{
			get { return true; }
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return JobServiceSchema.ES_ParentID; }
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			var service = (JobService)bizOAdded;
			service.Parent = Parent;
		}

		#endregion

		#region Synchronise Services

		#region SetServicesOfThisToMatchOther

		public void SetServicesOfThisToMatchOther(JobServiceDependentCollection other)
		{
			foreach (JobService otherService in other)
			{
				if (new FreightServiceTypes().ContainsCode(otherService.ES_ServiceCode))
				{
					var matchingService = GetService(otherService);
					if (matchingService != null)
					{
						matchingService.ES_Booked = matchingService.ES_Booked.IsEmpty ? otherService.ES_Booked : matchingService.ES_Booked;
						matchingService.ES_Completed = matchingService.ES_Completed.IsEmpty ? otherService.ES_Completed : matchingService.ES_Completed;
						matchingService.ES_Duration = matchingService.ES_Duration.IsEmpty ? otherService.ES_Duration : matchingService.ES_Duration;
						matchingService.ES_OA_Location = matchingService.ES_OA_Location.IsEmpty ? otherService.ES_OA_Location : matchingService.ES_OA_Location;
						matchingService.ES_OH_Contractor = matchingService.ES_OH_Contractor.IsEmpty ? otherService.ES_OH_Contractor : matchingService.ES_OH_Contractor;
						matchingService.ES_References = matchingService.ES_References.IsEmpty ? otherService.ES_References : matchingService.ES_References;
						matchingService.ES_ServiceCount = matchingService.ES_ServiceCount.IsEmpty ? otherService.ES_ServiceCount : matchingService.ES_ServiceCount;
						matchingService.ES_ServiceCode = matchingService.ES_ServiceCode.IsEmpty ? otherService.ES_ServiceCode : matchingService.ES_ServiceCode;
					}
					else
					{
						matchingService = (JobService)otherService.Clone();
						Add(matchingService);
					}
					matchingService.UpdateExternalServiceId(otherService, twoWay: false);
				}
			}
		}

		#endregion

		#region SetServicesOfOtherToMatchThis

		public void SetServicesOfOtherToMatchThis(JobServiceDependentCollection other)
		{
			other.SetServicesOfThisToMatchOther(this);
		}

		#endregion

		#region SetServicesForContainerUnpack

		public void SetServicesForContainerUnpack(IHaveServices container)
		{
			foreach (JobService containerService in container.Services)
			{
				RemoveIfExists(containerService);
			}
		}

		#endregion

		#region AddIfNotExists

		public IEnumerable<JobService> AddIfNotExists(ZString serviceCode)
		{
			var result = GetServices(serviceCode);
			if (result.Any())
			{
				return result;
			}

			var service = AddNew();
			service.ES_ServiceCode = serviceCode;
			return new[] { service };
		}

		#endregion

		#region RemoveIfExists

		internal void RemoveIfExists(JobService service)
		{
			var matchingService = GetService(service);
			if (matchingService != null)
			{
				RemoveAndDelete(matchingService);
			}
		}

		#endregion

		#endregion

		#region Update Contractors

		public void UpdateDefaultContractor(ZString serviceCode)
		{
			foreach (JobService service in this)
			{
				if (service.ES_ServiceCode == serviceCode)
				{
					service.SetToDefaultContractor();
				}
			}
		}

		#endregion

		#region GetService

		public JobService GetService(JobService service)
			=> this.Cast<JobService>().GetService(service);

		public IEnumerable<JobService> GetServices(ZString serviceCode)
			=> Where(service => service.ES_ServiceCode == serviceCode);

		#endregion

		#region IsServiceRequired

		public JobService GetRequiredService(JobService service)
		{
			return service == null || !new FreightServiceTypes().ContainsCode(service.ES_ServiceCode) ? null : GetService(service);
		}

		public ZBool IsServiceRequired(string serviceCode)
		{
			if (new FreightServiceTypes().ContainsCode(serviceCode))
			{
				foreach (JobService service in this)
				{
					if (service.ES_ServiceCode == serviceCode)
					{
						return true;
					}
				}
			}

			return false;
		}

		#endregion

		#region IsServiceCompleted

		public ZBool IsServiceCompleted(ZString serviceCode)
		{
			if (new FreightServiceTypes().ContainsCode(serviceCode))
			{
				foreach (JobService service in this)
				{
					if (service.ES_ServiceCode == serviceCode)
					{
						if (!service.ES_Completed.IsEmpty)
						{
							return true;
						}
						break;
					}
				}
			}

			return false;
		}

		public ZBool AreAnyServicesIncomplete
		{
			get
			{
				ZBool result = false;

				foreach (JobService service in this)
				{
					if (service.ES_Completed.IsEmpty)
					{
						result = true;
						break;
					}
				}

				return result;
			}
		}

		#endregion

		#region ServiceCompletionDate

		public ZDateTime ServiceCompletionDate(ZString serviceCode)
		{
			if (new FreightServiceTypes().ContainsCode(serviceCode))
			{
				foreach (JobService service in this)
				{
					if (service.ES_ServiceCode == serviceCode)
					{
						return !service.ES_Completed.IsEmpty ? service.ES_Completed : ZDateTime.Empty;
					}
				}
			}

			return ZDateTime.Empty;
		}

		#endregion

		public IEnumerable<JobServiceInfo> GetServiceInfos(string serviceCode)
			=> GetServices(serviceCode).Select(service => new JobServiceInfo(service));

		#region Parent

		protected IHaveServices Parent;

		#endregion
	}
}
