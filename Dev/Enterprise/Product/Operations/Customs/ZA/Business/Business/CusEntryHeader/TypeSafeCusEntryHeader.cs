using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	public partial class CusEntryHeader
	{
		public new JobDeclaration Declaration => base.Declaration as JobDeclaration;

		public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

		public new JobComInvoiceHeader[] InvoiceHeaders => (JobComInvoiceHeader[])base.InvoiceHeaders;

		public new JobComInvoiceHeader RandomHeader => (JobComInvoiceHeader)base.RandomHeader;

		public new CusEntryHeaderLookups Lookups => (CusEntryHeaderLookups)base.Lookups;

		public new WeightUQCalculator WeightCalculator => (WeightUQCalculator)base.WeightCalculator;

		public new Customs.Business.AllCusEntryLineCollection<CusEntryLine> AllEntryLines => (Customs.Business.AllCusEntryLineCollection<CusEntryLine>)base.AllEntryLines;

		public new Customs.Business.CusEntryPayInfoCollection<CusEntryPayInfo> EntryPayInfos => (Customs.Business.CusEntryPayInfoCollection<CusEntryPayInfo>)base.EntryPayInfos;

		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		[ChildEditable(true)]
		public new EDIMessageCollection Messages => (EDIMessageCollection)base.Messages;

		CusEntryHeader EntryHeader => this;

		#endregion

		#region Overridden 'CreateNew' methods

		protected override Messaging.Business.EDIMessageCollection GetNewMessageCollection()
		{
			return new EDIMessageCollection(EntryHeader);
		}

		protected override Customs.Business.IAllCusEntryLineCollection<Customs.Business.CusEntryLine> GetAllEntryLinesCollection() => new Customs.Business.AllCusEntryLineCollection<CusEntryLine>(EntryHeader);

		protected override Customs.Business.BusinessObjects.Interfaces.ICusEntryPayInfoCollection<Customs.Business.CusEntryPayInfo> CreateNewEntryPayInfosCollection() => new Customs.Business.CusEntryPayInfoCollection<CusEntryPayInfo>(this);

		#endregion

		#region Merged Lines

		public new Customs.Business.ICusEntryLineCollection<CusEntryLine> MergedLines => (Customs.Business.ICusEntryLineCollection<CusEntryLine>)base.MergedLines;

		public ICusEntryLine[] MergedLinesArray => (ICusEntryLine[])MergedLines.ToArray(typeof(ICusEntryLine));

		protected override Customs.Business.ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection() => new Customs.Business.CusEntryLineCollection<CusEntryLine>(this);

		#endregion
	}
}
