using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using BitMap = System.Drawing.Bitmap;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class ContainerDetentionDocumentSupporterAgencyTest : BaseAgencyTest
	{
		public void TestContext()
		{
			string[] supportedDataContextsIncludingRetardedEntries = Supporter.CommaSeparatedListOfSupportedDataContexts.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			string[] supportedDataContexts = Array.FindAll(supportedDataContextsIncludingRetardedEntries, (s) => !s.StartsWith("."));
			AssertEquals(BusinessContext.DetentionInvoice, Supporter.BusinessContext);
			AssertContainsExactElementsInAnyOrder("", new string[] { nameof(DataContext.DetentionInvoice), nameof(DataContext.GenericFreightJob), }, supportedDataContexts);
		}

		public void TestGetDocumentWrappers()
		{
			DocumentWrapper[] wrappers = Supporter.GetDocumentWrappers(DataContext.DetentionInvoice, null);
			AssertContainsExactElementsInAnyOrder("", (b) => (b is ContainerDetention) ? ((ContainerDetention)b).NC_JobNumber.ToString() : b.ToString(), new BusinessObject[] { Detention }, Array.ConvertAll(wrappers, (w) => (BusinessObject)w.WrappedObject));
		}

		public void TestGetGenericDocumentWrappers()
		{
			DocumentWrapper[] wrappers = Supporter.GetDocumentWrappers(DataContext.GenericFreightJob, null);
			AssertContainsExactElementsInAnyOrder("", (b) => (b is ContainerDetention) ? ((ContainerDetention)b).NC_JobNumber.ToString() : b.ToString(), new BusinessObject[] { Detention }, Array.ConvertAll(wrappers, (w) => (BusinessObject)w.WrappedObject));
		}

		public void TestGetContactOrganisation()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			Detention.NC_OH_Client = client.PK;
			var contact = Supporter.GetContactOrganisation(null, ContactType.Receivables, DocumentDirection.ANY);
			AssertEquals(client, contact.OrgHeader);
		}

		public void TestGetAlternativeBranding()
		{
			OrgHeader principal1 = NewPrincipal();
			OrgHeader principal2 = NewPrincipal();
			OrgHeader principal3 = NewPrincipal();
			Factory.Save();
			PrincipalBrandingCollection brandingCollection = new PrincipalBrandingCollection();
			PrincipalBranding branding1 = brandingCollection.AddNew();
			branding1.PrincipalPK = principal1.PK;
			branding1.Code = "PB1";
			branding1.BrandName = "Brand 1";
			branding1.BrandEmailAddress = "generic@brand1.com";
			branding1.Image = new BitMap(1, 1);
			PrincipalBranding branding2 = brandingCollection.AddNew();
			branding2.PrincipalPK = principal2.PK;
			branding2.Code = "PB2";
			branding2.BrandName = "Brand 2";
			branding2.BrandEmailAddress = "generic@brand2.com";
			branding2.Image = new BitMap(2, 2);
			DocumentsDataRegistry.Instance.PrincipalDocumentBrand.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, brandingCollection);
			Detention.NC_OH_Principal = ZGuid.Empty;
			AssertEquals(null, Supporter.GetAlternativeBranding());
			Detention.NC_OH_Principal = principal1.PK;
			var alternativeBranding = Supporter.GetAlternativeBranding();
			AssertEquals(branding1.Code, alternativeBranding.Code);
			AssertEquals(branding1.BrandName, alternativeBranding.BrandName);
			AssertEquals(branding1.BrandEmailAddress, alternativeBranding.BrandEmailAddress);
			AssertEquals(branding1.Image.Size, alternativeBranding.Image.Size);
			Detention.NC_OH_Principal = principal2.PK;
			alternativeBranding = Supporter.GetAlternativeBranding();
			AssertEquals(branding2.Code, alternativeBranding.Code);
			AssertEquals(branding2.BrandName, alternativeBranding.BrandName);
			AssertEquals(branding2.BrandEmailAddress, alternativeBranding.BrandEmailAddress);
			AssertEquals(branding2.Image.Size, alternativeBranding.Image.Size);
			Detention.NC_OH_Principal = principal3.PK;
			AssertEquals(null, Supporter.GetAlternativeBranding());
		}

		#region Implementation
		ContainerDetentionDocumentSupporter Supporter
		{
			get
			{
				return supporter ?? (supporter = new ContainerDetentionDocumentSupporter(Detention));
			}
		}

		ContainerDetentionDocumentSupporter supporter;
		ContainerDetention Detention
		{
			get
			{
				return detention ?? (detention = Factory.New<ContainerDetention>());
			}
		}

		ContainerDetention detention;
		#endregion
	}
}
