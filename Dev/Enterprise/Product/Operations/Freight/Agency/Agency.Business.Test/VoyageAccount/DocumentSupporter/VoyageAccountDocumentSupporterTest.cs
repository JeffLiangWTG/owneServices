using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(VoyageAccountDocumentSupporter))]
	internal class VoyageAccountDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestContexts()
		{
			string[] supportedDataContextsIncludingRetardedEntries = DocumentSupporter.CommaSeparatedListOfSupportedDataContexts.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			string[] supportedDataContexts = Array.FindAll(supportedDataContextsIncludingRetardedEntries, (s) => !s.StartsWith("."));
			AssertEquals("BusinessContext", BusinessContext.AgencyVoyageAccount, DocumentSupporter.BusinessContext);
			AssertContainsExactElementsInAnyOrder("DataContext", new string[] { nameof(DataContext.AgencyVoyageAccount), nameof(DataContext.GenericFreightJob) }, supportedDataContexts);
		}

		public void TestContactOrganisation()
		{
			AssertContactOrganisation("ContactType.ShippingLine => Principal", ContactType.ShippingLine, Principal, null, null);
		}

		public void TestDocumentWrapper()
		{
			AssertHasWrappersFor(DataContext.AgencyVoyageAccount, DocumentDirection.ANY, "DocAgencyVoyageAccount", Account);
		}

		#region Implementation
		void AssertHasWrappersFor(DataContext context, DocumentDirection direction, string expectedWrapperName, params BusinessObject[] expected)
		{
			List<BusinessObject> wrapped = new List<BusinessObject>();
			DocumentWrapper[] wrapers = DocumentSupporter.GetDocumentWrappers(context, StmMenuItem.GetForTesting(direction)) ?? Array.Empty<DocumentWrapper>();
			foreach (DocumentWrapper wrapper in wrapers)
			{
				BusinessObject wrappedObject = (BusinessObject)wrapper.WrappedObject;
				AssertEquals(string.Format("Wrapper type name for '{0}'", wrappedObject), expectedWrapperName, wrapper.GetType().Name);
				wrapped.Add(wrappedObject);
			}

			AssertContainsExactElementsInAnyOrder(expected, wrapped);
		}

		void AssertContactOrganisation(string message, ContactType contactType, OrgHeader expectedOrg, OrgContact expectedContact, OrgHeader expectedRelatedOrg)
		{
			IDocumentDeliveryContact contact = DocumentSupporter.GetContactOrganisation("", contactType, DocumentDirection.DEP);
			AssertEquals(message + " (org)", expectedOrg, contact.OrgHeader);
			AssertEquals(message + " (contact)", expectedContact, contact.OrgContact);
			AssertEquals(message + " (related org)", expectedRelatedOrg, contact.RelatedOrgHeader);
		}

		OrgHeader Principal
		{
			get
			{
				return principal ?? (principal = Factory.NewWithValidTestData<OrgHeader>());
			}
		}

		OrgHeader principal;
		JobVoyage Voyage
		{
			get
			{
				if (voyage == null)
				{
					voyage = Factory.New<JobVoyage>();
					voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
					voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
					voyage.GenerateSailings();
				}

				return voyage;
			}
		}

		JobVoyage voyage;
		VoyageAccount Account
		{
			get
			{
				if (account == null)
				{
					account = Factory.New<VoyageAccount>();
					account.NA_JV = Voyage.PK;
					account.NA_OH = Principal.PK;
				}

				return account;
			}
		}

		VoyageAccount account;
		VoyageAccountDocumentSupporter DocumentSupporter
		{
			get
			{
				return documentSupporter ?? (documentSupporter = new VoyageAccountDocumentSupporter(Account));
			}
		}

		VoyageAccountDocumentSupporter documentSupporter;
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var jobVoyage = Factory.New<JobVoyage>();
			jobVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			jobVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			jobVoyage.GenerateSailings();
			var principalHeader = Factory.NewWithValidTestData<OrgHeader>();
			var voyageAccount = Factory.New<VoyageAccount>();
			voyageAccount.NA_JV = jobVoyage.PK;
			voyageAccount.NA_OH = principalHeader.PK;
			return voyageAccount;
		}
		#endregion
	}
}
