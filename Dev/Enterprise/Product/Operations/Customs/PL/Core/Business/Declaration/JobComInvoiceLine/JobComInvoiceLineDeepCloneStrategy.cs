using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class JobComInvoiceLineDeepCloneStrategy : EU.Business.Declaration.JobComInvoiceLineDeepCloneStrategy
{
	public JobComInvoiceLineDeepCloneStrategy(BaseJobComInvoiceLine invoiceLineToClone, CloneType cloneType, BaseJobComInvoiceHeader clonedInvoice, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
		: base(invoiceLineToClone, cloneType, clonedInvoice, pkPairsDictionaryCollection)
	{
	}

	JobComInvoiceLine PLSourceInvoiceLine => plSourceInvoiceLine ?? (plSourceInvoiceLine = (JobComInvoiceLine)invoiceLineToClone);
	JobComInvoiceLine plSourceInvoiceLine;

	protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
	{
		var result = base.CloneInternal(args);
		var plResult = (JobComInvoiceLine)result;

		if (PLSourceInvoiceLine.IsImport)
		{
			CloneVehicles(plResult, args);
		}
		return plResult;
	}

	void CloneVehicles(JobComInvoiceLine plResult, BusinessObjectCloneArgs args)
	{
		foreach (CusVehicle vehicle in PLSourceInvoiceLine.Vehicles)
		{
			var clonedVehicle = (CusVehicle)vehicle.Clone(args);
			foreach (CusEngine engine in vehicle.Engines)
			{
				clonedVehicle.Engines.Add(engine.Clone(args));
			}
			plResult.Vehicles.Add(clonedVehicle);
		}
	}
}
