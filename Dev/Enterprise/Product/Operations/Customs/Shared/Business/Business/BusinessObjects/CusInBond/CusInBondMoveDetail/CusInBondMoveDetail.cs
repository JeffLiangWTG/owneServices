using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	public abstract class CusInBondMoveDetail : BaseCusInBondMoveDetail, Integration.Customs.ICusInBondMoveDetail, ICusInvPackTypeSupporter, ICusInBondContainerTypeSupporter
	{
		protected CusInBondMoveDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Type Decider

		public static new readonly TypeDecider TypeDecider = new CusInBondMoveDetailTypeDecider();

		#endregion

		[RelatedBusinessObject("Bill")]
		public override CargoWise.Types.ZGuid B9_B0
		{
			get { return base.B9_B0; }
			set { base.B9_B0 = value; }
		}

		public CusInBondBill Bill
		{
			get { return Factory.Load<CusInBondBill>(B9_B0); }
		}

		[RelatedBusinessObject("MoveHeader")]
		public override CargoWise.Types.ZGuid B9_BM
		{
			get { return base.B9_BM; }
			set { base.B9_BM = value; }
		}

		public new CusInBondMoveHeader MoveHeader => (CusInBondMoveHeader)base.MoveHeader;

		protected override BaseCusInBondMoveHeader GetMoveHeader() => Factory.Load<CusInBondMoveHeader>(B9_BM);

		[ChildEditable]
		public ICusInBondContainerCollection Containers
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
		ICusInBondContainerCollection containers;

		public Type ContainerType
		{
			get { return ContainerTypeCore; }
		}

		protected abstract Type ContainerTypeCore { get; }

		public Type MoveLineItemType
		{
			get { return MoveLineItemTypeCore; }
		}

		protected abstract Type MoveLineItemTypeCore { get; }

		Type ICusInvPackTypeSupporter.PackType => PackTypeCore;

		protected abstract Type PackTypeCore { get; }

		TypeDecider ICusInvPackTypeSupporter.PackTypeDecider => PackTypeDeciderCore;

		protected virtual TypeDecider PackTypeDeciderCore => null;

		protected abstract ICusInBondContainerCollection GetContainersCollection();

		public override void Delete()
		{
			Containers.DeleteAll();
			base.Delete();
		}

		public override void OnSaving()
		{
			if (B9_B0.IsEmpty)
			{
			}
			base.OnSaving();
		}

		#region ICusInBondContainerTypeSupporter

		Type ICusInBondContainerTypeSupporter.ContainerType => ContainerType;

		#endregion
	}
}
