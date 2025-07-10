using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class USDeclarationFSISLineCollection : DependentCusAddInfoCollection<USDeclarationFSISLine, JobDeclaration>
	{
		public USDeclarationFSISLineCollection(JobDeclaration declaration)
			: base(declaration, CusAddInfoTypeAttribute.Codes.USDeclarationFSISCertificate)
		{
		}

		protected override CargoWise.Schema.SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusAddInfoSchema.B7_ParentID; }
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newLine = (USDeclarationFSISLine)child;
			if (Master != null)
			{
				newLine.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
				newLine.US_DateOfInspection = Master.US_InspecDate;
				newLine.US_ImportingEstNo = Master.US_FSISInspec;
			}
		}
	}
}
