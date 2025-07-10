using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting
{
	[DependentBusinessObject(null, "")]
	public class CusEntryHeader : ECIWriteOff.CusEntryHeader, Integration.Customs.NZ.IECIWriteOffManifestingCusEntryHeader
	{
		public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CusEntryHeader Load(BusinessObjectFactory factory, ZString bGMReference)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, bGMReference);
			filter.AddToFilter(CusEntryHeaderSchema.CH_MessageType, EntryHeaderTypes.NZ.ECIWriteOffManifest);
			BusinessObject[] entryHeaders = factory.Load(typeof(CusEntryHeader), filter);
			CusEntryHeader result = null;
			foreach (CusEntryHeader entryHeader in entryHeaders)
			{
				result = entryHeader;
				if (entryHeader.IsActive)
				{
					break;
				}
			}
			return result;
		}

		public JobDeclarationCollectionECIWriteOff Declarations
		{
			get
			{
				if (fDeclarations == null)
				{
					fDeclarations = new JobDeclarationCollectionECIWriteOff(Factory, Declaration?.Company?.PK ?? GlbCompany.CurrentCompany.PK);
					if (!CH_BGMReference.IsEmpty)
					{
						var manifestQuery = new ZQuery(JobDeclarationSchema.JE_DeclarationReference, SQLComparisonOperator.StartsWith, CH_BGMReference);
						manifestQuery.AddToFilter(JobDeclarationSchema.JE_MessageSubType, JobMessageSubTypeList.Codes.WriteOff);
						fDeclarations.Load(manifestQuery);
					}
				}
				return fDeclarations;
			}
		}
		JobDeclarationCollectionECIWriteOff fDeclarations;

		public CusEntryHeaderManifestWrapper ManifestWrapper => manifestWrapper ?? (manifestWrapper = new CusEntryHeaderManifestWrapper(this));

		CusEntryHeaderManifestWrapper manifestWrapper;

		public override ZInt PackagesCount => Declarations.Cast<JobDeclaration>().Sum(x => x.JE_TotalNoOfPacks);

		[RelatedBusinessObject("Declaration")]
		public override ZGuid CH_JE
		{
			get { return base.CH_JE; }
			set { base.CH_JE = value; }
		}

		public override void OnSaving()
		{
			var bgmReferenceRefreshed = false;

			try
			{
				PopulateNumberPropertyIfRequired<ZString>(
					CH_BGMReferenceInfo,
					factory =>
					{
						bgmReferenceRefreshed = true;
						return Env.NumberFountains.ECIWriteOffManifestReference.GetNextFormatted(factory);
					}
				);

				if (bgmReferenceRefreshed)
				{
					LinkDeclarationsIntoManifest();
					if (Declarations.Count >= 1)
					{
						CH_JE = Declarations[0].PK;
					}
				}
			}
			finally
			{
				bgmReferenceRefreshed = false;
			}

			base.OnSaving();
		}

		public override bool LastCustomsStatusIsImpediment
		{
			get
			{
				bool result = false;
				foreach (JobDeclaration declaration in Declarations)
				{
					if (declaration.LastCustomsStatusIsImpediment)
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		internal override void DeleteIfContainsNoValuableData()
		{
			// Do Nothing. Manifesting Entry Headers are NEVER crap.
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CH_MessageType = EntryHeaderTypes.NZ.ECIWriteOffManifest;
			CH_EntryStatus = LowValueManifestStatusList.Codes.NotSentToCustoms;
		}

		void LinkDeclarationsIntoManifest()
		{
			for (int index = 0; index < Declarations.Count; index++)
			{
				JobDeclaration declaration = Declarations[index];
				declaration.LinkToManifest(this, index + 1);
			}
		}
	}
}
