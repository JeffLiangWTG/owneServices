using System;
using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(DeclarationFormLayoutProvider))]
	sealed class DeclarationFormLayoutProviderBaseOnlyTest : DeclarationFormLayoutProviderAbstractTest<DeclarationFormLayoutProvider, BaseJobDeclaration>
	{
		[AsycudaCustomsCountries(Core.Constants.CountryCodes.Congo)]
		public void TestObjectFactoryContents()
		{
			CombineAssertions(() =>
			{
				var provider = DeclarationFormLayoutProvider.GetLayoutProvider(null);
				AssertNull("Provider for NULL", provider);

				provider = DeclarationFormLayoutProvider.GetLayoutProvider(Factory.New<BaseJobDeclaration>());
				AssertType<DeclarationFormLayoutProvider>("Provider for BaseJobDeclaration", provider);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				{
					AssertNull("Provider for AU is not specified", DeclarationFormLayoutProvider.GetLayoutProvider(Factory.New<BaseJobDeclaration>()));
				}

				foreach (var expectedProviderType in expectedDeclarationFormLayoutProviders)
				{
					var countryCode = expectedProviderType.Key;
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
					{
						provider = DeclarationFormLayoutProvider.GetLayoutProvider(Factory.New<BaseJobDeclaration>());
						AssertEquals($"{countryCode} has expected provider", expectedProviderType.Value, provider.GetType().FullName);
					}
				}
			});
		}

		readonly Dictionary<string, string> expectedDeclarationFormLayoutProviders = new Dictionary<string, string>()
		{
			{ Core.Constants.CountryCodes.Belgium, "Enterprise.Customs.BE.GUI.DeclarationFormLayoutProvider" },
			{ Core.Constants.CountryCodes.Brazil, "Enterprise.Customs.BR.GUI.DeclarationFormLayoutProvider" },
			{ Core.Constants.CountryCodes.China, "Enterprise.Customs.CN.GUI.DeclarationFormLayoutProvider" },
			{ Core.Constants.CountryCodes.Congo, "Enterprise.Customs.AsycudaCustoms.GUI.DeclarationFormLayoutProvider" },
			{ Core.Constants.CountryCodes.France, "Enterprise.Customs.FR.GUI.DeclarationFormLayoutProvider" },
			{ Core.Constants.CountryCodes.Germany, "Enterprise.Customs.DE.GUI.DeclarationFormLayoutProvider" },
			{ Core.Constants.CountryCodes.Ireland, "Enterprise.Customs.IE.GUI.DeclarationFormLayoutProvider" },
			{ Core.Constants.CountryCodes.Italy, "Enterprise.Customs.IT.GUI.DeclarationFormLayoutProvider" },
			{ Core.Constants.CountryCodes.Latvia, "Enterprise.Customs.EU.GUI.DeclarationFormLayoutProvider" },
			{ Core.Constants.CountryCodes.Netherlands, "Enterprise.Customs.NL.GUI.DeclarationFormLayoutProvider" },
			{ Core.Constants.CountryCodes.Norway, "Enterprise.Customs.NO.GUI.DeclarationFormLayoutProvider" },
			{ Core.Constants.CountryCodes.Poland, "Enterprise.Customs.PL.GUI.DeclarationFormLayoutProvider" },
			{ Core.Constants.CountryCodes.Spain, "Enterprise.Customs.ES.GUI.DeclarationFormLayoutProvider" },
			{ Core.Constants.CountryCodes.Sweden, "Enterprise.Customs.SE.GUI.DeclarationFormLayoutProvider" },
			{ Core.Constants.CountryCodes.Switzerland, "Enterprise.Customs.CH.GUI.DeclarationFormLayoutProvider" },
			{ Core.Constants.CountryCodes.Turkey, "Enterprise.Customs.TR.GUI.DeclarationFormLayoutProvider" },
			{ Core.Constants.CountryCodes.UnitedKingdom, "Enterprise.Customs.GUI.NonDynamicDeclarationFormLayoutProvider" },
		};

		protected override Type ExpectedDeclarationDetailsLayoutType => typeof(CommonDeclarationDetailsLayouts);

		protected override Type ExpectedDeclarationShipmentDetailsLayoutType => typeof(ShipmentDetailsLayouts);

		protected override Type ExpectedDeclarationShipmentTypeLayoutType => typeof(ShipmentTypeLayouts);

		protected override IReadOnlyDictionary<string, Type> ExpectedDeclarationTransportDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type> { { JobMessageTypeList.Codes.Export, typeof(TransportDetailsLayouts) } };

		protected override IReadOnlyDictionary<string, Type> ExpectedGetMiscOptionsLayoutTypeForMessageTypes => new Dictionary<string, Type> { { JobMessageTypeList.Codes.Export, typeof(CommonMiscOptionsLayouts) } };

		protected override Type ExpectedDeclarationGetOrganisationsLayoutType => typeof(CommonOrganisationsLayouts);

		protected override IReadOnlyDictionary<string, Type> ExpectedCommercialInvoiceDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type> { { JobMessageTypeList.Codes.Export, typeof(CommercialInvoiceDetailsLayoutProvider) } };

		protected override IReadOnlyDictionary<string, Type> ExpectedInstructionDetailsLayoutTypeForMessageTypes => new Dictionary<string, Type> { { JobMessageTypeList.Codes.Export, typeof(EntryInstructionBasicDetailsLayoutProvider) } };

		protected override IReadOnlyDictionary<string, Type> ExpectedInvoiceChargesGridColumnLayoutForMessageTypes => new Dictionary<string, Type> { { JobMessageTypeList.Codes.Export, null }, { JobMessageTypeList.Codes.Import, null } };

		protected override IReadOnlyDictionary<string, Type> ExpectedApportionedInvoiceChargesGridColumnLayoutForMessageTypes => new Dictionary<string, Type> { { JobMessageTypeList.Codes.Export, null }, { JobMessageTypeList.Codes.Import, null } };

		protected override IReadOnlyDictionary<string, Type> ExpectedGroupChargesGridColumnLayoutForMessageTypes => new Dictionary<string, Type> { { JobMessageTypeList.Codes.Export, null }, { JobMessageTypeList.Codes.Import, null } };
	}
}
