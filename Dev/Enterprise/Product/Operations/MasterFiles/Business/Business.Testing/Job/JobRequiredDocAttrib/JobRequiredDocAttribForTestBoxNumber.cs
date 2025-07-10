using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobRequiredDocAttribForTestBoxNumber : JobRequiredDocAttrib
	{
		public JobRequiredDocAttribForTestBoxNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override JobRequiredDocument RequiredDocument => (JobRequiredDocument)Factory.Load(typeof(JobRequiredDocumentForTestBoxNumber), D0_EQ);
	}
}
