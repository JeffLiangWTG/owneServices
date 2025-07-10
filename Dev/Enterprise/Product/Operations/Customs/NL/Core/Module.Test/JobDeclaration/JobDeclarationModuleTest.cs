using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Module.Testing;

[TestedType(typeof(JobDeclarationModule))]
sealed class JobDeclarationModuleTest : EU.Module.Testing.JobDeclarationModuleTest
{
	protected override string CountryCode => Core.Constants.CountryCodes.Netherlands;

	protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);

	protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);

	protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);

	protected override BaseJobDeclaration CreateDeclarationForFetchHintTest(BusinessObjectFactory factory, string messageType, int i)
	{
		var declaration = (JobDeclaration)base.CreateDeclarationForFetchHintTest(factory, messageType, i);
		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.SUP;
		return declaration;
	}

	protected override bool HasFailedFetchHint(TableHitCount tableSelect)
	{
		var tableNameIsExitReport = tableSelect.TableName.Equals(CusExitReportSchema.Constants.TableName);
		return (!tableNameIsExitReport && base.HasFailedFetchHint(tableSelect)) || (tableNameIsExitReport && tableSelect.Value > 7);
	}

	public void TestGetNewFilterBusinessObject()
	{
		using (var module = new JobDeclarationModule())
		{
			AssertType<JobDeclarationFilterBusinessObject>(module.FilterBusinessObject);
		}
	}

	public void TestGetNewGridCollection()
	{
		using (var module = new JobDeclarationModule())
		{
			AssertType<JobDeclarationCollection>(module.GridCollection);
		}
	}

	public void TestGetNewFilterControl()
	{
		using (var module = new JobDeclarationModule())
		using (var filterControl = module.GetNewFilterControlForGrid())
		{
			AssertType<JobDeclarationFilterStripControl>(filterControl);
		}
	}
}
