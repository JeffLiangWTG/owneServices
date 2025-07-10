using CargoWise.EntityFramework;
using Enterprise.Customs.Business.BusinessObjects.Interfaces;

namespace Enterprise.Customs.TW.Business
{
	public partial class CusEntryHeader
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new JobComInvoiceHeader RandomHeader => (JobComInvoiceHeader)base.RandomHeader;

		public new CusEntryHeader Clone() => (CusEntryHeader)base.Clone();

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

		public new JobComInvoiceHeader[] InvoiceHeaders() => (JobComInvoiceHeader[])base.InvoiceHeaders;

		public new Customs.Business.ICusEntryLineCollection<CusEntryLine> MergedLines => (Customs.Business.CusEntryLineCollection<CusEntryLine>)base.MergedLines;

		[ChildEditable(true)]
		public new Customs.Business.IAllCusEntryLineCollection<CusEntryLine> AllEntryLines => (Customs.Business.IAllCusEntryLineCollection<CusEntryLine>)base.AllEntryLines;

		public new CusEntryHeaderLookups Lookups => (CusEntryHeaderLookups)base.Lookups;

		[ChildEditable(true)]
		public new Customs.Business.ICusEntryHeaderChargesCollection<CusEntryHeaderCharges> Charges => (Customs.Business.CusEntryHeaderChargesCollection<CusEntryHeaderCharges>)base.Charges;

		[ChildEditable(true)]
		public new TWMessageCollection Messages => (TWMessageCollection)base.Messages;

		public new CusEntryHeaderValidation Validation => (CusEntryHeaderValidation)base.Validation;

		[ChildEditable(true)]
		public new Customs.Business.CusEntryPayInfoCollection<CusEntryPayInfo> EntryPayInfos => (Customs.Business.CusEntryPayInfoCollection<CusEntryPayInfo>)base.EntryPayInfos;

		protected override ICusEntryPayInfoCollection<Customs.Business.CusEntryPayInfo> CreateNewEntryPayInfosCollection() => new Customs.Business.CusEntryPayInfoCollection<CusEntryPayInfo>(this);

		#endregion

		#region Implementation
		protected override Enterprise.Messaging.Business.EDIMessageCollection GetNewMessageCollection() => new TWMessageCollection(this);

		protected override Customs.Business.ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection() => new Customs.Business.CusEntryLineCollection<CusEntryLine>(this);

		protected override Customs.Business.IAllCusEntryLineCollection<Customs.Business.CusEntryLine> GetAllEntryLinesCollection() => new Customs.Business.AllCusEntryLineCollection<CusEntryLine>(this);

		protected override Customs.Business.CusEntryHeaderLookups GetNewLookups() => new CusEntryHeaderLookups(this);

		protected override Customs.Business.ICusEntryHeaderChargesCollection<Customs.Business.CusEntryHeaderCharges> CreateNewCusEntryHeaderChargesCollection() => new Customs.Business.CusEntryHeaderChargesCollection<CusEntryHeaderCharges>(this);

		protected override Customs.Business.CusEntryHeaderValidation GetNewValidation() => new CusEntryHeaderValidation(this);

		#endregion
	}
}
