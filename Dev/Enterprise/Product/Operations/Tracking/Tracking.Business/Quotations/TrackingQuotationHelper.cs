using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Utilities.Environment;

namespace Enterprise.Tracking.Business
{
	public class TrackingQuotationHelper
	{
		public TrackingQuotationHelper(BusinessObjectFactory factory, OrgContact contact, QuotedBooking spotQuote)
		{
			this.Factory = factory;
			this.Contact = contact;
			this.fSpotQuote = spotQuote;
			if (this.fSpotQuote.Quote != null)
			{
				this.fSpotQuote.Quote.ShowApprovalDialog += (sender, e) => e.Cancel = true;
			}
		}

		readonly BusinessObjectFactory Factory;
		readonly OrgContact Contact;
		readonly QuotedBooking fSpotQuote;

		public QuotedBooking SpotQuote
		{
			get { return fSpotQuote; }
		}

		public List<QuotedBooking> ComparisonQuotes
		{
			get { return comparisonQuotes ?? (comparisonQuotes = new List<QuotedBooking>()); }
		}
		List<QuotedBooking> comparisonQuotes;

		#region Job

		protected Job Job
		{
			get
			{
				if (SpotQuote != null && SpotQuote.Job == null)
				{
					Job.Loader jobloader = new Job.Loader(SpotQuote);
					jobloader.TryLoadOrCreate();
				}

				return SpotQuote.Job as Job;
			}
		}

		Quote Quote
		{
			get { return SpotQuote.Quote; }
		}

		public ZDecimal TotalSellAmount
		{
			get { return JobCharges.Sum(c => c.JR_OSSellAmt); }
		}

		public bool HasFreightCharges
		{
			get { return Quote.ValidOneOffQuoteChargesExist; }
		}

		public IReadOnlyList<Charge> JobCharges
		{
			get
			{
				using (Quote.Company.PK != GlbCompany.CurrentCompany.PK ? new WebLoginBranch(Quote.Company.Branches[0]) : null)
				{
					return SpotQuote == null || SpotQuote.Job == null ? Array.Empty<Charge>() : LoadCharges();
				}
			}
		}

		Charge[] LoadCharges()
		{
			var charges = new ChargeCollection(Job);
			charges.Load(new ZQuery(JobChargeSchema.JR_JH, Job.PK));
			return charges.ToArray<Charge>();
		}

		#endregion

		#region OnQuoteUpdated

		protected void OnQuoteUpdated(EventArgs e)
		{
			if (QuoteUpdated != null)
			{
				QuoteUpdated(this, e);
			}
		}

		public event EventHandler QuoteUpdated;

		#endregion

		#region Properties

		public ZGuid QuoteKey
		{
			get { return Quote.PK; }
		}

		public ZString QuotationPrefix
		{
			get { return Quote.QuoteNumberWithoutAmendmentSuffix; }
		}

		#endregion

		#region Preview / Finalise

		public bool Preview()
		{
			DeleteUnsavedExistingJob();

			SpotQuote.RunPreSaveValidation();
			if (SpotQuote.HasErrors)
			{
				return false;
			}

			var cachedIncoTerms = SpotQuote.PaymentTerms;
			var cachedTransportMode = SpotQuote.TransportMode;
			var cachedServiceLevel = SpotQuote.ServiceLevel;
			var cachedIsCompareServiceLevel = SpotQuote.IsCompareServiceLevel;
			var cachedIsCompareMode = SpotQuote.IsCompareMode;
			var cachedCarrierPK = SpotQuote.OH_Carrier;

			bool result = Quote.TH_IsLocked;
			var spotQuoteHasErrors = false;

			if (!result)
			{
				if (Quote.Company.PK != GlbCompany.CurrentCompany.PK)
				{
					if (SpotQuote.ComparisonQuoteResults.Count > 0)
					{
						var selectedQuote = SpotQuote.ComparisonQuoteResults.Selected;
						if (selectedQuote != null)
						{
							if (SpotQuote.Job != null)
							{
								SpotQuote.Job.Delete();
							}
						}
					}

					using (WebLoginBranch quoteBranch = new WebLoginBranch(Quote.Company.Branches[0]))
					{
						result = PreviewCore();
						spotQuoteHasErrors = SetCachedProperties(cachedIncoTerms, cachedTransportMode, cachedServiceLevel, cachedIsCompareServiceLevel, cachedIsCompareMode, cachedCarrierPK);
					}
				}
				else
				{
					result = PreviewCore();
					spotQuoteHasErrors = SetCachedProperties(cachedIncoTerms, cachedTransportMode, cachedServiceLevel, cachedIsCompareServiceLevel, cachedIsCompareMode, cachedCarrierPK);
				}
			}

			if (spotQuoteHasErrors)
			{
				return false;
			}

			if (SpotQuote.ComparisonQuoteResults.Count == 1 && SpotQuote.ComparisonQuoteResults.Selected == null)
			{
				SpotQuote.ComparisonQuoteResults[0].Selected = true;

				if (SpotQuote.IsCompareMode && SpotQuote.SelectedComparisonModes.Count > 0)
				{
					result = Preview();
				}
			}

			if (SpotQuote.ComparisonQuoteResults.Count == 1 || SpotQuote.ComparisonQuoteResults.Selected != null)
			{
				return result;
			}

			return ChargesAreCorrectOrIgnored && SpotQuote.ComparisonQuoteResults.Count == 0 && SpotQuote.SelectedComparisonModes.Count <= 1 && SpotQuote.SelectedComparisonServiceLevels.Count <= 1;
		}

		void DeleteUnsavedExistingJob()
		{
			JobHeader job = SpotQuote.Job;
			if (job != null && !job.IsInDatabase)
			{
				job.DisposeAndDeleteNew();
			}
		}

		bool SetCachedProperties(ZString cachedIncoTerms, ZString cachedTransportMode, ZString cachedServiceLevel, ZBool cachedIsCompareServiceLevel, ZBool cachedIsCompareMode, ZGuid cachedCarrierPK)
		{
			SpotQuote.ComparisonQuoteResults.CleanZeroChargesAndResults();
			SpotQuote.IsCompareMode = cachedIsCompareMode;
			SpotQuote.IsCompareServiceLevel = cachedIsCompareServiceLevel;
			SpotQuote.TransportMode = cachedTransportMode;
			SpotQuote.ServiceLevel = cachedServiceLevel;
			SpotQuote.OH_Carrier = cachedCarrierPK;
			SpotQuote.PaymentTerms = cachedIncoTerms;
			SpotQuote.RunPreSaveValidation();
			return SpotQuote.HasErrors;
		}

		void ClearComparisonQuotes()
		{
			SpotQuote.ComparisonQuoteResults.RemoveAndDeleteAll();
		}

		bool ShowLocalCurrency
		{
			get
			{
				return DocumentsDataRegistry.Instance.ShowLocalCurrencyonSpotQuotePricingPage.Value;
			}
		}

		bool PreviewCore()
		{
			if (SpotQuote.ComparisonQuoteResults.Count > 0)
			{
				if (SpotQuote.ComparisonQuoteResults.Selected != null)
				{
					return PreviewCore(SpotQuote.ComparisonQuoteResults.Selected);
				}
			}

			bool previewResult = false;
			ClearComparisonQuotes();

			List<ZString> modes = new List<ZString>();
			if (SpotQuote.Mode.IsEmpty && SpotQuote.IsCompareMode && SpotQuote.SelectedComparisonModes.Count > 0)
			{
				foreach (ICodeDescription transportMode in SpotQuote.SelectedComparisonModes)
				{
					modes.Add(transportMode.Code);
				}
			}
			else
			{
				modes.Add(SpotQuote.Mode);
			}

			List<ZString> serviceLevels = new List<ZString>();
			if (SpotQuote.ServiceLevel.IsEmpty && SpotQuote.IsCompareServiceLevel && SpotQuote.SelectedComparisonServiceLevels.Count > 0)
			{
				foreach (ICodeDescription serviceLevel in SpotQuote.SelectedComparisonServiceLevels)
				{
					serviceLevels.Add(serviceLevel.Code);
				}
			}
			else
			{
				serviceLevels.Add(SpotQuote.ServiceLevel);
			}

			foreach (ZString transportMode in modes)
			{
				foreach (ZString serviceLevel in serviceLevels)
				{
					SpotQuote.ComparisonQuoteResults.Add(new ComparisonQuoteResult(Factory, transportMode, serviceLevel, ZGuid.Empty, ShowLocalCurrency));
				}
			}

			ComparisonQuoteResult comparisonQuoteToPreview;
			ComparisonQuoteResult newComparisonQuote;
			do
			{
				PossibleMatches.Clear();
				comparisonQuoteToPreview = SpotQuote.ComparisonQuoteResults.GetNextUnprocessedItem();
				if (comparisonQuoteToPreview != null)
				{
					ZString cachedServiceLevel = comparisonQuoteToPreview.ServiceLevel;
					previewResult = PreviewCore(comparisonQuoteToPreview);

					if (!previewResult && PossibleMatches.Count == 0)
					{
						SpotQuote.ComparisonQuoteResults.Remove(comparisonQuoteToPreview);
						continue;
					}

					if (PossibleMatches.Count > 0)
					{
						if (cachedServiceLevel.IsEmpty || cachedServiceLevel == PossibleMatches[0].TI_RS_NKServiceLevel_NI)
						{
							newComparisonQuote = new ComparisonQuoteResult(Factory, comparisonQuoteToPreview.Mode, PossibleMatches[0].TI_RS_NKServiceLevel_NI, PossibleMatches[0].TI_OH_TransportProvider, ShowLocalCurrency);
							if (!SpotQuote.ComparisonQuoteResults.Contains(newComparisonQuote))
							{
								comparisonQuoteToPreview.ServiceLevel = PossibleMatches[0].TI_RS_NKServiceLevel_NI;
								comparisonQuoteToPreview.CarrierPK = PossibleMatches[0].TI_OH_TransportProvider;
							}
							else
							{
								comparisonQuoteToPreview.IsProcessed = ZBool.True;
							}
						}
						else
						{
							comparisonQuoteToPreview.IsProcessed = ZBool.True;
						}
						for (int i = 1; i < PossibleMatches.Count; i++)
						{
							if (cachedServiceLevel.IsEmpty || cachedServiceLevel == PossibleMatches[i].TI_RS_NKServiceLevel_NI)
							{
								newComparisonQuote = new ComparisonQuoteResult(Factory, comparisonQuoteToPreview.Mode, PossibleMatches[i].TI_RS_NKServiceLevel_NI, PossibleMatches[i].TI_OH_TransportProvider, ShowLocalCurrency);
								if (!SpotQuote.ComparisonQuoteResults.Contains(newComparisonQuote))
								{
									SpotQuote.ComparisonQuoteResults.Add(newComparisonQuote);
								}
							}
						}
					}
					else
					{
						comparisonQuoteToPreview.IsProcessed = ZBool.True;
					}
					if (comparisonQuoteToPreview.IsProcessed)
					{
						foreach (Charge chargeItem in JobCharges)
						{
							comparisonQuoteToPreview.Charges.Add(chargeItem);
						}
					}
				}
			}
			while (comparisonQuoteToPreview != null);
			return previewResult;
		}

		bool PreviewCore(ComparisonQuoteResult comparisonQuote)
		{
			bool result = false;

			SpotQuote.IsInComparisonMode = true;

			ZDecimal cachedWeight = SpotQuote.Weight;
			ZDecimal cachedVolume = SpotQuote.Volume;
			ZString cachedIncoTerms = SpotQuote.PaymentTerms;

			SpotQuote.Mode = comparisonQuote.Mode;
			SpotQuote.ServiceLevel = comparisonQuote.ServiceLevel;
			SpotQuote.OH_Carrier = comparisonQuote.CarrierPK;
			SpotQuote.PaymentTerms = cachedIncoTerms;

			if (SpotQuote != null && SpotQuote.Job != null)
			{
				SpotQuote.Job.MarkAsInactive();
			}
			SetDefaultValuesForJobIfNotAlreadySet();

			SpotQuote.IsInComparisonMode = false;

			SpotQuote.RunPreSaveValidation();

			if (!SpotQuote.HasErrors)
			{
				result = AutoRate(false);
			}

			SpotQuote.IsInComparisonMode = true;

			SpotQuote.Weight = cachedWeight;
			SpotQuote.Volume = cachedVolume;
			SpotQuote.PaymentTerms = cachedIncoTerms;

			SpotQuote.IsInComparisonMode = false;

			return result;
		}

		public bool Finalise()
		{
			bool result = false;

			if (SpotQuote.ComparisonQuoteResults.Count > 0)
			{
				if (SpotQuote.ComparisonQuoteResults.Selected != null)
				{
					SpotQuote.ComparisonQuoteResults.Selected.PopulateQuote(SpotQuote);
				}
			}

			if (Quote.Company.PK != GlbCompany.CurrentCompany.PK)
			{
				using (WebLoginBranch quoteBranch = new WebLoginBranch(Quote.Company.Branches[0]))
				{
					result = FinaliseCore();
				}
			}
			else
			{
				result = FinaliseCore();
			}

			return result;
		}

		bool FinaliseCore()
		{
			bool result = false;

			SetDefaultValuesForJobIfNotAlreadySet();
			Quote.RunPreSaveValidation();

			if (!Quote.HasErrors)
			{
				result = AutoRate(true);

				if (result)
				{
					Deliver();
					SendEmailToSales();
				}
			}

			return result;
		}

		#endregion

		#region AutoRate

		public bool ChargesAreCorrectOrIgnored
		{
			get { return WebDataRegistry.Instance.SaveQuotesWithoutRates.Value || Quote.HasNonZeroSellAmt; }
		}

		bool AutoRate(bool finalise)
		{
			bool result = false;
			SetDefaultValuesForJobIfNotAlreadySet();

			if (!Quote.HasErrors && !Quote.TH_IsLocked || finalise)
			{
				if (SpotQuote.HasChanges || TotalSellAmount == 0)
				{
					RateCharges();
				}

				if (ChargesAreCorrectOrIgnored)
				{
					if (finalise)
					{
						Quote.TH_IsLocked = true;
					}

					OnQuoteUpdated(EventArgs.Empty);

					if (SpotQuote.HasChanges)
					{
						Quote.Factory.Save();
					}

					result = true;
				}
			}

			return result || Quote.TH_IsLocked;
		}

		void RateCharges()
		{
			PossibleMatches.Clear();

			if (!Quote.TH_IsLocked)
			{
				try
				{
					var runner = new AutoRatingRunner(SpotQuote, new RatingContext());
					AutoRateInfoCollection results = null;

					if (Job.LocalCharges == null || Job.LocalCharges.IsMiscellaneous)
					{
						Job.LocalChargesPK = Contact.ParentOrg.PK;
					}

					var adaptersProvider = ((IRatingSupporter)SpotQuote).AdaptersProvider;
					var ratingAdapters = adaptersProvider.GetAdapters(null, AutoRateOptions.AutorateRevenue).ToArray();
					var interactor = new LoggerDecorator();
					using (_Rating.Start(interactor))
					using (_Rating.StartSell())
					{
						results = runner.RetrieveAllCharges(ratingAdapters, adaptersProvider, CostSell.Revenue);
					}

					if (runner.PossibleMathes.Any() && !results.Any(r => r.IsFreight))
					{
						PossibleMatches.AddRange(runner.PossibleMathes);
					}
					else if (results.Count > 0 && Job != null && !Job.IsClosed)
					{
						var strategy = new AutoRateInvoicingStrategy(SpotQuote, Job);
						strategy.AddAutoRates(interactor, results, CostSell.Revenue, ratingAdapters.Select(a => a.OperationalJobCode).ToArray());
						Job.JH_RatingHasBeenRun = true;
						Job.ReOpenJobStatus();
					}
				}
				catch (AutoRater.NoExchangeRateException e)
				{
					if (SpotQuote.Job != null)
					{
						var currencyPKs = e.Currencies.Select(c => c.PK).ToArray();
						var currencies = Factory.Load<RefCurrency>(new ZQuery(RefCurrencySchema.PK, currencyPKs));
						foreach (var currency in currencies)
						{
							Job.AddCurrency(currency, ExchangeRateValidLedgerEnum.AR);
						}
					}
				}
			}
		}

		protected List<RateEntry> PossibleMatches
		{
			get { return fPossibleMatches ?? (fPossibleMatches = new List<RateEntry>()); }
		}
		List<RateEntry> fPossibleMatches;

		void SetDefaultValuesForJobIfNotAlreadySet()
		{
			if (Job != null)
			{
				if (Job.PlugInData == null)
				{
					Job.PlugInData = SpotQuote;
				}

				if (Job.LocalChargesPK != SpotQuote.Client.PK)
				{
					Job.LocalChargesPK = SpotQuote.Client.PK;
				}

				if (Job.JH_GE.IsEmpty)
				{
					Job.SetDefaultDepartment(SpotQuote);
				}

				if (Job.JH_GB.IsEmpty)
				{
					Job.SetDefaultBranch(SpotQuote);

					if (Job.JH_GB.IsEmpty)
					{
						Job.JH_GB = GlbBranch.CurrentBranch.PK;
					}
				}
			}
		}

		#endregion

		#region QuotationDocumentPack

		public DocumentPack QuotationDocumentPack
		{
			get
			{
				var command = DocumentCommand.GetDocumentCommand(Factory, Quote, Core.Constants.MenuNameConstantsForPrinting.QuotationPack);
				var documentSupporter = (RatingHeaderDocumentSupporter)((IDocumentSupportable)Quote).DocumentSupporter;

				return documentSupporter.BuildDocumentPack(command);
			}
		}

		#endregion

		#region Deliver

		void Deliver()
		{
			Quote.SetFinalMode += new EventHandler(ParentQuote_SetFinalMode);
			Quote.SpotQuoteChargesIncorrect += new CancelEventHandler(Quote_SpotQuoteChargesIncorrect);

			var command = DocumentCommand.GetDocumentCommand(Factory, Quote, Core.Constants.MenuNameConstantsForPrinting.QuotationPack);
			if (command == null)
			{
				return;
			}
			command.Parent = Quote;
			RatingHeaderDocumentSupporter docSupporter = (RatingHeaderDocumentSupporter)((IDocumentSupportable)Quote).DocumentSupporter;

			using (PrintTask task = docSupporter.BuildPrintTask(command))
			{
				if (task != null && Contact != null)
				{
					DocDeliveryContact deliveryContact = new DocAutoDelivery().GetDeliveryDetailsForContact(Contact);
					deliveryContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
					deliveryContact.AttachmentType = OrgConstants.AttachmentType.PDF;

					DeliveryInstructions deliveryInstr = new DeliveryInstructions();
					deliveryInstr.Destination = DeliveryInstructionDestination.Auto;
					deliveryInstr.Recipients.Add(deliveryContact);

					task.Run(deliveryInstr);
				}
			}
		}

		void Quote_SpotQuoteChargesIncorrect(object sender, CancelEventArgs e)
		{
			e.Cancel = !WebDataRegistry.Instance.SaveQuotesWithoutRates.Value;
		}

		#endregion

		#region SendEmailToSales

		void SendEmailToSales()
		{
			if (Contact != null)
			{
				RatingEmailDef webQuoteEmail = RatingEmailDef.WebOneOffQuote(((IRatingSupporter)SpotQuote).AdaptersProvider.GetAdapters(null, AutoRateOptions.AutorateRevenue).FirstOrDefault(), Quote, Contact);

				if (webQuoteEmail != null)
				{
					webQuoteEmail.Send();
				}
			}
		}

		#endregion

		#region SalesRepEmailLink

		public StringCollection SalesRepEmailAddresses
		{
			get
			{
				if (fSalesRepEmailAddresses == null)
				{
					var emailDef = RatingEmailDef.WebOneOffQuote(((IRatingSupporter)SpotQuote).AdaptersProvider.GetAdapters(null, AutoRateOptions.AutorateRevenue).FirstOrDefault(), Quote, Contact);
					fSalesRepEmailAddresses = emailDef.Recipients.ToStringCollection();
				}
				return fSalesRepEmailAddresses;
			}
		}
		StringCollection fSalesRepEmailAddresses;

		#endregion

		#region SetFinalMode

		void ParentQuote_SetFinalMode(object sender, EventArgs e)
		{
			((Quote.SetFinalModeArgs)e).Result = Quote.TH_IsLocked;
		}

		#endregion
	}
}
