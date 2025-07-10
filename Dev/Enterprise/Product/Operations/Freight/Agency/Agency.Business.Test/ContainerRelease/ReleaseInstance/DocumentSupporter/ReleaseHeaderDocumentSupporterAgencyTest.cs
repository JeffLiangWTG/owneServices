using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using BitMap = System.Drawing.Bitmap;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class ReleaseHeaderDocumentSupporterAgencyTest : BaseAgencyTest
	{
		public void TestContext()
		{
			string[] supportedDataContextsIncludingRetardedEntries = Supporter.CommaSeparatedListOfSupportedDataContexts.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			string[] supportedDataContexts = Array.FindAll(supportedDataContextsIncludingRetardedEntries, (s) => !s.StartsWith("."));
			AssertEquals(BusinessContext.ContainerRelease, Supporter.BusinessContext);
			AssertContainsExactElementsInAnyOrder("", new string[] { nameof(BusinessContext.ContainerRelease) }, supportedDataContexts);
		}

		public void GetDocumentWrappers()
		{
			AgencyBookingContainer container1 = Booking.BookedContainers.AddNew();
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container1.JC_ContainerCount = 2;
			AgencyBookingContainer container2 = Booking.BookedContainers.AddNew();
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			container2.JC_ContainerCount = 2;
			foreach (ReleaseDetail detail in Header.Details)
			{
				detail.ReleaseCount = detail.Container.JC_ContainerCount;
			}

			DocumentWrapper[] wrappers = Supporter.GetDocumentWrappers(DataContext.ContainerRelease, null);
			AssertContainsExactElementsInAnyOrder("", (b) => (b is ReleaseHeader) ? ((ReleaseHeader)b).ReleaseNumber.ToString() : b.ToString(), new BusinessObject[] { Header }, Array.ConvertAll(wrappers, (w) => (BusinessObject)w.WrappedObject));
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
			Instance.Shipment.JS_OH_DeliveryAgent = ZGuid.Empty;
			AssertEquals(null, Supporter.GetAlternativeBranding());
			Instance.Shipment.JS_OH_DeliveryAgent = principal1.PK;
			var alternativeBranding = Supporter.GetAlternativeBranding();
			AssertEquals(branding1.Code, alternativeBranding.Code);
			AssertEquals(branding1.BrandName, alternativeBranding.BrandName);
			AssertEquals(branding1.BrandEmailAddress, alternativeBranding.BrandEmailAddress);
			AssertEquals(branding1.Image.Size, alternativeBranding.Image.Size);
			Instance.Shipment.JS_OH_DeliveryAgent = principal2.PK;
			alternativeBranding = Supporter.GetAlternativeBranding();
			AssertEquals(branding2.Code, alternativeBranding.Code);
			AssertEquals(branding2.BrandName, alternativeBranding.BrandName);
			AssertEquals(branding2.BrandEmailAddress, alternativeBranding.BrandEmailAddress);
			AssertEquals(branding2.Image.Size, alternativeBranding.Image.Size);
			Instance.Shipment.JS_OH_DeliveryAgent = principal3.PK;
			AssertEquals(null, Supporter.GetAlternativeBranding());
		}

		#region Implementation
		DocumentSupporter Supporter
		{
			get
			{
				return Instance.DocumentSupporter;
			}
		}

		ReleaseInstance Instance
		{
			get
			{
				return instance ?? (instance = Header.Instances.AddNew());
			}
		}

		ReleaseInstance instance;
		ReleaseHeader Header
		{
			get
			{
				return header ?? (header = new ReleaseHeader(Booking, false));
			}
		}

		ReleaseHeader header;
		AgencyBooking Booking
		{
			get
			{
				return booking ?? (booking = Factory.New<AgencyBooking>());
			}
		}

		AgencyBooking booking;
		#endregion
	}
}
