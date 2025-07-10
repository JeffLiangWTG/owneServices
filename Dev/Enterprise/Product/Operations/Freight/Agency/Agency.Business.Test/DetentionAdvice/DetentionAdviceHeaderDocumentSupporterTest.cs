using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class DetentionAdviceHeaderDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestContexts()
		{
			string[] supportedDataContextsIncludingRetardedEntries = DocumentSupporter.CommaSeparatedListOfSupportedDataContexts.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			string[] supportedDataContexts = Array.FindAll(supportedDataContextsIncludingRetardedEntries, (s) => !s.StartsWith("."));
			AssertEquals("BusinessContext", BusinessContext.AgencyDtnAdvice, DocumentSupporter.BusinessContext);
			AssertContainsExactElementsInAnyOrder("DataContext", new string[] { nameof(DataContext.AgencyDetentionAdvice), nameof(DataContext.GenericFreightJob) }, supportedDataContexts);
		}

		public void TestContactOrganisation()
		{
			AssertContactOrganisation("ContactType.ImportFreightAgent => Client", ContactType.ImportFreightAgent, Client, null, null);
		}

		public void TestDocumentWrapper()
		{
			AssertHasWrappersFor(DataContext.AgencyDetentionAdvice, DocumentDirection.ANY, "DocAgencyDetentionAdvice", Advice);
			AssertHasWrappersFor(DataContext.GenericFreightJob, DocumentDirection.ANY, "FreightWrapperFromDetentionAdvice", Advice);
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
			AssertNotNull(message + " (not null)", contact);
			AssertEquals(message + " (org)", expectedOrg, contact.OrgHeader);
			AssertEquals(message + " (contact)", expectedContact, contact.OrgContact);
			AssertEquals(message + " (related org)", expectedRelatedOrg, contact.RelatedOrgHeader);
		}

		public OrgHeader Client
		{
			get
			{
				return client ?? (client = Factory.New<OrgHeader>());
			}
		}

		OrgHeader client;
		public DetentionAdviceHeader Advice
		{
			get
			{
				return advice ?? (advice = new DetentionAdviceHeader(Client));
			}
		}

		DetentionAdviceHeader advice;
		public DetentionAdviceHeaderDocumentSupporter DocumentSupporter
		{
			get
			{
				return documentSupporter ?? (documentSupporter = (DetentionAdviceHeaderDocumentSupporter)Advice.DocumentSupporter);
			}
		}

		DetentionAdviceHeaderDocumentSupporter documentSupporter;
		#endregion
	}
}
