using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketReferenceValidation : AutoWhsDocketReferenceValidation
	{
		public WhsDocketReferenceValidation(AutoWhsDocketReference parent)
			: base(parent)
		{
		}

		public new WhsDocketReference Parent
		{
			get { return (WhsDocketReference)base.Parent; }
		}

		#region CheckWX_RefType

		protected override void CheckWX_RefType()
		{
			MandatoryValidation.CheckEntered(Parent.WX_RefTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.WX_RefTypeInfo, Parent.Lookups.ReferenceTypes);
			CheckWX_RefTypeTPC();
		}

		void CheckWX_RefTypeTPC()
		{
			var reference = Parent;
			if (!reference.WX_RefTypeInfo.HasErrors() && reference.WX_RefType == WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber)
			{
				var docket = reference.Docket;
				if (docket != null && !docket.IsDeleted)
				{
					if (docket.TransportBillToDocAddress.IsEmpty)
					{
						reference.WX_RefTypeInfo.AddError(Res.GetString("5724ff96-59b1-46a4-9e64-d21eb02c760c", "Third party carrier account number requires a transport bill to address."));
					}
					else if (docket.References.ToArray<WhsDocketReference>().Any(r => r.WX_RefType == WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber && r.PK != reference.PK))
					{
						reference.WX_RefTypeInfo.AddError(Res.GetString("e35aa911-b2a3-4bf4-8030-c2c7dc3f5c5a", "Duplicate TPC Reference Type."));
					}
				}
			}
		}

		#endregion

		#region CheckWX_Reference

		protected override void CheckWX_Reference()
		{
			var reference = Parent;
			MandatoryValidation.CheckEntered(reference.WX_ReferenceInfo);

			if (!reference.WX_ReferenceInfo.HasErrors())
			{
				if (reference.WX_RefType == WarehouseAdditionalReferenceTypes.Codes.OrderTypeCode && reference.WX_Reference.Length != 6)
				{
					reference.WX_ReferenceInfo.AddError(Res.GetString("c0b7aa6d-09c2-4b6f-83c9-8a3fd914071f", "Order Type Code reference must be six characters."));
				}
				else if (reference.WX_RefType == WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber && (reference.WX_Reference.IsEmpty || reference.WX_Reference.Length > OrgCarrierAccountSchema.OAN_AccountNumber.MaxLength))
				{
					reference.WX_ReferenceInfo.AddError(Res.GetString("5e60d4c5-19ca-4e90-976e-00fa255709b7", "Third party carrier account number cannot be more than {0} characters.", OrgCarrierAccountSchema.OAN_AccountNumber.MaxLength));
				}
			}
		}

		#endregion
	}
}
