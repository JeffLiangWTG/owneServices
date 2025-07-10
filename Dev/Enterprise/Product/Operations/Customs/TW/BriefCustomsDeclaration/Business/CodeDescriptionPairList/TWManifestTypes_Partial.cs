using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public partial class TWManifestTypes
	{
		public IManifestType X1 => x1 ?? (x1 = new ManifestType(
			Codes.ImportDocuments,
			Descriptions.ImportDocuments,
			transportModes,
			new[] { ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration },
			MessageLevel.Manifest,
			ShipmentTypeList.Import23Only()));
		IManifestType x1;

		public IManifestType X2 => x2 ?? (x2 = new ManifestType(
			Codes.ImportLowValueDutyFreeGoods,
			Descriptions.ImportLowValueDutyFreeGoods,
			transportModes,
			new[] { ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration },
			MessageLevel.Manifest,
			ShipmentTypeList.Import23Only()));
		IManifestType x2;

		public IManifestType X3 => x3 ?? (x3 = new ManifestType(
			Codes.ImportLowValueDutiableGoods,
			Descriptions.ImportLowValueDutiableGoods,
			transportModes,
			new[] { ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration },
			MessageLevel.Manifest,
			ShipmentTypeList.Import23Only()));
		IManifestType x3;

		public IManifestType X6 => x6 ?? (x6 = new ManifestType(
			Codes.ExportDocuments,
			Descriptions.ExportDocuments,
			transportModes,
			new[] { ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration },
			MessageLevel.Manifest,
			ShipmentTypeList.Export22Only()));
		IManifestType x6;

		public IManifestType X7 => x7 ?? (x7 = new ManifestType(
			Codes.ExportLowValueGoods,
			Descriptions.ExportLowValueGoods,
			transportModes,
			new[] { ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration },
			MessageLevel.Manifest,
			ShipmentTypeList.Export22Only()));
		IManifestType x7;

		public IReadOnlyList<IManifestType> All => new[] { X1, X2, X3, X6, X7 };

		static readonly IEnumerable<string> transportModes = new[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Sea };
	}
}
