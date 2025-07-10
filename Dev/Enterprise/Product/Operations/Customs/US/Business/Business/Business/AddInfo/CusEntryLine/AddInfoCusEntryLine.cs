using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class AddInfoCusEntryLine : AddInfo
	{
		public AddInfoCusEntryLine(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		public CusEntryLine EntryLine
		{
			get { return (CusEntryLine)Parent; }
		}

		public new AddInfoCusEntryLineLookups Lookups
		{
			get { return (AddInfoCusEntryLineLookups)base.Lookups; }
		}

		public new AddInfoCusEntryLineValidation Validation
		{
			get { return (AddInfoCusEntryLineValidation)base.Validation; }
		}

		protected override USAddInfoLookups GetNewLookups()
		{
			return new AddInfoCusEntryLineLookups(this);
		}

		protected override USAddInfoValidation GetNewValidation()
		{
			return new AddInfoCusEntryLineValidation(this);
		}

		protected override ZString GetTransportMode()
		{
			return EntryLine.Declaration != null ? EntryLine.Declaration.JE_TransportMode : (ZString)Core.Constants.TransportModes.Unknown;
		}

		protected override bool IsExportCore
		{
			get { return (EntryLine.Declaration != null && EntryLine.Declaration.IsExport); }
		}
	}
}
