using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	public class CusEntrySnapshot : AutoCusEntrySnapshot, ITypeDeciderContext
	{
		public CusEntrySnapshot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public static readonly CusEntrySnapshotTypeDecider TypeDecider = new CusEntrySnapshotTypeDecider();

		[RelatedBusinessObject("EntryHeader")]
		public override ZGuid CES_CH_EntryHeader
		{
			get { return base.CES_CH_EntryHeader; }
			set { base.CES_CH_EntryHeader = value; }
		}

		public CusEntryHeader EntryHeader => Factory.Load<CusEntryHeader>(CES_CH_EntryHeader);

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country => (EntryHeader as ITypeDeciderContext)?.Country ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		#endregion
	}
}
