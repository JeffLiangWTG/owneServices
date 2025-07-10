using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Packing.Business
{
	public interface ISupportPackageIDGeneration
	{
		ZString KP_PackageID { get; set; }
		bool IsContainer { get; }
		bool IsBookedViaCarrier { get; }
		IEnumerable<ISupportPackageIDGeneration> Packages { get; }
		BusinessObjectFactory Factory { get; }
		PkgPackageJob PackageJob { get; }
		ZGuid PackageJobPK { get; }
		ZInt KP_PackageQty { get; }

		bool ShouldGenerateIDOnSaving { get; set; }

		event EventHandler AfterIDGenerated;
	}
}
