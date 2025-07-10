using System;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using IBaseJobDeclaration = Enterprise.Integration.Customs.IBaseJobDeclaration;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ForwardingConsolValueObjectDataAdapter : ConsolValueObjectDataAdapter<ForwardingConsol, ForwardingShipment, Xsd.Consol>
	{
		#region Contstructors

		public ForwardingConsolValueObjectDataAdapter()
		{
		}

		public ForwardingConsolValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
			: base(triggeredByEvents)
		{
		}

		public ForwardingConsolValueObjectDataAdapter(ForwardingShipment shipmentToExportFilter)
			: this(shipmentToExportFilter, EventsWithSourceType.Empty)
		{
		}

		public ForwardingConsolValueObjectDataAdapter(ForwardingShipment shipmentToExportFilter, EventsWithSourceType triggeredByEvents)
			: this(triggeredByEvents)
		{
			this.shipmentToExportFilter = shipmentToExportFilter;
		}

		public ForwardingConsolValueObjectDataAdapter(Order orderToExportFilter)
			: this(orderToExportFilter, EventsWithSourceType.Empty)
		{
		}

		public ForwardingConsolValueObjectDataAdapter(Order orderToExportFilter, EventsWithSourceType triggeredByEvents)
			: this(orderToExportFilter.Shipment, triggeredByEvents)
		{
			this.orderToExportFilter = orderToExportFilter;
		}

		public ForwardingConsolValueObjectDataAdapter(bool isManualImport)
		{
			this.IsManualImport = isManualImport;
		}

		readonly bool IsManualImport;

		#endregion

		protected override void AfterImportFromValueObject(ForwardingConsol bizObj, Xsd.Consol value, IValueObjectImportContext context)
		{
			base.AfterImportFromValueObject(bizObj, value, context);
			if (bizObj.IsAir || bizObj.IsSea)
			{
				ConsolCustomsCargoMessageHelper cargoMessageHelper = new ConsolCustomsCargoMessageHelper(bizObj, ((ValueObjectImportContext)context).Notifications, false);
				cargoMessageHelper.CreateConsolCargoJobWithSACFlag();
				cargoMessageHelper.SendConsolCargoMessage();
			}
		}

		protected override void OnAfterExportFromValueObjectCore(ForwardingConsol consol, Xsd.Consol consolValue, IValueObjectExportContext context)
		{
			ExportARInvoices(consol, consolValue, context);
			ExportAWBHeader(consol, consolValue, context);
			base.OnAfterExportFromValueObjectCore(consol, consolValue, context);
		}

		void ExportAWBHeader(ForwardingConsol consol, Xsd.Consol consolValue, IValueObjectExportContext context)
		{
			if (IsSendXMLWithAWB && consol.IsAir)
			{
				ExportAWBHeaderValueObjectDataAdapter aWBHeaderExporter = new ExportAWBHeaderValueObjectDataAdapter(consol);
				consol.PopulateAWB();
				consolValue.AWBHeaders.Add(aWBHeaderExporter.ExportToValueObject(consol.AWBHeader, context));
			}
		}

		bool IsSendXMLWithAWB
		{
			get
			{
				return
				this.TriggeredByEvents != null &&
				this.TriggeredByEvents.TriggerAction != null &&
				WorkflowTriggerActionTypeConstants.IsSendXMLWithAWB(this.TriggeredByEvents.TriggerAction.PQ_TriggerType);
			}
		}

		void ExportARInvoices(ForwardingConsol consol, Xsd.Consol consolValue, IValueObjectExportContext context)
		{
			if (SystemRegistry.IncludeConsolOrShipmentARInvoices.Value)
			{
				ForwardingJobInvoicesExporter invoiceExporter = new ForwardingJobInvoicesExporter(consol.Factory);
				consolValue.ARInvoices = invoiceExporter.PopulateInvoicesToXSD(consol.JK_UniqueConsignRef, context, true);
			}
		}

		protected override ShipmentValueObjectDataAdapter<ForwardingShipment> GetNewShipmentValueObjectDataAdapter(ForwardingConsol existingConsol)
		{
			return new ForwardingShipmentValueObjectDataAdapter(existingConsol, orderToExportFilter, TriggeredByEvents);
		}

		#region Generate Declaration for Shipment

		ShipmentDeclarationCreateHelper ShipmentJobDecCreateHelper
		{
			get { return shipmentJobDecCreateHelper ?? (shipmentJobDecCreateHelper = new ShipmentDeclarationCreateHelper(IsManualImport, new BusinessObjectFactory())); }
		}
		ShipmentDeclarationCreateHelper shipmentJobDecCreateHelper;

		protected override void GenerateDeclarationForShipment(Xsd.Shipment shipmentValue, CommonShipment shipment, IValueObjectImportContext context)
		{
			ForwardingShipment forwardingShipment = (ForwardingShipment)shipment;
			if (!shipment.IsInDatabase)
			{
				if (ShipmentJobDecCreateHelper.ShouldCreateJobDecIrrespectiveOfCompanies)
				{
					CreateDeclarationBasedOnUNLOCO(forwardingShipment, shipmentValue, context, true);
					CreateDeclarationBasedOnUNLOCO(forwardingShipment, shipmentValue, context, false);
				}
				else
				{
					CreateDeclarationBasedOnUNLOCO(forwardingShipment, shipmentValue, context, shipment.IsExport());
				}
			}
		}

		void CreateDeclarationBasedOnUNLOCO(ForwardingShipment shipment, Xsd.Shipment shipmentValue, IValueObjectImportContext context, bool isExport)
		{
			var broker = isExport ? shipment.ExportBroker : shipment.ImportBroker;
			var originOrDischarge = isExport ? shipment.Consols[0].LoadPort : shipment.Consols[0].DischargePort;

			if (broker != null && originOrDischarge != null && IsAUOrNZOrUSPort(originOrDischarge))
			{
				var orgProxybranch = ShipmentJobDecCreateHelper.GetBranchWithSamePortCode(originOrDischarge, broker);
				if (orgProxybranch != null && ShipmentJobDecCreateHelper.ShouldImportJobDeclaration(orgProxybranch))
				{
					var declarationExists = shipment.Declarations.Cast<IBaseJobDeclaration>()
													.Select(declaration => shipment.Factory.Load<GlbBranch>(declaration.JE_GB))
													.Any(branch => branch != null && branch.GB_GC == orgProxybranch.GB_GC);
					if (!declarationExists)
					{
						var userContext = ShipmentJobDecCreateHelper.GetUserContextForBranch(orgProxybranch);
						if (userContext != null)
						{
							using (Env.SetTemporaryUserContext(userContext))
							{
								try
								{
									CreateDeclarationFromDeclarationGenerator(shipment, shipmentValue.Invoices, context);
								}
								catch (Exception ex)
								{
									if (ex.IsCriticalException())
									{
										throw;
									}

									var errorMesg = Res.GetString("ac60b82a-1bed-4d44-8d9c-d78064b55960", "Cannot Create Declaration for Shipment with House Bill {0} - {1}", shipment.JS_HouseBill, ex.Message);
									context.Notify(new WarningNotification(WarningType.Warning, errorMesg));
								}
							}
						}
					}
				}
			}
		}
#if DEBUG
		protected virtual
#endif
 void CreateDeclarationFromDeclarationGenerator(ForwardingShipment shipment, Xsd.InvoiceHeaderCollection invoices, IValueObjectImportContext context)
		{
			MethodInfo newInfo = ObjectFactory.GetType<IShipmentDeclarationGenerator>().GetMethod("New", BindingFlags.Static | BindingFlags.Public);
			IShipmentDeclarationGenerator generator = (IShipmentDeclarationGenerator)newInfo.Invoke(null, null);

			if (generator != null)
			{
				generator.CreateDeclarationForShipment(shipment, invoices, context);
			}
		}

		bool IsAUOrNZOrUSPort(RefUNLOCO originOrDischarge)
		{
			return (originOrDischarge.RL_RN_NKCountryCode == Core.Constants.CountryCodes.NewZealand ||
				originOrDischarge.RL_RN_NKCountryCode == Core.Constants.CountryCodes.Australia ||
				originOrDischarge.RL_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates);
		}

		#endregion

		protected override void ExportShipments(ForwardingConsol consol, Xsd.Consol consolValue, IValueObjectExportContext context)
		{
			if (shipmentToExportFilter == null)
			{
				base.ExportShipments(consol, consolValue, context);
			}
			else if (consol.Shipments.Contains(shipmentToExportFilter))
			{
				consolValue.Shipments.Add(GetNewShipmentValueObjectDataAdapter(consol).ExportToValueObject(shipmentToExportFilter, context));
			}
		}

		protected override void ExportContainers(ForwardingConsol consol, Xsd.Consol consolValue, IValueObjectExportContext context)
		{
			if (shipmentToExportFilter == null)
			{
				base.ExportContainers(consol, consolValue, context);
			}
			else if (consol.Shipments.Contains(shipmentToExportFilter))
			{
				var adapter = new ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(consol);
				consolValue.ConsolDetail.Containers = new Xsd.ContainerCollection();

				foreach (ForwardingContainer container in consol.Containers)
				{
					if (shipmentToExportFilter.OuterPackLines.OfType<ForwardingPackLine>().SelectMany(pack => pack.Containers).Contains(container))
					{
						consolValue.ConsolDetail.Containers.Add(adapter.ExportToValueObject(container, context));
					}
				}
			}
		}

		readonly ForwardingShipment shipmentToExportFilter;
		readonly Order orderToExportFilter;

		protected override void ExportAddresses(ForwardingConsol consol, Xsd.Consol consolValue, IValueObjectExportContext context)
		{
			DocAddressValueObjectHelper addressHelper = new DocAddressValueObjectHelper("");
			addressHelper.ExportToValueObjectCollection(consol.DocAddresses, consolValue.ConsolDetail.Addresses.DocAddress, context);
		}

		protected override void ImportAddresses(ForwardingConsol consol, Xsd.Consol consolValue, IValueObjectImportContext context)
		{
			DocAddressValueObjectHelper addressHelper = new DocAddressValueObjectHelper("");
			addressHelper.ImportFromValueObjectCollection(consolValue.ConsolDetail.Addresses.DocAddress, consol.DocAddresses, context);
		}
	}
}
