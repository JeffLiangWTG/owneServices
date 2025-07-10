using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.GVMSReferenceData.Models
{
	public interface IReferenceDataModel
	{
		bool IsValid(StringBuilder errorCollector);
	}
}
