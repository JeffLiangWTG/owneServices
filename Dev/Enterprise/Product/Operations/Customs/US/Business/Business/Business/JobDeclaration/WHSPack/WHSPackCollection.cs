using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class WHSPackCollection : DependentBusinessObjectCollection<WHSPack, JobDeclaration>
	{
		public WHSPackCollection(JobDeclaration declaration)
			: base(declaration, new ZQuery(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.USWHSPack))
		{
		}

		public new JobDeclaration Master
		{
			get { return base.Master; }
		}

		public ICodeDescriptionPairList UniquePackageReferenceList
		{
			get
			{
				if (uniquePackageReferenceList == null)
				{
					uniquePackageReferenceList = new CodeDescriptionPairList();
					foreach (ICodeDescription whsPack in this.OfType<ICodeDescription>().OrderBy(x => x.Code))
					{
						if (!uniquePackageReferenceList.ContainsCode(whsPack.Code))
						{
							uniquePackageReferenceList.Add(whsPack);
						}
					}
				}
				return uniquePackageReferenceList;
			}
		}
		ICodeDescriptionPairList uniquePackageReferenceList;

		public void RefreshPackageReferenceList()
		{
			uniquePackageReferenceList = null;
		}

		#region Implementation

		protected override bool AllowNewCore
		{
			get
			{
				var master = Master;
				return master.PackableInvoiceLines.Count > 0;
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var pack = (WHSPack)child;
			pack.US_PackageReference = PackName + (Count + 1).ToString();
		}
		const string PackName = "PACK";

		protected override CargoWise.Schema.SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusAddInfoSchema.B7_ParentID; }
		}

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);
			RefreshPackageReferenceList();
		}

		#endregion
	}
}
