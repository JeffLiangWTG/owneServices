using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.Customs.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class JobDeclarationAddInfoChildSupporter : BaseJobDeclarationWithAddInfo, IAddInfoChildSupporter
	{
		public JobDeclarationAddInfoChildSupporter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		DummyBusinessObject addInfoChild;
		public BusinessObject AddInfoChild => this.LoadOrCreateAddInfoChild(ref addInfoChild);

		SchemaGuidColumn IAddInfoChildSupporter.ChildForeignKeyColumn => DummyBizoSchema.Z0_Guid;
	}
}
