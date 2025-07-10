using System.Collections;
using CargoWise.Application;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.GUI
{
	public sealed class DeclarationFormLayoutProvider : IDeclarationFormLayoutProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Pending adjustment of CW1161")]
		public static IDeclarationFormLayoutProvider GetLayoutProvider(BaseJobDeclaration declaration)
		{
			IDeclarationFormLayoutProvider provider = null;

			if (declaration != null)
			{
				var countryCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(declaration.CountryCode);
				var providers = ObjectFactory.Get<Hashtable>("DeclarationFormLayoutProviders");

				if (string.IsNullOrWhiteSpace(countryCode) || countryCode == Core.Constants.CountryCodes._TemplateCountryName_ || declaration.GetType() == typeof(BaseJobDeclaration))
				{
					var objectHandle = (ObjectHandle)providers?["Default"];
					provider = (IDeclarationFormLayoutProvider)objectHandle?.GetObject();
				}
				else
				{
					var objectHandle = (ObjectHandle)providers?[countryCode];
					provider = (IDeclarationFormLayoutProvider)objectHandle?.GetObject();

					if (provider == null && ObjectFactory.Get<Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(countryCode))
					{
						objectHandle = (ObjectHandle)providers[Core.Constants.CountryCodes.EuropeanUnion];
						provider = (IDeclarationFormLayoutProvider)objectHandle?.GetObject();
					}
					else if (provider == null && ObjectFactory.Get<Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry(countryCode))
					{
						objectHandle = (ObjectHandle)providers["AsycudaCustoms"];
						provider = (IDeclarationFormLayoutProvider)objectHandle?.GetObject();
					}
				}
			}
			return provider;
		}

		public IPanelLayoutProvider GetDeclarationDetailsLayout() => new CommonDeclarationDetailsLayouts();

		public IPanelLayoutProvider GetDeclarationShipmentTypeLayout() => new ShipmentTypeLayouts();

		public IPanelLayoutProvider GetDeclarationTransportDetailsLayout(BaseJobDeclaration declaration) => new TransportDetailsLayouts();

		public IPanelLayoutProvider GetDeclarationOrganisationsLayout() => new CommonOrganisationsLayouts();

		public IPanelLayoutProvider GetDeclarationShipmentDetailsLayout() => new ShipmentDetailsLayouts();

		public IPanelLayoutProvider GetMiscOptionsLayout(BaseJobDeclaration declaration) => new CommonMiscOptionsLayouts();

		public IPanelLayoutProvider GetCommercialInvoiceDetailsLayout(BaseJobDeclaration declaration) => new CommercialInvoiceDetailsLayoutProvider();

		public IPanelLayoutProvider GetInstructionDetailsLayoutProvider(BaseJobDeclaration declaration) => new EntryInstructionBasicDetailsLayoutProvider();

		public IPanelLayoutProvider GetInvoiceLineCalculationsPanelLayout(BaseJobDeclaration declaration) => null;

		public IGridColumnLayoutProvider GetInvoiceChargesGridColumnLayout(BaseJobDeclaration declaration) => null;

		public IGridColumnLayoutProvider GetApportionedInvoiceChargesGridColumnLayout(BaseJobDeclaration declaration) => null;

		public IGridColumnLayoutProvider GetGroupChargesGridColumnLayout(BaseJobDeclaration declaration) => null;
	}
}
