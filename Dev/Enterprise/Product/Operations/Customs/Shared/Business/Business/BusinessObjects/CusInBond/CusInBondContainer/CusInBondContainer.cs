using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public abstract class CusInBondContainer : BaseCusInBondContainer, IUNDGDataItemProvider
		, ISynchableContainer, ISynchroniserReadOnlyMembersProvider, Integration.Customs.ICusInBondContainer
	{
		protected CusInBondContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Type Decider

		public new static readonly CusInBondContainerTypeDecider TypeDecider = new CusInBondContainerTypeDecider();

		#endregion

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
		#endregion

		#region BC_B0

		[RelatedBusinessObject("MoveDetail")]
		public override ZGuid BC_ParentID
		{
			get { return base.BC_ParentID; }
			set { base.BC_ParentID = value; }
		}

		public CusInBondMoveDetail MoveDetail
		{
			get { return MoveDetailCore; }
		}

		protected abstract CusInBondMoveDetail MoveDetailCore { get; }

		#endregion

		public CusInBondHeader Header
		{
			get
			{
				var moveHeader = MoveHeader;
				return moveHeader == null ? null : moveHeader.Header;
			}
		}

		public CusInBondMoveHeader MoveHeader
		{
			get
			{
				var moveDetail = MoveDetail;
				return moveDetail == null ? null : moveDetail.MoveHeader;
			}
		}

		public CusInBondBill Bill
		{
			get
			{
				CusInBondMoveDetail moveDetail = MoveDetail;
				return moveDetail == null ? null : moveDetail.Bill;
			}
		}

		[ChildEditable]
		public ICusInBondCargoDescCollection<CusInBondCargoDesc> Commodities
		{
			get
			{
				if (commodities == null)
				{
					commodities = GetNewCommoditiesCollection();
					RegisterEditableChildObject(commodities);
				}
				return commodities;
			}
		}
		ICusInBondCargoDescCollection<CusInBondCargoDesc> commodities;

		protected abstract ICusInBondCargoDescCollection<CusInBondCargoDesc> GetNewCommoditiesCollection();

		[ChildEditable]
		public UNDGDataItemCollection UNDGs
		{
			get
			{
				if (fUNDGs == null)
				{
					fUNDGs = new UNDGDataItemCollection(this);
					RegisterEditableChildObject(fUNDGs);
				}
				return fUNDGs;
			}
		}
		UNDGDataItemCollection fUNDGs;

		bool IUNDGDataItemProvider.NeedFetchHintForLoad => true;

		public override void Delete()
		{
			Commodities.DeleteAll();
			UNDGs.DeleteAll();
			base.Delete();
		}

		public bool ShouldSynchronise
		{
			get
			{
				var header = Header;
				return header != null && header.ShouldSynchronise;
			}
		}

		public bool IsNonContainerized
		{
			get { return BC_ContainerNum == NonContainerizedNumber; }
		}
		public const string NonContainerizedNumber = "NC";

		ZPropertyInfo ISynchableContainer.ContainerNumInfo
		{
			get { return BC_ContainerNumInfo; }
		}

		ZPropertyInfo ISynchableContainer.Seal1Info
		{
			get { return BC_Seal1Info; }
		}

		bool ISynchableContainer.Seal2Supported
		{
			get { return true; }
		}

		ZPropertyInfo ISynchableContainer.Seal2Info
		{
			get { return BC_Seal2Info; }
		}

		bool ISynchableContainer.Seal3Supported
		{
			get { return false; }
		}

		ZPropertyInfo ISynchableContainer.Seal3Info
		{
			get { return null; }
		}
	}
}
