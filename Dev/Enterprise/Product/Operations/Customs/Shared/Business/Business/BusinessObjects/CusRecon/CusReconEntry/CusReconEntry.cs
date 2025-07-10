using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.CusReconBase;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	public class CusReconEntry : AutoCusReconEntry, Integration.Customs.ICusReconEntry
	{
		public CusReconEntry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusReconEntry.Schema
		{
			public const string DeclarantCode = nameof(CusReconEntry.DeclarantCode);
			public const string ImporterCode = nameof(CusReconEntry.ImporterCode);
			public const string RepresentativeCode = nameof(CusReconEntry.RepresentativeCode);
			public const string BuyingAgentCode = nameof(CusReconEntry.BuyingAgentCode);
			public const string RepresentationType = nameof(CusReconEntry.RepresentationType);
		}

		[ResourceStringData("3DFEAFC3-2C27-44B1-BC30-ECDD7F688B2B", Caption = "Entry Type")]
		[List(nameof(Lookups) + "." + nameof(CusReconEntryLookups.EntryTypeList))]
		public override ZString CRE_EntryType
		{
			get => base.CRE_EntryType;
			set => base.CRE_EntryType = value;
		}

		[RelatedBusinessObject(nameof(EntryHeader))]
		public override ZGuid CRE_CH_OriginalEntry
		{
			get => base.CRE_CH_OriginalEntry;
			set => base.CRE_CH_OriginalEntry = value;
		}

		[RelatedBusinessObject(nameof(ReconDeclaration))]
		public override ZGuid CRE_CRD
		{
			get => base.CRE_CRD;
			set => base.CRE_CRD = value;
		}

		[ResourceStringData("F2E510CF-937D-47FC-809D-AC2FD941E8C6", Caption = "Declarant")]
		public virtual ZString DeclarantCode => DeclarantAddress?.Header?.OH_Code ?? ZString.Empty;

		[ResourceStringData("9D57D053-4BC2-4D6D-9593-56BE48B9A644", Caption = "Importer")]
		public virtual ZString ImporterCode => ImporterAddress?.Header?.OH_Code ?? ZString.Empty;

		[ResourceStringData("AD935BD1-FB2E-4DB3-B9DB-3B6D25DB3085", Caption = "Representative")]
		public virtual ZString RepresentativeCode => RepresentativeAddress?.Header?.OH_Code ?? ZString.Empty;

		[ResourceStringData("589D9908-4D51-4B70-97F8-3F715BE87214", Caption = "Represented Party")]
		public virtual ZString BuyingAgentCode => BuyingAgentAddress?.Header?.OH_Code ?? ZString.Empty;

		[ResourceStringData("75CC3E08-2771-4DE0-B227-3B84739A9BB2", Caption = "Rep. Type")]
		public ZString RepresentationType => EntryHeader?.Declaration?.JE_DeclarantType ?? ZString.Empty;

		public CusEntryHeader EntryHeader => Factory.Load<CusEntryHeader>(CRE_CH_OriginalEntry);

		public CusReconDeclaration ReconDeclaration => Factory.Load<CusReconDeclaration>(CRE_CRD);

		public static readonly CusReconEntryTypeDecider TypeDecider = new CusReconEntryTypeDecider();

		[ChildEditable(true)]
		public CusReconEntryLineCollection CusReconEntryLines
		{
			get
			{
				if (cusReconEntryLines == null)
				{
					cusReconEntryLines = CreateNewCusReconEntryLineCollection();
					RegisterEditableChildObject(cusReconEntryLines);
				}
				return cusReconEntryLines;
			}
		}
		CusReconEntryLineCollection cusReconEntryLines;

		[ChildEditable(true)]
		public CusReconSnapshotCollection CusReconSnapshots
		{
			get
			{
				if (cusReconSnapshots == null)
				{
					cusReconSnapshots = CreateNewCusReconSnapshotCollection();
					RegisterEditableChildObject(cusReconSnapshots);
				}
				return cusReconSnapshots;
			}
		}
		CusReconSnapshotCollection cusReconSnapshots;

		public override void Delete()
		{
			CusReconSnapshots.DeleteAll();
			CusReconEntryLines.DeleteAll();
			base.Delete();
		}

		protected virtual CusReconEntryLineCollection CreateNewCusReconEntryLineCollection() => new CusReconEntryLineCollection(this);

		protected virtual CusReconSnapshotCollection CreateNewCusReconSnapshotCollection() => new CusReconSnapshotCollection(this);

		public new CusReconEntryLookups Lookups => (CusReconEntryLookups)base.Lookups;
		protected override CusReconBase.CusReconEntryLookups GetNewLookups() => new CusReconEntryLookups(this);

		protected override CusReconBase.CusReconEntryValidation GetNewValidation() => new CusReconEntryValidation(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CRE_GB_Branch = GlbBranch.CurrentBranch.PK;
		}

		protected override bool SupportsCloneCore() => true;
	}
}
