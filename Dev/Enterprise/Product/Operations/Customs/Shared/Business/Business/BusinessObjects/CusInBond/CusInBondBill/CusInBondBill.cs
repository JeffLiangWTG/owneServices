using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public abstract class CusInBondBill : AutoCusInBondBill
		, ISynchroniserReadOnlyMembersProvider, Integration.Customs.ICusInBondBill
	{
		protected CusInBondBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Type Decider

		public static readonly CusInBondBillTypeDecider TypeDecider = new CusInBondBillTypeDecider();

		#endregion

		[RelatedBusinessObject("Header")]
		public override ZGuid B0_BH
		{
			get { return base.B0_BH; }
			set { base.B0_BH = value; }
		}

		public CusInBondHeader Header => (header ?? (header = new CachedRelatedBusinessObject<CusInBondHeader>((ZPropertyInfoGuid)B0_BHInfo, () => Factory.Load<CusInBondHeader>(B0_BH)))).Value;
		CachedRelatedBusinessObject<CusInBondHeader> header;

		public CusInBondMoveDetail MovementDetail
		{
			get
			{
				if (movementDetail == null || movementDetail.IsDeleted || movementDetail.B9_B0 != PK || movementDetail.MoveHeader == null || movementDetail.MoveHeader.BM_BH != B0_BH)
				{
					if (movementDetail != null)
					{
						UnRegisterEditableChildObject(movementDetail);
					}
					movementDetail = null;
					var header = Header;
					if (header != null)
					{
						Type moveType = MovementDetailType;
						movementDetail = (CusInBondMoveDetail)Factory.LoadTop1(moveType, MovementDetailQuery);
						if (movementDetail == null && IsMovementDetailSupported)
						{
							movementDetail = GetMoveHeader(header).MovementDetails.AddNew(PK);
						}
						RegisterEditableChildObject(movementDetail);
					}
				}
				return movementDetail;
			}
		}
		CusInBondMoveDetail movementDetail;

		protected virtual CusInBondMoveHeader GetMoveHeader(CusInBondHeader header) => header.MovementHeader;

		protected abstract Type MovementDetailType { get; }

		protected virtual bool IsMovementDetailSupported => true;

		public virtual ZQuery MovementDetailQuery
		{
			get
			{
				var query = new ZQuery(CusInBondMoveDetailSchema.B9_B0, PK);
				query.FetchOnlyFromLocalCache = !IsInDatabase;
				var header = Header;
				var moveHeader = header != null ? GetMoveHeader(header) : null;
				if (header == null || moveHeader == null)
				{
					query.IsNoResultQuery = true;
				}
				else
				{
					query.AddToFilter(CusInBondMoveDetailSchema.B9_BM, moveHeader.PK);
				}
				return query;
			}
		}

		public bool ShouldSynchronise
		{
			get
			{
				var header = Header;
				return header != null && header.ShouldSynchronise;
			}
		}

		#region Override Methods

		public override void Delete()
		{
			this.DeleteChildren<CusInBondMoveDetail>(CusInBondMoveDetailSchema.B9_B0);
			base.Delete();
		}

		#endregion

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected virtual bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
		#endregion
	}
}
