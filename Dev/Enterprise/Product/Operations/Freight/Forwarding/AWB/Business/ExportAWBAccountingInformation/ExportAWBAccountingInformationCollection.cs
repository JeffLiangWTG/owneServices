using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class ExportAWBAccountingInformationCollection : DependentBusinessObjectCollection<ExportAWBAccountingInformation, ExportAWBHeader>
	{
		public ExportAWBAccountingInformationCollection(ExportAWBHeader master, BusinessObjectFactory factory)
			: base(master, factory)
		{
			Sort(ExportAWBAccountingInformationSchema.EA_Sequence.Name);
		}

		public override void Load()
		{
			base.Load();
			Sort(ExportAWBAccountingInformationSchema.EA_Sequence.Name);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (Count > 0)
			{
				((ExportAWBAccountingInformation)child).EA_Sequence = (ZByte)(this.Cast<ExportAWBAccountingInformation>().Max(accInfo => accInfo.EA_Sequence) + 1);
			}
		}

		protected override bool AllowSort
		{
			get { return false; }
		}
	}
}
