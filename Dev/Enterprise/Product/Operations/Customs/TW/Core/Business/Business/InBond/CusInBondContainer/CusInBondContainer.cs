using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.TW.Business
{
	public partial class CusInBondContainer
	{
		public CusInBondContainer(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		protected override Customs.Business.CusInBondMoveDetail MoveDetailCore => Factory.Load<CusInBondMoveDetail>(BC_ParentID);

		public new CusInBondHeader Header => (CusInBondHeader)base.Header;

		public new CusInBondMoveHeader MoveHeader => (CusInBondMoveHeader)base.MoveHeader;

		public new CusInBondMoveDetail MoveDetail => (CusInBondMoveDetail)base.MoveDetail;

		protected override Customs.Business.ICusInBondCargoDescCollection<Customs.Business.CusInBondCargoDesc> GetNewCommoditiesCollection() => new CusInBondCargoDescCollection(this);

		[MaxLength(17)]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondContainer|BC_ContainerNum", Caption = "Number")]
		public override ZString BC_ContainerNum { get => base.BC_ContainerNum; set => base.BC_ContainerNum = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondContainer|BC_RC", Caption = "Type")]
		[List(nameof(Lookups) + "." + nameof(CusInBondContainerLookups.ContainerTypes))]
		public override ZGuid BC_RC { get => base.BC_RC; set => base.BC_RC = value; }

		[MaxLength(17)]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondContainer|BC_Seal1", Caption = "Seal")]
		public override ZString BC_Seal1 { get => base.BC_Seal1; set => base.BC_Seal1 = value; }

		[List(nameof(Lookups) + "." + nameof(CusInBondContainerLookups.ModeList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondContainer|BC_Mode", Caption = "Mode")]
		public override ZString BC_Mode
		{
			get => base.BC_Mode;
			set
			{
				base.BC_Mode = value;
				if (IsPartReadOnly)
				{
					BC_IsPart = false;
				}
			}
		}

		[ReadOnlyMember(nameof(IsPartReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondContainer|BC_IsPart", Caption = "Is Part")]
		public override ZBool BC_IsPart { get => base.BC_IsPart; set => base.BC_IsPart = value; }

		public bool IsPartReadOnly => BC_Mode != CusInBondContainerModeList.Codes.FCL && BC_Mode != CusInBondContainerModeList.Codes.BCN;
	}
}
