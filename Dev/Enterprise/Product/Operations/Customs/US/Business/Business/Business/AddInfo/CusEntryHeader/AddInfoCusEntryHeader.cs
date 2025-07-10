using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class AddInfoCusEntryHeader : AddInfo
	{
		public AddInfoCusEntryHeader(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		public CusEntryHeader EntryHeader
		{
			get { return (CusEntryHeader)Parent; }
		}

		[ReadOnlyMember(nameof(US_R_IsHMFApplicable_ReadOnly))]
		public override ZString US_R_IsHMFApplicable
		{
			get { return base.US_R_IsHMFApplicable; }
			set { base.US_R_IsHMFApplicable = value; }
		}

		internal bool US_R_IsHMFApplicable_ReadOnly
		{
			get
			{
				var result = false;
				if (EntryHeader.IsReconImportEntry)
				{
					result = US_R_NoLineDetails;
				}
				return result;
			}
		}

		public new AddInfoCusEntryHeaderLookups Lookups
		{
			get { return (AddInfoCusEntryHeaderLookups)base.Lookups; }
		}

		public new AddInfoCusEntryHeaderValidation Validation
		{
			get { return (AddInfoCusEntryHeaderValidation)base.Validation; }
		}

		protected override USAddInfoLookups GetNewLookups()
		{
			return new AddInfoCusEntryHeaderLookups(this);
		}

		protected override USAddInfoValidation GetNewValidation()
		{
			if (EntryHeader.IsReconImportEntry)
			{
				return new ReconAddInfoOriginalCusEntryHeaderValidation(this);
			}
			else
			{
				return new AddInfoCusEntryHeaderValidation(this);
			}
		}

		protected override bool IsExportCore
		{
			get
			{
				JobDeclaration declaration = EntryHeader.Declaration;
				return (declaration != null && declaration.IsExport);
			}
		}

		protected override ZString GetTransportMode()
		{
			return EntryHeader.Declaration != null
				? EntryHeader.Declaration.JE_TransportMode
				: (ZString)Core.Constants.TransportModes.Unknown;
		}

		protected override SchemaColumn[] ColumnsForFastSearch
		{
			get
			{
				return new SchemaColumn[]
				{
					USAddInfoSchema.US_TIBExpiryDate,
					USAddInfoSchema.US_ALDate
				};
			}
		}

		protected override BusinessObject UseWrappedPropertiesOnly()
		{
			var entry = EntryHeader;
			return entry.ReconOriginalEntry is ReconOriginalEntryHeader reconOriginalEntry ? (BusinessObject)reconOriginalEntry : entry;
		}
	}
}
