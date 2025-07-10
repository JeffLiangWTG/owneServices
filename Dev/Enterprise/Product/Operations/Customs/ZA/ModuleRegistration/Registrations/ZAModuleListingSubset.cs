using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.Core.Environment;
using Enterprise.Core.Modules;
using Enterprise.Customs.Common.ModuleRegistration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using CountryCodes = Enterprise.Core.Constants.CountryCodes;
using static Enterprise.ZArchitecture.Modules.ModuleTreeLoaderHelpers;

namespace Enterprise.Customs.ZA.ModuleRegistration;

public class ZAModuleListingSubset : IModuleListingSubset
{
	public IEnumerable<ModuleIdentifier> ModuleIdentifiers =>
	[
		ZAModuleIDs.CustomsResponse,
		ZAModuleIDs.CustomsStatement,
		ZAModuleIDs.GenralMessage,
		ZAModuleIDs.OutturnAndGateInOut,
		ZAModuleIDs.WarehouseOperatorTransactions,
		ZAModuleIDs.ZA404ProofOfPayment,
	];

	public IEnumerable<ModuleInfo> ModuleInfos =>
	[
		CreateModuleInfo(ModuleIDs.RefVessel, "RefVesselModule"),
		CreateModuleInfo(CustomsModuleIDs.JobDeclaration, "JobDeclarationModule"),
		CreateModuleInfo(ModuleIDs.SingleTariffClassification, "CusClassificationModule"),
		CreateModuleInfo(ModuleIDs.SupplierPart, "OrgSupplierPartModule"),
		CreateModuleInfo(ZAModuleIDs.CustomsStatement, "CusStatementModule"),
		CreateModuleInfo(ZAModuleIDs.ZA404ProofOfPayment, "ProofOfPaymentModule"),
		CreateModuleInfo(ZAModuleIDs.CustomsResponse, "CustomsResponseModule"),
		CreateModuleInfo(CustomsModuleIDs.EntryHeader, "EntryHeaderModule"),
		CreateModuleInfo(ZAModuleIDs.OutturnAndGateInOut, "OutturnAndGateInOutModule"),
		CreateModuleInfo(CustomsModuleIDs.Universal.RefCusTariff, "RefCusTariffModule"),
		CreateModuleInfo(ZAModuleIDs.GenralMessage, "GenralMessageModule"),
		CreateModuleInfo(ZAModuleIDs.WarehouseOperatorTransactions, "WarehouseOperatorTransactionsModule"),
	];

	public IEnumerable<ControllerInfo> ControllerInfos =>
	[
		CreateControllerInfo(CustomsControllerIDs.JobDeclaration, "JobDeclarationController"),
		CreateControllerInfo(CustomsControllerIDs.JobDeclarationPluggedIntoShipment, "JobDeclarationShipmentController"),
		CreateControllerInfo(CustomsControllerIDs.SingleTariffClassification, "CusClassificationController"),
		CreateControllerInfo(CustomsControllerIDs.SupplierPart, "OrgSupplierPartController"),
		CreateControllerInfo(ControllerIDs.CommercialInvoice, "CommercialInvoiceController"),
		CreateControllerInfo(CustomsControllerIDs.CustomsStatement, "CusStatementController"),
		CreateControllerInfo(ZAControllerIDs.ZA404ProofOfPayment, "ProofOfPaymentController"),
		CreateControllerInfo(ZAControllerIDs.CustomsResponse, "CustomsResponseController"),
		CreateControllerInfo(ZAControllerIDs.EntryHeader, "EntryHeaderController"),
		CreateControllerInfo(ZAControllerIDs.OrganisationDetailsPlugIn, "ZAOrganisationDetailsController"),
		CreateControllerInfo(ZAControllerIDs.OutturnAndGateInOut, "OutturnAndGateInOutController"),
		CreateControllerInfo(ZAControllerIDs.CALINFMessagingPlugin, "CALINFMessagingController"),
		CreateControllerInfo(ZAControllerIDs.RefCusTariff, "RefCusTariffController"),
		CreateControllerInfo(ZAControllerIDs.GenralMessage, "GenralMessageController"),
		CreateControllerInfo(ZAControllerIDs.WarehouseOperatorTransactions, "WarehouseOperatorTransactionsController"),
	];

	public void InitializeSecurityCheckpoints(IZSecurity security)
	{
		var customsMain = security.FindCheckPoint(CustomsSecurityCheckpoints.CustomsMain);
		security.AddCheckPoint(ZASecurityCheckpoints.ZACustomsStatement, new CustomsSecurityCheckPoint(ZASecurityCheckpoints.ZACustomsStatement.Code, ZAModuleIDs.CustomsStatement.Description, customsMain, security, CountryCodes.SouthAfrica));
		var za404ProofOfPayment = new SecurityCheckpoint(ZASecurityCheckpoints.ZA404ProofOfPayment.Code, ZAModuleIDs.ZA404ProofOfPayment.Description, customsMain, security);
		security.AddCheckPoint(ZASecurityCheckpoints.ZA404ProofOfPayment, za404ProofOfPayment);
		security.AddCheckPoint(ZASecurityCheckpoints.ZA404ProofOfPaymentCustomiseDocuments, new SecurityCheckpoint(ZASecurityCheckpoints.ZA404ProofOfPaymentCustomiseDocuments.Code, SecurityCore.Captions.CustomizeDocuments, za404ProofOfPayment, security));
		security.AddCheckPoint(ZASecurityCheckpoints.ZACustomsResponse, new SecurityCheckpoint(ZASecurityCheckpoints.ZACustomsResponse.Code, ZAModuleIDs.CustomsResponse.Description, customsMain, security, CountryCodes.SouthAfrica));
		security.AddCheckPoint(ZASecurityCheckpoints.ZAOutturnAndGateInOut, new SecurityCheckpoint(ZASecurityCheckpoints.ZAOutturnAndGateInOut.Code, ZAModuleIDs.OutturnAndGateInOut.Description, customsMain, security, CountryCodes.SouthAfrica));
		security.AddCheckPoint(ZASecurityCheckpoints.ZAGenralMessage, new SecurityCheckpoint(ZASecurityCheckpoints.ZAGenralMessage.Code, ZAModuleIDs.GenralMessage.Description, customsMain, security, CountryCodes.SouthAfrica));
		security.AddCheckPoint(ZASecurityCheckpoints.ZAWarehouseOperatorTransactions, new SecurityCheckpoint(ZASecurityCheckpoints.ZAWarehouseOperatorTransactions.Code, ZAModuleIDs.WarehouseOperatorTransactions.Description, customsMain, security, CountryCodes.SouthAfrica));
	}

	public void InitializeModuleTree(ModuleTreeCategories moduleTreeCategories, IZSecurity security)
	{
		var customsMainSection = moduleTreeCategories.Operate.Sections[ModuleTreeLoaderConstant.Section.CustomsMain.Name];
		if (customsMainSection is not null)
		{
			AddCustomsMainSection(customsMainSection);
		}
	}

	static ModuleInfo CreateModuleInfo(ModuleIdentifier moduleId, string moduleName)
		=> new(moduleId, AssemblyName, $"{RootNamespace}.{moduleName}", CountryCodes.SouthAfrica);

	static ControllerInfo CreateControllerInfo(ControllerID controllerId, string controllerName)
		=> new(controllerId, AssemblyName, $"{RootNamespace}.{controllerName}", CountryCodes.SouthAfrica);

	static void AddCustomsMainSection(ModuleSection section)
	{
		var customsReportModule = ModuleIDs.CustomsReport;
		InsertModuleBefore(customsReportModule, section, ZAModuleIDs.CustomsStatement);

		var currentCountryIsZa = CurrentCountry == CountryCodes.SouthAfrica;
		InsertModulesBeforeIf(customsReportModule, currentCountryIsZa, section,
			ZAModuleIDs.CustomsResponse,
			ZAModuleIDs.GenralMessage,
			ZAModuleIDs.OutturnAndGateInOut);
		InsertModulesBeforeIf(customsReportModule, currentCountryIsZa && ObjectFactory.Get<Integration.Customs.ZA.IZACustomsRegistry>().IsWarehouseOperatorTransactionsModuleEnabled, section, ZAModuleIDs.WarehouseOperatorTransactions);
		InsertModuleAfterIf(customsReportModule, currentCountryIsZa, section, ZAModuleIDs.ZA404ProofOfPayment);
	}

	const string AssemblyName = "Enterprise.Customs.ZA.Module";
	const string RootNamespace = AssemblyName;
}
