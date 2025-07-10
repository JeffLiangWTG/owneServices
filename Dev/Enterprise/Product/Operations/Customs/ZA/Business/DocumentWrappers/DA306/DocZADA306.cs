using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	[DefaultField(nameof(JobNumber))]
	public class DocZADA306 : DocumentWrapper
	{
		DocZADA306(ForwardingShipment objectToWrap, BusinessObjectFactory factory) : base(objectToWrap, factory)
		{
			docShipment = DocShipment.New(objectToWrap, Factory);
		}

		public static DocZADA306 New(ForwardingShipment shipment, BusinessObjectFactory factoryToWrap)
		{
			return shipment == null ? null : new DocZADA306(shipment, factoryToWrap);
		}

		ForwardingShipment Shipment => (ForwardingShipment)WrappedObject;

		readonly DocShipment docShipment;

		#region Fields

		public ZString DeclarantAddress => GetDeclarantAddress();
		public ZString TransportDocumentNumber => docShipment.MasterBillNumber;
		public ZDateTime DateOfTransportDocumentNumber => docShipment.MasterBillIssueDate;
		public ZString FlightNumber => docShipment.FlightNo;
		public ZDateTime DateOfFlight => docShipment.FlightDate;
		public ZInt TotalNumberOfPackages => Lines.Count;
		public ZDecimal TotalCustomsValueInZAR => GetTotalCustomsValueInZAR();
		public ZString MarksNumbers => docShipment.MarksAndNumbers;
		public ZString DescriptionOfGoods => docShipment.GoodsDescription;
		public ZString PlaceOfEntry => GetPlaceOfEntry();
		public ZString ImportPermitAndAmount => ZString.Empty;
		public ZString ContainerNumbers => GetContainerNumbers();
		public ZString DocumentsAssortedDutiables => ZString.Empty;

		public DocumentWrapperCollection<DocZADA306Line> Lines => lines ??= new DocZADA306LineCollection(Shipment.HVLVItemCollectionForDocument);
		DocumentWrapperCollection<DocZADA306Line> lines;

		public ZString JobNumber => docShipment.JobNumber;
		#endregion

		ZString GetDeclarantAddress()
		{
			var company = (GlbCompany)Env.CurrentCompany;
			var address = company as IAddressDetails;
			var countryDisplay = (company.OrgProxy?.Country ?? company.Country)?.RN_DescMultilingual ?? (NoResString)string.Empty;
			var formatter = new AddressFormatter(Factory, address.CompanyName, "", address.AddressLine1, address.AddressLine2, address.City, address.State, address.PostCode, countryDisplay, "", true, Core.Constants.CountryCodes.SouthAfrica);
			return formatter.PostalAddressAsASingleLineWithoutCompanyName();
		}

		ZDecimal GetTotalCustomsValueInZAR()
		{
			var total = 0m;
			foreach (IHVLVItemForDocument item in Shipment.HVLVItemCollectionForDocument)
			{
				var totalLinesCustomsValue = item.Lines.Cast<IHVLVItemLine>().Sum(x => x.HVS_CustomsValue);
				total += CurrencyConverter.ConvertRounded(new Money(totalLinesCustomsValue, new Currency(item.Consignment.HVC_RX_NKGoodsValueCurrency)), RefCurrency.LoadFromCurrencyCode(Factory, "ZAR")).Amount;
			}
			return total;
		}

		ZString GetPlaceOfEntry()
		{
			ZString placeOfEntry = string.Empty;
			if (Shipment.JS_RL_NKDestination == "ZAJNB")
			{
				placeOfEntry = "JSA";
			}
			else if (Shipment.JS_RL_NKDestination == "ZACPT")
			{
				placeOfEntry = "DFM";
			}

			return placeOfEntry;
		}

		ZString GetContainerNumbers()
		{
			var numbers = Shipment.HVLVItemCollectionForDocument.Cast<IHVLVItemForDocument>().Select(x => x.HVI_ContainerNumber).Distinct();
			return string.Join(",", numbers);
		}

		CurrencyConverter CurrencyConverter => currencyConverter ??= CurrencyConverter.New(Factory, ZDateTime.Today, ZArchitecture.Core.ExchangeRateType.Customs, roundToTargetCurrencyDecimals: true);
		CurrencyConverter currencyConverter;
	}
}
