using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	[IDontMindLoadingASubclassInstead]
	public abstract class BaseCusInBondContainer : AutoCusInBondContainer, Integration.Customs.IBaseCusInBondContainer
	{
		public BaseCusInBondContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Type Decider

		public static readonly CusInBondContainerTypeDecider TypeDecider = new CusInBondContainerTypeDecider();

		#endregion

		public override ZString BC_DataModel
		{
			get { return base.BC_DataModel; }
			set
			{
				this.ReportDataModelErrorIfNeeded(BC_DataModelInfo, value);
				base.BC_DataModel = value;
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsInDatabase)
			{
				if (GetHeader() is BaseCusInBondHeader header && header.Company is GlbCompany company)
				{
					BC_DataModel = company.GC_RN_NKCountryCode + header.BH_ApplicationCode;
				}
				else
				{
					BC_DataModel = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					ErrorReporter.ReportOnce("BC_DataModel was set without a valid CusInBondHeader.");
				}
			}
		}

		internal BaseCusInBondHeader GetHeader()
		{
			BaseCusInBondHeader result = null;
			switch (BC_ParentTableCode)
			{
				case CusInBondHeaderSchema.Constants.Prefix:
					result = Header;
					break;
				case CusInBondMoveDetailSchema.Constants.Prefix:
					result = MoveDetail?.MoveHeader?.Header;
					break;
				case CusInBondBillSchema.Constants.Prefix:
					result = Bill?.Header;
					break;
				case CusInBondCargoDescSchema.Constants.Prefix:
					result = CargoDesc?.Header;
					break;
				case CusInBondEventSchema.Constants.Prefix:
					result = InBondEvent?.Header;
					break;
			}

			return result;
		}

		BaseCusInBondHeader Header => (header ?? (header = new CachedRelatedBusinessObject<BaseCusInBondHeader>((ZPropertyInfoGuid)BC_ParentIDInfo, () => Factory.Load<BaseCusInBondHeader>(BC_ParentID)))).Value;
		CachedRelatedBusinessObject<BaseCusInBondHeader> header;

		BaseCusInBondMoveDetail MoveDetail => (moveDetail ?? (moveDetail = new CachedRelatedBusinessObject<BaseCusInBondMoveDetail>((ZPropertyInfoGuid)BC_ParentIDInfo, () => Factory.Load<BaseCusInBondMoveDetail>(BC_ParentID)))).Value;
		CachedRelatedBusinessObject<BaseCusInBondMoveDetail> moveDetail;

		CusInBondBill Bill  => (bill ?? (bill = new CachedRelatedBusinessObject<CusInBondBill>((ZPropertyInfoGuid)BC_ParentIDInfo, () => Factory.Load<CusInBondBill>(BC_ParentID)))).Value;
		CachedRelatedBusinessObject<CusInBondBill> bill;

		CusInBondCargoDesc CargoDesc => (cargoDesc ?? (cargoDesc = new CachedRelatedBusinessObject<CusInBondCargoDesc>((ZPropertyInfoGuid)BC_ParentIDInfo, () => Factory.Load<CusInBondCargoDesc>(BC_ParentID)))).Value;
		CachedRelatedBusinessObject<CusInBondCargoDesc> cargoDesc;

		CusInBondEvent InBondEvent => (inBondEvent ?? (inBondEvent = new CachedRelatedBusinessObject<CusInBondEvent>((ZPropertyInfoGuid)BC_ParentIDInfo, () => Factory.Load<CusInBondEvent>(BC_ParentID)))).Value;
		CachedRelatedBusinessObject<CusInBondEvent> inBondEvent;
	}
}
