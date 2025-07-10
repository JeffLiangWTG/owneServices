using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Integration
{
	public interface ICommissionHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		List<Tuple<ICommissionableTransaction, IAccCommissionHeader[]>> GetNonReversedCommissionHeaders(BusinessObjectFactory factory, IJobHeader job);
	}
}
