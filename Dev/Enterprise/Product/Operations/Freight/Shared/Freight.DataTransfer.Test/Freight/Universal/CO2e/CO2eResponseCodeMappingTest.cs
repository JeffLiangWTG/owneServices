using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	public abstract class CO2eResponseCodeMappingTest : TestCaseWithFactoryAndMessagingHelpers
	{
		protected void CreateMappings(OrgHeader orgHeader, (string Relationship, string ForeignCode, ZGuid LocalGuid)[] mappings)
		{
			var patternMatchOverrides = new OrgPatternMatchOverrideCollection(orgHeader, Factory.BOFactory);
			foreach (var mapping in mappings)
			{
				var unlocoPattern = patternMatchOverrides.AddNew();
				unlocoPattern.OO_Relationship = mapping.Relationship;
				unlocoPattern.OO_ForeignCode = mapping.ForeignCode;
				unlocoPattern.OO_LocalGuid = mapping.LocalGuid;
			}
		}

		protected string CreateAndProcessUniversalShipment(UniversalShipment shipment)
		{
			var message = GetQueuedUniversalShipmentMessage(shipment);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			return string.Join("\r\n", manager.Logger.Logs.Select(log => log.Type + " - " + log.Message));
		}

		protected IDisposable SetupCurrentBranchWithOrgProxy(OrgHeader orgProxy)
		{
			var currentCompany = Factory.BOFactory.NewWithValidTestData<GlbCompany>();
			currentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			currentCompany.GC_OH_OrgProxy = orgProxy.PK;
			var currentBranch = Factory.BOFactory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_GC = currentCompany.PK;
			Factory.SaveForTesting();

			return DisposableEnvironment.ForBranch(currentBranch.PK.ToGuid());
		}

		protected void ProcessUniversalShipmentAndAssert(UniversalShipment dataObject, string boTypeName)
		{
			var logs = CreateAndProcessUniversalShipment(dataObject);
			AssertNotContains($"Warning - Cannot populate CO2e for {boTypeName} because CO2e Calculation input parameters have been changed", logs);
		}
	}
}
