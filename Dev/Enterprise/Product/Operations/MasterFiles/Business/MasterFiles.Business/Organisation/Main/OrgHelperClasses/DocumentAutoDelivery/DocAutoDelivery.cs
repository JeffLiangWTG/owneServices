using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	public class DocAutoDelivery
	{
		OrgContact GenerateSystemDefaultContact(OrgHeader organisation)
		{
			var finder = new DefaultContactFinder(organisation, fMenuItem);
			var result = finder.SystemDefaultContact(fContactType);
			result.OC_ContactName = result.SystemDefaultContactName.ToString(string.IsNullOrEmpty(deliveryLanguage) ? Res.DefaultLanguage : deliveryLanguage.ToString());
			result.IsSystemDefaultContactForAutoDelivery = true;

			if (result.OC_NotifyMode == Constants.ContactNotifyModes.Email)
			{
				result.OC_AttachmentType = OrgConstants.AttachmentType.PDF;
			}

			result.IsSystemDefaultContact = true;

			return result;
		}

		public void SetDeliveryLanguage(ZString language)
		{
			this.deliveryLanguage = language;
		}

		#region Auto Delivery

		class ContactDocumentRelation
		{
			public ContactDocumentRelation(OrgContact contact, OrgDocument documentDeliveryType)
			{
				this.Contact = contact;
				this.DocumentDeliveryType = documentDeliveryType;
			}

			public readonly OrgContact Contact;
			public readonly OrgDocument DocumentDeliveryType;
		}

		public virtual DocDeliveryContactCollection GetDeliveryContactsForDocPack(IStmMenuItem menuItem, DocumentSupporter documentSupporter, ZString documentGroup, IStmMenuItem parentMenuCommand = null)
		{
			InitialiseDeliveryDetails(menuItem, documentSupporter, documentGroup, parentMenuCommand);

			return GetDeliveryContactsCommon(menuItem, documentSupporter);
		}

		public virtual DocDeliveryContactCollection GetDeliveryContacts(IStmMenuItem menuItem, DocumentSupporter documentSupporter)
		{
			InitialiseDeliveryDetails(menuItem, documentSupporter, null, null);

			return GetDeliveryContactsCommon(menuItem, documentSupporter);
		}

		DocDeliveryContactCollection GetDeliveryContactsCommon(IStmMenuItem menuItem, DocumentSupporter documentSupporter)
		{
			JobDocAddress overridenDeliveryDetailsAsJobDocAddress = fOverriddenDeliveryDetails as JobDocAddress;
			if (overridenDeliveryDetailsAsJobDocAddress != null)
			{
				DocDeliveryContactCollection result = new DocDeliveryContactCollection(menuItem, fOverriddenDeliveryDetails, Factory, this);
				result.Add(overridenDeliveryDetailsAsJobDocAddress.GetDocumentDeliveryContact(menuItem));
				return result;
			}

			List<ContactDocumentRelation> contactAndDocumentDeliveryTypes = GetContactDocumentRelations(menuItem, documentSupporter)
				.Where(x => x.Contact.OC_NotifyMode != Constants.ContactNotifyModes.DoNotDeliver)
				.ToList();

			DocDeliveryContactCollection contacts = null;
			DocDeliveryContactCollection jobSpecificContacts = new DocDeliveryContactCollection(Factory);

			if (menuItem != null && documentSupporter?.BusinessObject != null && shouldGetJobSpecificRecipients)
			{
				jobSpecificContacts.PopulateAutoDeliveryContactsForJob(menuItem, documentSupporter.BusinessObject);
			}

			var isSystemDefaultContactGenerated = contactAndDocumentDeliveryTypes.Count == 1 && contactAndDocumentDeliveryTypes.Exists(r => r.Contact.IsSystemDefaultContactForAutoDelivery);

			if (isSystemDefaultContactGenerated && jobSpecificContacts.Any())
			{
				contacts = jobSpecificContacts;
			}
			else
			{
				contacts = GetDeliveryDetailsForContactList(contactAndDocumentDeliveryTypes, fOverriddenDeliveryDetails);
				if (jobSpecificContacts.Any())
				{
					contacts.AddRange(jobSpecificContacts);
				}
			}

			if (fOverriddenDeliveryDetails != null)
			{
				foreach (DocDeliveryContact con in contacts)
				{
					con.Address1 = fOverriddenDeliveryDetails.E2_Address1;
					con.Address2 = fOverriddenDeliveryDetails.E2_Address2;
					con.City = fOverriddenDeliveryDetails.E2_City;
					con.PostCode = fOverriddenDeliveryDetails.E2_Postcode;
					con.State = fOverriddenDeliveryDetails.E2_State;
					con.CompanyName = fOverriddenDeliveryDetails.E2_CompanyName;
				}
			}

			return contacts;
		}

		List<ContactDocumentRelation> GetContactDocumentRelations(IStmMenuItem menuItem, DocumentSupporter documentSupporter)
		{
			if ((menuItem != null && menuItem.SU_PreventAutoDelivery) || fOrganisation == null)
			{
				var overriddenContact = documentSupporter?.GetOverriddenDeliveryContact(menuItem);
				var defaultContact = overriddenContact as OrgContact ?? new DefaultContactFinder(fOrganisation, menuItem).DefaultContact(fContactType);
				return new List<ContactDocumentRelation> { new ContactDocumentRelation(defaultContact, null) };
			}

			return GetAutoDeliveryContacts(documentSupporter);
		}

		public virtual DocDeliveryContact GetDeliveryDetailsForContact(OrgContact contact)
		{
			return GetDeliveryDetailsForContact(contact, null);
		}

		public virtual DocDeliveryContact GetDeliveryDetailsForContact(OrgContact contact, IStmMenuItem menuItem)
		{
			using (contact.GenerateDocDeliveryRecipient())
			{
				return contact.DocDeliveryDetails(null, menuItem);
			}
		}

		public virtual DocDeliveryContact GetDeliveryDetailsForSystemDefaultContact(OrgHeader orgHeader, IStmMenuItem menuItem)
		{
			fContactType = fMenuItem != null ? ContactType.Find(fMenuItem.SU_ContactType) : ContactType.NoContactType;

			var orgContact = GenerateSystemDefaultContact(orgHeader);

			return GetDeliveryDetailsForContact(orgContact, menuItem);
		}

		protected void InitialiseDeliveryDetails(IStmMenuItem menuItem, DocumentSupporter documentSupporter, ZString documentGroup, IStmMenuItem parentMenuCommand)
		{
			Factory = new BusinessObjectFactory() { NameForDebugging = "DocAutoDelivery_InitialiseDeliveryDetails" };
			fMenuItem = menuItem;
			ParentMenuCommand = parentMenuCommand;
			DocumentSupporter = documentSupporter;
			shouldGetJobSpecificRecipients = documentSupporter?.BusinessObject is ISupportJobDocumentRecipient;

			if (documentGroup.IsEmpty)
			{
				fContactType = fMenuItem != null ? ContactType.Find(fMenuItem.SU_ContactType) : ContactType.NoContactType;
			}
			else
			{
				fContactType = ContactType.Find(documentGroup);
			}
			if (fContactType == null && fMenuItem != null)
			{
				fContactType = ContactType.NoContactType;
				ErrorReporter.ReportOnce("NoContactTypeFoundForSpecifiedCode", string.Format("No Contact Type is found for code '{0}'.", fMenuItem.SU_ContactType));
			}

			ZString menuName = (fMenuItem != null) ? fMenuItem.SU_MenuName : ZString.Empty;

			if (documentSupporter != null)
			{
				var docDirection = fMenuItem == null ? DocumentDirection.ANY : fMenuItem.GetDocumentDirection();

				OrgHeaderContact headerContact = (OrgHeaderContact)documentSupporter.GetContactOrganisation(menuName, fContactType, docDirection);
				if (headerContact != null)
				{
					fOrganisation = headerContact.OrgHeader;
					fRelatedParty = headerContact.RelatedOrgHeader;
				}

				fOverriddenDeliveryDetails = documentSupporter.GetOverriddenDeliveryDetails(menuName, fContactType, docDirection);
				fLocalPort = documentSupporter.LocalPort(fContactType, docDirection);
				fForeignPort = documentSupporter.ForeignPort(fContactType, docDirection);
				fRelatedBranch = documentSupporter.RelatedBranch;
				fRelatedCompany = documentSupporter.RelatedCompany;
				fRelatedDepartment = documentSupporter.RelatedDepartment;

				fTransportMode = documentSupporter.TransportMode;
				fContainerMode = documentSupporter.ContainerMode;

				fIsImport = documentSupporter.IsImport;

				if (documentSupporter.BusinessObject is IImportExport importExport)
				{
					direction = importExport.JobDirection;
				}
				else
				{
					direction = fIsImport ? Directions.Import : Directions.Export;
				}
			}

			fContactType = GetPreciseContactTypeByTransportMode(fContactType, fTransportMode);
		}

		public static ContactType GetPreciseContactTypeByTransportMode(ContactType parentContactType, string transportMode)
		{
			var isAir = transportMode == Constants.TransportModes.Air;
			var isSea = transportMode == Constants.TransportModes.Sea;
			ContactType result = null;

			if (parentContactType == ContactType.ImportFreightAgent)
			{
				result = isAir ? ContactType.ImportAirFreightAgent : isSea ? ContactType.ImportSeaFreightAgent : parentContactType;
			}
			else if (parentContactType == ContactType.ExportFreightAgent)
			{
				result = isAir ? ContactType.ExportAirFreightAgent : isSea ? ContactType.ExportSeaFreightAgent : parentContactType;
			}
			else if (parentContactType == ContactType.ImportDepot)
			{
				result = isAir ? ContactType.ImportAirDepot : isSea ? ContactType.ImportSeaDepot : parentContactType;
			}
			else if (parentContactType == ContactType.ExportDepot)
			{
				result = isAir ? ContactType.ExportAirDepot : isSea ? ContactType.ExportSeaDepot : parentContactType;
			}

			return result ?? parentContactType;
		}

		List<ContactDocumentRelation> GetAutoDeliveryContacts(DocumentSupporter documentSupporter)
		{
			if (fContactType == ContactType.Consignee)
			{
				return GetAutoDeliveryContactsForConsignee(documentSupporter);
			}
			else
			{
				return GetAutoDeliveryContacts(fOrganisation, documentSupporter);
			}
		}

		List<ContactDocumentRelation> GetAutoDeliveryContacts(OrgHeader organisation, DocumentSupporter documentSupporter)
		{
			List<ContactDocumentRelation> results = new List<ContactDocumentRelation>();

			if (organisation != null)
			{
				if (!SuppressForOrganisation(organisation))
				{
					OrgContact additionalContact = null;
					var overrideContact = documentSupporter.GetAdditionalDeliveryContact();
					if (overrideContact != null && overrideContact.OC_IsActive)
					{
						additionalContact = overrideContact as OrgContact;
					}

					foreach (OrgContact contact in organisation.Contacts)
					{
						if (additionalContact != null && additionalContact.PK == contact.PK)
						{
							var listContactDocumentRelations = BuildContactDocumentRelation(contact, documentSupporter);
							if (listContactDocumentRelations.Count == 0)
							{
								results.Add(new ContactDocumentRelation(contact, null));
							}
							else
							{
								results.AddRange(listContactDocumentRelations);
							}
						}
						else if (contact.OC_IsActive)
						{
							results.AddRange(BuildContactDocumentRelation(contact, documentSupporter));
						}
					}

					if (organisation.OH_IsGlobalAccount)
					{
						results = FilterContactDocumentRelationsForGlobalOrganization(results);
					}

					if (results.Count == 0)
					{
						results.Add(new ContactDocumentRelation(GenerateSystemDefaultContact(organisation), null));
					}
				}
			}

			return results;
		}

		internal List<OrgDocument> FilterOrgDocuments(OrgContact contact, DocumentSupporter documentSupporter)
		{
			if (documentSupporter == null)
			{
				return new List<OrgDocument>();
			}

			var documentDeliveryTypes = GetMatchingNonSuppressedDocumentDeliveryTypes(contact);
			return documentDeliveryTypes.Where(d => !documentSupporter.AdditionalExcludeFilter(fMenuItem, d.PK)).ToList();
		}

		List<ContactDocumentRelation> BuildContactDocumentRelation(OrgContact contact, DocumentSupporter documentSupporter)
		{
			var documentDeliveryTypes = FilterOrgDocuments(contact, documentSupporter);
			return documentDeliveryTypes.Select(documentDeliveryType => new ContactDocumentRelation(contact, documentDeliveryType)).ToList();
		}

		List<ContactDocumentRelation> FilterContactDocumentRelationsForGlobalOrganization(List<ContactDocumentRelation> documents)
		{
			List<ContactDocumentRelation> result = new List<ContactDocumentRelation>();
			var bestRank = 0;

			foreach (var document in documents)
			{
				var rank = 0;
				var contact = document != null ? document.Contact : null;

				if (contact != null && contact.OrgAddress != null && GlbBranch.CurrentBranch != null)
				{
					var contactUNLOCO = contact.OrgAddress.OA_RL_NKRelatedPortCode;

					if (!contactUNLOCO.IsEmpty && contactUNLOCO == GlbBranch.CurrentBranch.GB_RL_NKHomePort)
					{
						rank = 2;
					}
					else if (!contactUNLOCO.IsEmpty && contactUNLOCO.SubstringSafe(0, 2) == GlbBranch.CurrentBranch.GB_RL_NKHomePort.SubstringSafe(0, 2))
					{
						rank = 1;
					}
				}

				if (rank > bestRank)
				{
					result.Clear();
					bestRank = rank;
				}

				if (rank == bestRank)
				{
					result.Add(document);
				}
			}

			return result;
		}

		/// <summary>
		/// Special behaviour as Consignee documents could be sent to other related organisations
		/// </summary>
		/// <returns></returns>
		List<ContactDocumentRelation> GetAutoDeliveryContactsForConsignee(DocumentSupporter documentSupporter)
		{
			switch (SendDocsTo)
			{
				case OrgConstants.SendDocsTo.Importer:
					return GetAutoDeliveryContacts(fOrganisation, documentSupporter);

				case OrgConstants.SendDocsTo.Broker:
					return GetAutoDeliveryContacts(BrokerOrganisation, documentSupporter);

				case OrgConstants.SendDocsTo.Both:
					List<ContactDocumentRelation> results = new List<ContactDocumentRelation>();
					results.AddRange(GetAutoDeliveryContacts(fOrganisation, documentSupporter));
					if (fOrganisation != BrokerOrganisation)
					{
						results.AddRange(GetAutoDeliveryContacts(BrokerOrganisation, documentSupporter));
					}

					return results;
			}
			return new List<ContactDocumentRelation>();
		}

		bool SuppressForOrganisation(OrgHeader organisation)
		{
			var documentDeliveries = GetMatchingDocumentDeliveryTypes(organisation.SuppressedDocuments, fContactType);
			return documentDeliveries.Count > 0;
		}

		List<OrgDocument> GetMatchingNonSuppressedDocumentDeliveryTypes(OrgContact contact)
		{
			var matchingDocumentDeliveryTypes = GetMatchingDocumentDeliveryTypes(contact.Documents, fContactType);
			var result = new List<OrgDocument>();

			foreach (OrgDocument documentDeliveryType in matchingDocumentDeliveryTypes)
			{
				if (!documentDeliveryType.SuppressDocument && (!shouldGetJobSpecificRecipients || !documentDeliveryType.IsSuppressedForSpecificJob(DocumentSupporter.BusinessObject.PK, DocumentSupporter.BusinessObject.TablePrefix, fMenuItem)))
				{
					result.Add(documentDeliveryType);
				}
			}

			return result;
		}

		List<OrgDocument> GetMatchingDocumentDeliveryTypes(BusinessObjectCollection documentCollection, ContactType menutItemContactType)
		{
			var menuItemMatches = new List<OrgDocument>();
			var docGroupMatches = new List<OrgDocument>();

			foreach (OrgDocument documentDeliveryInfo in documentCollection)
			{
				if (FilterMatches(documentDeliveryInfo))
				{
					if (documentDeliveryInfo.OD_DocumentGroup == menutItemContactType)
					{
						docGroupMatches.Add(documentDeliveryInfo);
					}
					else if (fMenuItem != null && documentDeliveryInfo.OD_SU_MenuItem == fMenuItem.PK)
					{
						menuItemMatches.Add(documentDeliveryInfo);
					}
					else if (fMenuItem != null)
					{
						MatchMenuItemOnDocumentId(menuItemMatches, documentDeliveryInfo);
					}
				}
			}

			List<OrgDocument> results = MatchFilterOnFallback(menuItemMatches);

			if (results.Count == 0)
			{
				results = MatchFilterOnFallback(docGroupMatches);
			}

			if (results.Count == 0)
			{
				if (!string.IsNullOrEmpty(menutItemContactType.AggregateParentType))
				{
					results = GetMatchingDocumentDeliveryTypes(documentCollection, ContactType.Find(menutItemContactType.AggregateParentType));
				}
			}

			return results;
		}

		void MatchMenuItemOnDocumentId(List<OrgDocument> menuItemMatches, OrgDocument documentDeliveryInfo)
		{
			var menuItem = documentDeliveryInfo.MenuItem;
			if (MatchMenuItems(menuItem, fMenuItem) || MatchMenuItems(menuItem, ParentMenuCommand))
			{
				menuItemMatches.Add(documentDeliveryInfo);
			}
		}

		bool MatchMenuItems(IStmMenuItem menuItem1, IStmMenuItem menuItem2)
		{
			return menuItem1 != null && menuItem2 != null && (MatchDocumentIds(menuItem1, menuItem2) || MatchInvoiceMenus(menuItem1, menuItem2) || MatchPaymentVoucherMenus(menuItem1, menuItem2));
		}

		bool MatchDocumentIds(IStmMenuItem menuItem1, IStmMenuItem menuItem2)
		{
			return menuItem1.DocumentId.EqualsIgnoringCase(menuItem2.DocumentId);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard coded menu name")]
		bool MatchInvoiceMenus(IStmMenuItem menuItem1, IStmMenuItem menuItem2)
		{
			return ((menuItem1.SU_MenuName == "Invoice" || menuItem1.SU_MenuName == "DocBuilder Invoice"))
				&& ((menuItem2.SU_MenuName == "Invoice" || menuItem2.SU_MenuName == "DocBuilder Invoice"))
				&& menuItem1.SU_BusinessContext == menuItem2.SU_BusinessContext;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard coded menu name")]
		bool MatchPaymentVoucherMenus(IStmMenuItem menuItem1, IStmMenuItem menuItem2)
		{
			return (menuItem1.SU_MenuName == "Payment Voucher" && (menuItem1.SU_BusinessContext == "APTransaction" || menuItem1.SU_BusinessContext == "PaymentApproval"))
				&& (menuItem2.SU_MenuName == "Payment Voucher" && (menuItem2.SU_BusinessContext == "APTransaction" || menuItem2.SU_BusinessContext == "PaymentApproval"));
		}

		#region Fallback and Ranking

		/// <summary>
		/// Stores an OrgDocument and a ranking value
		/// </summary>
		class DocumentDeliveryTypeRanking
		{
			public DocumentDeliveryTypeRanking(int rank, OrgDocument documentDeliveryType)
			{
				this.Rank = rank;
				this.DocumentDeliveryType = documentDeliveryType;
			}

			public readonly int Rank;
			public readonly OrgDocument DocumentDeliveryType;
		}

		List<OrgDocument> MatchFilterOnFallback(List<OrgDocument> documentDeliveryTypes)
		{
			List<DocumentDeliveryTypeRanking> rankedRelations = new List<DocumentDeliveryTypeRanking>();
			int bestRank = 0;

			foreach (OrgDocument documentDeliveryType in documentDeliveryTypes)
			{
				int rank = 0;

				if (fRelatedParty != null && documentDeliveryType.OD_OH_RelatedFilterByParty == fRelatedParty.PK)
				{
					rank = MatchOnPorts(documentDeliveryType, 4);
				}
				else if (documentDeliveryType.OD_OH_RelatedFilterByParty.IsEmpty)
				{
					rank = MatchOnPorts(documentDeliveryType, 0);
				}

				if (rank > 0)
				{
					rankedRelations.Add(new DocumentDeliveryTypeRanking(rank, documentDeliveryType));
				}

				if (rank > bestRank)
				{
					bestRank = rank;
				}
			}

			List<OrgDocument> results = new List<OrgDocument>();
			foreach (DocumentDeliveryTypeRanking rankedRelation in rankedRelations)
			{
				if (rankedRelation.Rank == bestRank)
				{
					results.Add(rankedRelation.DocumentDeliveryType);
				}
			}

			return results;
		}

		int MatchOnPorts(OrgDocument documentDeliveryType, int baseRank)
		{
			if (LocalPortExactMatch(documentDeliveryType) && ForeignPortExactMatch(documentDeliveryType))
			{
				return 4 + baseRank;
			}
			else if (LocalPortExactMatch(documentDeliveryType) && ForeignPortMatches(documentDeliveryType))
			{
				return 3 + baseRank;
			}
			else if (LocalPortMatches(documentDeliveryType) && ForeignPortExactMatch(documentDeliveryType))
			{
				return 2 + baseRank;
			}
			else if (LocalPortMatches(documentDeliveryType) && ForeignPortMatches(documentDeliveryType))
			{
				return 1 + baseRank;
			}
			else
			{
				return 0;
			}
		}

		bool LocalPortExactMatch(OrgDocument documentDeliveryType)
		{
			return documentDeliveryType.OD_FilterLocalPort == fLocalPort || documentDeliveryType.OD_FilterLocalPort == LocalCountry;
		}

		bool ForeignPortExactMatch(OrgDocument documentDeliveryType)
		{
			return documentDeliveryType.OD_FilterForeignPort == fForeignPort || documentDeliveryType.OD_FilterForeignPort == ForeignCountry;
		}

		bool FilterMatches(OrgDocument documentDeliveryType)
		{
			bool matchesFilter = TransportModeMatches(documentDeliveryType) &&
				RelatedPartyMatches(documentDeliveryType) &&
				LocalPortMatches(documentDeliveryType) &&
				ForeignPortMatches(documentDeliveryType) &&
				RelatedBranchMatches(documentDeliveryType) &&
				RelatedCompanyMatches(documentDeliveryType) &&
				RelatedDepartmentMatches(documentDeliveryType) &&
				DirectionMatches(documentDeliveryType.OD_FilterDirection);

			return matchesFilter;
		}

		bool RelatedPartyMatches(OrgDocument documentDeliveryType)
		{
			bool result = fRelatedParty == null
				|| !documentDeliveryType.OD_OH_RelatedFilterByParty.IsValid
				|| documentDeliveryType.OD_OH_RelatedFilterByParty == fRelatedParty.PK;

			return result;
		}

		bool TransportModeMatches(OrgDocument documentDeliveryType)
		{
			bool result = string.IsNullOrEmpty(fTransportMode)
				|| fTransportMode == documentDeliveryType.OD_FilterShipmentMode
				|| documentDeliveryType.OD_FilterShipmentMode == Constants.TransportModes.All;

			return result;
		}

		bool DirectionMatches(ZString filterDirection)
		{
			return filterDirection.EqualsIgnoringCase(OrgDocumentLookups.FilterDirectionConstants.Codes.All) || filterDirection.EqualsIgnoringCase(ImportExportHelper.GetDirectionCode(direction));
		}

		bool LocalPortMatches(OrgDocument documentDeliveryType)
		{
			return documentDeliveryType.OD_FilterLocalPort == "" || LocalPortExactMatch(documentDeliveryType) || fLocalPort.IsNullOrEmpty();
		}

		bool ForeignPortMatches(OrgDocument documentDeliveryType)
		{
			return documentDeliveryType.OD_FilterForeignPort == "" || ForeignPortExactMatch(documentDeliveryType);
		}

		bool RelatedBranchMatches(OrgDocument documentDeliveryType)
		{
			return documentDeliveryType.FilterBranch == null || string.IsNullOrEmpty(fRelatedBranch) || documentDeliveryType.FilterBranch.GB_Code == fRelatedBranch;
		}

		bool RelatedCompanyMatches(OrgDocument documentDeliveryType)
		{
			return documentDeliveryType.FilterCompany == null || string.IsNullOrEmpty(fRelatedCompany) || documentDeliveryType.FilterCompany.GC_Code == fRelatedCompany;
		}

		bool RelatedDepartmentMatches(OrgDocument documentDeliveryType)
		{
			return documentDeliveryType.FilterDepartment == null || string.IsNullOrEmpty(fRelatedDepartment) || documentDeliveryType.FilterDepartment.GE_Code == fRelatedDepartment;
		}

		#endregion

		#endregion

		protected BusinessObjectFactory Factory;
		protected OrgHeader fOrganisation;
		IStmMenuItem fMenuItem;
		IStmMenuItem ParentMenuCommand;
		ContactType fContactType;
		string fLocalPort;
		string fForeignPort;
		string fRelatedBranch;
		string fRelatedCompany;
		string fRelatedDepartment;
		IDocAddress fOverriddenDeliveryDetails;
		protected OrgHeader fRelatedParty;
		string fTransportMode;
		string fContainerMode;
		bool fIsImport;
		ZString deliveryLanguage;
		bool shouldGetJobSpecificRecipients;

		internal DocumentSupporter DocumentSupporter { get; private set; }

#if DEBUG
		internal void SetOrganisationForTestOnly(OrgHeader org)
		{
			fOrganisation = org;
		}

		internal ZString GetLocalCountryForTesting()
		{
			return LocalCountry;
		}

		internal ZString GetForeignCountryForTesting()
		{
			return ForeignCountry;
		}

		internal void SetLocalAndForeignPortNullForTesting()
		{
			fLocalPort = null;
			fForeignPort = null;
		}
#endif
		Directions direction;

		ZString LocalCountry => !string.IsNullOrEmpty(fLocalPort) && fLocalPort.Length >= 2 ? fLocalPort.Substring(0, 2) : "";

		ZString ForeignCountry => !string.IsNullOrEmpty(fForeignPort) && fForeignPort.Length >= 2 ? fForeignPort.Substring(0, 2) : "";

		OrgHeader BrokerOrganisation
		{
			get
			{
				if (fBrokerOrganisation == null)
				{
					OrgSupplierBuyerLink supplierBuyerLink = GetSupplierBuyerLink();

					string brokerTransportMode = string.IsNullOrEmpty(fTransportMode)
						? Constants.TransportModes.All
						: fTransportMode;

					fBrokerOrganisation = (supplierBuyerLink != null && supplierBuyerLink.OL_OH_ImportBroker.IsValid)
						? supplierBuyerLink.ImportBroker
						: fOrganisation.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, brokerTransportMode, fContainerMode, fLocalPort);
				}

				return fBrokerOrganisation;
			}
		}
		OrgHeader fBrokerOrganisation;

		ZString SendDocsTo
		{
			get
			{
				OrgSupplierBuyerLink supplierBuyerLink = GetSupplierBuyerLink();
				if (supplierBuyerLink != null && !supplierBuyerLink.OL_SendImportDocsTo.IsEmpty)
				{
					return supplierBuyerLink.OL_SendImportDocsTo;
				}
				else
				{
					if (fTransportMode == Constants.TransportModes.Air)
					{
						return fOrganisation.MiscServ.OM_IMSendImportDocsTo;
					}
					else
					{
						return fOrganisation.MiscServ.OM_IMSendSeaImportDocsTo;
					}
				}
			}
		}

		OrgSupplierBuyerLink GetSupplierBuyerLink()
		{
			if (fRelatedParty != null)
			{
				foreach (OrgSupplierBuyerLink link in fOrganisation.SupplierLinks)
				{
					if (link.OL_OH_Supplier == fRelatedParty.PK && link.OrgSupBuyLinkTrnModes.Find(fTransportMode, fContainerMode) != null)
					{
						return link;
					}
				}
			}
			return null;
		}

		DocDeliveryContactCollection GetDeliveryDetailsForContactList(List<ContactDocumentRelation> contactDocumentRelations, IDocAddress overridenDocAddress)
		{
			var result = new DocDeliveryContactCollection(fMenuItem, overridenDocAddress, Factory, this);
			if (!string.IsNullOrEmpty(deliveryLanguage))
			{
				result.SetDeliveryLanguage(deliveryLanguage);
			}

			FetchCopyRecipients(contactDocumentRelations, new ZString[] { Constants.CopyRecipientType.CarbonCopyRecipient, Constants.CopyRecipientType.BlindCarbonCopyRecipient });

			foreach (ContactDocumentRelation contactDocumentRelation in contactDocumentRelations)
			{
				using (contactDocumentRelation.Contact.GenerateDocDeliveryRecipient())
				{
					result.Add(contactDocumentRelation.Contact.DocDeliveryDetails(contactDocumentRelation.DocumentDeliveryType, fMenuItem));
				}
			}
			return result;
		}

		#region Fetch Hints

		void FetchCopyRecipients(List<ContactDocumentRelation> contactDocumentRelations, ZString[] copyRecipientTypes)
		{
			foreach (ContactDocumentRelation relation in contactDocumentRelations)
			{
				foreach (ZString type in copyRecipientTypes)
				{
					if (relation.DocumentDeliveryType != null)
					{
						var filter = new ZQuery(OrgDocumentCopyRecipientSchema.ODR_OD, relation.DocumentDeliveryType.PK);
						filter.AddToFilter(new ZQuery(OrgDocumentCopyRecipientSchema.ODR_RecipientType, type).AddToFilter(new ZQuery(), JoinCondition.And));
						relation.DocumentDeliveryType.Factory.AddFetchHint(OrgDocumentCopyRecipientSchema.Instance, filter);
					}
				}
			}
		}

		#endregion
	}
}
