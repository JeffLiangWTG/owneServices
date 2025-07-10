using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public interface ICusEntryCPDecParent
	{
		ZString Prefix { get; }
		CusEntryCPDecCollection CPDecCollection { get; }
	}

	public class CusEntryCPDecCollection : DependentBusinessObjectCollection<CusEntryCPDec, BusinessObject>
	{
		public CusEntryCPDecCollection(BusinessObject master) : base(master)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusEntryCPDecSchema.ON_ParentID; }
		}
	}
}
