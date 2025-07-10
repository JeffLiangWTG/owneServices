using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.Customs.US.Business
{
	public class InvoiceLineImportWizard : ImportWizard
	{
		public InvoiceLineImportWizard(IImportCollectionInfo collectionInfo, ISettingsStorage settingsStorage, IFileMapper fileMapper)
			: base(collectionInfo, settingsStorage, fileMapper)
		{
			Factory = collectionInfo.Collection.Factory;
		}

		protected override void RunPreSaveValidationCore()
		{
			ClearRowNotifications();
			base.RunPreSaveValidationCore();
			if (Mapping.OfType<ImportWizardMapping>().Any(x => x.HasMappedFrom() && (x.MappingName == JobComInvoiceLine.Schema.ManufacturerOrgPK || x.MappingName == JobComInvoiceLine.Schema.JI_OA_ManufacturerAddress))
				&& Mapping.OfType<ImportWizardMapping>().Any(x => x.HasMappedFrom() && x.MappingName == JobComInvoiceLine.Schema.ManufacturerMID))
			{
				AddRowError("Cannot import both Manufacturer/Address and Manufacturer MID.");
			}
		}

		protected override void ImportIntoBizObjCore(Action<BusinessObject, string, object> setValue, IEnumerable<ImportWizardMapping> mappedRecords, BusinessObject bizObj, string[] values)
		{
			base.ImportIntoBizObjCore(setValue, mappedRecords, bizObj, values);

			var invoiceLine = bizObj as JobComInvoiceLine;
			if (invoiceLine != null)
			{
				var mappings = mappedRecords.Where(m => m.HasMappedFrom());
				var manufacturerMapping = mappings.FirstOrDefault(m => m.MappingName == JobComInvoiceLine.Schema.ManufacturerMID);
				if (manufacturerMapping != null && manufacturerMapping.FileColumnIndexOrder.Max() < values.Length)
				{
					var value = manufacturerMapping.GetMappedFieldValue(values).Trim();
					invoiceLine.ManufacturerMID = value;
				}

				UpdateDetailsForCombinedLineAfterImport(invoiceLine, values, mappings);
			}
		}

		void UpdateDetailsForCombinedLineAfterImport(JobComInvoiceLine invoiceLine, string[] values, IEnumerable<ImportWizardMapping> invoiceLineMapping)
		{
			if (invoiceLine.IsCombinedLine())
			{
				var regularTariffLine = invoiceLine.GetCombinedLines().FirstOrDefault(x => x.IsNormalTariffLine());
				if (regularTariffLine != null && invoiceLine != regularTariffLine)
				{
					var firstCustomsQuantity = ZDecimal.ParseSafe(GetValueFromMapping(values, invoiceLineMapping, JobComInvoiceLine.Schema.JI_CustomsQuantity), 0);
					var firstCustomsUQ = GetValueFromMapping(values, invoiceLineMapping, JobComInvoiceLine.Schema.JI_CustomsUnitQty);

					var secondCustomsQuantity = ZDecimal.ParseSafe(GetValueFromMapping(values, invoiceLineMapping, JobComInvoiceLine.Schema.JI_CustomsSecondQuantity), 0);
					var secondCustomsUQ = GetValueFromMapping(values, invoiceLineMapping, JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty);

					var thirdCustomsQuantity = ZDecimal.ParseSafe(GetValueFromMapping(values, invoiceLineMapping, JobComInvoiceLine.Schema.JI_CustomsThirdQuantity), 0);
					var thirdCustomsUQ = GetValueFromMapping(values, invoiceLineMapping, JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty);

					ResetInvoiceLineValueForCombinedLine(regularTariffLine, invoiceLine.JI_LinePrice, invoiceLine.JI_InvoiceQuantity, invoiceLine.JI_InvoiceUQ, invoiceLine.JI_Weight, invoiceLine.JI_WeightUQ, invoiceLine.JI_NetWeight, invoiceLine.JI_NetWeightUQ, firstCustomsQuantity, firstCustomsUQ, secondCustomsQuantity, secondCustomsUQ, thirdCustomsQuantity, thirdCustomsUQ);
					ResetInvoiceLineValueForCombinedLine(invoiceLine, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty);
				}
			}
		}

		ZString GetValueFromMapping(string[] values, IEnumerable<ImportWizardMapping> invoiceLineMapping, ZString mappingName)
		{
			var result = ZString.Empty;
			var fieldMapping = invoiceLineMapping.FirstOrDefault(x => x.MappingName == mappingName);
			if (fieldMapping != null)
			{
				result = fieldMapping.GetMappedFieldValue(values);
			}

			return result;
		}

		void ResetInvoiceLineValueForCombinedLine(JobComInvoiceLine invoiceLine, ZDecimal linePrice, ZDecimal invoiceQuantity, ZString invoiceUQ, ZDecimal weight, ZString weightUQ, ZDecimal netWeight, ZString netWeightUQ, ZDecimal firstCustomsQty, ZString firstCustomsUQ, ZDecimal secondCustomQty, ZString secondCustomsUQ, ZDecimal thirdCustomsQty, ZString thirdCustomsUQ)
		{
			invoiceLine.JI_LinePrice = linePrice;
			invoiceLine.JI_InvoiceQuantity = invoiceQuantity;
			var setterSuspender = invoiceLine.SetterSuspender;
			ResumeSettingAndSetValue(setterSuspender, JobComInvoiceLine.Schema.JI_InvoiceUQ, () => invoiceLine.JI_InvoiceUQ = invoiceUQ);
			invoiceLine.JI_Weight = weight;
			invoiceLine.JI_WeightUQ = weightUQ;
			invoiceLine.JI_NetWeight = netWeight;
			invoiceLine.JI_NetWeightUQ = netWeightUQ;
			if (!firstCustomsUQ.IsEmpty)
			{
				ResumeSettingAndSetValue(setterSuspender, JobComInvoiceLine.Schema.JI_InvoiceUQ, () => invoiceLine.JI_CustomsUnitQty = firstCustomsUQ);
			}
			ResumeSettingAndSetValue(setterSuspender, JobComInvoiceLine.Schema.JI_InvoiceUQ, () => invoiceLine.JI_CustomsQuantity = firstCustomsQty);
			if (!secondCustomsUQ.IsEmpty)
			{
				ResumeSettingAndSetValue(setterSuspender, JobComInvoiceLine.Schema.JI_InvoiceUQ, () => invoiceLine.JI_CustomsSecondUnitQty = secondCustomsUQ);
			}
			ResumeSettingAndSetValue(setterSuspender, JobComInvoiceLine.Schema.JI_InvoiceUQ, () => invoiceLine.JI_CustomsSecondQuantity = secondCustomQty);
			if (!thirdCustomsUQ.IsEmpty)
			{
				ResumeSettingAndSetValue(setterSuspender, JobComInvoiceLine.Schema.JI_InvoiceUQ, () => invoiceLine.JI_CustomsThirdUnitQty = thirdCustomsUQ);
			}
			ResumeSettingAndSetValue(setterSuspender, JobComInvoiceLine.Schema.JI_InvoiceUQ, () => invoiceLine.JI_CustomsThirdQuantity = thirdCustomsQty);
		}

		void ResumeSettingAndSetValue(SetterSuspender setterSuspender, string propertyName, Action setValue)
		{
			using (setterSuspender.ResumeSetting(propertyName))
			{
				setValue();
			}
		}

		public new BusinessObjectFactory Factory { get; }
	}
}
