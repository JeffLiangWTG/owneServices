using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class InvoicePostingAccountingIntegrator
	{
		public InvoicePostingAccountingIntegrator()
		{
		}

		public InvoicePostingAccountingIntegrator(LoggingInformation logger)
		{
			this.logger = logger;
		}

		readonly LoggingInformation logger;

		public bool HasExceptionDuringIntegration { get; private set; }

		public IAutoBillingResult IntegrateIfNecessary(IAccIntegrationDataProvider integrationDataProvider)
		{
			AutoBillingResult result = new AutoBillingResult();

			if (integrationDataProvider.SupportIntegration)
			{
				try
				{
					if (integrationDataProvider.Company.PK != GlbCompany.CurrentCompany.PK)
					{
						result.Message = GetAutoBillingCompanyMismatchText(integrationDataProvider);
						logger?.Log(result.Message);
					}
					else if (integrationDataProvider.Action > 0)
					{
						var hasCustomsCharges = integrationDataProvider.HasCustomsCharges();
						if (hasCustomsCharges)
						{
							var chargePosterCreator = new CustomsDisbursementChargePosterCreator();

							var poster = chargePosterCreator.GetNewChargePoster(integrationDataProvider.Action, integrationDataProvider.DisbursementChargeCodes);
							var postingResult = poster.RaiseInvoices(integrationDataProvider);
							result.SetResult(postingResult);

							integrationDataProvider.OnIntegrated();

							integrationDataProvider.Factory.Save();
						}
					}
				}
				catch (ZSaveConcurrencyException)
				{
					result.WasSuccessful = false;
					result.Message = IAccIntegrationDataProviderExtensionMethods.ConcurrencyExceptionUserExplanation;
					logger?.Log(result.Message);
					HasExceptionDuringIntegration = true;
				}
				catch (ZSaveException)
				{
					result.WasSuccessful = false;
					result.Message = IAccIntegrationDataProviderExtensionMethods.SaveExceptionUserExplanation;
					logger?.Log(result.Message);
					HasExceptionDuringIntegration = true;
				}
				catch (CustomsInvoiceRaiseException invoicingException)
				{
					result.WasSuccessful = false;
					result.Message = invoicingException.Message;
					logger?.Log(result.Message);
					HasExceptionDuringIntegration = true;
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					result.WasSuccessful = false;
					result.Message = GetBillingFailureMessage(e);
					logger?.Log(result.Message);
					ErrorReporter.ReportOnce("Invoicing broken for " + integrationDataProvider.JobType, e);
					HasExceptionDuringIntegration = true;
				}

				if (HasExceptionDuringIntegration)
				{
					integrationDataProvider.SendEmailOnException(result.Message);
				}
			}

			return result;
		}

		public static ZString GetBillingFailureMessage(Exception e)
		{
			return Res.GetString("8feb5e9a-453c-4d66-acc5-0f7139fb6c44", "There was a system error while attempting billing. Please check the record. See below for more information.\r\n{0}\r\n{1}", e.Message, e.StackTrace);
		}

		public static ZString GetAutoBillingCompanyMismatchText(IAccIntegrationDataProvider integrationDataProvider)
		{
			return Res.GetString("536b1acd-6905-4599-b1a5-c9a123b86414", "Auto-Billing is attempted in the company '{0}' for a {1} {2} that belongs to the company {3}. System will not proceed and will not perform auto-billing.", GlbCompany.CurrentCompany.GC_Code, integrationDataProvider.JobType, integrationDataProvider.ReferenceID, integrationDataProvider.Company.GC_Code);
		}
	}
}
