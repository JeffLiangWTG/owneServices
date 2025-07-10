using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.LandedCosting.Business.Testing
{
	public sealed class DummyLandedCostHistoryMaster : DummyBusinessObject, ILandedCostHistoryMaster
	{
		public DummyLandedCostHistoryMaster(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		ZGuid ILandedCostHistoryMaster.PK => PK;

		public SchemaGuidColumn FKSchemaColumnInLandedCostHistoryExposed;
		public SchemaGuidColumn FKSchemaColumnInLandedCostHistory => FKSchemaColumnInLandedCostHistoryExposed;

		public ZBool ShouldMarginPercentagesReadOnlyExposed;
		public ZBool ShouldMarginPercentagesReadOnly => ShouldMarginPercentagesReadOnlyExposed;

		public ZString CountryCodeExposed;
		public ZString CountryCode => CountryCodeExposed.IsEmpty ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : CountryCodeExposed;

		IEnumerable<ICustomsChargeLCItemSetting> ILandedCostHistoryMaster.CustomsChargeLCItemSettings => Array.Empty<ICustomsChargeLCItemSetting>();
	}
}
