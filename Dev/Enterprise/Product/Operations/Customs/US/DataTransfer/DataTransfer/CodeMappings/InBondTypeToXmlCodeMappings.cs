using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	[Immutable]
	public class InBondTypeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		InBondTypeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(InbondTypeList.Codes.IEWarehouseWithdrawal, nameof(Xsd.USInvoiceInBondType.Item36));
			yield return new Mapping(InbondTypeList.Codes.TAndEWarehouseWithdrawal, nameof(Xsd.USInvoiceInBondType.Item37));
			yield return new Mapping(InbondTypeList.Codes.IEForeignTradeZoneWithdrawal, nameof(Xsd.USInvoiceInBondType.Item67));
			yield return new Mapping(InbondTypeList.Codes.TAndEForeignTradeZoneWithdrawal, nameof(Xsd.USInvoiceInBondType.Item68));
			yield return new Mapping(InbondTypeList.Codes.MerchandiseNOTShippedInbond, nameof(Xsd.USInvoiceInBondType.Item70));
		}

		public static readonly InBondTypeToXmlCodeMappings Instance = new InBondTypeToXmlCodeMappings();

		protected override string Name
		{
			get { return "Export InBond Type"; }
		}

		public new Xsd.USInvoiceInBondType GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.USInvoiceInBondType.Item36, errorContext, notify);
		}
	}
}
