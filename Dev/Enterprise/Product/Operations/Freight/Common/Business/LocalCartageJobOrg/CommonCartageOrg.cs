using System.Data;
using System.Diagnostics;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Common.Business
{
	[CodeProperty(CommonCartageOrg.Schema.DistinctCode), DescriptionProperty(CommonCartageOrg.Schema.OrgTypeFullDescription)]
	public class CommonCartageOrg : AutoLocalCartageJobOrg, ICanDelete
	{
		public CommonCartageOrg(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		public new class Schema : AutoLocalCartageJobOrg.Schema
		{
			public const string OrgTypeFullDescription = "OrgTypeFullDescription";
			public const string DistinctCode = "DistinctCode";
		}

		#region Overrides

		public override void Delete()
		{
			CommonCartageType.AllCartageLegTypes.RemoveByCartageOrg(this.PK);
			base.Delete();
		}

		#endregion

		#region Propety Overrides

		#region E5_E3

		[RelatedBusinessObject("CommonCartageType")]
		public override ZGuid E5_E3
		{
			[DebuggerStepThrough()]
			get { return base.E5_E3; }
			set
			{
				if (base.E5_E3 == value)
				{
					base.E5_E3 = value;
				}
				else
				{
					base.E5_E3 = value;

					if (!E5_E3.IsEmpty)
					{
						CartageTypeAttached();
					}
				}
			}
		}

		void CartageTypeAttached()
		{
			if (CommonCartageType != null && CommonCartageType.CommonCartageOrganisations.OrgWithBillToParty == null)
			{
				E5_IsBillToParty = true;
			}
		}

		#endregion

		#region E5_OrgType_ReadOnly

		[List("Lookups.OrgTypeList")]
		public override ZString E5_OrgType
		{
			get { return base.E5_OrgType; }
			set { base.E5_OrgType = value; }
		}

		protected bool E5_OrgType_ReadOnly
		{
			get { return CommonCartageType != null && CommonCartageType.E3_IsSystem; }
		}

		#endregion

		#endregion

		#region Related Objects

		public CommonCartageType CommonCartageType
		{
			get { return Factory.Load<CommonCartageType>(E5_E3); }
		}

		#endregion

		#region New Properties

		#region IsMSC

		public ZBool IsMSC
		{
			get { return E5_OrgType == LocalCartageJobOrgTypeList.Codes.MSC; }
		}

		#endregion

		#region DistinctCode

		public ZString DistinctCode
		{
			get
			{
				string code = E5_OrgType;
				if (!E5_UsageComment.IsEmpty)
				{
					code += " - " + E5_UsageComment;
				}
				else
				{
					code += E5_UsageComment;
				}
				return code;
			}
		}

		#endregion

		#region OrgTypeFullDescription

		[MaxLength(Schema.E5_UsageCommentMaxLength + 10)]
		public ZString OrgTypeFullDescription
		{
			get
			{
				string orgDesc = Lookups.OrgTypeList.GetDescriptionFromCode(E5_OrgType);
				if (!E5_UsageComment.IsEmpty)
				{
					orgDesc += " - ";
				}
				orgDesc += E5_UsageComment;
				return orgDesc;
			}
		}

		public ZPropertyInfo OrgTypeFullDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.OrgTypeFullDescription); }
		}

		#endregion

		#endregion

		#region ICanDelete Members

		bool ICanDelete.CanDelete
		{
			get { return !CommonCartageType.E3_IsSystem; }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("760d3d9f-31cd-4963-810c-6b04f3add6db", "Port Transport Job Type is System Defined. This Local Transport Organization cannot be deleted."); }
		}

		#endregion
	}
}
