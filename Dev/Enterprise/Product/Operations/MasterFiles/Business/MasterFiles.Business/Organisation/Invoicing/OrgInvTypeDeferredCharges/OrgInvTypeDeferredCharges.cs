using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgInvTypeDeferredCharges : AutoOrgInvTypeDeferredCharges
	{
		public OrgInvTypeDeferredCharges(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public OrgInvTypeDeferredChargesCollection ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections.Length == 1)
				{
					return ((IBusinessObjectInternals)this).ParentCollections[0] as OrgInvTypeDeferredChargesCollection;
				}
				else
				{
					return new OrgInvTypeDeferredChargesCollection(Factory);
				}
			}
		}

		[List("Lookups.ChargeGroupList")]
		public override ZString PO_ChargeGroup
		{
			get
			{
				return base.PO_ChargeGroup;
			}
			set
			{
				base.PO_ChargeGroup = value;
				Validation.ValidatePO_AC();
				PO_DescriptionInfo.RefreshBinding();
				foreach (OrgInvTypeDeferredCharges deffCharge in ParentCollection)
				{
					deffCharge.Validation.ValidatePO_ChargeGroup();
					deffCharge.Validation.ValidatePO_AC();
				}
			}
		}

		public override ZGuid PO_AC
		{
			get
			{
				return base.PO_AC;
			}
			set
			{
				base.PO_AC = value;
				Validation.ValidatePO_ChargeGroup();
				PO_DescriptionInfo.RefreshBinding();
				foreach (OrgInvTypeDeferredCharges deffCharge in ParentCollection)
				{
					deffCharge.Validation.ValidatePO_AC();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		public ZString PO_Description
		{
			get
			{
				if (PO_AC == ZGuid.Empty && PO_ChargeGroup != ZString.Empty)
				{
					return Lookups.ChargeGroupList.GetDescriptionFromCode(PO_ChargeGroup);
				}
				else
				{
					AccChargeCode chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.PK, PO_AC));
					if (chargeCode != null)
					{
						return chargeCode.AC_Desc;
					}
					else
					{
						return ZString.Empty;
					}
				}
			}
		}

		public virtual ZPropertyInfo PO_DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(PO_Description)); }
		}
	}
}
