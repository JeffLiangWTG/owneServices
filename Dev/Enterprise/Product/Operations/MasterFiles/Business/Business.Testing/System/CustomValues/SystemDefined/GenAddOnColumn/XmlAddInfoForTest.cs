using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	sealed class XmlAddInfoForTest : TestXmlAddInfo
	{
		public XmlAddInfoForTest(DummyBusinessObject dummy)
			: base(dummy)
		{
		}

		protected override SchemaColumn[] ColumnsForFastSearch
		{
			get { return new SchemaColumn[] { TestXmlAddInfoSchema.Z3_Int }; }
		}
	}
}
