using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class GoodsDetailBuilder
	{
		internal GoodsDetail Build(ForwardingShipment shipmentBO, ForwardingContainer containerBOForPackingLineFilter = null, string pcs = "")
		{
			if (shipmentBO == null)
			{
				return null;
			}
			var context = new CommonContext(shipmentBO.Factory);

			var goodsDetail = new GoodsDetail(shipmentBO.PK, pcs);
			goodsDetail.HouseBillNumber = shipmentBO.JS_HouseBill;
			goodsDetail.ShipmentNumber = shipmentBO.JS_UniqueConsignRef;
			goodsDetail.ShipmentWeight = new Measurement
			{
				Value = Constants.Weight.Convert(shipmentBO.JS_ActualWeight, shipmentBO.JS_UnitOfWeight, Constants.Weight.Kilograms)
			};
			goodsDetail.DeclaredPackWeight = new Measurement
			{
				Value = ZDecimal.Zero
			};

			goodsDetail.PortOfOrigin = Unloco.Create(context, shipmentBO.Origin);
			goodsDetail.RequiresTemperatureControl = shipmentBO.OuterPackLines.Cast<PackLine>().Any(p => p.JL_RequiresTemperatureControl);
			goodsDetail.HazardousCargo = shipmentBO.OuterPackLines.Cast<PackLine>().Any(p => p.UNDGs.Any());

			goodsDetail.PackageType = new CodeDescription(shipmentBO.Lookups.PackTypes)
			{
				Code = shipmentBO.JS_F3_NKPackType
			};

			goodsDetail.MarksAndNumbers = shipmentBO.JS_MarksAndNumbers;
			goodsDetail.GoodsDescription = shipmentBO.JS_GoodsDescription;

			goodsDetail.TaxAmount = new Money
			{
				Currency = new CodeDescription(context.Currencies as IFindBoxListProvider)
				{
					Code = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency
				}
			};

			goodsDetail.GoodsHandlingNotes = shipmentBO.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description)?.FirstOrDefault()?.ST_NoteDataAsText ?? ZString.Empty;

			goodsDetail.PackingLines = GetPackingLines(shipmentBO, containerBOForPackingLineFilter).ToArray();

			var referenceExportConventional = shipmentBO.Numbers.GetFirstReferenceNumberByTypeAndCountry(FranceAdditionalReferenceNumberTypes.Codes.ExportConventional, Core.Constants.CountryCodes.France);
			if (referenceExportConventional != null && !referenceExportConventional.CE_EntryNum.IsEmpty)
			{
				goodsDetail.CommodityReference = referenceExportConventional.CE_EntryNum;
			}

			PopulateAddresses(context, goodsDetail, shipmentBO);

			return goodsDetail;
		}

		IEnumerable<BookingPackingLine> GetPackingLines(ForwardingShipment shipmentBO, ForwardingContainer containerBO)
		{
			var packLineBuilder = new BookingPackingLineBuilder();
			return shipmentBO.OuterPackLines.Cast<PackLine>().Where(x => containerBO == null || x.JL_Calc_ContainerNum == containerBO.JC_ContainerNum).Select(x => packLineBuilder.Build(x));
		}

		void PopulateAddresses(CommonContext context, GoodsDetail goodsDetail, ForwardingShipment shipmentBO)
		{
			goodsDetail.Consignee = AddressBuilder.Create(context, shipmentBO.ConsigneeDocumentaryAddress);
			goodsDetail.Shipper = AddressBuilder.Create(context, shipmentBO.ConsignorDocumentaryAddress);
			goodsDetail.NotifyParty = AddressBuilder.Create(context, shipmentBO.NotifyPartyDocumentaryAddress);
			goodsDetail.NotifyParty2 = AddressBuilder.Create(context, shipmentBO.NotifyParty2DocumentaryAddress);
			goodsDetail.ThirdParty = AddressBuilder.Create(context, shipmentBO.DeliveryAgent?.MainAddress);
			goodsDetail.ThirdPartySON = shipmentBO.DeliveryAgent?.MainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			goodsDetail.ThirdPartyCI5 = shipmentBO.DeliveryAgent?.MainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
		}
	}
}
