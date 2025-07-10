using System.Collections.Generic;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineIntegration;
using Enterprise.eManifest.Integration;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.eManifest.Business
{
	public class SupplierBookingHeaderDocumentSupporter : DocumentSupporter, ISupportCustomizedDocumentPrintSet
	{
		#region Constructor

		public SupplierBookingHeaderDocumentSupporter(SupplierBookingHeader header)
			: base(header)
		{
		}

		#endregion

		protected SupplierBookingHeader SupplierBookingHeader
		{
			get { return (SupplierBookingHeader)BusinessObject; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, SupplierBookingHeader);
			return genericWrappers;
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[] { Constants.DataContext.GenericFreightJob };
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.SupplierBookingHdr; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.MaintainShipmentCustomiseDocuments; }
		}

		public override BusinessContext[] SupportedChildBusinessContexts
		{
			get { return new[] { BusinessContext.SupplierBookingLine }; }
		}

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menuToBeRun, BusinessContext businessContext, IStmMenuItem childCommandBeingRun)
		{
			IDocumentSupportable[] result = null;

			if (businessContext == BusinessContext.SupplierBookingLine)
			{
				var supplierBookingLines = SupplierBookingHeader.BookingLines.Cast<SupplierBookingLine>().OrderBy(x => x.DL_ConsigneeReference);
				result = supplierBookingLines.Cast<IDocumentSupportable>().ToArray();
			}

			return result;
		}

		#region ISupportCustomizedDocumentPrintSet & HVLV Document

		IPrintTask ISupportCustomizedDocumentPrintSet.GetCustomizedDocumentPrintSet(IDocumentCommand command)
		{
			var packs = GetPacksIEnum((DocumentCommand)command);
			return new DocumentPrintSetWithStreaming((DocumentCommand)command, CalculateNumberOfDocumentPacksForStreaming(), packs, true);
		}

		bool ISupportCustomizedDocumentPrintSet.ShouldCustomizedDocumentPrintSet(IDocumentCommand command)
		{
			return command.SU_MenuName == HVLVDeliveryLabelHelper.MenuItemName && command.SU_BusinessContext == nameof(BusinessContext.SupplierBookingHdr) && SupplierBookingHeader.BookingLines.Count > HVLVDeliveryLabelHelper.NumberOfDocsPerPackForStreamingMode;
		}

		int CalculateNumberOfDocumentPacksForStreaming()
		{
			int numberOfBizO = SupplierBookingHeader.BookingLines.Count;
			int docPackCount = numberOfBizO / HVLVDeliveryLabelHelper.NumberOfDocsPerPackForStreamingMode;
			if (numberOfBizO % HVLVDeliveryLabelHelper.NumberOfDocsPerPackForStreamingMode != 0)
			{
				docPackCount++;
			}

			return docPackCount;
		}

		IEnumerable<DocumentPack> GetPacksIEnum(DocumentCommand command)
		{
			var documentPacks = Enumerable.Empty<DocumentPack>();
			if (command.SU_MenuName == HVLVDeliveryLabelHelper.MenuItemName)
			{
				var commandToRunInStreamingMode = Factory.Load<DocumentCommand>(command.ChildMenus[0].SF_SU_Outward);// The menu item command to run is the first child command
				commandToRunInStreamingMode.UserContextForWebServiceEnvironment = Env.CurrentUserContext;
				documentPacks = GetEnumCollectionForSupplierBookingLine(commandToRunInStreamingMode);
			}

			return documentPacks;
		}

		IEnumerable<DocumentPack> GetEnumCollectionForSupplierBookingLine(DocumentCommand command)
		{
			var listOfPKForPacks = SupplierBookingHeader.BookingLines.Cast<ISupplierBookingLine>().OrderBy(b => b.DL_ConsigneeReference)
				.Chunk(HVLVDeliveryLabelHelper.NumberOfDocsPerPackForStreamingMode)
				.Select(chunk => chunk.Select(d => d.PK).ToList())
				.ToList();

			return IterateThreadSafe(listOfPKForPacks, command.PK, command.UserContextForWebServiceEnvironment);
		}

		IEnumerable<DocumentPack> IterateThreadSafe(List<List<ZGuid>> listOfPKForPacks, ZGuid documentCommandPk, IUserContext userContextUnsafe)
		{
			// Each yield could be run on a different thread
			// Thar be dragons
			foreach (var pkOFDocsInOnePack in listOfPKForPacks)
			{
				DocumentPack docPack = null;
				using (Db.DisposableActionForDbConnection())
				using (Env.SetTemporaryUserContext(userContextUnsafe.ThreadSafeClone())) //This will make it set up the environment in the new thread
				{
					var factory = new BusinessObjectFactory();

					var commandInNewFactory = factory.Load<DocumentCommand>(documentCommandPk);
					var linesInNewFactory = factory.Load<SupplierBookingLine>(new ZQuery(SupplierBookingLineSchema.PK, pkOFDocsInOnePack)).OrderBy(b => b.DL_ConsigneeReference);

					foreach (var bizO in linesInNewFactory)
					{
						if (docPack == null)
						{
							docPack = new DocumentPack(commandInNewFactory, bizO, null, null);
							docPack.ShareSameTemplateInPack = true;
						}
						else
						{
							docPack.AddReportsToPack(commandInNewFactory, null, bizO, null);
						}
					}
				}
				yield return docPack;
			}
		}

		#endregion
	}
}
