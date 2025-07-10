using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSLoadListConsolDocumentSupporter : CommonConsolDocumentSupporter, ILabelDocumentSupporter
	{
		public CFSLoadListConsolDocumentSupporter(CFSLoadListConsol consol)
			: base(consol)
		{
		}

		protected CFSLoadListConsol LoadListConsol
		{
			get { return (CFSLoadListConsol)BusinessObject; }
		}

		#region Overrides

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[]
			{
				Core.Constants.DataContext.LoadListConsol,
				Core.Constants.DataContext.ERA,
				Core.Constants.DataContext.CartageAdvice,
				Core.Constants.DataContext.IMO,
				Core.Constants.DataContext.LoadListDocument,
				Core.Constants.DataContext.CommonConsol,
				Core.Constants.DataContext.PackUnpackContainerRego,
				Core.Constants.DataContext.PickListDocument,
				Core.Constants.DataContext.GenericFreightJob,
				Core.Constants.DataContext.GenericFreightJobByPackages
			};
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CFSLoadList; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, LoadListConsol);
			if (genericWrappers != null)
			{
				return genericWrappers;
			}

			DocumentWrapper[] result = null;
			switch (dataContext)
			{
				case Core.Constants.DataContext.ERA:
					result = GetWrappersByContainer(ContainersToSelectFrom);
					break;

				case Core.Constants.DataContext.PackUnpackContainerRego:
					result = new DocumentWrapper[LoadListConsol.Containers.Count];
					for (int i = 0; i < LoadListConsol.Containers.Count; i++)
					{
						DocumentWrapper packUnpackContainerWrapper = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.PackUnpackContainerRego, LoadListConsol.Containers[i]);
						result.SetValue(packUnpackContainerWrapper, i);
					}
					break;

				case Core.Constants.DataContext.LoadListConsol:
				case Core.Constants.DataContext.CommonConsol:
					result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.LoadListConsol, LoadListConsol) };
					break;

				case Core.Constants.DataContext.IMO:
				case Core.Constants.DataContext.Container:
					result = GetContainerDocBusinessObjects(commandBeingRun);
					break;

				case Core.Constants.DataContext.PickListDocument:
					DocumentCommonConsol docCommonConsol = new DocumentCommonConsol(LoadListConsol, dataContext);
					DocumentWrapper docWrapper = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.LoadListConsol, docCommonConsol);
					if (docWrapper != null)
					{
						result = new DocumentWrapper[] { docWrapper };
					}

					break;

				case Core.Constants.DataContext.LoadListDocument:
					DocumentWrapper wrapper = GetWrapperForCommonConsol(dataContext);
					if (wrapper != null)
					{
						result = new DocumentWrapper[] { wrapper };
					}

					break;

				case Core.Constants.DataContext.GenericFreightJobByPackages:
					return GetGenericWrapperForPackageLabels(commandBeingRun);
			}
			return result;
		}

		public DocumentWrapper[] GetGenericWrapperForPackageLabels(IStmMenuItem commandBeingRun)
		{
			var result = new List<DocumentWrapper>();
			foreach (CFSShipment shipment in LoadListConsol.Shipments.Cast<CFSShipment>().Where(x => ShouldIncludeShipmentOnLabels(x, commandBeingRun)))
			{
				foreach (PackLine packLine in shipment.OuterPackLines)
				{
					result.AddRange(GetPackLinesWrapper(packLine));
				}
			}

			return result.ToArray();
		}

		static bool ShouldIncludeShipmentOnLabels(CFSShipment shipment, IStmMenuItem commandBeingRun)
		{
			return (((commandBeingRun.SU_MenuName.StartsWith(LabelsName.ImportLabel) && shipment.IsImport())
							|| (commandBeingRun.SU_MenuName == LabelsName.OnForwardingLabel && shipment.IsOnForwarding())
									|| (commandBeingRun.SU_MenuName == LabelsName.TranshipmentLabel && shipment.IsTranshipment()))
							&& !shipment.IsCoLoadMaster
							&& !shipment.IsBlindCoLoadMaster
							&& !shipment.IsAssemblyMaster);
		}

		DocumentWrapper[] GetPackLinesWrapper(PackLine package)
		{
			var result = new List<DocumentWrapper>();

			var wrappers = DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, LoadListConsol);
			if (wrappers.Length > 0)
			{
				var wrapper = wrappers[0];
				var iPackLineOverrider = (IPackLineOverrider)wrapper;
				iPackLineOverrider.SetPackageOverride(package);
				for (int i = 0; i < package.JL_PackageCount; i++)
				{
					result.Add(wrapper);
				}
			}

			return result.ToArray();
		}

		protected override IDocumentEventsHandler[] GetDocumentEventsHandlers()
		{
			var result = new List<IDocumentEventsHandler>(base.GetDocumentEventsHandlers());

			var labelDocumentEventsHandler = ObjectFactory.Get<IDocumentEventsHandler>("IDocumentEventsHandler.Labels");
			labelDocumentEventsHandler.DocumentSupporter = this;
			result.Add(labelDocumentEventsHandler);

			return result.ToArray();
		}

		public override BusinessContext[] SupportedChildBusinessContexts
		{
			get { return new[] { BusinessContext.CFSContainerRego }; }
		}

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menuToBeRun, BusinessContext businessContext, IStmMenuItem childCommand)
		{
			IDocumentSupportable[] result = null;

			if (businessContext == BusinessContext.CFSContainerRego)
			{
				result = GetChildCollectionByContainer(ContainersToSelectFrom);
			}
			else
			{
				result = base.GetChildCollection(menuToBeRun, businessContext, childCommand);
			}

			return result;
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType type, DocumentDirection direction)
		{
			IDocumentDeliveryContact result = base.GetContactOrganisation(menuName, type, direction);

			if (result == null)
			{
				if (type == ContactType.LocalTransport)
				{
					result = new OrgHeaderContact(LoadListConsol.CartageCo, LoadListConsol.CartageCoAddress);
				}
			}

			return result;
		}

		#endregion

		DocumentWrapper GetWrapperForCommonConsol(Constants.DataContext dataContext)
		{
			var consolToPrint = GetConsolToPrint(dataContext);
			return consolToPrint != null ? DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.LoadListConsol, consolToPrint) : null;
		}

		DocumentCommonConsol GetConsolToPrint(Constants.DataContext dataContext)
		{
			var docCommonConsolBizObject = new DocumentCommonConsol(LoadListConsol, dataContext);
			docCommonConsolBizObject.SetDefaultsFromDataContext(docCommonConsolBizObject.DataContext);

			return QueryProvider.GetConsolToPrint(docCommonConsolBizObject);
		}

		#region Implementation

		protected override DocumentWrapper CreateContainerWrapper(CommonContainer container)
		{
			var cfsContainer = Factory.Load<CFSContainer>(container.PK);
			return DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.PackUnpackContainerRego, cfsContainer);
		}

		protected override CommonContainer CreateContainerDocumentSupportable(CommonContainer container)
		{
			return Factory.Load<CFSContainer>(container.PK);
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var message = ZString.Empty;

			switch (dataContextValue.DataContext)
			{
				case Core.Constants.DataContext.ERA:
				case Core.Constants.DataContext.IMO:
				case Core.Constants.DataContext.Container:
				case Core.Constants.DataContext.PackUnpackContainerRego:
					{
						if (!Consol.Containers.Any())
						{
							message = Res.GetString("ef764975-0b42-4483-b0ae-09cf36a3b8ea", "No containers are entered for Load List {0}.", LoadListConsol.JK_UniqueConsignRef);
						}

						break;
					}

				case Core.Constants.DataContext.GenericFreightJobByPackages:
					{
						var shipments = LoadListConsol.Shipments
								.Cast<CFSShipment>()
								.Where(x => ShouldIncludeShipmentOnLabels(x, commandBeingRun))
								.ToArray();

						if (!shipments.Any() || shipments.All(c => c.OuterPackLines.Count == 0))
						{
							message = Res.GetString("463f0774-9193-47d1-9a73-cf669404ec9b", "A Shipment attached to this Load List must have at least one packline.");
						}

						break;
					}
			}

			return string.IsNullOrWhiteSpace(message)
				? base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun)
				: message;
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return dataContext != Core.Constants.DataContext.LoadListConsol
				&& dataContext != Core.Constants.DataContext.CommonConsol
				&& dataContext != Core.Constants.DataContext.PickListDocument
				&& dataContext != Core.Constants.DataContext.LoadListDocument
				&& base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}

		#endregion

		#region ISupportLabels Members

		bool ILabelDocumentSupporter.SupportImportLabels
		{
			get { return LoadListConsol.Shipments.Cast<CFSShipment>().Any(shipment => shipment.IsImport()); }
		}

		bool ILabelDocumentSupporter.SupportOnForwardingLabels
		{
			get { return LoadListConsol.Shipments.Cast<CFSShipment>().Any(shipment => shipment.IsOnForwarding()); }
		}

		bool ILabelDocumentSupporter.SupportTranshipmentLabels
		{
			get { return LoadListConsol.Shipments.Cast<CFSShipment>().Any(shipment => shipment.IsTranshipment()); }
		}

		string ILabelDocumentSupporter.ImportLabelErrorMessage
		{
			get { return Res.GetString("b4ae3ce8-0fde-411f-bce1-3512d63ae9c8", "This Load List does not have any Import shipments with at least one pack line."); }
		}

		string ILabelDocumentSupporter.OnForwardingErrorMessage
		{
			get { return Res.GetString("965acd08-93d2-45c0-87a7-e6df36a830e9", "This Load List does not have any On Forwarding shipments with at least one pack line."); }
		}

		string ILabelDocumentSupporter.TranshipmentErrorMessage
		{
			get { return Res.GetString("d71e9174-1409-4086-85de-ff52be307c59", "This Load List does not have any Transhipment shipments with at least one pack line."); }
		}

		#endregion
	}
}
