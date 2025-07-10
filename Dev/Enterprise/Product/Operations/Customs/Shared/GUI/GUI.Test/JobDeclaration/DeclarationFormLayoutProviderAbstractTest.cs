using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestsSubclassesOf(typeof(IDeclarationFormLayoutProvider))]
	public abstract class DeclarationFormLayoutProviderAbstractTest<TLayoutProvider, TDeclaration> : TestCaseWithFactory
		where TLayoutProvider : IDeclarationFormLayoutProvider, new()
		where TDeclaration : BaseJobDeclaration
	{
		public void TestGetDeclarationDetailsLayout()
		{
			AssertEquals("GetDeclarationDetailsLayout", ExpectedDeclarationDetailsLayoutType, provider.GetDeclarationDetailsLayout()?.GetType());
		}

		public void TestGetDeclarationShipmentTypeLayout()
		{
			AssertEquals("GetDeclarationShipmentTypeLayout", ExpectedDeclarationShipmentTypeLayoutType, provider.GetDeclarationShipmentTypeLayout()?.GetType());
		}

		public void TestGetDeclarationTransportDetailsLayoutUsesControlOrLayout()
		{
			UsesControlOrLayout((layoutProvider, declaration) => layoutProvider.GetDeclarationTransportDetailsLayout(declaration), nameof(IDeclarationFormLayoutProvider.GetDeclarationShipmentDetailsLayout));
		}

		public void TestGetDeclarationTransportDetailsLayout()
		{
			CheckLayoutsForDifferentMessageTypes(
				(layoutProvider, declaration) => layoutProvider.GetDeclarationTransportDetailsLayout(declaration), ExpectedDeclarationTransportDetailsLayoutTypeForMessageTypes, nameof(IDeclarationFormLayoutProvider.GetDeclarationShipmentDetailsLayout));
		}

		public void TestGetDeclarationShipmentDetailsLayout()
		{
			AssertEquals("GetDeclarationShipmentDetailsLayout", ExpectedDeclarationShipmentDetailsLayoutType, provider.GetDeclarationShipmentDetailsLayout()?.GetType());
		}

		public void TestGetDeclarationOrganisationsLayout()
		{
			AssertEquals("GetDeclarationOrganisationsLayout", ExpectedDeclarationGetOrganisationsLayoutType, provider.GetDeclarationOrganisationsLayout()?.GetType());
		}

		public void TestGetMiscOptionsLayout()
		{
			CheckLayoutsForDifferentMessageTypes((layoutProvider, declaration) => layoutProvider.GetMiscOptionsLayout(declaration), ExpectedGetMiscOptionsLayoutTypeForMessageTypes, nameof(IDeclarationFormLayoutProvider.GetMiscOptionsLayout));
		}

		public void TestGetMiscOptionsLayoutUsesControlOrLayout()
		{
			UsesControlOrLayout((layoutProvider, declaration) => layoutProvider.GetMiscOptionsLayout(declaration), nameof(IDeclarationFormLayoutProvider.GetMiscOptionsLayout));
		}

		public void TestGetCommercialInvoiceDetailsLayout()
		{
			CheckLayoutsForDifferentMessageTypes(
				(layoutProvider, declaration) => layoutProvider.GetCommercialInvoiceDetailsLayout(declaration), ExpectedCommercialInvoiceDetailsLayoutTypeForMessageTypes, nameof(IDeclarationFormLayoutProvider.GetCommercialInvoiceDetailsLayout));
		}

		public void TestGetInstructionDetailsLayout()
		{
			UsesControlOrLayout((layoutProvider, declaration) => layoutProvider.GetInstructionDetailsLayoutProvider(declaration), nameof(IDeclarationFormLayoutProvider.GetInstructionDetailsLayoutProvider));
		}

		public void TestGetInstructionDetailsLayout2()
		{
			CheckLayoutsForDifferentMessageTypes(
				(layoutProvider, declaration) => layoutProvider.GetInstructionDetailsLayoutProvider(declaration), ExpectedInstructionDetailsLayoutTypeForMessageTypes, nameof(IDeclarationFormLayoutProvider.GetInstructionDetailsLayoutProvider));
		}

		public void TestGetCommercialInvoiceDetailsLayoutUsesControlOrLayout()
		{
			UsesControlOrLayout((layoutProvider, declaration) => layoutProvider.GetCommercialInvoiceDetailsLayout(declaration), nameof(IDeclarationFormLayoutProvider.GetCommercialInvoiceDetailsLayout));
		}

		public void TestGetInstructionDetailsLayoutUsesControlOrLayout()
		{
			UsesControlOrLayout((layoutProvider, declaration) => layoutProvider.GetInstructionDetailsLayoutProvider(declaration), nameof(IDeclarationFormLayoutProvider.GetInstructionDetailsLayoutProvider));
		}

		public void TestGetInvoiceLineCalculationsPanelLayout()
		{
			UsesControlOrLayout((layoutProvider, declaration) => layoutProvider.GetInvoiceLineCalculationsPanelLayout(declaration), nameof(IDeclarationFormLayoutProvider.GetInvoiceLineCalculationsPanelLayout));
		}

		public void TestInvoiceChargesGridColumnLayout()
		{
			CheckLayoutsForDifferentMessageTypes((layoutProvider, declaration) => layoutProvider.GetInvoiceChargesGridColumnLayout(declaration), ExpectedInvoiceChargesGridColumnLayoutForMessageTypes, nameof(IDeclarationFormLayoutProvider.GetInvoiceChargesGridColumnLayout));
		}

		public void TestApportionedChargesGridColumnLayout()
		{
			CheckLayoutsForDifferentMessageTypes((layoutProvider, declaration) => layoutProvider.GetApportionedInvoiceChargesGridColumnLayout(declaration), ExpectedApportionedInvoiceChargesGridColumnLayoutForMessageTypes, nameof(IDeclarationFormLayoutProvider.GetApportionedInvoiceChargesGridColumnLayout));
		}

		public void TestGroupChargesGridColumnLayout()
		{
			CheckLayoutsForDifferentMessageTypes((layoutProvider, declaration) => layoutProvider.GetGroupChargesGridColumnLayout(declaration), ExpectedGroupChargesGridColumnLayoutForMessageTypes, nameof(IDeclarationFormLayoutProvider.GetGroupChargesGridColumnLayout));
		}

		protected virtual TDeclaration GetNewDeclaration() => Factory.New<TDeclaration>();

		protected IDeclarationFormLayoutProvider GetDeclarationFormLayoutProviderForTesting() => new TLayoutProvider();

		protected abstract Type ExpectedDeclarationDetailsLayoutType { get; }

		protected abstract Type ExpectedDeclarationShipmentDetailsLayoutType { get; }

		protected abstract Type ExpectedDeclarationShipmentTypeLayoutType { get; }

		protected abstract IReadOnlyDictionary<string, Type> ExpectedDeclarationTransportDetailsLayoutTypeForMessageTypes { get; }

		protected abstract Type ExpectedDeclarationGetOrganisationsLayoutType { get; }

		protected abstract IReadOnlyDictionary<string, Type> ExpectedGetMiscOptionsLayoutTypeForMessageTypes { get; }

		protected abstract IReadOnlyDictionary<string, Type> ExpectedCommercialInvoiceDetailsLayoutTypeForMessageTypes { get; }

		protected abstract IReadOnlyDictionary<string, Type> ExpectedInstructionDetailsLayoutTypeForMessageTypes { get; }

		protected abstract IReadOnlyDictionary<string, Type> ExpectedInvoiceChargesGridColumnLayoutForMessageTypes { get; }

		protected abstract IReadOnlyDictionary<string, Type> ExpectedApportionedInvoiceChargesGridColumnLayoutForMessageTypes { get; }

		protected abstract IReadOnlyDictionary<string, Type> ExpectedGroupChargesGridColumnLayoutForMessageTypes { get; }

		protected override void SetUp()
		{
			base.SetUp();
			provider = GetDeclarationFormLayoutProviderForTesting();
		}
		protected IDeclarationFormLayoutProvider provider;

		void CheckLayoutsForDifferentMessageTypes<TProvider>(Func<IDeclarationFormLayoutProvider, TDeclaration, TProvider> getLayoutFunc, IReadOnlyDictionary<string, Type> expectedTypes, string testingMethodName)
		{
			CombineAssertions(() =>
			{
				if (expectedTypes == null)
				{
					AssertNull($"Null {testingMethodName}", getLayoutFunc(provider, GetNewDeclaration()));
				}
				else
				{
					foreach (var expectedType in expectedTypes)
					{
						var declaration = GetNewDeclaration();
						declaration.JE_MessageType = expectedType.Key;
						AssertEquals($"{testingMethodName} for MessageType = {expectedType.Key}", expectedType.Value, getLayoutFunc(provider, declaration)?.GetType());
					}
				}
			});
		}

		void UsesControlOrLayout(Func<IDeclarationFormLayoutProvider, TDeclaration, IPanelLayoutProvider> getLayoutFunc, string testingMethodName)
		{
			if (UsesControlOrLayoutExceptionList.Contains($"{GlbCompany.CurrentCompany.GC_RN_NKCountryCode}.{testingMethodName}"))
			{
				Assert(true);
				return;
			}

			var declaration = GetNewDeclaration();
			var allDeclarationMessageTypes = declaration.Lookups.MessageTypeList.GetAllCodes();
			int nullCounter = 0;
			foreach (var messageType in allDeclarationMessageTypes)
			{
				declaration.JE_MessageType = messageType;
				if (getLayoutFunc(provider, declaration) == null)
				{
					nullCounter++;
				}
			}

			AssertEquals($"If {testingMethodName} uses layout, it needs to be used for every message type", true, nullCounter == 0 || nullCounter == allDeclarationMessageTypes.Length);
		}

		List<string> UsesControlOrLayoutExceptionList => new List<string>
		{
			"DE.GetCommercialInvoiceDetailsLayout",
			"IE.GetCommercialInvoiceDetailsLayout",
			"IE.GetInvoiceLineCalculationsPanelLayout",
			"KR.GetMiscOptionsLayout",
			"KR.GetCommercialInvoiceDetailsLayout",
			"IT.GetCommercialInvoiceDetailsLayout"
		};
	}
}
