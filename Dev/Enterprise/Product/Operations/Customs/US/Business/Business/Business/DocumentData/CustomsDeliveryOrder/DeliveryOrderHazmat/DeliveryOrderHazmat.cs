using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class DeliveryOrderHazmat : AutoDeliveryOrderHazmat
	{
		public DeliveryOrderHazmat(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[BusinessObjectTestExclude]
		public override ZString B7_ParentTableCode
		{
			get { return base.B7_ParentTableCode; }
			set
			{
				if (!value.IsEmpty && value != CusAddInfoSchema.Constants.Prefix)
				{
					throw new NotSupportedException("Setting DeliveryOrderHazmat.B7_ParentTableCode is not supported.");
				}
				base.B7_ParentTableCode = value;
			}
		}

		public new DeliveryOrderHeader Parent
		{
			get
			{
				if (B7_Type != CusAddInfoTypeAttribute.Codes.USDeliveryOrderHazmat)
				{
					fParent = null;
				}
				else if ((fParent == null) || (fParent.PK != B7_ParentID))
				{
					fParent = base.Factory.Load<DeliveryOrderHeader>(B7_ParentID);
				}
				return fParent;
			}
		}

		public ZString DisplayUNNumber
		{
			get
			{
				var hazmat = Hazmat;
				return hazmat != null ? hazmat.DG_UNNO : US_UNNumber;
			}
		}

		public override ZString US_UNNumber
		{
			get { return base.US_UNNumber; }
			set
			{
				ZString oldValue = US_UNNumber;
				base.US_UNNumber = value;
				if (!IsCopying && oldValue != US_UNNumber)
				{
					UpdateHazmatDetails();
				}
			}
		}

		void UpdateHazmatDetails()
		{
			UNDGSubstance hazmat = Hazmat;
			if (hazmat != null)
			{
				US_HazardClass = hazmat.DG_Class;
				US_ProperShippingName = hazmat.DG_PSN;
				US_PackingGroup = hazmat.DG_PG;
			}
		}

		#region Implementation
		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			base.B7_ParentTableCode = CusAddInfoSchema.Constants.Prefix;
			B7_Type = CusAddInfoTypeAttribute.Codes.USDeliveryOrderHazmat;
		}

		DeliveryOrderHeader fParent;
		#endregion
	}
}
