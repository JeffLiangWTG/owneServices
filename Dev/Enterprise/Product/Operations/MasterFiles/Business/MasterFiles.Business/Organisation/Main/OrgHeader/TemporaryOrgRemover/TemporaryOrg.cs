using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.TemporaryOrgRemover
{
	public class TemporaryOrg : NonPersistentBusinessObject, IObsoleteValidation
	{
		public TemporaryOrg(ZGuid orgPK, ZString code, ZString fullName)
		{
			fOrgPK = orgPK;
			fCode = code;
			fFullName = fullName;
		}

		public static class Schema
		{
			public const string FullName = "FullName";
			public const string Code = "Code";
			public const string ChildTableForFailedDelete = "ChildTableForFailedDelete";
		}

		#region Properties

		#region OrgPK

		public ZGuid OrgPK
		{
			get { return fOrgPK; }
		}

		readonly ZGuid fOrgPK;

		#endregion

		#region Code

		[MaxLength(OrgHeader.Schema.OH_CodeMaxLength)]
		public ZString Code
		{
			get { return fCode; }
		}

		readonly ZString fCode;

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(Schema.Code); }
		}

		#endregion

		#region FullName

		[MaxLength(OrgHeader.Schema.OH_FullNameMaxLength)]
		public ZString FullName
		{
			get { return fFullName; }
		}

		readonly ZString fFullName;

		public ZPropertyInfo FullNameInfo
		{
			get { return GetZPropertyInfo(Schema.FullName); }
		}

		#endregion

		#region ChildTableForFailedDelete

		[MaxLength(255)]
		public ZString ChildTableForFailedDelete
		{
			get { return childTableForFailedDelete; }
			set
			{
				if (childTableForFailedDelete != value)
				{
					CheckMaximumLength(ChildTableForFailedDeleteInfo, value);
					SetNonPersistentPropertyValue(ChildTableForFailedDeleteInfo, ref childTableForFailedDelete, value);
				}
			}
		}
		ZString childTableForFailedDelete;

		public ZPropertyInfo ChildTableForFailedDeleteInfo
		{
			get { return GetZPropertyInfo(Schema.ChildTableForFailedDelete); }
		}

		#endregion

		#region IncludeInDelete

		public ZBool IncludeInDelete
		{
			get { return includeInDelete; }
			set { SetNonPersistentPropertyValue(IncludeInDeleteInfo, ref includeInDelete, value); }
		}

		ZBool includeInDelete = ZBool.True;

		public ZPropertyInfo IncludeInDeleteInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeInDelete)); }
		}

		#endregion

		#endregion
	}
}
