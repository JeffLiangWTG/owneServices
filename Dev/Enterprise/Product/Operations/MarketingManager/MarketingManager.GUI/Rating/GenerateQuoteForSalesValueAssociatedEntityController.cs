using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Integration.Rating;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public class GenerateQuoteForSalesValueAssociatedEntityController : IGenerateQuoteForSalesValueAssociatedEntityController
	{
		#region New / Static

		public static IGenerateQuoteForSalesValueAssociatedEntityController New()
		{
			return ObjectFactory.Get<IGenerateQuoteForSalesValueAssociatedEntityController>();
		}

		protected GenerateQuoteForSalesValueAssociatedEntityController()
		{
		}

		#endregion

		#region Properties

		public ZForm ParentModalForm
		{
			get;
			set;
		}

		#endregion

		#region Execute

		public void Execute(ISalesValueAssociatedEntity entity)
		{
			var allTradeDetails =
				from SalesHeader salesHeader in entity.ActualAndProspectiveSalesHeaderCollection
				from EntitySalesWrapper sales in salesHeader.EntitySalesCollectionProductView
				from EntityTradeDetailWrapper tradeDetail in sales.EntityTradeDetailsCollection
				select tradeDetail;

			Execute(entity, allTradeDetails);
		}

		public void Execute(ISalesValueAssociatedEntity entity, IEnumerable<OrgTradeDetail> tradeDetails)
		{
			var settings = GetNewGenerateQuoteSettings(entity, tradeDetails);
			var dialogResult = ShowGenerateQuoteSettingsForm(settings);
			if (dialogResult == DialogResult.Cancel)
			{
				return;
			}
			else
			{
				ShowNewQuoteForm(entity, settings);
			}
		}

		#endregion

		#region GenerateQuoteSettings

		internal static GenerateQuoteSettings GetNewGenerateQuoteSettings(ISalesValueAssociatedEntity entity, IEnumerable<OrgTradeDetail> tradeDetails)
		{
			var quoteSelectionItemCollection = new QuoteSelectionItemCollection();
			var entityAsRelatableActivity = entity as IRelatableActivity;
			if (entityAsRelatableActivity != null)
			{
				foreach (var quote in RecursivelyGetAllQuotationsForCurrentCompany(entityAsRelatableActivity))
				{
					quoteSelectionItemCollection.AddNew(quote);
				}
			}

			var tradeDetailSelectionItemCollection = new TradeDetailSelectionItemCollection();
			foreach (var tradeDetail in tradeDetails)
			{
				tradeDetailSelectionItemCollection.AddNew(tradeDetail).Selected = true;
			}

			return new GenerateQuoteSettings(quoteSelectionItemCollection, tradeDetailSelectionItemCollection);
		}

		static IEnumerable<IRelatableActivity> RecursivelyGetAllQuotationsForCurrentCompany(IRelatableActivity parent)
		{
			foreach (var quote in parent.RelatedChildActivityPivotCollection.Activities
						.Where(x => x.ActivityType == RelatableActivityTypeList.Codes.Quotations)
						.Where(x => (ZGuid)((BusinessObject)x)[RatingHeaderSchema.TH_GC] == GlbCompany.CurrentCompany.PK))
			{
				yield return quote;

				foreach (var grandChildQuote in RecursivelyGetAllQuotationsForCurrentCompany(quote))
				{
					yield return grandChildQuote;
				}
			}
		}

		#endregion

		#region ShowGenerateQuoteSettingsForm

		DialogResult ShowGenerateQuoteSettingsForm(GenerateQuoteSettings settings)
		{
			if (settings.QuoteSelectionItems.Count > 0 || settings.TradeDetailSelectionItems.Count > 0)
			{
				var form = new GenerateQuoteSettingsForm(settings);
				ZFormModaliser.ShowDialogAndDispose(form);

				return form.DialogResult;
			}

			return DialogResult.OK;
		}

		#endregion

		#region ShowNewQuoteForm

		void ShowNewQuoteForm(ISalesValueAssociatedEntity parentEntity, GenerateQuoteSettings settings)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Quotations);
			controller.SetFormsModalTo(ParentModalForm);
			var quote = GetNewQuote(controller, parentEntity, settings);
			foreach (var tradeDetail in settings.SelectedTradeDetails)
			{
				((ISupportTradeDetailImporting)quote).ImportTradeDetailData(tradeDetail);
			}

			var form = (ZForm)controller.ShowFormForNewEntity(quote);
			if (form != null && parentEntity != null)
			{
				FormClosedEventHandler formClosedHandler = null;
				formClosedHandler = (sender, e) =>
				{
					form.FormClosed -= formClosedHandler;

					if (((BusinessObject)quote).IsInDatabase)
					{
						PromptUserToResynchronise(parentEntity, quote);
					}
				};

				form.FormClosed += formClosedHandler;
			}
		}

		internal static IRelatableActivity GetNewQuote(ZController quoteController, ISalesValueAssociatedEntity parentEntity, GenerateQuoteSettings settings)
		{
			ZControllerInternals controllerInternals = quoteController;

			if (settings.ShouldCreateAmendment)
			{
				var quoteToAmend = settings.GetQuoteToAmend();
				if (quoteToAmend != null)
				{
					var quoteToAmendInLocalFactory = quoteController.Factory.ImportFromAnotherFactory((BusinessObject)quoteToAmend) as IQuote;
					if (quoteToAmendInLocalFactory != null)
					{
						var quotationsController = (IQuotationsController)quoteController;
						try
						{
							if (quotationsController.CheckIsCopyAllowed((BusinessObject)quoteToAmendInLocalFactory))
							{
								quoteToAmendInLocalFactory.SameClientCopy = true;
								quoteToAmendInLocalFactory.AmendmentCopy = true;
								return (IRelatableActivity)((ITemplateCopyable)quoteToAmendInLocalFactory).TemplateCopy();
							}
						}
						finally
						{
							quoteToAmendInLocalFactory.SameClientCopy = false;
							quoteToAmendInLocalFactory.AmendmentCopy = false;
						}
					}
				}
			}

			var quote = (IRelatableActivity)controllerInternals.GetNewBusinessEntityInLocalFactory();
			var parentEntityAsRelatableActivity = parentEntity as IRelatableActivity;
			if (parentEntityAsRelatableActivity != null)
			{
				quote.RelatedParentActivityPivotCollection.AddActivity(parentEntityAsRelatableActivity);

				var importDeciderFactory = new ImportRelatedActivityPromptUserDeciderFactory((ZForm)quoteController.ParentModalForm, CreateQuotationCaption);
				SalesRelationTree.DoImportParentRelatedActivityInfoOnNewActions(parentEntityAsRelatableActivity, quote, importDeciderFactory);
			}
			else
			{
				var org = parentEntity as OrgHeader;
				var ratingHeader = quote as IRatingHeader;
				if (org != null && ratingHeader != null)
				{
					ratingHeader.TH_OH = org.PK;
				}
			}

			return quote;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void PromptUserToResynchronise(ISalesValueAssociatedEntity parentEntity, IBusiness newQuote)
		{
			var newFactory = new BusinessObjectFactory();
			var quoteInNewFactory = (ISalesRelationActivity)newFactory.ImportFromAnotherFactory((BusinessObject)newQuote);
			if (quoteInNewFactory == null)
			{
				return;
			}

			var orgInNewFactory = (OrgHeader)quoteInNewFactory.Client;
			if (orgInNewFactory != null)
			{
				using (var progressForm = new ProgressForm())
				{
					progressForm.Status = Res.GetString("fff434f2-dac1-4ea7-a628-418fc6e10b17", "Resynchronizing {0} estimate values from quotations...", orgInNewFactory.OH_Code);
					progressForm.ShowCancelButton = false;
					progressForm.ShowProgressBar = false;
					progressForm.Show();
					Application.DoEvents();
					try
					{
						SyncNewSalesValues(parentEntity, newQuote, orgInNewFactory, () =>
						{
							var dialogResult = Globals.Message.Show(
							Res.GetString("d66312f9-a588-4d15-a763-8297d1651883", "We've noticed that you've entered different trade lanes into the quotation than what you had on the opportunity, would you like to update the opportunity with the variations?"),
							Res.GetString("22dd83b8-4535-40ba-92f9-a14e08279025", "New trade lanes"),
							MessageBoxButtons.YesNo,
							DialogResult.No);

							return dialogResult == DialogResult.Yes;
						});
					}
					catch (ZSaveConcurrencyException ex)
					{
						ZExceptionReporting.HandleSaveException(ex, ParentModalForm);
					}
				}
			}
		}

		internal static void SyncNewSalesValues(ISalesValueAssociatedEntity parentEntity, IBusiness newQuote, OrgHeader org, Func<bool> confirmCreateAssociations)
		{
			if (!ObjectFactory.Get<IJCDServiceTaskStatusChecker>().IsJCDServiceTaskComplete)
			{
				Globals.Message.ShowError(
					Res.GetString("feec7a3a-755e-4c7f-b29d-7ffc72f4d51e", "Before you can synchronize sales values, you must finish processing all Transaction Lines through the Job Costing Data Queue Service Task (https://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/CargoWiseOneUpdateNote20180319d.pdf)."),
					Res.GetString("7168328a-ae49-4a70-a598-c3ca325095d3", "Job Costing Data Queue Service Task Incomplete"));
				return;
			}

			var newAssociationsAdded = false;
			var salesValueFactories = new HashSet<BusinessObjectFactory>();

			using (var summaryProvider = new NonCachedTradeLinesSummaryProvider(CargoWise.Data.Db.Connection, includeActuals: false, includeProspect: true))
			{
				var date = ZDateTime.UtcNow.Date;
				var summary = summaryProvider.GetForOrg(org, date, date.AddDays(1));
				var tradeLinesSynchroniser = new TradeLinesSynchroniser(org.PK, parentEntity);
				var associatedSalesValues = new HashSet<ISalesValue>();
				if (parentEntity != null)
				{
					tradeLinesSynchroniser.SalesValueAssociationCreated += (sender, e) =>
					{
						var pivot = e.SalesValueAssociationPivot;
						if ((ZGuid)((BusinessObject)pivot.AssociatedEntity)?[RateEntrySchema.Constants.TI_TH] == newQuote.Identifier)
						{
							var salesValue = pivot.SalesValue;
							if (salesValue != null)
							{
								associatedSalesValues.Add(salesValue.TablePrefix == OrgTradeDetailSchema.Constants.Prefix ? EntityTradeDetailWrapper.Get((OrgTradeDetail)salesValue, parentEntity) : salesValue);
							}
						}
					};
				}

				tradeLinesSynchroniser.Execute(summary);

				if (parentEntity != null)
				{
					if (parentEntity.EntityType == RelatableActivityTypeList.Codes.OpportunityManager)
					{
						SetStatusForOpportunityDetails(parentEntity, associatedSalesValues);
					}

					var salesValuesToAddPivot = associatedSalesValues.Where(salesValue => salesValue.SalesAssociationPivotCollectionGlobal.All(x => x.SVP_ActivityId != parentEntity.Identifier)).ToArray();
					if (salesValuesToAddPivot.Length > 0)
					{
						if (confirmCreateAssociations())
						{
							foreach (var salesValue in salesValuesToAddPivot)
							{
								salesValue.SalesAssociationPivotCollectionGlobal.AddNew(parentEntity);
								salesValueFactories.Add(((BusinessObject)salesValue).Factory);
							}
							newAssociationsAdded = true;
						}
					}
				}
			}

			BusinessObjectFactory.SaveTogether(salesValueFactories.ToArray());

			if (newAssociationsAdded)
			{
				parentEntity.ProspectiveSalesHeaderCollection.Refresh();
			}
		}

		static void SetStatusForOpportunityDetails(ISalesValueAssociatedEntity parentEntity, HashSet<ISalesValue> associatedSalesValues)
		{
			var parentAsOrgOpportunity = (OrgOpportunity)parentEntity;
			var desiredCode = OrganisationsDataRegistry.Instance.OpportunityStatus.Value.GetTradeStatusFromCode(parentAsOrgOpportunity.P8_Status);
			var detailArray = associatedSalesValues.Where(salesValue => salesValue.TablePrefix == OrgTradeDetailSchema.Constants.Prefix).ToArray();

			foreach (var salesValue in detailArray)
			{
				var detail = (OrgTradeDetail)salesValue;
				if (detail != null && detail.PA_Status != desiredCode)
				{
					detail.PA_Status = desiredCode;
				}
			}
		}

		#endregion

		#region Messages

		public static string CreateQuotationCaption
		{
			get { return Res.GetString("5157fcd6-e717-48e8-a1e3-a14734713117", "Create Quotation"); }
		}

		public static string CannotCreateQuoteCaption
		{
			get { return Res.GetString("ac1c3140-b935-48da-b9d0-9be4abf0b0eb", "Cannot create Quotation"); }
		}

		#endregion
	}
}
