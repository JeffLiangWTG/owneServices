using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccTemplateFileStorage : AutoAccTemplateFileStorage
	{
		public AccTemplateFileStorage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			DataRow row = ((IBusinessObjectInternals)this).Row;

			row[AccTemplateFileStorageSchema.Constants.TFS_GC] = Env.CurrentCompanyPK;
			row[AccTemplateFileStorageSchema.Constants.TFS_Ledger] = LedgerTypesList.Codes.AccountsReceivable;
			row[AccTemplateFileStorageSchema.Constants.TFS_ExternalReference] = System.Guid.Empty;
		}

		public bool IsDuplicateOf(AccTemplateFileStorage exTemplateFile)
		{
			return TFS_Ledger == exTemplateFile.TFS_Ledger
				&& TFS_GC == exTemplateFile.TFS_GC
				&& TFS_Code == exTemplateFile.TFS_Code;
		}

		public override bool CanDelete => !ConfigurationExistsForThisTemplate();

		bool ConfigurationExistsForThisTemplate()
		{
			var filter = new ZQuery(AccEInvoicingTemplateFileViewSchema.ETF_TemplateCode, TFS_Code);
			return Factory.Exists(typeof(AccEInvoicingTemplateFileView), filter);
		}

		public ZString TFS_ExternalReference_ForBinding
		{
			get => TFS_ExternalReference.IsValid ? TFS_ExternalReference.ToString() : externalReference_ForEdit.ToString();
			set
			{
				if (ZGuid.TryParse(value, out var guidValue))
				{
					TFS_ExternalReference = guidValue;
				}
				else if (value.IsEmpty)
				{
					TFS_ExternalReference = ZGuid.Empty;
				}
				else
				{
					TFS_ExternalReference = ZGuid.Invalid;
				}

				externalReference_ForEdit = value;
			}
		}

		ZString externalReference_ForEdit;

		public ZPropertyInfo TFS_ExternalReference_ForBindingInfo => GetZPropertyInfo(nameof(TFS_ExternalReference));

		public override ZString TFS_FileName
		{
			get => base.TFS_FileName;
			set
			{
				if (value.IsEmpty && TFS_FileName != value)
				{
					base.TFS_FileData = ZBlob.Empty;
				}

				base.TFS_FileName = value;
			}
		}

		protected bool TFS_FileName_ReadOnly => TFS_FileData.IsEmpty;

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("AccTemplateFileStorage|CannotDeleteError", "Delete operation is not allowed for templates which are already configured.");
	}
}
