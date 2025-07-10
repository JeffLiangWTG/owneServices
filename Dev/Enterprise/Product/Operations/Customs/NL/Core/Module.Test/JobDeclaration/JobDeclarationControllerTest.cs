using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Module.Testing;

[TestedType(typeof(JobDeclarationController))]
sealed class JobDeclarationControllerTest : EU.Module.Testing.JobDeclarationControllerTest
{
	public override Type ControllerToBashType => typeof(JobDeclarationController);

	protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.SupplierDocumentaryAddress.OrganisationPK = GlbCompany.CurrentCompany.OrgProxy.PK;
		return jobDeclaration;
	}
}
