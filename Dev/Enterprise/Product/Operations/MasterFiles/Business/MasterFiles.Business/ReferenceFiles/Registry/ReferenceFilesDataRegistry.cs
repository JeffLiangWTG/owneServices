using System;
using System.Collections;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.ReferenceFiles.Registry.CertificateTypes;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public sealed class ReferenceFilesDataRegistry : RegistryItemSet
	{
		#region Construction

		public static ReferenceFilesDataRegistry Instance
		{
			get { return instance ?? (instance = new ReferenceFilesDataRegistry()); }
		}

		[ThreadStatic]
		static ReferenceFilesDataRegistry instance;

		ReferenceFilesDataRegistry()
		{
		}

		#endregion

		public override bool IsForProductivityWise => true;

		#region Compliance List Defaults

		public CodeDescriptionBoolRegistryItem ComplianceListDefaults
		{
			get
			{
				var defaultValue = new CodeDescriptionBoolCollection();

				defaultValue.Add("ADV", ResString.GetMultilingualString("ReferenceFiles|Advisories", "Advisories"), false);
				defaultValue.Add("ENF", ResString.GetMultilingualString("ReferenceFiles|EnforcementOrders", "Enforcement Orders"), true);
				defaultValue.Add("EXC", ResString.GetMultilingualString("ReferenceFiles|ExclusionList", "Exclusion List"), false);
				defaultValue.Add("FIN", ResString.GetMultilingualString("ReferenceFiles|FinancialSanctions", "Financial Sanctions"), false);
				defaultValue.Add("LAW", ResString.GetMultilingualString("ReferenceFiles|LawEnforcement", "Law Enforcement"), true);
				defaultValue.Add("SAN", ResString.GetMultilingualString("ReferenceFiles|SanctionsOwnership", "Sanctions Ownership"), false);
				defaultValue.Add("TRA", ResString.GetMultilingualString("ReferenceFiles|TradeRestrictions", "Trade Restrictions"), false);
				defaultValue.Add("VIS", ResString.GetMultilingualString("ReferenceFiles|VisaTravelBan", "Visa/Travel Ban"), false);
				defaultValue.Add("WAC", ResString.GetMultilingualString("ReferenceFiles|WarCrimes", "War Crimes"), false);
				defaultValue.Add("WAN", ResString.GetMultilingualString("ReferenceFiles|Warnings", "Warnings"), true);

				return GetItem("ComplianceListDefaults", delegate
				{
					return new CodeDescriptionBoolRegistryItem(
						"ComplianceListDefaults",
						RawDataRegistry.Categories.ReferenceFiles,
						ResString.GetMultilingualString("0AB70870-2370-4089-8C4B-EB7AD15BBDD1", "Compliance List Defaults"),
						ResString.GetMultilingualString("CB4C9416-890F-4AB2-B252-65F66CDD7D68", "This registry determines the default exclusion settings for newly created Compliance List records."),
						RegistryStorageFlags.System,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("1D6CADD5-E267-48D9-9733-6994B0D36701", "Is Excluded"), null, true, true, false),
						defaultValue);
				});
			}
		}

		#endregion

		#region Equipment

		public CertificateTypeRegistryItem EquipmentCertificateTypes
		{
			get
			{
				return GetItem("EquipmentCertificateTypes", delegate
				{
					var collection = new CertificateTypeCollection();
					collection.AddNew().SetupValues(EquipmentCertificateTypeList.Codes.Service, EquipmentCertificateTypeList.Descriptions.Service.GetUnresolvedString(), false, false, false, AlertTypeList.Codes.NoAlert);
					foreach (IDefaultCertificateTypesProvider provider in ObjectFactory.Get<IEnumerable>("EquipmentCertificateTypesProviders"))
					{
						provider.AddDefaultCertificateTypes(collection);
					}
					collection.Sort(CertificateType.Schema.Code);

					return new CertificateTypeRegistryItem(
						"EquipmentCertificateTypes",
						RawDataRegistry.Categories.ReferenceFiles,
						ResString.GetMultilingualString("ac98ebba-2c37-4b2e-a30e-c3ef672616f7", "Equipment Certificate Types"),
						ResString.GetMultilingualString("dc4c4d2c-7e16-46aa-8340-cd5d3a5180f8", "Types of certificates/licenses/registrations/services applicable to Equipment."),
						RegistryStorageFlags.System,
						collection);
				});
			}
		}

		#endregion

		#region ShowPersonalEffects

		public BooleanRegistryItem ShowPersonalEffects
		{
			get
			{
				return GetItem("ShowPersonalEffects", delegate
				{
					return new BooleanRegistryItem(
						"ShowPersonalEffects",
						RawDataRegistry.Categories.ReferenceFiles,
						(NoResString)"Show Personal Effects",
						(NoResString)"Enable this option to show the 'Is Personal Effects' option on Commodity.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		#endregion

		#region EnableShippingLineReferenceFile

		public BooleanRegistryItem EnableShippingLineReferenceFile
		{
			get
			{
				return GetItem("EnableShippingLineReferenceFile", delegate
				{
					return new BooleanRegistryItem(
						"EnableShippingLineReferenceFile",
						RawDataRegistry.Categories.ReferenceFiles,
						(NoResString)"Enable Shipping Line Reference File",
						(NoResString)"Set option to 'Yes' will enable Shipping Line Reference File",
						RegistryStorageFlags.System,
						IsEdiProd ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		#endregion
	}
}
