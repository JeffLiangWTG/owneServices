using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class CusPackingList : Customs.Business.CusPackingList, Integration.Customs.TW.ICusPackingList
	{
		public CusPackingList(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;
		public new JobComInvoiceHeader Invoice => (JobComInvoiceHeader)base.Invoice;

		protected override Customs.Business.CusPackageJob GetCusPackageJobCore()
		{
			if (cusPackageJob == null || cusPackageJob.IsDeleted)
			{
				cusPackageJob = Factory.LoadTop1<CusPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, PK));
			}
			return cusPackageJob != null && !cusPackageJob.IsDeleted ? cusPackageJob : null;
		}

		CusPackageJob cusPackageJob;

		public new CusPackableItemCollection PackableItems => (CusPackableItemCollection)base.PackableItems;

		protected override Customs.Business.CusPackableItemCollection GetPackageCollectionCore() => new CusPackableItemCollection(this);

		protected override DocumentSupporter CreateNewDocumentSupporter() => new CusPackingListDocumentSupporter(this);

		public override ZGuid CUL_JE
		{
			get => base.CUL_JE;
			set
			{
				var oldValue = CUL_JE;
				base.CUL_JE = value;
				if (!IsCopying && oldValue != CUL_JE)
				{
					loadEntryInstruction = false;
				}
			}
		}

		public override ZGuid CUL_JZ
		{
			get => base.CUL_JZ;
			set
			{
				var oldValue = CUL_JZ;
				base.CUL_JZ = value;
				if (!IsCopying && oldValue != CUL_JZ)
				{
					loadEntryInstruction = false;
				}
			}
		}

		// NOTE: this code is temporary as Josh Lu will add a new workitem to ensure that each CusPackList has its' own PackageDescription
		public CusEntryInstruction EntryInstruction
		{
			get
			{
				if (!loadEntryInstruction)
				{
					loadEntryInstruction = true;
					entryInstruction = (Declaration ?? Invoice?.JobDeclaration)?.CusEntryInstruction;
				}
				return entryInstruction;
			}
		}
		CusEntryInstruction entryInstruction;
		bool loadEntryInstruction;
	}
}
