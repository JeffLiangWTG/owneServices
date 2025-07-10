using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class SecondaryNotifyParty : CusCodeData
	{
		public SecondaryNotifyParty(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString CY_Code
		{
			get { return base.CY_Code; }
			set
			{
				ZString oldValue = CY_Code;
				base.CY_Code = value;
				if (!IsCopying && oldValue != CY_Code)
				{
					MarkParentAsNeedingValidation();
				}
			}
		}

		public override ZString CY_Data
		{
			get { return base.CY_Data; }
			set
			{
				ZString oldValue = CY_Data;
				base.CY_Data = value;
				if (!IsCopying && oldValue != CY_Data)
				{
					MarkParentAsNeedingValidation();
				}
			}
		}

		public override ZGuid CY_ParentID
		{
			get { return base.CY_ParentID; }
			set
			{
				ZGuid oldValue = CY_ParentID;
				base.CY_ParentID = value;
				if (!IsCopying && oldValue != CY_ParentID)
				{
					MarkParentAsNeedingValidation();
				}
			}
		}

		public override ZString CY_ParentTableCode
		{
			get { return base.CY_ParentTableCode; }
			set
			{
				ZString oldValue = CY_ParentTableCode;
				base.CY_ParentTableCode = value;
				if (!IsCopying && oldValue != CY_ParentTableCode)
				{
					MarkParentAsNeedingValidation();
				}
			}
		}

		public override void OnSaving()
		{
			if (CY_Data.IsEmpty)
			{
				Delete();
			}
			base.OnSaving();
		}

		public new SecondaryNotifyPartyLookups Lookups
		{
			get { return (SecondaryNotifyPartyLookups)base.Lookups; }
		}

		public new SecondaryNotifyPartyValidation Validation
		{
			get { return (SecondaryNotifyPartyValidation)base.Validation; }
		}

		#region Implementation

		protected override CusCodeDataLookups GetNewLookups()
		{
			return new SecondaryNotifyPartyLookups(this);
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new SecondaryNotifyPartyValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.SecondaryNotifyParty;
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(CusInBondMoveDetail)); }
		}

		#endregion

	}
}
