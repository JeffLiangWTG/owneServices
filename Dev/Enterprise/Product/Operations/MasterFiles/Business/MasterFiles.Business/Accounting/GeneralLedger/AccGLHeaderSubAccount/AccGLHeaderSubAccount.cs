using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccGLHeaderSubAccount : AutoAccGLHeaderSubAccount
	{
		public AccGLHeaderSubAccount(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		AccGLHeader Parent => Factory.Load<AccGLHeader>(ASA_AG);

		[MaxLength(3)]
		public ZString ASA_SubClassDisplayName
		{
			get
			{
				return SubAccountCodeConverter.ConvertSubAccountDBParentTableCodeToSubClassCode(ASA_SubClass);
			}
			set
			{
				ASA_SubClass = SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateASA_SubClassDisplayName();
				}
				ASA_SubClassDisplayNameInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ASA_SubClassDisplayNameInfo
		{
			get { return GetZPropertyInfo(nameof(ASA_SubClassDisplayName)); }
		}

		public override bool CanDelete
		{
			get
			{
				if (IsInDatabase)
				{
					ErrorMessageWhenDeleteUsedData = Validation.IsUsedByTransactionSubAccountType((ZString)this.ASA_SubClassInfo.OriginalValue);
					return ErrorMessageWhenDeleteUsedData.IsNullOrEmpty();
				}
				return true;
			}
		}

		string ErrorMessageWhenDeleteUsedData { get; set; }

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				return string.IsNullOrEmpty(ErrorMessageWhenDeleteUsedData) ? null : ResString.GetMultilingualString("74844e8e-3e89-4941-93f8-a8bc39416939",
					"Sub account type can not be deleted because this account is currently in use. It is used by {0}.", ErrorMessageWhenDeleteUsedData);
			}
		}

		public ZInt ASA_Sequence => Parent.AG_SubAccountTypeList.IndexOfCode(ASA_SubClassDisplayName) + 1;

		public ZPropertyInfo ASA_SequenceInfo => GetZPropertyInfo(nameof(ASA_Sequence));
	}
}
