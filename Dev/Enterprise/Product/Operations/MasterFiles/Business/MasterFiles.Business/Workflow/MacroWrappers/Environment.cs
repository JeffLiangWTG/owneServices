using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using IBranch = Enterprise.DocumentVisualizer.DocDataObjects.IBranch;
using ICompany = Enterprise.DocumentVisualizer.DocDataObjects.ICompany;
using IDepartment = Enterprise.DocumentVisualizer.DocDataObjects.IDepartment;
using IUser = Enterprise.DocumentVisualizer.DocDataObjects.IUser;

namespace Enterprise.MasterFiles.Business.Macros
{
	public sealed class Environment : IEnvironment
	{
		public IUser CurrentUser => currentUser ?? (currentUser = new User(GlbStaff.CurrentUser));
		User currentUser;

		public ICompany Company => company ?? (company = new Company(GlbCompany.CurrentCompany));
		Company company;

		public IDepartment Department => department ?? (department = new Department(GlbDepartment.CurrentDepartment));
		Department department;

		public IReadOnlyCollection<ICompany> Companies
		{
			get
			{
				if (companies == null)
				{
					var factory = new BusinessObjectFactory();
					var query = new ZQuery(GlbCompanySchema.GC_IsActive, true);

					companies = factory.Load<GlbCompany>(query).Select(comp => new Company(comp)).ToArray();
				}

				return companies;
			}
		}

		IReadOnlyCollection<ICompany> companies;

		public IBranch Branch => branch ?? (branch = new Branch(GlbBranch.CurrentBranch, GlbCompany.CurrentCompany));
		Branch branch;

		public ZString LocalCurrency => GlbCompany.CurrentCompany?.GC_RX_NKLocalCurrency ?? ZString.Empty;

#if DEBUG
		internal
#endif
		IReadOnlyDictionary<string, IRegistryItem> RegistryItems
		{
			get
			{
				if (registryItems.Value == null)
				{
					registryItems.Value = GetRegistryItems();
				}

				return registryItems.Value;
			}
		}

		static readonly ThreadLocalOverridable<IReadOnlyDictionary<string, IRegistryItem>> registryItems = new ThreadLocalOverridable<IReadOnlyDictionary<string, IRegistryItem>>();

		#region SuppressResourceStringsCheckRegion

		[MacroInvokable]
		public object GetRegistryItem(string nameOrPath)
		{
			if (RegistryItems.TryGetValue(nameOrPath, out var item))
			{
				return item.Value;
			}

			if (nameOrPath.Equals("MERGEFORMANDDOCUMENTSMENUS", StringComparison.OrdinalIgnoreCase) || nameOrPath.Equals("Documents/Merge Forms and Documents Menus", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			return null;
		}

		IReadOnlyDictionary<string, IRegistryItem> GetRegistryItems()
		{
			var portMessagingRegistry = ObjectFactory.Get<IPortMessagingRegistry>();
			var allowToSendExportNotification = portMessagingRegistry.AllowToSendExportNotification;
			var allowToSendExportNotificationToCargonaut = portMessagingRegistry.AllowToSendExportNotificationToCargonaut;

			return new Dictionary<string, IRegistryItem>(StringComparer.OrdinalIgnoreCase)
			{
				["ENABLESUPPLYCHAINSECURITY_EU"] = FreightDataRegistry.Instance.EnableSupplyChainSecurity_EU,
				["Freight/Supply Chain Security/European Union/Enable Supply Chain Security for the European Union"] = FreightDataRegistry.Instance.EnableSupplyChainSecurity_EU,

				["ENABLESUPPLYCHAINSECURITY_HK"] = FreightDataRegistry.Instance.EnableSupplyChainSecurity_HK,
				["Freight/Supply Chain Security/Hong Kong/Enable Supply Chain Security for Hong Kong"] = FreightDataRegistry.Instance.EnableSupplyChainSecurity_HK,

				["ENABLESUPPLYCHAINSECURITY_UK"] = FreightDataRegistry.Instance.EnableSupplyChainSecurity_UK,
				["Freight/Supply Chain Security/United Kingdom/Enable Supply Chain Security for United Kingdom"] = FreightDataRegistry.Instance.EnableSupplyChainSecurity_UK,

				["ENABLESUPPLYCHAINSECURITY_US"] = FreightDataRegistry.Instance.EnableSupplyChainSecurity_US,
				["Freight/Supply Chain Security/United States/Enable Supply Chain Security for United States"] = FreightDataRegistry.Instance.EnableSupplyChainSecurity_US,

				["HOUSEBILLOFLADINGLOGO"] = RawDataRegistry.Instance.HouseBillOfLadingLogo,
				["Freight/House Bills/House Bill Of Lading Logo"] = RawDataRegistry.Instance.HouseBillOfLadingLogo,

				["BILL OF LADINGWEIGHTANDVOLUMEDISPLAY"] = RawDataRegistry.Instance.BillOfLadingWeightAndVolumeDisplay,
				["Documents/Forwarding/Bill of Lading/Weight And Volume Display"] = RawDataRegistry.Instance.BillOfLadingWeightAndVolumeDisplay,

				["INCOTERMDEFINITION"] = RatingDataRegistry.Instance.IncoTermDefinition,
				["AutoRating/Charge Code Groups/Incoterm Charge Code Group Configuration"] = RatingDataRegistry.Instance.IncoTermDefinition,

				["SHOWPACKLINEDETAILSONHOUSEBILLS"] = FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills,
				["Freight/House Bills/Show Pack Line Details On House Bills"] = FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills,

				["SHIPPERLOADANDCOUNT"] = FreightDataRegistry.Instance.ShipperLoadAndCount,
				["Documents/Forwarding/Shipment/Bill of Lading/Shipper Load and Count"] = FreightDataRegistry.Instance.ShipperLoadAndCount,

				["ENABLEHOUSEBILLOFLADINGREGISTRYITEMS"] = FreightDataRegistry.Instance.EnableHouseBillOfLadingRegistryItems,
				["Freight/House Bills/House Bill of Lading Types/Enable House Bill of Lading Registry Items"] = FreightDataRegistry.Instance.EnableHouseBillOfLadingRegistryItems,

				["HOUSEBILLOFLADINGLOGOTYPES"] = FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes,
				["Freight/House Bills/House Bill of Lading Types/House Bill of Lading Settings"] = FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes,

				["HOUSEBILLOFLADINGLOGOIMAGES"] = FreightDataRegistry.Instance.HouseBillOfLadingLogoImages,
				["Freight/House Bills/House Bill of Lading Types/Logos"] = FreightDataRegistry.Instance.HouseBillOfLadingLogoImages,

				["HOUSEBILLOFLADINGTERMSANDCONDITIONSIMAGES"] = FreightDataRegistry.Instance.HouseBillOfLadingTermsAndConditionsImages,
				["Freight/House Bills/House Bill of Lading Types/Terms & Conditions"] = FreightDataRegistry.Instance.HouseBillOfLadingTermsAndConditionsImages,

				["BOLLUMPSUMDISPLAYCOUNTRIES"] = DocumentsDataRegistry.Instance.BOLLumpSumDisplayCountries,
				["Documents/Forwarding/Shipment/Bill of Lading/Print Charges as Lump Sum"] = DocumentsDataRegistry.Instance.BOLLumpSumDisplayCountries,

				["ADDITIONALHBLTYPES"] = FreightDataRegistry.Instance.AddtionalHouseBillOfLadingTypes,
				["Freight/House Bills/House Bill of Lading Types/Additional Types"] = FreightDataRegistry.Instance.AddtionalHouseBillOfLadingTypes,

				["BOLCLAUSE"] = FreightDataRegistry.Instance.BOLClause,
				["Freight/House Bills/Clauses/Standard"] = FreightDataRegistry.Instance.BOLClause,

				["ENABLEGOODSVALUEFORSHIPPINGINSTRUCTION"] = FreightDataRegistry.Instance.EnableGoodsValueForShippingInstruction,
				["Freight/Consolidations/Ocean Carrier Messaging/Shipping Instruction Goods Value"] = FreightDataRegistry.Instance.EnableGoodsValueForShippingInstruction,

				["PRINTSIGNATUREFORHBLDOCUMENTS"] = FreightDataRegistry.Instance.PrintSignatureForHBLDocuments,
				["Freight/House Bills/Print Signature"] = FreightDataRegistry.Instance.PrintSignatureForHBLDocuments,

				["ENABLEMEXICANPORTINTEGRATIONFEATURES"] = FreightDataRegistry.Instance.EnableMexicanPortIntegrationFeatures,
				["Freight/Enables various Mexican port integration features"] = FreightDataRegistry.Instance.EnableMexicanPortIntegrationFeatures,

				["ENABLESPANISHPORTINTEGRATIONFEATURES"] = FreightDataRegistry.Instance.EnableSpanishPortIntegrationFeatures,
				["Freight/Enables various Spanish sea port integration features"] = FreightDataRegistry.Instance.EnableSpanishPortIntegrationFeatures,

				["ENABLEBOOKINGCONFIRMATION"] = FreightDataRegistry.Instance.EnableBookingConfirmation,
				["Freight/Consolidations/Ocean Carrier Messaging/Enable Booking Confirmation form"] = FreightDataRegistry.Instance.EnableBookingConfirmation,

				["ENABLEDRAFTBILLOFLADINGFORM"] = FreightDataRegistry.Instance.EnableDraftBillOfLadingForm,
				["Freight/Consolidations/Ocean Carrier Messaging/Enable Draft Bill Of Lading form"] = FreightDataRegistry.Instance.EnableDraftBillOfLadingForm,

				["ENABLEFIATAHOUSEBILLSFEATURES"] = FreightDataRegistry.Instance.EnableFIATAHouseBillsFeatures,
				["Freight/House Bills/House Bill of Lading Types/FIATA HBL Settings/Enable electronic FIATA Bills of Lading (eFBL)"] = FreightDataRegistry.Instance.EnableFIATAHouseBillsFeatures,

				["ENABLEINTERNATIONALTRADEDOCUMENTSFUNCTIONALITY"] = FreightDataRegistry.Instance.EnableInternationalTradeDocumentsFunctionality,
				["Freight/Enables Certificate of Origin functionaltiy for International Trade Documents"] = FreightDataRegistry.Instance.EnableInternationalTradeDocumentsFunctionality,

				["ENABLENZCFTASUBMISSIONTOCAB"] = DocumentsDataRegistry.Instance.EnableNZCFTASubmissionToCAB,
				["Documents/Digital Docs/Certification/NZCFTA"] = DocumentsDataRegistry.Instance.EnableNZCFTASubmissionToCAB,

				["ENABLEAANZFTASUBMISSIONTOCAB"] = DocumentsDataRegistry.Instance.EnableAANZFTASubmissionToCAB,
				["Documents/Digital Docs/Certification/AANZFTA"] = DocumentsDataRegistry.Instance.EnableAANZFTASubmissionToCAB,

				["ENABLECPTPPSUBMISSIONTOCAB"] = DocumentsDataRegistry.Instance.EnableCPTPPSubmissionToCAB,
				["Documents/Digital Docs/Certification/CPTPP"] = DocumentsDataRegistry.Instance.EnableCPTPPSubmissionToCAB,

				["ENABLERCEPSUBMISSIONTOCAB"] = DocumentsDataRegistry.Instance.EnableRCEPSubmissionToCAB,
				["Documents/Digital Docs/Certification/RCEP"] = DocumentsDataRegistry.Instance.EnableRCEPSubmissionToCAB,

				["ALLOWTOSENDEXPORTNOTIFICATION"] = allowToSendExportNotification,
				["Freight/Port Messaging/France/Allow to send Export Notification (755) to Cargo Information Network"] = allowToSendExportNotification,

				["ENABLECOONZSUBMISSIONTOCAB"] = DocumentsDataRegistry.Instance.EnableCOONZSubmissionToCAB,
				["Documents/Digital Docs/Certification/Cert of Origin NZ"] = DocumentsDataRegistry.Instance.EnableCOONZSubmissionToCAB,

				["ENABLECHAFTASUBMISSIONTOCAB"] = DocumentsDataRegistry.Instance.EnableChAFTASubmissionToCAB,
				["Documents/Digital Docs/Certification/ChAFTA"] = DocumentsDataRegistry.Instance.EnableChAFTASubmissionToCAB,

				["ENABLESENDINGCIN750MESSAGE"] = WarehouseDataRegistry.Instance.EnableSendingCIN750Message,
				["Warehouse/Transit Warehouse/Certification/Enable Sending CIN 750 Message"] = WarehouseDataRegistry.Instance.EnableSendingCIN750Message,

				["ENABLEJAEPASUBMISSIONTOCAB"] = DocumentsDataRegistry.Instance.EnableJAEPASubmissionToCAB,
				["Documents/Digital Docs/Certification/JAEPA"] = DocumentsDataRegistry.Instance.EnableJAEPASubmissionToCAB,

				["ENABLEPAFTASUBMISSIONTOCAB"] = DocumentsDataRegistry.Instance.EnablePAFTASubmissionToCAB,
				["Documents/Digital Docs/Certification/PAFTA"] = DocumentsDataRegistry.Instance.EnablePAFTASubmissionToCAB,

				["ALLOWTOSENDEXPORTNOTIFICATIONTOCARGONAUT"] = allowToSendExportNotificationToCargonaut,
				["Freight/Port Messaging/Netherlands/Allow to send Export Notification (755) to Cargonaut"] = allowToSendExportNotificationToCargonaut,

				["ENABLEAUKFTASUBMISSIONTOCAB"] = DocumentsDataRegistry.Instance.EnableAUKFTASubmissionToCAB,
				["Documents/Digital Docs/Certification/A-UKFTA"] = DocumentsDataRegistry.Instance.EnableAUKFTASubmissionToCAB,

				["ENABLETAFTASUBMISSIONTOCAB"] = DocumentsDataRegistry.Instance.EnableTAFTASubmissionToCAB,
				["Documents/Digital Docs/Certification/TAFTA"] = DocumentsDataRegistry.Instance.EnableTAFTASubmissionToCAB,

				["ENABLEIAECTASUBMISSIONTOCAB"] = DocumentsDataRegistry.Instance.EnableIAECTASubmissionToCAB,
				["Documents/Digital Docs/Certification/IA-ECTA"] = DocumentsDataRegistry.Instance.EnableIAECTASubmissionToCAB,

				["ENABLEKAFTASUBMISSIONTOCAB"] = DocumentsDataRegistry.Instance.EnableKAFTASubmissionToCAB,
				["Documents/Digital Docs/Certification/KAFTA"] = DocumentsDataRegistry.Instance.EnableKAFTASubmissionToCAB,

				["ENABLEIACEPASUBMISSIONTOCAB"] = DocumentsDataRegistry.Instance.EnableIACEPASubmissionToCAB,
				["Documents/Digital Docs/Certification/IA-CEPA"] = DocumentsDataRegistry.Instance.EnableIACEPASubmissionToCAB,

				["ENABLECERTOFORIGINAUSUBMISSIONTOCAB"] = DocumentsDataRegistry.Instance.EnableCertOfOriginAUSubmissionToCAB,
				["Documents/Digital Docs/Certification/Cert of Origin AU"] = DocumentsDataRegistry.Instance.EnableCertOfOriginAUSubmissionToCAB,

				["ENABLECOOUSSUBMISSIONTOCAB"] = DocumentsDataRegistry.Instance.EnableCOOUSSubmissionToCAB,
				["Documents/Digital Docs/Certification/COOUS"] = DocumentsDataRegistry.Instance.EnableCOOUSSubmissionToCAB,
			};
		}

		#endregion

	}
}
