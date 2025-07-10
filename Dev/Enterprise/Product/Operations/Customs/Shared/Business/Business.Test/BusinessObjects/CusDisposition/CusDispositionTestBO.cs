using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusDispositionTestBO : BaseJobDeclaration, ICusDispositionParent
	{
		public CusDispositionTestBO(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		ZString ICusDispositionParent.Type
		{
			get { return "TST"; }
		}

		ZString ICusDispositionParent.GetStatusDescription(ZString status)
		{
			return status;
		}

		ZString ICusDispositionParent.ParentTableCode
		{
			get { return "JE"; }
		}

		BusinessObject ICusDispositionParent.CollectionMaster
		{
			get { return this; }
		}
	}
}
