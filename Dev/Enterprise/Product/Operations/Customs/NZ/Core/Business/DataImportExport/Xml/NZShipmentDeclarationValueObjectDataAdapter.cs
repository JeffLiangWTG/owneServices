
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using JobDeclaration = Enterprise.Customs.NZ.Business.Declaration.JobDeclaration;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.NZ.Business.Data
{
	public class NZShipmentDeclarationValueObjectDataAdapter : ShipmentDeclarationValueObjectDataAdapter, Integration.Customs.NZ.INZShipmentDeclarationValueObjectDataAdapter
	{
		public NZShipmentDeclarationValueObjectDataAdapter()
		{
		}

		public NZShipmentDeclarationValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
			: base(triggeredByEvents)
		{
		}

		protected override BaseJobDeclaration GetJobDeclaration(IValueObjectImportContext context)
		{
			BaseJobDeclaration declaration = JobDeclaration.New(context.Factory);

			if (((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Target.Type == Xsd.InterchangeInfoTargetType.LowValueDeclaration)
			{
				declaration.JE_MessageSubType = Enterprise.Customs.NZ.Business.JobMessageSubTypeList.Codes.WriteOff;
			}
			else
			{
				declaration.JE_MessageSubType = Enterprise.Customs.NZ.Business.JobMessageSubTypeList.Codes.Normal;
			}
			return declaration;
		}
	}
}
