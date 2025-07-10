using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ExportAWBAccountingInformationCollection : Forwarding.AWB.Business.ExportAWBAccountingInformationCollection
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
			((ExportAWBAccountingInformation)child).EA_Sequence = (ZByte)(Count > 0
				? this.Cast<ExportAWBAccountingInformation>().Max(accInfo => accInfo.EA_Sequence) + 1
				: 0);
		}

		protected override bool AllowSort
		{
			get { return false; }
		}

		public new ExportAWBAccountingInformation this[int index]
		{
			get { return (ExportAWBAccountingInformation)(Elements[index]); }
		}

		public new ExportAWBAccountingInformation AddNew()
		{
			return (ExportAWBAccountingInformation)base.AddNew();
		}

		public new ExportAWBAccountingInformation AddNew(Type bizObjType)
		{
			return (ExportAWBAccountingInformation)base.AddNew(bizObjType);
		}

		public new ExportAWBHeader Master
		{
			get { return (ExportAWBHeader)base.Master; }
		}
	}
}
