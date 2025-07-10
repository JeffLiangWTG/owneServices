using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	public static class JobInvoicingPlugInExtensions
	{
		#region IsIsCreditLimitCheckRequired

		public static ZBool IsCreditLimitCheckRequired(this IJobInvoicingPlugIn jobInvoicingPlugIn, ZString organizationType)
		{
			var creditLimitCheckRequired = false;
			if (jobInvoicingPlugIn != null && jobInvoicingPlugIn.InvoicingSupporter != null)
			{
				var jobType = jobInvoicingPlugIn.InvoicingSupporter.ConsumerType.Code;
				if (!string.IsNullOrEmpty(jobType))
				{
					var direction = GetJobDirection(jobInvoicingPlugIn.InvoicingSupporter);
					var transportMode = jobInvoicingPlugIn.InvoicingSupporter.TransportMode;
					var incoTerm = GetINCOTerm(jobInvoicingPlugIn.InvoicingSupporter);
					var paymentTerm = GetPaymentTerm(jobInvoicingPlugIn.InvoicingSupporter, jobType);

					var registry = GetOrganizationsEvaluatedForCreditControlFromRegistryOrCache(jobInvoicingPlugIn.Factory);
					creditLimitCheckRequired = !registry.Exists(jobType, direction, transportMode, incoTerm, paymentTerm, organizationType);
				}
			}
			return creditLimitCheckRequired;
		}

		static OrgsEvaluatedForCreditControlCollection GetOrganizationsEvaluatedForCreditControlFromRegistryOrCache(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(AccountingMasterFilesRegistry.OrganizationsEvaluatedForCreditControlCacheKey(), ReadFromRegistry, CacheStalenessPolicy.StaleOnFactorySave);

			OrgsEvaluatedForCreditControlCollection ReadFromRegistry()
				=> AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
		}

		#endregion

		#region JobDirection

		public static ZString GetJobDirection(IJobInvoicingSupporter jobInvoicingSupporter)
		{
			Directions directrion;

			if (jobInvoicingSupporter.IsImport)
			{
				directrion = Directions.Import;
			}
			else if (jobInvoicingSupporter.IsExport)
			{
				directrion = Directions.Export;
			}
			else if (jobInvoicingSupporter.IsDomestic)
			{
				directrion = Directions.Domestic;
			}
			else
			{
				directrion = Directions.Unknown;
			}

			return ImportExportHelper.GetDirectionCode(directrion);
		}

		#endregion

		#region INCOTerm

		public static ZString GetINCOTermCode(this IJobInvoicingPlugIn jobInvoicingPlugIn)
		{
			var code = ZString.Empty;
			if (jobInvoicingPlugIn != null && jobInvoicingPlugIn.InvoicingSupporter != null)
			{
				code = GetINCOTerm(jobInvoicingPlugIn.InvoicingSupporter);
			}
			return code;
		}

		public static ZString GetINCOTermDescription(this IJobInvoicingPlugIn jobInvoicingPlugIn, BusinessObjectFactory factory)
		{
			var description = ZString.Empty;
			if (jobInvoicingPlugIn != null && jobInvoicingPlugIn.InvoicingSupporter != null)
			{
				var paymentTermInfo = GetRevenuePaymentTermInfo(jobInvoicingPlugIn.InvoicingSupporter);
				if (paymentTermInfo != null)
				{
					description = GetRelatedCodeDescriptionPairList(paymentTermInfo.InfoType, factory).GetDescriptionFromCode(GetINCOTerm(jobInvoicingPlugIn.InvoicingSupporter));
				}
			}
			return description;
		}

		static ZString GetINCOTerm(IJobInvoicingSupporter jobInvoicingSupporter)
		{
			return GetRevenuePaymentTermInfo(jobInvoicingSupporter)?.Value ?? ZString.Empty;
		}

		static PaymentTermInfo GetRevenuePaymentTermInfo(IJobInvoicingSupporter jobInvoicingSupporter)
		{
			return jobInvoicingSupporter.PaymentTerm.GetPaymentTermInfo(CostSell.Revenue);
		}

		static CodeDescriptionPairList GetRelatedCodeDescriptionPairList(PaymentTermType infoType, BusinessObjectFactory factory)
		{
			var result = new CodeDescriptionPairList();
			switch (infoType)
			{
				case PaymentTermType.Incoterm:
					result = factory.GetCachedValue<IncoTermsCodeDescriptionPairList>();
					break;
				case PaymentTermType.PrepaidCollect:
					result = factory.GetCachedCodeDescriptionPairList(OLookUpEditType.PaymentType);
					break;
				case PaymentTermType.DomesticPaymentTerm:
					result = factory.GetCachedCodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms);
					break;
				default:
					break;
			}
			return result;
		}
		#endregion

		#region PaymentTerm

		static ZString GetPaymentTerm(IJobInvoicingSupporter jobInvoicingSupporter, string jobType)
		{
			var chargeCodeGroup = string.Empty;
			if (jobType == JobInvoicingConsumerTypes.Shipment.Code
				|| jobType == JobInvoicingConsumerTypes.QuotedBooking.Code
				|| jobType == JobInvoicingConsumerTypes.Brokerage.Code)
			{
				chargeCodeGroup = ChargeCodeGroupList.Codes.Freight;
			}

			return jobInvoicingSupporter.PaymentTerm.GetPrepaidCollect(CostSell.Revenue, chargeCodeGroup);
		}

		#endregion

		#region Measures

		public static ZDecimal GrossWeightInKilos(this IJobInvoicingPlugIn plugInData)
		{
			if (plugInData != null)
			{
				var invoicingSupporter = plugInData.InvoicingSupporter;
				var actualWeightUnit = invoicingSupporter.ActualWeightUnit;
				var isValidWeightUnit = plugInData.Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight).ContainsCode(actualWeightUnit);
				if (isValidWeightUnit)
				{
					return Weight.Convert(invoicingSupporter.ActualWeight, actualWeightUnit, Weight.Kilograms);
				}
			}

			return 0m;
		}

		public static ZDecimal GrossVolumeInCubicMeters(this IJobInvoicingPlugIn plugInData)
		{
			if (plugInData != null)
			{
				var invoicingSupporter = plugInData.InvoicingSupporter;
				var actualVolumeUnit = invoicingSupporter.ActualVolumeUnit;
				var isValidVolumeUnit = plugInData.Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume).ContainsCode(actualVolumeUnit);
				if (isValidVolumeUnit)
				{
					return Volume.Convert(invoicingSupporter.ActualVolume, actualVolumeUnit, Volume.CubicMetres);
				}
			}

			return 0m;
		}

		#endregion
	}
}
