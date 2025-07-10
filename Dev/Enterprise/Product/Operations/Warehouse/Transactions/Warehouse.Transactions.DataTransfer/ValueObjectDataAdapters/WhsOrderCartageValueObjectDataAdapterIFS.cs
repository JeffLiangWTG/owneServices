using System;
using System.Xml.Schema;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WhsOrderCartageValueObjectDataAdapterIFS : WhsValueObjectDataAdapter<WhsOrder, Xsd.ConNote>
	{
		#region Export

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		protected override void ExportToValueObjectCore(WhsOrder bizObj, Xsd.ConNote conNote, IValueObjectExportContext context)
		{
			var transportBillTo = bizObj.TransportBillTo;

			if (bizObj.TransportCoDocAddress.Organisation != null)
			{
				conNote.AccNo = bizObj.GetTransportBillToAccountCodeWithTransportCo();
				conNote.CarrierName = GetForeignCodeFromIFSOrgProxy(bizObj.TransportCoDocAddress.Organisation); // must match a SmartFreight carrier name
			}

			conNote.AdHocRec = "Y"; // this will NOT add the receiver address to SmartFreight's DB

			if (transportBillTo != null)
			{
				if (transportBillTo.PK == bizObj.Client.PK || transportBillTo.PK == bizObj.Warehouse.WarehouseAddress.OA_OH)
				{
					conNote.ChargeTo = "S";
				}
				else if (transportBillTo.PK == bizObj.ConsigneePK)
				{
					conNote.ChargeTo = "R";
				}
				else
				{
					conNote.ChargeTo = "T";
					conNote.TpyAdHoc = "Y";
					ExportTransportBillToAddress(bizObj.TransportBillToDocAddress, conNote);
				}
			}

			if (bizObj.CarrierServiceLevel != null)
			{
				conNote.Service = bizObj.CarrierServiceLevel.PL_CarrierServiceLevelDescription; // must match SmartFreight list
			}

			ExportLineDetails(bizObj, conNote);
			ExportSenderAddress(bizObj.Client, bizObj.Warehouse.WarehouseAddress, conNote);
			ExportReceiverAddress(bizObj.ConsigneeDocAddress, conNote);
			ExportHandlingInstructions(bizObj, conNote.SpIns);

			AddExportEvent(conNote, bizObj, context);
		}

		#region ExportLineDetails

		void ExportLineDetails(WhsOrder bizObj, Xsd.ConNote conNote)
		{
			var conNoteLine = conNote.FreightLineDetails.AddNew();
			var helper = new WhsOrderCartageHelper(bizObj);

			conNoteLine.Amt = helper.OrderLinesPackageCount;
			conNoteLine.Desc = helper.OrderLinesGoodsTypeDescription;

			conNoteLine.Ref = bizObj.WD_ExternalReference;
			conNoteLine.Wgt = bizObj.WD_WeightSent.ToString(2); // req'd by IFS
			conNoteLine.Cube = bizObj.WD_CubicSent.ToString(3); // req'd by IFS
		}

		#endregion

		#region ExportSenderAddress, ExportReceiverAddress

		void ExportSenderAddress(OrgHeader client, OrgAddress senderAddress, Xsd.ConNote conNote)
		{
			conNote.SendName = GetForeignCodeFromIFSOrgProxy(client);
			ExportOrgAddress(senderAddress, conNote.SendAddr);
		}

		void ExportReceiverAddress(JobDocAddress docAddress, Xsd.ConNote conNote)
		{
			conNote.RecAccNo = docAddress.Organisation.OH_Code;

			if (docAddress.E2_AddressOverride)
			{
				conNote.RecName = docAddress.E2_CompanyNameTruncated;
				ExportDocAddress(docAddress, conNote.RecAddr);
			}
			else
			{
				conNote.RecName = docAddress.Address.Header.OH_FullNameTruncated;
				ExportOrgAddress(docAddress.Address, conNote.RecAddr);
			}
		}

		void ExportTransportBillToAddress(JobDocAddress docAddress, Xsd.ConNote conNote)
		{
			conNote.TpyAccNo = docAddress.Organisation.OH_Code;

			if (docAddress.E2_AddressOverride)
			{
				conNote.TpyName = docAddress.E2_CompanyNameTruncated;
				ExportDocAddress(docAddress, conNote.TpyAddr);
			}
			else
			{
				conNote.TpyName = docAddress.Address.Header.OH_FullNameTruncated;
				ExportOrgAddress(docAddress.Address, conNote.TpyAddr);
			}
		}

		void ExportDocAddress(JobDocAddress docAddress, Xsd.RecAddr conNoteAddress)
		{
			conNoteAddress.add1 = docAddress.E2_Address1;
			conNoteAddress.add2 = docAddress.E2_Address2;
			conNoteAddress.add3 = docAddress.E2_City;
			conNoteAddress.add4 = docAddress.E2_State;
			conNoteAddress.add5 = docAddress.E2_Postcode;

			if (docAddress.Country != null)
			{
				conNoteAddress.add6 = docAddress.Country.Description;
			}
		}

		void ExportOrgAddress(OrgAddress address, Xsd.RecAddr conNoteAddress)
		{
			conNoteAddress.add1 = address.OA_Address1;
			conNoteAddress.add2 = address.OA_Address2;
			conNoteAddress.add3 = address.OA_City;
			conNoteAddress.add4 = address.OA_State;
			conNoteAddress.add5 = address.OA_PostCode;

			if (address.RelatedCountry != null)
			{
				conNoteAddress.add6 = address.RelatedCountry.Description;
			}
		}

		#endregion

		#region ExportHandlingInstructions

		void ExportHandlingInstructions(WhsOrder bizObj, Xsd.SpecialInstructions conNoteHandlingInstructions)
		{
			var handlingInstructions = bizObj.WD_HandlingInstructions.Split(30);

			if (handlingInstructions.Length > 0)
			{
				conNoteHandlingInstructions.Sp1 = handlingInstructions[0];
				if (handlingInstructions.Length > 1)
				{
					conNoteHandlingInstructions.Sp2 = handlingInstructions[1];
					if (handlingInstructions.Length > 2)
					{
						conNoteHandlingInstructions.Sp3 = handlingInstructions[2];
					}
				}
			}
		}

		#endregion

		#region GetForeignCodeFromIFSOrgProxy

		string GetForeignCodeFromIFSOrgProxy(OrgHeader org)
		{
			var ifsOrgProxy = GetIFSOrgProxy(org.Factory);

			if (ifsOrgProxy != null)
			{
				var filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, ifsOrgProxy.PK)
				.AddToFilter(OrgPatternMatchOverrideSchema.OO_LocalGuid, org.PK);
				return ifsOrgProxy.Factory.LoadTop1<OrgPatternMatchOverride>(filter)?.OO_ForeignCode ?? ZString.Empty;
			}

			return "";
		}

		#endregion

		#endregion

		#region Import

		protected override void ImportFromValueObjectCore(WhsOrder bizObj, Xsd.ConNote conNote, IValueObjectImportContext context)
		{
			if (bizObj.IsInDatabase) // we only ever want to update existing orders from IFS XML imports.
			{
				var calcTotalsEnabled = bizObj.CalculateTotalsEnabled;
				try
				{
					bizObj.CalculateTotalsEnabled = false;

					if (!context.NotificationsHasErrors)
					{
						ImportTransportRef(bizObj, conNote);
					}

					if (!context.NotificationsHasErrors)
					{
						ImportTransportCo(bizObj, conNote, context);
					}

					if (!context.NotificationsHasErrors)
					{
						ImportServiceLevel(bizObj, conNote, context);
					}

					if (!context.NotificationsHasErrors)
					{
						ImportFreightCharge(bizObj, conNote, context);
					}

					if (!context.NotificationsHasErrors)
					{
						FinaliseImportedOrder(bizObj, context);
					}

					if (!context.NotificationsHasErrors)
					{
						AddImportEvent(bizObj);
					}
				}
				finally
				{
					bizObj.CalculateTotalsEnabled = calcTotalsEnabled;
				}
			}
		}

		#region ImportFreightCharge

		void ImportFreightCharge(WhsOrder bizObj, Xsd.ConNote conNote, IValueObjectImportContext context)
		{
			if (bizObj.TransportBillTo != null)
			{
				var warehousePaysTransport = (bizObj.TransportBillTo.PK == bizObj.Warehouse.WarehouseAddress.OA_OH);

				if (warehousePaysTransport)
				{
					if (bizObj.TransportCoPK.IsValid)
					{
						CreateFreightCharge(bizObj, conNote, context);
					}
					else
					{
						context.Notify(new ErrorNotification(ErrorType.ImportingDataError,
							Res.GetString("64400fc9-8dfc-4858-aca4-0faa880fad76", "Freight Charges were found but cannot be imported because the Order has no Transport Company.")));
					}
				}
			}
		}

		void CreateFreightCharge(WhsOrder bizObj, Xsd.ConNote conNote, IValueObjectImportContext context)
		{
			var chargeCode = GetChargeCode(bizObj.Factory, context);

			if (chargeCode != null)
			{
				var currency = GetChargeCurrency(bizObj.Factory);

				if (currency != null)
				{
					var billingAdapter = CreateBillingDataAdapter();
					var billingObject = CreateBillingValueObject(bizObj, conNote, chargeCode, currency);
					billingAdapter.ImportFromValueObject(bizObj, billingObject, context);
				}
			}
		}

		AccChargeCode GetChargeCode(BusinessObjectFactory factory, IValueObjectImportContext context)
		{
			var chargeCode = factory.Load<AccChargeCode>(RatingDataRegistry.Instance.WarehouseCartageChargeCode.Value);

			if (chargeCode == null)
			{
				context.Notify(new ErrorNotification(ErrorType.ImportingDataError,
					Res.GetString("e4f2cfd2-5da4-47ed-92ba-53d8a23775ad",
						"The registry ({0}) does not have a default Charge Code to use.",
						((IRegistryItemInternals)RatingDataRegistry.Instance.WarehouseCartageChargeCode).Location)));
			}

			return chargeCode;
		}

		RefCurrency GetChargeCurrency(BusinessObjectFactory factory)
		{
			return factory.Load<RefCurrency>(Env.CurrentCompany.Country.Currency.PK);
		}

		IValueObjectDataAdapter CreateBillingDataAdapter()
		{
			return (IValueObjectDataAdapter)Activator.CreateInstance(ObjectFactory.GetType<Accounting.Integration.IBillingDataAdapter>());
		}

		Xsd.Billing CreateBillingValueObject(WhsOrder bizObj, Xsd.ConNote conNote, AccChargeCode chargeCode, RefCurrency currency)
		{
			var totalOrdersOnConNote = GetTotalOrdersOnConNote(conNote);

			var billingObject = new Xsd.Billing();
			var chargeLine = billingObject.ChargeLines.AddNew();

			chargeLine.ChargeCode = chargeCode.AC_Code;
			chargeLine.Description = conNote.ConNo;

			var totalCostAmount = Utilities.Round(conNote.TotCost, currency.Decimals);
			var costForThisOrder = Utilities.Round(totalCostAmount / totalOrdersOnConNote, currency.Decimals);

			var totalSellAmount = Utilities.Round(conNote.TotCostPlusMrkup, currency.Decimals);
			var sellForThisOrder = Utilities.Round(totalSellAmount / totalOrdersOnConNote, currency.Decimals);

			// The 'LastOrderToUpdate' flag is used to tell the DataAdapter that this is the last order on this conNote value object. The DataAdapter then checks that the   
			// costs applied to each order will match the total cost for the ConNote. If there is a discrepancy caused by rounding, the billing on the last order is adjusted as required.

			if (IsLastOrderToUpdate && totalOrdersOnConNote > 1)
			{
				var apportionedCost = costForThisOrder * totalOrdersOnConNote;
				var differenceInCost = totalCostAmount - apportionedCost;
				if (differenceInCost != 0)
				{
					costForThisOrder += differenceInCost;
				}

				var apportionedSell = sellForThisOrder * totalOrdersOnConNote;
				var differenceInSell = totalSellAmount - apportionedSell;
				if (differenceInSell != 0)
				{
					sellForThisOrder += differenceInSell;
				}
			}

			chargeLine.OSCostAmount.CurrencyCode = currency.RX_Code;
			chargeLine.OSCostAmount.Value = costForThisOrder;
			chargeLine.Creditor.EDICode = bizObj.GetTransportCo().OH_Code;

			chargeLine.OSSellAmount.CurrencyCode = currency.RX_Code;
			chargeLine.OSSellAmount.Value = sellForThisOrder;

			return billingObject;
		}

		int GetTotalOrdersOnConNote(Xsd.ConNote conNote)
		{
			var result = 0;
			foreach (Xsd.FreightLineDetails freightDetail in conNote.FreightLineDetails)
			{
				if (!freightDetail.Ref.IsEmpty)
				{
					result++;
				}
			}
			return result;
		}

		#endregion

		#region ImportTransportRef

		void ImportTransportRef(WhsOrder bizObj, Xsd.ConNote conNote)
		{
			bizObj.WD_TransportReference = conNote.ConNo;
		}

		#endregion

		#region ImportTransportCo

		void ImportTransportCo(WhsOrder bizObj, Xsd.ConNote conNote, IValueObjectImportContext context)
		{
			if (!conNote.CarrierName.IsEmpty)
			{
				var newTransportCoPK = GetOrgPKFromIFSOrgProxy(conNote.CarrierName, context);

				if (newTransportCoPK != ZGuid.Empty)
				{
					bizObj.TransportCoPK = newTransportCoPK;
				}
				else
				{
					context.Notify(new ErrorNotification(ErrorType.ImportingDataError,
						Res.GetString("c5665da6-596d-44ef-8ac8-91bb3e5ab2c0", "Transport Company ({0}) does not exist in {1}.", conNote.CarrierName, Core.Constants.ProductName)));
				}
			}
		}

		ZGuid GetOrgPKFromIFSOrgProxy(string foreignCode, IValueObjectImportContext context)
		{
			var ifsOrg = GetIFSOrgProxy(context.Factory);

			if (ifsOrg != null)
			{
				var filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, ifsOrg.PK)
				.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, foreignCode);
				return ifsOrg.Factory.LoadTop1<OrgPatternMatchOverride>(filter)?.OO_LocalGuid ?? ZGuid.Empty;
			}

			return ZGuid.Empty;
		}

		#endregion

		#region ImportServiceLevel

		void ImportServiceLevel(WhsOrder bizObj, Xsd.ConNote conNote, IValueObjectImportContext context)
		{
			var transportCo = bizObj.GetTransportCo();
			if (transportCo != null && !conNote.Service.IsEmpty)
			{
				var qry = new ZQuery(OrgCarrierServiceLevelSchema.PL_CarrierServiceLevelDescription, conNote.Service);
				qry.AddToFilter(OrgCarrierServiceLevelSchema.PL_OM, transportCo.MiscServ.PK);

				var service = context.Factory.LoadTop1<OrgCarrierServiceLevel>(qry);
				if (service != null)
				{
					bizObj.WD_PL_NKCarrierServiceLevel = service.PL_Code;
				}
				else
				{
					context.Notify(new ErrorNotification(ErrorType.ImportingDataError,
						Res.GetString("7a7dc30b-d070-45fe-9f76-11f61e8629ed", "Service Level ({0}) does not exist in {1}.", conNote.Service, Core.Constants.ProductName)));
				}
			}
		}

		#endregion

		#region FinaliseImportedOrder

		void FinaliseImportedOrder(WhsOrder order, IValueObjectImportContext context)
		{
			if (SystemDataRegistry.Instance.FinaliseOrderOnCartageImport.Value && !order.IsFinalised)
			{
				if (!order.FinaliseDocketAlwaysFinalisingPick())
				{
					context.Notify(new WarningNotification(Res.GetString("ffd15f37-7b71-411e-a5f4-c5e95b8ddf9f", "Order No. {0} was imported but could not be finalized due to validation error(s).", order.WD_ExternalReference)));
				}
			}
		}

		#endregion

		#region FindBusinessObject

		protected override WhsOrder FindBusinessObject(Xsd.ConNote conNote, IValueObjectImportContext context)
		{
			WhsOrder result = null;

			if (OrderExternalReference != null && conNote.SendName != "")
			{
				var filter = new ZQuery();
				filter.AddToFilter(WhsDocketSchema.WD_OH_Client, GetOrgPKFromIFSOrgProxy(conNote.SendName, context));
				filter.AddToFilter(WhsDocketSchema.WD_ExternalReference, OrderExternalReference);
				filter.AddToFilter(WhsDocketSchema.WD_DocketType, "ORD");

				result = context.Factory.LoadTop1<WhsOrder>(filter);

				if (result == null)
				{
					context.Notify(new ErrorNotification(ErrorType.ImportingDataError, Res.GetString("3e35cdff-f381-40c7-82f7-c9d77e0be71c", "The related Warehouse Order for the SmartFreight Connote was not found.")));
				}
			}

			return result;
		}

		#endregion

		#endregion

		#region Boring Overrides

		public override string RootCollectionElementName => "ConNoteObject";

		public override string RootElementName => "ConNote";

		public override XmlSchema CollectionSchema => WarehouseXmlSchemaDefinitions.Instance.WhsDocketsIFSSchema;

		public override XmlSchema Schema => WarehouseXmlSchemaDefinitions.Instance.SingleWhsDocketIFSSchema;

		#endregion

		#region OrderExternalReference

		public string OrderExternalReference { get; set; }

		#endregion

		#region IsLastOrderToUpdate

		public bool IsLastOrderToUpdate { get; set; }

		#endregion

		#region GetIFSOrgProxy

		OrgHeader GetIFSOrgProxy(BusinessObjectFactory factory)
		{
			return factory.Load<OrgHeader>(WarehouseDataRegistry.Instance.IFSOrgProxy.Value);
		}

		#endregion
	}
}
