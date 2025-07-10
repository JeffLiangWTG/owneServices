using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Packing.Business
{
	public class PkgPackageDocumentSupporter : DocumentSupporter
	{
		public PkgPackageDocumentSupporter(PkgPackage package)
			: base(package)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Package; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		#region Events

		protected override void InitialiseCore(IDocumentEvents documentEventSource)
		{
			base.InitialiseCore(documentEventSource);

			documentEventSource.DocumentPrinted += DocumentEventSource_DocumentPrinted;
		}

		#region DocumentEventSource_DocumentPrinted

		void DocumentEventSource_DocumentPrinted(object sender, DocumentPrintedEventArgs e)
		{
			var command = e.MenuItem as DocumentEngine.DocumentCommand;
			var dataContext = GetDataContext(command);

			if (dataContext != Constants.DataContext.None)  // any label printed
			{
				Package.IsLabelPrinted = true;
			}
		}

		#region GetDataContext

		Constants.DataContext GetDataContext(DocumentEngine.DocumentCommand command)
		{
			var result = Constants.DataContext.None;

			var menuItems = new List<StmMenuItem>(command.ChildMenus.Cast<StmMenuMenuPivotBase>().Select(m => m.Outward));
			menuItems.Add(command);

			var documentPivots = Factory.Load<StmMenuTemplatePivotBase>(new ZQuery(StmMenuTemplatePivotSchema.SI_SU, menuItems.Select(m => m.PK))).ToList();
			var templates = documentPivots.Select(d => d.Template);
			var configs = new List<StmMenuDocumentConfig>();
			documentPivots.ForEach(d => configs.AddRange(d.DocConfigs.Cast<StmMenuDocumentConfig>()));

			var dataContexts = new List<ZString>();
			dataContexts.AddRange(configs.Select(c => c.S3_OverrideDataContext));
			dataContexts.AddRange(templates.Select(t => t.SO_DataContext));

			var validDataContexts = GetSupportedDataContexts().Select(dc => dc.ToString());

			foreach (string dataContext in dataContexts)
			{
				if (validDataContexts.Contains(dataContext))
				{
					Constants.DataContext validContext;
					if (Enum.TryParse(dataContext, out validContext))
					{
						result = validContext;
						break;
					}
				}
			}

			return result;
		}

		#endregion

		#endregion

		#endregion

		#region Contact

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			return PackageJobDocumentSupporter != null
				? PackageJobDocumentSupporter.GetContactOrganisation(menuName, contactType, direction)
				: base.GetContactOrganisation(menuName, contactType, direction);
		}

		#endregion

		#region DataContexts

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			var dataContext = new List<Constants.DataContext>
				{
					Constants.DataContext.GenericFreightJob,
					Constants.DataContext.GenericBasicLabel,
					Constants.DataContext.GenericBasicLabelAll,
					Constants.DataContext.GenericProductLabel,
					Constants.DataContext.GenericProductLabelAll,
					Constants.DataContext.GenericDeliveryLabel,
					Constants.DataContext.GenericDeliveryLabelAll,
					Constants.DataContext.GenericProductDeliveryLabel,
					Constants.DataContext.GenericProductDeliveryLabelAll,
				};

			if (PackageJobDocumentSupporter != null)
			{
				dataContext.AddRange(PackageJobDocumentSupporter.GetModuleSpecificDataContexts());
			}

			if (GlbStaff.CurrentUser.IsSupportUser)
			{
				dataContext.AddRange(new List<Constants.DataContext>
				{
					Constants.DataContext.GenericCarrierLabel,
					Constants.DataContext.GenericCarrierLabelAll
				});
			}

			return dataContext.ToArray();
		}

		#endregion

		#region DocumentWrappers

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result;

			switch (dataContext)
			{
				case Constants.DataContext.GenericFreightJob:
					result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);
					break;
				case Constants.DataContext.GenericBasicLabel:
				case Constants.DataContext.GenericBasicLabelAll:
				case Constants.DataContext.GenericDeliveryLabel:
				case Constants.DataContext.GenericDeliveryLabelAll:
				case Constants.DataContext.GenericProductDeliveryLabel:
				case Constants.DataContext.GenericProductDeliveryLabelAll:
				case Constants.DataContext.GenericRetailersLabel:
				case Constants.DataContext.GenericRetailersLabelAll:
				case Constants.DataContext.GenericCarrierLabel:
				case Constants.DataContext.GenericCarrierLabelAll:
					result = GetLabelWrappers(dataContext);
					break;

				case Constants.DataContext.GenericProductLabel:
				case Constants.DataContext.GenericProductLabelAll:
					result = PackageJobDocumentSupporter != null ? PackageJobDocumentSupporter.GetProductLabelWrappers(dataContext, true) : Array.Empty<DocumentWrapper>();
					break;

				default:
					result = GetLabelWrappers(dataContext);
					break;
			}

			return result;
		}

		#region GetLabelWrappers

		DocumentWrapper[] GetLabelWrappers(Constants.DataContext dataContext)
		{
			DocumentWrapper[] wrappers;

			if (PackageJobDocumentSupporter != null)
			{
				if (Globals.IsWeb || Globals.IsWebService)
				{
					wrappers = PackageJobDocumentSupporter.GetLabelWrappersForWebService(dataContext, Package);
				}
				else
				{
					wrappers = PackageJobDocumentSupporter.GetLabelWrappers(dataContext, useSelected: true);
				}
			}
			else
			{
				wrappers = Array.Empty<DocumentWrapper>();
			}

			return wrappers;
		}

		#endregion

		#region ShowMenuNotFoundMessages

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			ZString message;
			if (PackageJobDocumentSupporter != null)
			{
				var dataContext = dataContextValue.DataContext;
				if (dataContext is Constants.DataContext.GenericBasicLabel
					&& Package.KP_KPH_PackageHeader.IsEmpty
					&& Package.PackageJob.KJ_ParentTableCode == WhsItemReceiveConsignmentSchema.Constants.Prefix)
				{
					message = Res.GetString("749d46a4-43d1-4983-aa83-e903e10423fb", "Package ID is Required.");
				}
				else
				{
					message = PackageJobDocumentSupporter.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
				}
			}
			else
			{
				message = base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
			}

			return message;
		}

		#endregion

		#endregion

		#region Package

		PkgPackage Package
		{
			get { return (PkgPackage)BusinessObject; }
		}

		#endregion

		#region PackageJob

		PkgPackageJobDocumentSupporter PackageJobDocumentSupporter
		{
			get
			{
				var packageJob = Package.PackageJob as IDocumentSupportable;
				return packageJob != null ? (PkgPackageJobDocumentSupporter)packageJob.DocumentSupporter : null;
			}
		}

		#endregion

		#region GetDataStateBeforeRun

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Menu name")]
		const string BasicLabelMenuName = "Basic Label";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Menu name")]
		const string DetailedLabelMenuName = "Detailed Label";

		public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
		{
			var result = base.GetDataStateBeforeRun(commandAboutToBeRun);
			if (result.IsValid
				&& commandAboutToBeRun != null
				&& (commandAboutToBeRun.SU_MenuName == BasicLabelMenuName || commandAboutToBeRun.SU_MenuName == DetailedLabelMenuName)
				&& Package.KP_KPH_PackageHeader.IsEmpty)
			{
				result = new DocumentSupporterDataState(false, Res.GetString("d0f2f850-fc6c-4512-b1cb-6e6b99d1f2bc", "Package ID is Required."));
			}
			return result;
		}

		#endregion
	}
}

