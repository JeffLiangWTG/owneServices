using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer
{
	[Immutable]
	public class MessageTypeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		MessageTypeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(JobMessageTypeList.Codes.Export, nameof(Xsd.ShipmentType.EXP));
			yield return new Mapping(JobMessageTypeList.Codes.Import, nameof(Xsd.ShipmentType.IMP));
			yield return new Mapping(JobMessageTypeList.Codes.ExWarehouse, nameof(Xsd.ShipmentType.EXW));
			yield return new Mapping(JobMessageTypeList.Codes.MiscellaneousCustoms, nameof(Xsd.ShipmentType.MSC));
			yield return new Mapping(JobMessageTypeList.Codes.ExportDeclarationByExternalBroker, nameof(Xsd.ShipmentType.EXX));
			yield return new Mapping(JobMessageTypeList.Codes.ImportDeclarationByExternalBroker, nameof(Xsd.ShipmentType.IMX));
			yield return new Mapping(JobMessageTypeList.Codes.Drawback, nameof(Xsd.ShipmentType.DRW));
		}

		public static readonly MessageTypeToXmlCodeMappings Instance = new MessageTypeToXmlCodeMappings();

		protected override string Name
		{
			get { return Res.GetString("6b3e4f9b-ae21-4d0c-9a22-09e2a5d1b90b", "Message Type"); }
		}

		public new Xsd.ShipmentType GetExternalCode(string enterpriseCode, string errorContext, INotifications notifications)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.ShipmentType.EXP, errorContext, notifications);
		}
	}
}
