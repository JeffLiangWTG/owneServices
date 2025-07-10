using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(SundryChargesDocumentSupporter))]
	internal class SundryChargesDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestContexts()
		{
			string[] supportedDataContextsIncludingRetardedEntries = DocumentSupporter.CommaSeparatedListOfSupportedDataContexts.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			string[] supportedDataContexts = Array.FindAll(supportedDataContextsIncludingRetardedEntries, (s) => !s.StartsWith("."));
			AssertEquals("BusinessContext", BusinessContext.AgencySundryCharges, DocumentSupporter.BusinessContext);
			AssertContainsExactElementsInAnyOrder("DataContext", new string[] { nameof(DataContext.GenericFreightJob) }, supportedDataContexts);
		}

		public void TestContactOrganisation()
		{
			AssertContactOrganisation("ContactType.ShippingLine => Principal", ContactType.ShippingLine, Principal, null, null);
		}

		public void TestDocumentWrapper()
		{
			AssertHasWrappersFor(DataContext.GenericFreightJob, DocumentDirection.ANY, "FreightWrapperFromSundryCharges", Charges);
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
		SundryCharges Charges
		{
			get
			{
				if (charges == null)
				{
					charges = Factory.New<SundryCharges>();
					charges.D4_OH_BillToParty = Principal.PK;
				}

				return charges;
			}
		}

		SundryCharges charges;
		SundryChargesDocumentSupporter DocumentSupporter
		{
			get
			{
				return documentSupporter ?? (documentSupporter = new SundryChargesDocumentSupporter(Charges));
			}
		}

		SundryChargesDocumentSupporter documentSupporter;
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var billToParty = Factory.NewWithValidTestData<OrgHeader>();
			var sundryCharges = Factory.New<SundryCharges>();
			sundryCharges.D4_OH_BillToParty = billToParty.PK;
			return sundryCharges;
		}
		#endregion
	}
}
