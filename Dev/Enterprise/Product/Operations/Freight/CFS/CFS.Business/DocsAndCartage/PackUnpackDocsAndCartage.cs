using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class PackUnpackDocsAndCartage : CFSDocsAndCartage
	{
		public PackUnpackDocsAndCartage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Related Business Objects

		public new TallyServiceDependentCollection Services
		{
			get { return (TallyServiceDependentCollection)base.Services; }
		}

		protected override JobServiceDependentCollection GetNewServiceCollection()
		{
			return new TallyServiceDependentCollection(this, Factory);
		}

		public new PackUnpackRequiredDocumentDependentCollection RequiredDocuments
		{
			get { return (PackUnpackRequiredDocumentDependentCollection)base.RequiredDocuments; }
		}

		protected override JobRequiredDocumentDependentCollection GetNewRequiredDocumentCollection()
		{
			return new PackUnpackRequiredDocumentDependentCollection(this, Factory);
		}

		#endregion
	}
}
