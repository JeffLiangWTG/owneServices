using System;
using Enterprise.Customs.US.ISF.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[RunInExtraTransaction]
	sealed class ImporterSecurityFilingNumberGeneratorTargetTest : NumberGeneratorTargetTest
	{
		public void TestParameters()
		{
			var customisation = new ISFNumberCustomisation();
			var element = customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1];
			element.Include = true;
			element.Order = 1;
			element.Detail = "HDN";
			element.Fountain = false;
			ISFRegistry.Instance.ImporterSecurityFilingNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);
			ImporterSecurityFilingNumberGeneratorTarget target = new ImporterSecurityFilingNumberGeneratorTarget();
			target.Context = new NumberGeneratorContext();
			AssertCustomisation("Should find the ImporterSecurityFilingNumberCustomisation", "HDN", target.NumberCustomisation);
			AssertLocation(ISFRegistry.Instance.ImporterSecurityFilingNumberCustomisation, target.NumberCustomisationLocation);
			AssertEquals(CusISFHeaderSchema.BF_JobReference.MaxLength, target.MaxLength);
			AssertEquals("Importer Security Filing number", target.Name);
		}
	}
}
