using System.Data;
using System.Diagnostics;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eManifest.Integration;

namespace Enterprise.eManifest.Business
{
	[CodeProperty(Schema.DO_UniqueReference), DescriptionProperty(Schema.DO_MasterBillNumber)]
	[DebuggerDisplay("PK = {PK} NO = {DO_UniqueReference}")]
	public class ELoadList : AutoELoadList, IELoadList
	{
		public ELoadList(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("9ffb7677-c1f9-40e4-bfe8-bc5e7f86cb97", "eLoadList {0}", DO_UniqueReference);
			}
		}
	}
}
