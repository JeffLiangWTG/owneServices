using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Rating.Business
{
	public class RatingHeaderDocumentSupporter : DocumentSupporter
	{
		public RatingHeaderDocumentSupporter(RatingHeader header)
			: base(header)
		{
		}

		RatingHeader Header
		{
			get { return (RatingHeader)BusinessObject; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override void InitialiseCore(IDocumentEvents documentEventSource)
		{
			documentEventSource.DocumentPrintRequested += new DocumentCancelEventHandler(DocumentEventSource_DocumentPrintRequested);
			base.InitialiseCore(documentEventSource);
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			if (filterName == DocumentFilters.HIDE)
			{
				return "N";
			}
			else
			{
				return base.GetFilterValue(filterName);
			}
		}

		#region BusinessContext

		public override BusinessContext BusinessContext
		{
			get
			{
				BusinessContext result;
				RatingHeaderBusinessContexts.TryGetValue(Header.GetType(), out result);

				return result;
			}
		}

		static Dictionary<Type, BusinessContext> RatingHeaderBusinessContexts
		{
			get
			{
				if (fRatingHeaderBusinessContexts == null)
				{
					fRatingHeaderBusinessContexts = new Dictionary<Type, BusinessContext>();

					var assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
					foreach (var type in assemblyTypes)
					{
						if (type.IsSubclassOf(typeof(RatingHeader)))
						{
							var businessContexts = type.GetCustomAttributes(typeof(BusinessContextAttribute), false);
							if (businessContexts.Length == 1)
							{
								var attr = (BusinessContextAttribute)businessContexts[0];
								fRatingHeaderBusinessContexts.Add(type, attr.BusinessContext);
							}
						}
					}
				}

				return fRatingHeaderBusinessContexts;
			}
		}

		[ThreadStatic]
		static Dictionary<Type, BusinessContext> fRatingHeaderBusinessContexts;

		#endregion

		#region SupportedDataContexts

		protected override DataContext[] GetSupportedDataContexts()
		{
			return UniqueDataContexts;
		}

		public static DataContext[] UniqueDataContexts
		{
			get
			{
				return new DataContext[]
				{
					DataContext.GenericFreightJob,
					DataContext.Rating,
					DataContext.Quotation,
					DataContext.ShippingRating,
					DataContext.ShippingDetentionRating,
					DataContext.CFSRating,
					DataContext.WarehouseRating,
					DataContext.TransportRating,
				};
			}
		}

		#endregion

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Header) ?? Array.Empty<DocumentWrapper>();
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			return Header.Header != null ? new OrgHeaderContact(Header.Header, null) : base.GetContactOrganisation(menuName, contactType, direction);
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return dataContext != DataContext.GenericFreightJob
				&& dataContext != DataContext.Rating
				&& dataContext != DataContext.Quotation
				&& dataContext != DataContext.ShippingRating
				&& dataContext != DataContext.ShippingDetentionRating
				&& dataContext != DataContext.CFSRating
				&& dataContext != DataContext.WarehouseRating
				&& dataContext != DataContext.TransportRating
				&& base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}

		public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
		{
			if (commandAboutToBeRun != null)
			{
				var templatePivots = BusinessObject.Factory.Load<StmMenuTemplatePivot>(new ZQuery(StmMenuTemplatePivotSchema.SI_SU, commandAboutToBeRun.PK));
				if (templatePivots != null)
				{
					foreach (var templatePivot in templatePivots)
					{
						var template = templatePivot.Template;
						if (template != null)
						{
							try
							{
								Enum.Parse(typeof(DataContext), template.SO_DataContext);
							}
							catch (ArgumentException)
							{
								return new DocumentSupporterDataState
								(
									false,
									Res.GetString
									(
										"f9ff84bf-4a29-4220-a3ff-e872bcd60b73",
										"The data context {0} is invalid. Please check your template, amend the data context, reload the template and try again.",
										template.SO_DataContext
									)
								);
							}
						}
					}
				}
			}

			return base.GetDataStateBeforeRun(commandAboutToBeRun);
		}

		#region Implementation

		void DocumentEventSource_DocumentPrintRequested(object sender, DocumentCancelEventArgs e)
		{
			var task = BuildPrintTask(e.MenuItem);
			e.Cancel = true;
			if (task != null)
			{
				RunTask(task);
			}
		}

		public virtual void RunTask(PrintTask task)
		{
			task.RunWithPartialInstructions(AllowedDeliveryOptions.All, NewDeliveryInstructions(task), Env.Security.None);
		}

		protected DeliveryInstructions NewDeliveryInstructions(PrintTask task)
		{
			var result = task.Count == 1 ? new DeliveryInstructions(task[0]) : new DeliveryInstructions();
			result.ExcludeDocumentsThatHaveSectionsWhichContainNoDataRows = true;
			return result;
		}

		#region BuildDocumentPackage

		public PrintTask BuildPrintTask(IStmMenuItem menuItem)
		{
			if (!Header.OnValidateDocument())
			{
				return null;
			}

			if (PerformChecks(menuItem))
			{
				var printTask = BuildPrintTaskCore(menuItem);
				new PrintTaskBuilder(Header).BuildPrintTask(printTask);
				return printTask.Count > 0 ? printTask : null;
			}
			else
			{
				return null;
			}
		}

		public virtual PrintTask BuildPrintTaskCore(IStmMenuItem menuItem)
		{
			return new PrintTask(menuItem);
		}

		public override void SetupDeliveryForAutoDocumentDeliveryJob(IPrintTask printTask)
		{
			if (PerformChecks(((PrintTask)printTask).ParentMenuCommand))
			{
				new PrintTaskBuilder(Header).BuildPrintTask((PrintTask)printTask);
				base.SetupDeliveryForAutoDocumentDeliveryJob(printTask);
			}
		}

		public DocumentPack BuildDocumentPack(IStmMenuItem menuItem)
		{
			if (PerformChecks(menuItem))
			{
				return new PrintTaskBuilder(Header).BuildDocumentPack(menuItem);
			}
			else
			{
				return null;
			}
		}

		protected virtual bool PerformChecks(IStmMenuItem menuItem)
		{
			return true;
		}

		#endregion

		#endregion
	}
}

