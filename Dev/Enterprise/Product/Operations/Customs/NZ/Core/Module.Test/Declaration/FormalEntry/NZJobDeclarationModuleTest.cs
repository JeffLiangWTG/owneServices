using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Declaration.FormalEntry.Testing
{
	[TestedType(typeof(JobDeclarationModule))]
	public class NZJobDeclarationModuleTest : Customs.Module.Testing.JobDeclarationModuleAbstractTest
	{
		[ExpectNoExceptions]
		public void TestGetNewFilterControl_DoesNotCastGridCollectionToJobDeclarationCollectionBecauseSomeModulesCanReturnBaseJobDeclarationCollection()
		{
			using (var module = new JobDeclarationModuleForTesting())
			{
				AssertNotNull(module.EmbeddedControl);
			}
		}

		public void TestGetNewFilterControlReturnsInheritedFilterStrip()
		{
			using (var module = new JobDeclarationModule())
			{
				Assert("GetNewFilterControl doesn't return a NZJobDeclarationFilterStripControl", module.EmbeddedControl is NZJobDeclarationFilterStripControl);
			}
		}

		protected override BaseJobDeclaration CreateDeclarationForFetchHintTest(BusinessObjectFactory factory, string messageType, int i)
		{
			var declaration = (JobDeclaration)base.CreateDeclarationForFetchHintTest(factory, messageType, i);
			declaration.JE_DateOfFirstArrival = ZDateTime.Today.AddDays(i);
			declaration.JE_RL_NKPortOfFirstArrival = declaration.JE_RL_NKFinalDestination;
			declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			((IHaveNZAddInfo)declaration).AddInfo.ZN_MAF_ConsignmentNumber = "CN" + i.ToString().PadLeft(2, '0');
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_NZCSStatus = "819";
			entryHeader.CH_MPIFoodStatus = "F04";
			entryHeader.CH_MPIBioStatus = "B04";
			return declaration;
		}

		protected override string CountryCode => Core.Constants.CountryCodes.NewZealand;

		protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);

		protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);

		protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);

		sealed class JobDeclarationModuleForTesting : JobDeclarationModule
		{
			protected override IBusinessObjectCollection GetNewGridCollection() => new BaseJobDeclarationCollection(Factory);
		}
	}
}
