using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class ReportingBookAccountingJournalPrintOptionRegistryItem : StronglyTypedRegistryItem<ReportingBookAccountingJournalPrintOptionCollection>
	{
		public ReportingBookAccountingJournalPrintOptionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new ReportingBookAccountingJournalPrintOptionRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.ReportingBookAccountingJournalPrintOptionRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	public class ReportingBookAccountingJournalPrintOptionRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ReportingBookAccountingJournalPrintOptionCollection>
	{
		public ReportingBookAccountingJournalPrintOptionRegistryDataType()
		{
		}

		protected override void ValidateCore(IRegistryItem registryItem, ReportingBookAccountingJournalPrintOptionCollection proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			proposedValue.CurrentFallbackLevel = new FallbackLevel(companyPK, branchPK, departmentPK);
			proposedValue.Cast<ReportingBookAccountingJournalPrintOption>().ForEach(x => x.CurrentFallbackLevel = new FallbackLevel(companyPK, branchPK, departmentPK));
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
		}
	}
}
