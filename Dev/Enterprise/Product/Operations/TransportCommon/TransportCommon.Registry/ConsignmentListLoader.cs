using System;
using CargoWise.Integration;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportCommon.Registry
{
	public class ConsignmentListLoader : IConsignmentListLoader
	{
		public ICodeDescriptionPairList GetTransportJobServices()
		{
			var jobServiceList = new CodeDescriptionPairList();
			var jobServicesCodePairs = TransportRegistry.Instance.PortTransportJobServices.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			foreach (SystemDefinableCodeDescriptionBool codeDescriptionBool in jobServicesCodePairs)
			{
				jobServiceList.AddPairIfNotExist(codeDescriptionBool.Code, codeDescriptionBool.Description);
			}

			return jobServiceList;
		}

		public ICodeDescriptionPairList GetTransportBookingJobServices()
		{
			var jobServiceList = new CodeDescriptionPairList();
			var jobServicesCodePairs = TransportRegistry.Instance.LandTransportJobServices.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			foreach (SystemDefinableCodeDescriptionBool codeDescriptionBool in jobServicesCodePairs)
			{
				jobServiceList.AddPairIfNotExist(codeDescriptionBool.Code, codeDescriptionBool.Description);
			}

			return jobServiceList;
		}
	}
}
