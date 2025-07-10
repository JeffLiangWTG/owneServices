//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmNumberRangeMatchingDetailValidation
//
//    This class should be used for overriding validation in AutoStmNumberRangeMatchingDetailValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.EntityFramework;
	using ZArchitecture.Schema;

	public class StmNumberRangeMatchingDetailValidation : AutoStmNumberRangeMatchingDetailValidation
	{
		public StmNumberRangeMatchingDetailValidation(AutoStmNumberRangeMatchingDetail parent) : base(parent)
		{
		}

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidatePatentNumber();
			ValidateCustomsArea();
		}

		#endregion

		#region CheckPatentNumber

		public void ValidatePatentNumber()
		{
			ValidateCalculatedProperty(Parent.PatentNumberInfo);
		}

		protected void CheckPatentNumber()
		{
			if (Parent.IsPatentNumber)
			{
				CheckIsUniqueByMatchingDetails(Parent.PatentNumberInfo);
			}
		}

		#endregion

		#region CheckCustomsArea

		public void ValidateCustomsArea()
		{
			ValidateCalculatedProperty(Parent.CustomsAreaInfo);
		}

		protected void CheckCustomsArea()
		{
			if (Parent.IsPatentNumber)
			{
				ListValidation.ErrorIfInvalidCodeOrEmpty(Parent.CustomsAreaInfo);
				CheckIsUniqueByMatchingDetails(Parent.CustomsAreaInfo);
			}
		}

		#endregion

		#region CheckNRM_OwnerId

		protected override void CheckNRM_OwnerId()
		{
			base.CheckNRM_OwnerId();
			CheckHasLinkedFountain(Parent.NRM_OwnerIdInfo);
			CheckIsUniqueByMatchingDetails(Parent.NRM_OwnerIdInfo);
			CheckPrefixIsUnique(Parent.NRM_OwnerIdInfo);
		}

		#endregion

		#region CheckNRM_Prefix

		protected override void CheckNRM_Prefix()
		{
			base.CheckNRM_Prefix();
			if (!Parent.Lookups.Prefixes.ContainsCode(""))
			{
				MandatoryValidation.CheckEntered(Parent.NRM_PrefixInfo);
			}
			ListValidation.ErrorIfInvalidCode(Parent.NRM_PrefixInfo);
			CheckHasLinkedFountain(Parent.NRM_PrefixInfo);
			CheckPrefixIsUnique(Parent.NRM_PrefixInfo);
		}

		#endregion

		#region CheckNRM_RangeType

		protected override void CheckNRM_RangeType()
		{
			base.CheckNRM_RangeType();
			ListValidation.ErrorIfInvalidCode(Parent.NRM_RangeTypeInfo);
			CheckHasLinkedFountain(Parent.NRM_RangeTypeInfo);
			CheckIsUniqueByMatchingDetails(Parent.NRM_RangeTypeInfo);
			CheckPrefixIsUnique(Parent.NRM_RangeTypeInfo);
		}

		#endregion

		#region CheckNRM_OH_Client

		protected override void CheckNRM_OH_Client()
		{
			base.CheckNRM_OH_Client();
			if (Parent.IsTransportReferenceNumbers)
			{
				CheckIsUniqueByMatchingDetails(Parent.NRM_OH_ClientInfo);
			}
		}

		#endregion

		#region CheckNRM_WW_Whs

		protected override void CheckNRM_WW_Whs()
		{
			base.CheckNRM_WW_Whs();
			if (Parent.IsTransportReferenceNumbers)
			{
				CheckIsUniqueByMatchingDetails(Parent.NRM_WW_WhsInfo);
			}
		}

		#endregion

		#region CheckNRM_OwnerTableCode

		protected override void CheckNRM_OwnerTableCode()
		{
			base.CheckNRM_OwnerTableCode();
			if (!ValidOwnerTableCodes.Contains(Parent.NRM_OwnerTableCode))
			{
				Parent.NRM_OwnerTableCodeInfo.AddError(Res.GetString("StmNumberRangeMatchingDetailsValidation|ValidateNRM_OwnerTableCode", "Please enter a valid owner table code."));
			}
		}

		HashSet<string> ValidOwnerTableCodes
		{
			get { return new HashSet<string> { OrgHeaderSchema.Constants.Prefix, GlbStaffSchema.Constants.Prefix }; }
		}

		#endregion

		#region CheckHasLinkedFountain

		void CheckHasLinkedFountain(ZPropertyInfo propertyInfo)
		{
			var parent = Parent;
			if (parent != null && parent.LinkedFountain == null)
			{
				propertyInfo.AddError(Res.GetString("StmNumberRangeMatchingDetailsValidation|LinkedFountain", "Matching details have no number range."));
			}
		}

		#endregion

		#region CheckIsUniqueByMatchingDetails

		void CheckIsUniqueByMatchingDetails(ZPropertyInfo propertyInfo)
		{
			var allMatchingDetails = LoadAllMatchingDetailsForOwner();
			if ((Parent.IsTransportReferenceNumbers && allMatchingDetails.Any(md => md.NRM_OH_Client == Parent.NRM_OH_Client && md.NRM_WW_Whs == Parent.NRM_WW_Whs))
				|| (Parent.IsPatentNumber && allMatchingDetails.Any(md => md.NRM_MatchingKey == Parent.NRM_MatchingKey)))
			{
				propertyInfo.AddError(Res.GetString("StmNumberRangeMatchingDetailsValidation|CheckIsUniqueByMatchingDetails", "The matching details have been duplicated and must be unique."));
			}
		}

		#endregion

		#region CheckPrefixIsUnique

		void CheckPrefixIsUnique(ZPropertyInfo propertyInfo)
		{
			// Transport reference number prefixes can be duplicated across different carriers, so we only need to check for uniqueness on one parent
			// Patent number prefixes can be duplicated across different staffs, so we only need to check for uniqueness on one parent
			// For other range types may need a new unique index and more specific validation 
			var allMatchingDetails = LoadAllMatchingDetailsForOwner();
			if (allMatchingDetails.Any(md => md.NRM_Prefix == Parent.NRM_Prefix))
			{
				propertyInfo.AddError(Res.GetString("StmNumberRangeMatchingDetailsValidation|CheckPrefixIsUnique", "The Prefix must be unique for this Range Type."));
			}
		}

		#endregion

		#region LoadAllMatchingDetailsForOwner

		IEnumerable<StmNumberRangeMatchingDetail> LoadAllMatchingDetailsForOwner()
		{
			var query = new ZQuery(StmNumberRangeMatchingDetailSchema.NRM_OwnerId, Parent.NRM_OwnerId);
			return Parent.Factory.Load<StmNumberRangeMatchingDetail>(query).Where(md => md.PK != Parent.PK && md.NRM_RangeType == Parent.NRM_RangeType);
		}

		#endregion

		#region Parent

		public new StmNumberRangeMatchingDetail Parent
		{
			get { return (StmNumberRangeMatchingDetail)base.Parent; }
		}

		#endregion
	}
}
