using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.ISF.DataTransfer
{
	[Immutable]
	class BillTypeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		BillTypeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(Common.US.ISF.BillTypeList.Codes.OceanBillOfLading, nameof(Xsd.ISFReferenceIDType.OB));
			yield return new Mapping(Common.US.ISF.BillTypeList.Codes.HouseBillOfLading, nameof(Xsd.ISFReferenceIDType.BM));
			yield return new Mapping(Common.US.ISF.BillTypeList.Codes.USCBPEntryNumber, nameof(Xsd.ISFReferenceIDType.Item6B));
			yield return new Mapping(Common.US.ISF.BillTypeList.Codes.MasterBillOfLading, nameof(Xsd.ISFReferenceIDType.MB));
			yield return new Mapping(Common.US.ISF.BillTypeList.Codes.ISFBondNumber, nameof(Xsd.ISFReferenceIDType.SBN));
			yield return new Mapping(Common.US.ISF.BillTypeList.Codes.SuretyCode, nameof(Xsd.ISFReferenceIDType.V1));
			yield return new Mapping(Common.US.ISF.BillTypeList.Codes.CarnetIssuingCountryCodeAndCarnetNumber, nameof(Xsd.ISFReferenceIDType.Item6C));
			yield return new Mapping(Common.US.ISF.BillTypeList.Codes.BondReferenceNumber, nameof(Xsd.ISFReferenceIDType.BRN));
			yield return new Mapping(Common.US.ISF.BillTypeList.Codes.UserDefinedReferenceNumber, nameof(Xsd.ISFReferenceIDType.CR));
			yield return new Mapping(Common.US.ISF.BillTypeList.Codes.FullNameOfISFImporter, nameof(Xsd.ISFReferenceIDType.FN));
		}

		public static readonly BillTypeToXmlCodeMappings Instance = new BillTypeToXmlCodeMappings();

		protected override string Name
		{
			get { return "ISF Bill Type"; }
		}

		public new Xsd.ISFReferenceIDType GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.ISFReferenceIDType.Item6B, errorContext, notify);
		}
	}
}
