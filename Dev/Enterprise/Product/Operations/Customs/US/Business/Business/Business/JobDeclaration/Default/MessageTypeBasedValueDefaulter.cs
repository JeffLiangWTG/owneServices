using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;

namespace Enterprise.Customs.US.Business
{
	public class MessageTypeBasedValueDefaulter : Customs.Business.MessageTypeBasedValueDefaulter
	{
		public MessageTypeBasedValueDefaulter(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override void DefaultAllCore()
		{
			base.DefaultAllCore();
			declaration.US_CargoReleaseType = ZString.Empty;
			SetCertifyCargoReleaseAndDefaultCargoReleaseTypeIfNecessary();
			DefaultValueFromSupplier(declaration.JE_OH_Supplier);
			DefaultValueFromExporter(declaration.JE_OH_Exporter);
		}

		internal void DefaultValueFromSupplier(ZGuid oldSupplierPK)
		{
			var supplier = declaration.Supplier;
			var supplierPK = declaration.JE_OH_Supplier;
			ZGuid addressPK = ZGuid.Empty;
			if (declaration.IsExport && (declaration.IsSupplierPickupAddressInitialised || declaration.IsInDatabase) && declaration.JE_OH_Supplier == declaration.SupplierPickupAddress.OrganisationPK)
			{
				addressPK = declaration.SupplierPickupAddress.E2_OA_Address;
			}
			else if (supplier != null)
			{
				addressPK = supplier.MainAddress.PK;
			}

			if (!addressPK.IsEmpty && !declaration.IsCreatedFromUSLowValue)
			{
				declaration.ClearInvoiceValuesIfSame(addressPK, JobComInvoiceHeader.Schema.JZ_OA_SupplierAddress, (x) =>
					{
						if (supplierPK != oldSupplierPK && x.JZ_OH_Supplier == oldSupplierPK && x.JZ_OA_SupplierAddress == addressPK)
						{
							x.JZ_OH_Supplier = supplierPK;
						}
					});
			}
			else if (supplierPK != oldSupplierPK)
			{
				foreach (var invoice in declaration.Invoices.OfType<JobComInvoiceHeader>().Where(x => x.JZ_OA_SupplierAddress.IsEmpty && x.JZ_OH_Supplier == oldSupplierPK))
				{
					invoice.JZ_OH_Supplier = supplierPK;
				}
			}

			if (declaration.IsImport && supplier != null)
			{
				if (USCustomsDataRegistry.Instance.DefaultManufacturerFromSupplier.GetFallBackValueAtAllLevels(declaration.RegistryCompanyPK, declaration.RegistryBranchPK, Guid.Empty))
				{
					declaration.JE_OA_ManufacturerAddress = supplier.MainAddress.PK;
				}
				if (USCustomsDataRegistry.Instance.DefaultSellerFromSupplier.GetFallBackValueAtAllLevels(declaration.RegistryCompanyPK, declaration.RegistryBranchPK, Guid.Empty))
				{
					declaration.JE_OA_SellerAddress = supplier.MainAddress.PK;
				}
			}
		}

		internal void DefaultValueFromExporter(ZGuid oldExporterPK)
		{
			var exporter = declaration.Exporter;
			var exporterPK = declaration.JE_OH_Exporter;
			ZGuid addressPK = ZGuid.Empty;
			if (exporter != null)
			{
				if (declaration.IsACE)
				{
					addressPK = DefaultAddressRelatedDeterminer.GetMIDAddress(exporter);
				}
				else
				{
					addressPK = exporter.MainAddress.PK;
				}
			}

			if (!addressPK.IsEmpty)
			{
				declaration.ClearInvoiceValuesIfSame(addressPK, JobComInvoiceHeader.Schema.JZ_OA_ExporterAddress, (x) =>
				{
					if (exporterPK != oldExporterPK && x.ExporterOrgPK == oldExporterPK && x.JZ_OA_ExporterAddress == addressPK)
					{
						x.ExporterOrgPK = exporterPK;
					}
				});
			}
			else if (exporterPK != oldExporterPK)
			{
				foreach (var invoice in declaration.Invoices.OfType<JobComInvoiceHeader>().Where(x => x.JZ_OA_ExporterAddress.IsEmpty && x.ExporterOrgPK == oldExporterPK))
				{
					invoice.ExporterOrgPK = exporterPK;
				}
			}
		}

		internal void DefaultValueFromImporter(ZGuid oldImporterPK)
		{
			if (declaration.IsExport)
			{
				var importerPK = declaration.JE_OH_Importer;
				if (importerPK != oldImporterPK)
				{
					foreach (var invoice in declaration.Invoices.OfType<JobComInvoiceHeader>().Where(x => x.JZ_OA_BuyerAddress.IsEmpty && x.JZ_OH_Buyer == oldImporterPK))
					{
						invoice.JZ_OH_Buyer = importerPK;
					}
				}
			}
		}

		internal void SetCertifyCargoReleaseAndDefaultCargoReleaseTypeIfNecessary()
		{
			if (declaration.IsFormalImport && (!declaration.IsReWarehouse || declaration.IsACEReWarehouse))
			{
				DefaultCargoReleaseTypeIfNecessary();
			}
		}

		internal void DefaultCargoReleaseTypeIfNecessary()
		{
			if (!declaration.IsReWarehouse || declaration.IsACEReWarehouse)
			{
				if (declaration.IsACE)
				{
					declaration.US_CargoReleaseType = GetDefaultCargoReleaseTypeForACE(declaration.US_CargoReleaseType, declaration.US_EnableENS, declaration.US_EnableCRL);
				}

				if (declaration.US_CargoReleaseType.IsEmpty)
				{
					declaration.US_CargoReleaseType = GetDefaultCargoReleaseType();
				}
			}
			else
			{
				declaration.US_CargoReleaseType = ZString.Empty;
			}
		}

		internal static ZString GetDefaultCargoReleaseTypeForACE(ZString cargoReleaseType, bool enableENS, bool enableCRL)
		{
			var result = string.Empty;
			if (enableCRL)
			{
				result = CargoReleaseTypeList.Codes.SE;
			}
			else if (enableENS)
			{
				result = cargoReleaseType == CargoReleaseTypeList.Codes.SE ? CargoReleaseTypeList.Codes.ACE : ((cargoReleaseType == CargoReleaseTypeList.Codes.ACE || cargoReleaseType == CargoReleaseTypeList.Codes.ACS) ? cargoReleaseType.ToString() : string.Empty);
			}
			return result;
		}

		ZString GetDefaultCargoReleaseType()
		{
			string result = string.Empty;
			if (!declaration.IsACE && declaration.US_EnableCRL && !declaration.JE_TransportMode.IsEmpty)
			{
				if (TransportTypeList.IsBorderTransportType(declaration.JE_TransportMode))
				{
					var bcrPorts = declaration.BCRPortsFromRegistry;
					if (bcrPorts.Count > 0)
					{
						if (!declaration.US_SchDEntry.IsEmpty)
						{
							result = bcrPorts.ContainsPortCode(declaration.US_SchDEntry) ? CargoReleaseTypeList.Codes.BCR : CargoReleaseTypeList.Codes.CR;
						}
					}
					else if (!declaration.US_EntryType.IsEmpty)
					{
						result = declaration.US_ITDate.IsEmpty && !declaration.IsConsumptionFTZ ? CargoReleaseTypeList.Codes.BCR : CargoReleaseTypeList.Codes.CR;
					}
				}
				else
				{
					result = CargoReleaseTypeList.Codes.CR;
				}
			}
			return result;
		}

		new JobDeclaration declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}
	}
}
