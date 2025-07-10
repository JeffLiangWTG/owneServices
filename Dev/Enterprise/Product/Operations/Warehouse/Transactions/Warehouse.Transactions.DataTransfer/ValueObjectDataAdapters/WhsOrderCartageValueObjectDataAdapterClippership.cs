using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WhsOrderCartageValueObjectDataAdapterClippership : WhsValueObjectDataAdapter<WhsOrder, Xsd.CartageJob>
	{
		#region ValueObjectDataAdapter Setup Overrides

		public override string RootCollectionElementName => "CartageJobs";

		public override string RootElementName => "CartageJob";

		public override XmlSchema Schema => FreightXmlSchemaDefinitions.Instance.SingleCartageJobSchema;

		public override XmlSchema CollectionSchema => FreightXmlSchemaDefinitions.Instance.CartageJobsSchema;

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(WhsOrder order, Xsd.CartageJob xsdCartage, IValueObjectExportContext context)
		{
			if (order.IsInDatabase
#if DEBUG
 || Globals.IsTest
#endif
)
			{
				xsdCartage.ClientJobReference = order.WD_ExternalReference;
				xsdCartage.InsuranceAmount = order.WD_LocalCartInsuranceCost; // usually get this on import
				xsdCartage.InsuranceAmountSpecified = true;
				xsdCartage.TransportReference = order.WD_TransportReference; // usually get this on import
				xsdCartage.ServiceLevel = order.WD_PL_NKCarrierServiceLevel;

				ExportAddresses(order, xsdCartage, context);
				ExportCartageLeg(order, xsdCartage, context);
				ExportHandlingInstructions(order, xsdCartage);
				ExportOuterPacks(order, xsdCartage);

				AddExportEvent(xsdCartage, order, context);

				xsdCartage.Type = "MISC";
				xsdCartage.JobNumber = order.WD_DocketID;
			}
		}

		#region ExportAddresses

		void ExportAddresses(WhsOrder order, Xsd.CartageJob xsdCartage, IValueObjectExportContext context)
		{
			var clientAddress = JobDocAddress.GetOrCreateNonPersistantDocAddress(order, DocAddressType.ConsignorPickupDeliveryAddress, order.Client.MainAddress.PK);

			xsdCartage.Consignor = new DocAddressValueObjectHelper(Res.GetString("5792464c-169a-46b4-9a7b-6edeb9893c15", "Client / Consignor")).ExportToValueObject(clientAddress, context);
			xsdCartage.CarrierAddress = new DocAddressValueObjectHelper(Res.GetString("05bd7f10-6a57-4e5c-92ea-6110304b694a", "Transport Company")).ExportToValueObject(order.TransportCoDocAddress, context);
			xsdCartage.BillToAddress = new DocAddressValueObjectHelper(Res.GetString("30fa2160-f750-49cd-ab4f-4091e6357f2d", "Bill To")).ExportToValueObject(order.GoodsBillToDocAddress, context);

			if (order.TransportBillToDocAddress != null)
			{
				xsdCartage.TransportBillToAddress = new DocAddressValueObjectHelper("TransportBillTo").ExportToValueObject(order.TransportBillToDocAddress, context);
			}
		}

		#endregion

		#region ExportCartageLeg

		void ExportCartageLeg(WhsOrder order, Xsd.CartageJob xsdCartage, IValueObjectExportContext context)
		{
			var xsdLeg = xsdCartage.CartageLegs.AddNew();

			xsdLeg.CartageLegDates.EstimatedDeliveryDate = order.WD_RequiredDate.ToZDateTime();
			xsdLeg.CartageLegDates.EstimatedPickupDate = order.WD_FinalisedDate.IsValid ? order.WD_FinalisedDate.ToZDateTime() : ZDateTime.Empty;

			var pickupAddress = JobDocAddress.GetOrCreateNonPersistantDocAddress(order.Warehouse, DocAddressType.PickUpAddress, order.Warehouse.WarehouseAddress.PK);
			xsdLeg.Pickup.DocAddress = new DocAddressValueObjectHelper(Res.GetString("a68d7b0f-211d-4f7a-a99b-39a89ffd1aaf", "Pickup")).ExportToValueObject(pickupAddress, context);
			xsdLeg.Delivery.DocAddress = new DocAddressValueObjectHelper(Res.GetString("7cf50f55-1060-449c-8ebf-2c882bd5b671", "Delivery")).ExportToValueObject(order.ConsigneeDocAddress, context);

			xsdLeg.Type = "MISC"; // field is deprecated with DaveB's Cartage 2.0, but atm is required to pass import schema validation
		}

		#endregion

		#region ExportHandlingInstructions

		void ExportHandlingInstructions(WhsOrder order, Xsd.CartageJob xsdCartage)
		{
			if (!order.WD_HandlingInstructions.IsEmpty)
			{
				var note = xsdCartage.Notes.AddNew();
				note.NoteData = order.WD_HandlingInstructions;
				note.NoteType = Xsd.NotesNoteNoteType.HandlingInstructions;
			}
		}

		#endregion

		#region ExportOuterPacks

		void ExportOuterPacks(WhsOrder order, Xsd.CartageJob xsdCartage)
		{
			var helper = new WhsOrderCartageHelper(order);

			xsdCartage.OuterPacks.OuterPacksVolume.Value = order.WD_TotalCubic;
			xsdCartage.OuterPacks.OuterPacksWeight.Value = order.WD_TotalWeight;
			xsdCartage.OuterPacks.OuterPacksVolume.DimensionType = order.WD_TotalCubicUnit;
			xsdCartage.OuterPacks.OuterPacksWeight.DimensionType = order.WD_TotalWeightUnit;

			xsdCartage.OuterPacks.OuterPacksNoSpecified = true;
			xsdCartage.OuterPacks.OuterPacksNo = helper.OrderLinesPackageCount;
			xsdCartage.OuterPacks.OuterPacksType = helper.OrderLinesGoodsTypeDescription;
			xsdCartage.GoodsDescription = order.GoodsDescriptionWithFallback;
		}

		#endregion

		#endregion

		#region Import

		protected override void ImportFromValueObjectCore(WhsOrder order, Xsd.CartageJob xsdCartage, IValueObjectImportContext context)
		{
			if (order.IsInDatabase) // we only ever want to update existing orders from Cartage XML imports.
			{
				var calcTotalsEnabled = order.CalculateTotalsEnabled;
				try
				{
					order.CalculateTotalsEnabled = false;

					if (!context.NotificationsHasErrors)
					{
						ImportTransportReferences(order, xsdCartage);
					}

					if (!context.NotificationsHasErrors)
					{
						ImportFreightCharges(order, xsdCartage, context);
					}

					if (!context.NotificationsHasErrors)
					{
						ImportOuterPacks(order, xsdCartage, context);
					}

					if (!context.NotificationsHasErrors)
					{
						ImportServiceLevel(order, xsdCartage);
					}

					if (!context.NotificationsHasErrors)
					{
						FinaliseImportedOrder(order, context);
					}

					if (!context.NotificationsHasErrors)
					{
						AddImportEvent(order);
					}
				}
				finally
				{
					order.CalculateTotalsEnabled = calcTotalsEnabled;
				}
			}
		}

		#region ImportOuterPacks

		void ImportOuterPacks(WhsOrder order, Xsd.CartageJob xsdCartage, IValueObjectImportContext context)
		{
			ImportWeightAndVolume(order, xsdCartage);
			ImportOuterTotals(order, xsdCartage, context);
			order.WD_GoodsDescription = xsdCartage.GoodsDescription;
		}

		void ImportWeightAndVolume(WhsOrder order, Xsd.CartageJob xsdCartage)
		{
			var outers = xsdCartage.OuterPacks;

			if (outers.OuterPacksWeight.Value > 0 && !outers.OuterPacksWeight.DimensionType.IsEmpty)
			{
				order.WD_TotalWeightUnit = outers.OuterPacksWeight.DimensionType;
				order.WD_WeightSent = outers.OuterPacksWeight.Value;
				order.WD_TotalWeight = outers.OuterPacksWeight.Value;
			}

			if (outers.OuterPacksVolume.Value > 0 && !outers.OuterPacksVolume.DimensionType.IsEmpty)
			{
				order.WD_TotalCubicUnit = outers.OuterPacksVolume.DimensionType;
				order.WD_CubicSent = outers.OuterPacksVolume.Value;
				order.WD_TotalCubic = outers.OuterPacksVolume.Value;
			}
		}

		void ImportOuterTotals(WhsOrder order, Xsd.CartageJob xsdCartage, IValueObjectImportContext context)
		{
			var helper = new WhsOrderCartageHelper(order);

			var importError = helper.SetOrderLinesPackageCount(xsdCartage.OuterPacks.OuterPacksNo, xsdCartage.OuterPacks.OuterPacksType);
			if (importError != null)
			{
				context.Notify(importError);
			}
		}

		#endregion

		#region ImportServiceLevel

		void ImportServiceLevel(WhsOrder order, Xsd.CartageJob xsdCartage)
		{
			if (!xsdCartage.ServiceLevel.IsEmpty)
			{
				if (order.CarrierServiceLevel == null || order.WD_PL_NKCarrierServiceLevel != xsdCartage.ServiceLevel)
				{
					order.WD_PL_NKCarrierServiceLevel = xsdCartage.ServiceLevel;
				}
			}
		}

		#endregion

		#region ImportTransportReferences

		void ImportTransportReferences(WhsOrder order, Xsd.CartageJob xsdCartage)
		{
			var packs = xsdCartage.CartageLegs[0].Item as Xsd.CartageLegPackageRecords;

			order.WD_TransportReference = xsdCartage.TransportReference;

			if (packs != null && packs.Packs != null)
			{
				foreach (Xsd.Package package in packs.Packs)
				{
					if (order.WD_TransportReference.IsEmpty)
					{
						order.WD_TransportReference = package.TransportRef;
					}
					else
					{
						var reference = order.References.AddNew();
						reference.WX_Reference = package.TransportRef;
						reference.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.TransportReference;
					}
				}
			}
		}

		#endregion

		#region ImportFreightCharges

		void ImportFreightCharges(WhsOrder order, Xsd.CartageJob xsdCartage, IValueObjectImportContext context)
		{
			if (xsdCartage.FreightCharges.Count > 0)
			{
				if (order.TransportCoDocAddress.HasRealAddress)
				{
					var job = (Job)order.JobHeader ?? new Job.Loader(order).TryCreate();
					var costPostDate = ZDateTime.Today;

					foreach (Xsd.FreightCharges xsdCharge in xsdCartage.FreightCharges)
					{
						if (!ImportFreightCharge(order, job, xsdCharge, costPostDate, xsdCartage.InvoiceNumber, context))
						{
							return;
						}
					}

					// post costs so that AutoRating won't overwrite them.
					if (!xsdCartage.InvoiceNumber.IsEmpty)
					{
						new APInvoiceCreator(job).CreateTransactions(new TransactionCreatorHashtable());
					}
				}
				else
				{
					context.Notify(new ErrorNotification(ErrorType.ImportingDataError,
						Res.GetString("c229aa21-3600-46de-89c5-a370b4f53409", "Freight Charges were found but cannot be imported because the Order has no Transport Company.")));
				}
			}
		}

		bool ImportFreightCharge(WhsOrder order, Job job, Xsd.FreightCharges xsdCharge, ZDateTime costPostDate, ZString invoiceNumber, IValueObjectImportContext context)
		{
			bool importChargeSuccess = false;

			var chargeCode = GetChargeCode(job.Factory, xsdCharge, context);
			if (chargeCode != null)
			{
				var currency = GetChargeCurrency(job.Factory, xsdCharge, context);
				if (currency != null)
				{
					var charge = job.Charges.AddNew();

					charge.JR_AC = chargeCode.PK;
					charge.JR_Desc = xsdCharge.Description;

					charge.JR_RX_NKCostCurrency = currency.RX_Code;
					charge.JR_OSCostAmt = xsdCharge.TotalAmount.Value;
					charge.JR_OH_CostAccount = order.TransportCoPK;
					charge.JR_APInvoiceDate = costPostDate;
					charge.JR_APInvoiceNum = invoiceNumber;

					importChargeSuccess = true;
				}
			}

			return importChargeSuccess;
		}

		AccChargeCode GetChargeCode(BusinessObjectFactory factory, Xsd.FreightCharges xsdCharge, IValueObjectImportContext context)
		{
			var chargeCodeQuery = new ZQuery();
			chargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_GC, Env.CurrentCompany.PK);
			chargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_Code, xsdCharge.ChargeCode);

			var chargeCode = factory.LoadTop1<AccChargeCode>(chargeCodeQuery);
			if (chargeCode == null)
			{
				chargeCode = factory.Load<AccChargeCode>(RatingDataRegistry.Instance.WarehouseCartageChargeCode.Value);
				if (chargeCode == null)
				{
					context.Notify(new ErrorNotification(ErrorType.ImportingDataError, Res.GetString("78a01d1e-682f-475a-98ad-3c7466efb85b",
						"No charge code was specified and the registry ({0}) does not have a default Charge Code to fall back on.",
						((IRegistryItemInternals)RatingDataRegistry.Instance.WarehouseCartageChargeCode).Location)));
				}
			}

			return chargeCode;
		}

		RefCurrency GetChargeCurrency(BusinessObjectFactory factory, Xsd.FreightCharges xsdCharge, IValueObjectImportContext context)
		{
			RefCurrency result = null;

			if (xsdCharge.TotalAmount.CurrencyCode.IsEmpty)
			{
				result = factory.Load<RefCurrency>(Env.CurrentCompany.Country.Currency.PK);
			}
			else
			{
				var currencyCode = xsdCharge.TotalAmount.CurrencyCode;
				result = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode);

				if (result == null)
				{
					context.Notify(new ErrorNotification(ErrorType.ImportingDataError, Res.GetString("694be512-dea3-446b-a487-6e187e9c94ec", "Unknown Currency '{0}' found on one of the charges.", currencyCode)));
				}
			}

			return result;
		}

		#endregion

		#region FinaliseImportedOrder

		void FinaliseImportedOrder(WhsOrder order, IValueObjectImportContext context)
		{
			if (SystemDataRegistry.Instance.FinaliseOrderOnCartageImport.Value)
			{
				if (!order.FinaliseDocketAlwaysFinalisingPick())
				{
					context.Notify(new WarningNotification(Res.GetString("5323e0de-3fa5-4673-8b55-e9becdadc15f", "Order No. {0} was imported but could not be finalized due to validation error(s).", order.WD_ExternalReference)));
				}
			}
		}

		#endregion

		#region FindBusinessObject (loading in the Order we are importing charges & references from)

		protected override WhsOrder FindBusinessObject(Xsd.CartageJob xsdCartage, IValueObjectImportContext context)
		{
			WhsOrder result = null;

			if (!xsdCartage.CartageLegs.IsSpecified || xsdCartage.CartageLegs.Count == 0)
			{
				context.Notify(new ErrorNotification(ErrorType.ImportingDataError, Res.GetString("2346ff04-780c-4ff2-b523-dfd9641c692b", "Port Transport Job does not have a Port Transport Leg.")));
			}
			else if (xsdCartage.ClientJobReference.IsEmpty)
			{
				context.Notify(new ErrorNotification(ErrorType.ImportingDataError, Res.GetString("987f38ae-fd3f-4264-bb42-0aec071ba819", "Port Transport Job does not have a reference number (Client Job Reference).")));
			}
			else
			{
				var clientPk = context.FindOrganisationPK(xsdCartage.Consignor.AddressReference.Organisation, null, OrganisationTypes.None);

				if (!clientPk.IsValid)
				{
					context.Notify(new ErrorNotification(ErrorType.ImportingDataError, Res.GetString("86cde75b-d80b-4baf-8787-67cc3ee1ef56", "Port Transport Job does not have a valid Client.")));
				}
				else
				{
					var filter = new ZQuery();
					filter.AddToFilter(WhsDocketSchema.WD_OH_Client, clientPk);
					filter.AddToFilter(WhsDocketSchema.WD_ExternalReference, xsdCartage.ClientJobReference);
					filter.AddToFilter(WhsDocketSchema.WD_DocketType, "ORD");

					result = context.Factory.LoadTop1<WhsOrder>(filter);

					if (result == null)
					{
						context.Notify(new ErrorNotification(ErrorType.ImportingDataError, Res.GetString("e9a66088-f24c-436b-9c47-a9683c3b999d", "The related Warehouse Order for the Port Transport Job was not found.")));
					}
				}
			}

			return result;
		}

		#endregion

		#endregion
	}
}
