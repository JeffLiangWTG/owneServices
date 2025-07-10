using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class JobServicesCollection : Collection<JobServiceInfo>
	{
		public bool Contains(ZString chargeCodeGroup, ZString serviceCode)
		{
			return this.Any(serviceInfo => serviceInfo.IsServiceFor(chargeCodeGroup, serviceCode));
		}

		public bool IsEnabledOrServiceInactive(AccChargeCode chargeCode)
		{
			if (chargeCode != null)
			{
				var services = FindServices(chargeCode);
				if (services.Any())
				{
					return services.Any(x => x.IsEnabled);
				}

				return this.All(s => s.ServiceCode != chargeCode.AC_ChargeSubGroup);
			}

			return true;
		}

		public bool IsEnabled(ZString chargeCodeGroup, ZString chargeCodeSubGroup)
		{
			var services = FindServices(chargeCodeGroup, chargeCodeSubGroup);
			return services.Any(x => x.IsEnabled);
		}

		public IEnumerable<JobServiceInfo> FindServices(AccChargeCode chargeCode)
		{
			return FindServices(chargeCode.AC_ChargeGroup, chargeCode.AC_ChargeSubGroup);
		}

		public TimeInfo Time(AccChargeCode chargeCode)
		{
			var services = chargeCode != null ? FindServices(chargeCode) : null;

			if (!services.Any())
			{
				return null;
			}

			var servicesWithTime = services.Where(service => service.IsEnabled && service.ServiceDuration != new TimeSpan(0)).ToList();

			var totalTime = TimeInfo.Empty;
			foreach (var serviceInfo in servicesWithTime)
			{
				totalTime += serviceInfo.ServiceDuration;
			}

			return totalTime;
		}

		public void AddRange(IEnumerable<JobServiceInfo> collectionToAdd)
		{
			foreach (var serviceInfo in collectionToAdd)
			{
				Add(serviceInfo);
			}
		}

		#region Contractors

		public List<OrgHeader> GetContractors(AccChargeCode chargeCode)
		{
			return chargeCode != null ? GetContractorsCore(chargeCode).Select(x => x.Org).ToList() : new List<OrgHeader>();
		}

		public List<OrgHeader> GetContractors()
		{
			return GetContractorsWithSource().Select(x => x.Org).ToList();
		}

		public List<OrgWithSource> GetContractorsWithSource()
		{
			return GetContractorsCore(null);
		}

		List<OrgWithSource> GetContractorsCore(AccChargeCode chargeCode)
		{
			return this
				.Where(serviceInfo => serviceInfo.IsEnabled && (chargeCode == null || IsContractorFor(serviceInfo.Contractor, chargeCode)) && serviceInfo.Contractor != null)
				.Select(x => OrgWithSource.New(x.Contractor, new List<string> { Res.GetString("846DA266-CFAA-4D84-B7B9-9184A2027692", "{0} service contractor", x.ServiceCode) }))
				.Distinct(OrgWithSource.Comparer.OrgPK)
				.ToList();
		}

		public bool IsContractorFor(OrgHeader contractor, AccChargeCode chargeCode)
		{
			if (contractor != null && chargeCode != null)
			{
				foreach (JobServiceInfo serviceInfo in this)
				{
					if (serviceInfo.IsServiceFor(chargeCode.AC_ChargeGroup, chargeCode.AC_ChargeSubGroup))
					{
						if (serviceInfo.IsEnabled && serviceInfo.Contractor != null && contractor.PK == serviceInfo.Contractor.PK)
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		#endregion

		public IEnumerable<JobServiceInfo> FindServices(ZString chargeCodeGroup, ZString chargeCodeSubGroup)
		{
			var foundServices = new List<JobServiceInfo>();
			foreach (JobServiceInfo service in this)
			{
				if (service.IsServiceFor(chargeCodeGroup, chargeCodeSubGroup))
				{
					foundServices.Add(service);
				}
			}

			if (foundServices.Any())
			{
				return foundServices;
			}

			string generalizedChargeCodeGroup = GeneralizeChargeCodeGroup(chargeCodeGroup);
			if (!string.IsNullOrWhiteSpace(generalizedChargeCodeGroup))
			{
				var generalizedServices = FindServices(generalizedChargeCodeGroup, chargeCodeSubGroup);
				foreach (var service in generalizedServices)
				{
					foundServices.Add(service);
				}
			}

			return foundServices;
		}

		static string GeneralizeChargeCodeGroup(string chargeCodeGroup)
		{
			if (chargeCodeGroup == ChargeCodeGroupList.Codes.Origin || chargeCodeGroup == ChargeCodeGroupList.Codes.Destination)
			{
				return string.Empty;
			}

			var prepaidCollect = PaymentTermInfos.GetPrepaidCollect(chargeCodeGroup);
			switch (prepaidCollect)
			{
				case Core.Constants.PaymentType.Collect:
					return ChargeCodeGroupList.Codes.Destination;
				case Core.Constants.PaymentType.Prepaid:
					return ChargeCodeGroupList.Codes.Origin;
				default:
					return default;
			}
		}
	}
}
