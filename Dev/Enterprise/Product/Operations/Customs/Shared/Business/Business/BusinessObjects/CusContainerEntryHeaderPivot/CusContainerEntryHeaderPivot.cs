using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	public class CusContainerEntryHeaderPivot : AutoCusContainerEntryHeaderPivot
	{
		public CusContainerEntryHeaderPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(Container))]
		public override ZGuid CCE_CO_Container
		{
			get { return base.CCE_CO_Container; }
			set { base.CCE_CO_Container = value; }
		}

		[RelatedBusinessObject(nameof(EntryHeader))]
		public override ZGuid CCE_CH_EntryHeader
		{
			get { return base.CCE_CH_EntryHeader; }
			set { base.CCE_CH_EntryHeader = value; }
		}

		public BaseCusContainer Container => Factory.Load<BaseCusContainer>(CCE_CO_Container);

		public CusEntryHeader EntryHeader => Factory.Load<CusEntryHeader>(CCE_CH_EntryHeader);
	}
}
