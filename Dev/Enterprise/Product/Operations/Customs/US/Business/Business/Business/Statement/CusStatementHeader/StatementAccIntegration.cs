using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	class StatementAccIntegration
	{
		public StatementAccIntegration(LoggingInformation logger)
		{
			this.logger = logger;
		}
		readonly LoggingInformation logger;

		public AutoBillingResult Integrate(CusStatementHeaderIAccIntegrationDataProviderBase dataProvider)
		{
			var result = new AutoBillingResult();

			try
			{
				if (dataProvider.SupportIntegration)
				{
					if (logger != null)
					{
						logger.Log("Statement Saved: " + dataProvider.ReferenceID);
					}

					var postingInvoiceAction = dataProvider.Action;

					bool meetPrerequisiteCheck = dataProvider.HasCustomsCharges();
					if (meetPrerequisiteCheck && (postingInvoiceAction > 0 || dataProvider.ShouldMakePayment))
					{
						foreach (IAccInvoiceDataProvider line in dataProvider.InvDataProviders)
						{
							var customsJob = line.CustomsJob;
							if (customsJob != null && customsJob.Branch != null && customsJob.Branch.GB_GC != GlbCompany.CurrentCompany.PK)
							{
								result.Message = Res.GetString("456b1acd-6905-4599-b1a5-c9a123b86414", "While updating a statement {0}, Auto-Billing is attempted in the company {1} for a customs declaration {2} that belongs to the company {3}. System will not proceed and will not perform auto-billing.", dataProvider.ReferenceID, GlbCompany.CurrentCompany.GC_Code, line.CustomsJob.JobNumber, line.CustomsJob.Branch.Company.GC_Code);
								if (logger != null)
								{
									logger.Log(result.Message);
								}

								meetPrerequisiteCheck = false;
								break;
							}
						}
					}

					if (meetPrerequisiteCheck && postingInvoiceAction > 0)
					{
						if (logger != null)
						{
							logger.Log("Start Accounting Integration for Statement " + dataProvider.ReferenceID);
						}

						var invoicesResult = RaiseInvoicesIfRequired(dataProvider);
						if (invoicesResult.Message.Length > 0)
						{
							result.Message += "\r\nAR/AP Posting Processing Result\r\n-----------------------------------------------------";
						}
						result.SetResult(invoicesResult);
					}

					if (meetPrerequisiteCheck && dataProvider.ShouldMakePayment)
					{
						var paymentResult = MakePaymentIfRequired(dataProvider);

						if (logger != null)
						{
							logger.Log("Accounting Payment Integrated");
						}

						if (result.Message.Length > 0)
						{
							result.Message += "\r\n\r\n";
						}

						if (paymentResult.Message.Length > 0)
						{
							result.Message += "\r\nPayment Processing Result\r\n-------------------------------------------";
						}
						result.SetResult(paymentResult);
					}

					if (dataProvider.ShouldSaveAfterIntegration && result.WasSuccessful)
					{
						((IAccIntegrationDataProvider)dataProvider).Factory.Save();
						if (logger != null)
						{
							logger.Log("End Accounting Integration for Statement " + dataProvider.ReferenceID);
						}
					}
				}
			}
			catch (ZSaveConcurrencyException)
			{
				result.WasSuccessful = false;
				result.Message = IAccIntegrationDataProviderExtensionMethods.ConcurrencyExceptionUserExplanation;
				if (logger != null)
				{
					logger.Log(result.Message);
				}

				SendEmailOnException(dataProvider, result.Message);
			}
			catch (CustomsInvoiceRaiseException invoicingException)
			{
				result.WasSuccessful = false;
				result.Message = invoicingException.Message;
				if (logger != null)
				{
					logger.Log(result.Message);
				}

				SendEmailOnException(dataProvider, result.Message);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				result.WasSuccessful = false;
				var message = "There was a system error while attempting billing. Please check the record. See below for more information." + "\r\n" + e.Message + "\r\n" + e.StackTrace;
				result.Message = message;
				if (logger != null)
				{
					logger.Log(message);
				}

				ErrorReporter.ReportOnce("Invoicing broken for " + dataProvider.JobType, e);
				SendEmailOnException(dataProvider, result.Message);
			}

			return result;
		}

		void SendEmailOnException(CusStatementHeaderIAccIntegrationDataProviderBase dataProvider, string message)
		{
			if ((dataProvider.Action & ChargePosterBehaviours.SendEmail) == ChargePosterBehaviours.SendEmail)
			{
				dataProvider.SendEmailOnException(message);
			}
		}

		AutoBillingResult RaiseInvoicesIfRequired(CusStatementHeaderIAccIntegrationDataProviderBase dataProvider)
		{
			var result = new AutoBillingResult();

			var postingResult = new InvoicePostingAccountingIntegrator(logger).IntegrateIfNecessary(dataProvider);

			result.SetResult(postingResult);

			return result;
		}

		AutoBillingResult MakePaymentIfRequired(CusStatementHeaderIAccIntegrationDataProviderBase dataProvider)
		{
			var result = new AutoBillingResult();

			var creator = ObjectFactory.Get<ICustomsPaymentCreator>();

			var paymentCreationResult = creator.CreatePayment(dataProvider, dataProvider.APPaymentGroups);

			result.SetResult(paymentCreationResult);

			var message = paymentCreationResult != null ?
						(paymentCreationResult.WasSuccessful ? "Payment created successfully" : "Payment has errors: " + paymentCreationResult.ErrorMessage)
						: "";

			if (!string.IsNullOrEmpty(message) && logger != null)
			{
				logger.Log(message);
			}

			return result;
		}
	}
}
