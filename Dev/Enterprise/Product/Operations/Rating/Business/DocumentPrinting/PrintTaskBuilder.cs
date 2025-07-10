using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Rating.Business
{
	internal sealed class PrintTaskBuilder
	{
		public PrintTaskBuilder(RatingHeader header)
		{
			this.header = header ?? throw new ArgumentNullException(nameof(header));
			this.quote = header as Quote;
			this.supporter = ((IDocumentSupportable)header).DocumentSupporter;
		}

		public void BuildPrintTask(PrintTask printTask)
		{
			if (printTask == null)
			{
				throw new ArgumentNullException(nameof(printTask));
			}

			//We are deleting everything from print task to avoid duplicates as we create our own docs here.
			while (printTask.Count > 0)
			{
				printTask.Remove(printTask[0]);
			}

			var menuItem = (DocumentCommand)printTask.ParentMenuCommand;

			bool calledFromDocumentsMenu;

			if (menuItem == null)
			{
				calledFromDocumentsMenu = false;

				var filter = new DocumentZQuery(supporter.BusinessContext, Constants.MenuNameConstantsForPrinting.QuotationPack);
				menuItem = new BusinessObjectFactory().LoadTop1<DocumentCommand>(filter);   //Use new factory to avoid saving temporary menu item template pivot to database
				menuItem.Parent = header;
			}
			else
			{
				calledFromDocumentsMenu = true;
			}

			var documentPack = BuildDocumentPackCore(menuItem);

			if (documentPack != null)
			{
				if (!calledFromDocumentsMenu)
				{
					printTask.DeliveryInstructionsDefaultPK = menuItem.PK;
				}

				if (documentPack.Count > 0)
				{
					printTask.Add(documentPack);
				}
			}

			// Load OtherDocuments for NonSystemDocuments as per the client's request.
			// For SystemDocuments, need to update their Menus DocPack which will be done in another workitem.
			if (!menuItem.SU_IsSystemDefined)
			{
				var loader = new PrintTaskDocumentPackLoader(printTask, menuItem, null);
				loader.LoadChildCommands();
			}
		}

		public DocumentPack BuildDocumentPack(IStmMenuItem menuItem)
		{
			if (menuItem == null)
			{
				var filter = new DocumentZQuery(supporter.BusinessContext, Constants.MenuNameConstantsForPrinting.QuotationPack);
				menuItem = header.Factory.LoadTop1<DocumentCommand>(filter);
			}

			return BuildDocumentPackCore((DocumentCommand)menuItem);
		}

		DocumentPack BuildDocumentPackCore(DocumentCommand command)
		{
			return new RatingDocPackBuilder(command, header, BuildDocumentPack).DocPack;
		}

		void BuildDocumentPack(RatingDocPackBuilder builder)
		{
			if (quote != null && builder.Command.SU_MenuName == Constants.MenuNameConstantsForPrinting.QuotationPack)
			{
				BuildDocumentPackageFromAttachments(builder);
			}
			else
			{
				AddMenuItemToDocumentPackage(builder, builder.Command, "");
			}

			builder.AddEDocsForMainMenuItem();
		}

		void BuildDocumentPackageFromAttachments(RatingDocPackBuilder builder)
		{
			var documentTitleText = header.TH_OneTimeQuote ? Env.Registry.Rating.OneOffQuoteTitleText : Env.Registry.Rating.QuoteTitleText;
			var documentName = string.Format("{0} - {1}", documentTitleText, header.TH_QuoteNumber);
			EmailFormatter.UpdateDocumentName(documentName);

			var subject = EmailFormatter.GetEmailSubjectLine(RegistryEmailFormat.EmailSubjectFields);

			if (quote.QuotationClientAddress != null)
			{
				subject += " - " + quote.QuotationClientAddress.E2_CompanyNameTruncated;
			}

			builder.DocPack.EmailSubjectForConsolidateReports = subject;

			quote.SelectedPages.Sort(new RateAttachmentComparer());
			var trailingPagesAdded = false;

			foreach (RateAttachment attachment in quote.SelectedPages)
			{
				if (attachment.IsImage)
				{
					if (trailingPagesAdded)
					{
						// one trailing page template will cover all trailing pages, no need to add more.
						continue;
					}

					trailingPagesAdded = true;
				}

				var command = attachment.CurrentAttachment.Command;

				if (command != null)
				{
					command.Parent = header;
					AddMenuItemToDocumentPackage(builder, command, attachment.CurrentAttachment.TS_TemplateType);
					builder.AddEDocsForSubMenuItem(command);
				}
			}
		}

		EmailFormatter EmailFormatter
		{
			get
			{
				if (formatter == null)
				{
					formatter = new EmailFormatter(GlbStaff.CurrentUser);
				}
				return formatter;
			}
		}
		EmailFormatter formatter;

		EmailFormat RegistryEmailFormat
		{
			get
			{
				if (registryEmailFormat == null)
				{
					registryEmailFormat = DocumentsDataRegistry.Instance.EmailFormat.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				}
				return registryEmailFormat;
			}
		}
		EmailFormat registryEmailFormat;

		void AddMenuItemToDocumentPackage(RatingDocPackBuilder builder, IStmMenuItem menuItem, ZString templateType)
		{
			var templatePivot = header.Factory.Load<StmMenuTemplatePivot>(new ZQuery(StmMenuTemplatePivotSchema.SI_SU, menuItem.PK));
			Array.Sort(templatePivot, (p1, p2) => p1.SI_Index - p2.SI_Index);

			for (var i = 0; i < templatePivot.Length; i++)
			{
				AddDocumentToDocumentPackageByMagic(builder, templatePivot[i], templateType);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1121:DoNotIncludeColumnValuesOrNamesInErrorReporterKey", Justification = "Baseline")]
		void AddDocumentToDocumentPackageByMagic(RatingDocPackBuilder builder, StmMenuTemplatePivot pivot, ZString templateType)
		{
			DataContext templateDataContext;
			var template = pivot.Template;

			try
			{
				templateDataContext = (DataContext)Enum.Parse(typeof(DataContext), template.SO_DataContext);
			}
			catch (ArgumentException)
			{
				ErrorReporter.ReportOnce("Unknown Template.SO_DataContext: " + template.SO_DataContext);
				return;
			}

			switch (templateDataContext)
			{
				case DataContext.GenericFreightJob:
					AddDocumentBackedByGenericWrapper(builder, pivot);
					break;

				case DataContext.Quotation:
					AddDocumentBackedBySingleEntry(builder, pivot);
					break;

				case DataContext.Rating:
					if (templateType == RatingConstants.DocTemplateTypes.TableFormatPricingPage || string.IsNullOrEmpty(templateType))
					{
						AddDocumentBackedByMatchingEntries(builder, pivot, templateDataContext);
					}
					else if (quote != null && templateType == RatingConstants.DocTemplateTypes.StandardPricingPage)
					{
						AddDocumentBackedByQuoteEntries(builder, pivot, RateType.Forwarding);
					}
					break;

				case DataContext.ShippingRating:
					if (templateType == RatingConstants.DocTemplateTypes.TableFormatPricingPage || string.IsNullOrEmpty(templateType))
					{
						AddDocumentBackedByMatchingEntries(builder, pivot, templateDataContext);
					}
					else if (quote != null && templateType == RatingConstants.DocTemplateTypes.StandardPricingPage)
					{
						AddDocumentBackedByQuoteEntries(builder, pivot, RateType.Shipping);
					}
					break;

				case DataContext.CFSRating:
				case DataContext.ShippingDetentionRating:
				case DataContext.TransportRating:
				case DataContext.WarehouseRating:
					AddDocumentBackedByMatchingEntries(builder, pivot, templateDataContext);
					break;

				default:
					throw new InvalidOperationException("Unexpected data context: " + templateDataContext);
			}
		}

		void AddDocumentBackedByGenericWrapper(RatingDocPackBuilder builder, StmMenuTemplatePivot pivot)
		{
			builder.AddPages(pivot, DocumentWrapperFactory.GenerateGenericWrappers(DataContext.GenericFreightJob, header));
		}

		void AddDocumentBackedByQuoteEntries(RatingDocPackBuilder builder, StmMenuTemplatePivot pivot, RateType rateType)
		{
			var wrappers = new List<DocumentWrapper>();

			foreach (PricingPage page in GetPages(PricingPaginationStrategy.StandardStyle))
			{
				using (var e = page.RateEntries.GetEnumerator())
				{
					if (e.MoveNext() && (e.Current.RateType() & rateType) != 0)
					{
						wrappers.Add(DocumentWrapperFactory.CreateWrapper(DataContext.Quotation, page));
					}
				}
			}

			builder.AddPages(pivot, wrappers);
		}

		void AddDocumentBackedBySingleEntry(RatingDocPackBuilder builder, StmMenuTemplatePivot pivot)
		{
			PricingPage entry = null;

			if (quote != null)
			{
				if (quote.TH_OneTimeQuote)
				{
					entry = new PricingPage(quote.FirstMatchingEntryForOneOffQuote, quote.Factory, PricingPageStyle.Standard);
				}
				else
				{
					entry = GetPages(PricingPaginationStrategy.StandardStyle).Cast<PricingPage>().FirstOrDefault();
				}
			}
			else
			{
				entry = GetPages(PricingPaginationStrategy.LandscapeSimpleStyle).Cast<PricingPage>().FirstOrDefault();
			}

			if (entry != null)
			{
				builder.AddPage(pivot, DocumentWrapperFactory.CreateWrapper(DataContext.Quotation, entry));
			}
		}

		void AddDocumentBackedByMatchingEntries(RatingDocPackBuilder builder, StmMenuTemplatePivot pivot, DataContext context)
		{
			var wrappers = new List<DocumentWrapper>();
			var strategy = quote != null ? PricingPaginationStrategy.LandscapeComplexStyle : PricingPaginationStrategy.LandscapeSimpleStyle;
			if (builder.Command.SU_MenuName == Constants.MenuNameConstantsForPrinting.AgentPricingPage)
			{
				strategy = strategy | PricingPaginationStrategy.IsAgentPricingPage;
			}

			foreach (PricingPage page in GetPages(strategy))
			{
				if (page.FirstRateEntry != null)
				{
					IList<DataContext> dataContexts = RatingConstants.RateCategory.GetDataContexts(page.FirstRateEntry.TI_RateCategory);
					if (dataContexts.Contains(context))
					{
						wrappers.Add(DocumentWrapperFactory.CreateWrapper(DataContext.Quotation, page));
					}
				}
			}

			builder.AddPages(pivot, wrappers);
		}

		#region PricingPaginationStrategy Collections

		PricingPageCollection GetPages(PricingPaginationStrategy strategy)
		{
			return pricingPageCollections.GetOrAdd(strategy, () => CreateAndLoadPricingPageCollection(strategy));
		}

		PricingPageCollection CreateAndLoadPricingPageCollection(PricingPaginationStrategy strategy)
		{
			var result = new PricingPageCollection(header);
			result.Load(strategy);

			return result;
		}

		readonly Dictionary<PricingPaginationStrategy, PricingPageCollection> pricingPageCollections = new Dictionary<PricingPaginationStrategy, PricingPageCollection>();

		#endregion

		readonly RatingHeader header;
		readonly Quote quote;
		readonly DocumentSupporter supporter;
	}
}

