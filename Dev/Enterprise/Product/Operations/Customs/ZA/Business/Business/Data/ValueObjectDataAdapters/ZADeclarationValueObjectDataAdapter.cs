using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;

using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.ZA.Business.Data
{
	public class ZADeclarationValueObjectDataAdapter : DeclarationValueObjectDataAdapter, Integration.Customs.ZA.IZADeclarationValueObjectDataAdapter
	{
		public ZADeclarationValueObjectDataAdapter()
		{
		}

		public ZADeclarationValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
			: base(triggeredByEvents)
		{
		}

		protected override string AddInfoPrefix
		{
			get { return "UZ_"; }
		}

		protected override void ExportConsolValues(Xsd.Consol consolValue, BaseJobDeclaration jobDec, IValueObjectExportContext context)
		{
			JobDeclaration zAJobDec = jobDec as JobDeclaration;
			base.ExportConsolValues(consolValue, jobDec, context);

			if (zAJobDec != null && zAJobDec.JE_MasterBillIssuedDate.IsValid)
			{
				consolValue.ConsolDetail.MasterBillIssueDate = zAJobDec.JE_MasterBillIssuedDate;
			}
		}

		protected override void ExportShipmentValues(Xsd.Shipment toShipment, BaseJobDeclaration jobDec, IValueObjectExportContext context)
		{
			base.ExportShipmentValues(toShipment, jobDec, context);

			if (jobDec.HouseBillIssuedDate.IsValid)
			{
				toShipment.ShipmentDetails.HBLIssueDate = jobDec.HouseBillIssuedDate;
			}
			else if (jobDec.IsExport && jobDec.IsSea && jobDec.Shipment != null && jobDec.Shipment.JS_HouseBillIssueDate.IsValid)
			{
				toShipment.ShipmentDetails.HBLIssueDate = jobDec.Shipment.JS_HouseBillIssueDate;
			}
		}

		protected override void ExportDeclarationDetails(Xsd.Declaration xsdDec, BaseJobDeclaration jobDec, IValueObjectExportContext context)
		{
			JobDeclaration zAJobDec = jobDec as JobDeclaration;
			base.ExportDeclarationDetails(xsdDec, jobDec, context);

			if (zAJobDec != null && !zAJobDec.JE_CustomsOffice.IsEmpty)
			{
				Xsd.ZADeclaration zaCountryData = new Xsd.ZADeclaration();
				zaCountryData.DistrictOffice = zAJobDec.JE_CustomsOffice;
				xsdDec.CountryPayload.ZADeclaration = zaCountryData;
			}
		}

		protected override void ImportConsolDetails(BaseJobDeclaration jobDec, Xsd.Consol consolValue, IValueObjectImportContext context)
		{
			JobDeclaration zAJobDec = jobDec as JobDeclaration;

			base.ImportConsolDetails(jobDec, consolValue, context);

			if (zAJobDec != null && consolValue.ConsolDetail.MasterBillIssueDate.IsValid)
			{
				zAJobDec.JE_MasterBillIssuedDate = consolValue.ConsolDetail.MasterBillIssueDate;
			}
		}

		protected override void ImportShipmentDetails(BaseJobDeclaration jobDec, Xsd.Consol consol, Xsd.Shipment shipment, IValueObjectImportContext context)
		{
			base.ImportShipmentDetails(jobDec, consol, shipment, context);

			if (shipment.ShipmentDetails.HBLIssueDate.IsValid)
			{
				jobDec.HouseBillIssuedDate = shipment.ShipmentDetails.HBLIssueDate;
			}
		}

		protected override void ImportDeclarationDetails(BaseJobDeclaration jobDec, Xsd.Declaration declarationXsd, ZString masterBillOnConsol, IValueObjectImportContext context)
		{
			base.ImportDeclarationDetails(jobDec, declarationXsd, masterBillOnConsol, context);

			Xsd.ZADeclaration declarationCountryData = declarationXsd.CountryPayload.ZADeclaration;
			if (declarationCountryData.IsSpecified)
			{
				JobDeclaration zAJobDec = jobDec as JobDeclaration;
				context.SetPropertyInfoValue(zAJobDec.JE_CustomsOfficeInfo, declarationCountryData.DistrictOffice);
			}
		}

		protected override bool AllowExportOfFirstArrivalPort
		{
			//First Arrival Port and Dates are not being used in ZA Customs
			get { return false; }
		}
	}
}
