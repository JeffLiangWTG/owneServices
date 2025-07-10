using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CarbonEmissions.Business.Testing;

public class DummyCO2eCalculationSupporter : DummyBusinessObject, IAddressesValidation, ICO2eCalculationSupporter
{
	public DummyCO2eCalculationSupporter(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public virtual ISupportWebAddressValidation[] AddressesToValidate => Array.Empty<ISupportWebAddressValidation>();

	public void RefreshCO2e() { }

	public ZGuid JobCO2eParentID => PK;

	public ZString JobCO2eParentTableCode => Schema.TablePrefix;

	public IJobCO2eCollection JobCO2eCollection
	{
		get
		{
			if (jobCO2eCollection == null)
			{
				jobCO2eCollection = new JobCO2eCollection(this);
			}
			return jobCO2eCollection;
		}
	}
	IJobCO2eCollection jobCO2eCollection;

	public void RecordLog(CO2eEventType type, string extra, decimal previousCO2eValue = 0) { }

	public bool RequireTEU => false;

	public virtual List<string> ValidateInputs() => new();

	public virtual void OnRequested() { }

	public void OnRejected(string reason) { }

	public void OnCalculated(bool succeeded) { }

	public void OnAdditionalSupporterCalculated(ICO2eCalculationSupporter additionalSupporter) { }

	public bool SaveEmissionsLogToNoteOnCalculated => false;

	public virtual AdditionalCalculationSupporter[] AdditionalCalculationSupporters => Array.Empty<AdditionalCalculationSupporter>();
}
