using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class SPTSContainer : CusInBondContainer
	{
		public SPTSContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("ParentBusinessObject")]
		public override ZGuid BC_ParentID
		{
			get { return base.BC_ParentID; }
			set { base.BC_ParentID = value; }
		}

		public new SPTSContainerLookups Lookups => new SPTSContainerLookups(this);

		public SPTSHeader ParentBusinessObject
		{
			get
			{
				if (header == null && BC_ParentTableCode == CusInBondHeaderSchema.Constants.Prefix)
				{
					header = Factory.Load<SPTSHeader>(BC_ParentID);
				}
				return header;
			}
		}
		SPTSHeader header;

		public SPTSBill SPTSBill
		{
			get
			{
				if (bill == null && BC_ParentTableCode == CusInBondBillSchema.Constants.Prefix)
				{
					bill = Factory.Load<SPTSBill>(BC_ParentID);
				}
				return bill;
			}
		}
		SPTSBill bill;

		protected override CusInBondContainerValidation GetNewValidation()
		{
			return new SPTSHeaderContainerValidation(this);
		}

		public new class Schema : AutoCusInBondContainer.Schema
		{
			public new const int BC_ContainerNumMaxLength = 20;
		}

		[MaxLength(Schema.BC_ContainerNumMaxLength)]
		[List(nameof(Lookups) + "." + nameof(SPTSContainerLookups.ContainersList))]
		[ResourceStringData("Enterprise.Customs.TR.NCTS.Business.SPTSContainer|BC_ContainerNum", Caption = "Container No", ShortCaption = "Container")]
		public override ZString BC_ContainerNum
		{
			get => base.BC_ContainerNum;
			set => base.BC_ContainerNum = value;
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (BC_ContainerNum.IsEmpty)
			{
				this.Delete();
			}
		}

		protected override ICusInBondCargoDescCollection<CusInBondCargoDesc> GetNewCommoditiesCollection() => new SPTSCargoDescCollection(this);
		protected override CusInBondMoveDetail MoveDetailCore => null;
	}
}
