using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class SPTSBill : CusInBondBill
	{
		public SPTSBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new SPTSHeader Header => Factory.Load<SPTSHeader>(B0_BH);

		protected override Type MovementDetailType
		{
			get { return typeof(CusInBondMoveDetail); }
		}

		public new SPTSBillLookups Lookups
		{
			get { return (SPTSBillLookups)base.Lookups; }
		}

		protected override CusInBondBillLookups GetNewLookups()
		{
			return new SPTSBillLookups(this);
		}

		protected override CusInBondBillValidation GetNewValidation()
		{
			return new SPTSBillValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			B0_ServiceType = Universal.CodeDescriptionPairLists.YesNoList.Codes.No;
		}

		[MaxLength(35)]
		[ResourceStringData("Enterprise.Customs.TR.NCTS.Business.SPTSBill|B0_MasterBillNumber", Caption = "Bill No")]
		public override ZString B0_MasterBillNumber { get => base.B0_MasterBillNumber; set => base.B0_MasterBillNumber = value; }

		[MaxLength(3)]
		[ResourceStringData("Enterprise.Customs.TR.NCTS.Business.SPTSBill|B0_ReferenceQualifier", Caption = "Declaration Type")]
		[List(nameof(Lookups) + "." + nameof(SPTSBillLookups.DeclarationTypeList))]
		public override ZString B0_ReferenceQualifier { get => base.B0_ReferenceQualifier; set => base.B0_ReferenceQualifier = value; }

		[MaxLength(20)]
		[ResourceStringData("Enterprise.Customs.TR.NCTS.Business.SPTSBill|B0_ReferenceID", Caption = "Registration No")]
		public override ZString B0_ReferenceID { get => base.B0_ReferenceID; set => base.B0_ReferenceID = value; }

		[MaxLength(1)]
		[ResourceStringData("Enterprise.Customs.TR.NCTS.Business.SPTSBill|B0_ServiceType", Caption = "Partial?")]
		[List(nameof(Lookups) + "." + nameof(SPTSBillLookups.YesNoList))]
		public override ZString B0_ServiceType { get => base.B0_ServiceType; set => base.B0_ServiceType = value; }

		[ChildEditable]
		public SPTSContainerCollection SPTSBillContainers
		{
			get
			{
				if (containers == null)
				{
					containers = GetContainersCollection();
					RegisterEditableChildObject(containers);
				}
				return containers;
			}
		}
		SPTSContainerCollection containers;

		protected virtual SPTSContainerCollection GetContainersCollection() => new SPTSContainerCollection(this);
	}
}
