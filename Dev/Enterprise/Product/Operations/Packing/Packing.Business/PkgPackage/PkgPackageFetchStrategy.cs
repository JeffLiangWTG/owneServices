using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	public class PkgPackageFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public PkgPackageFetchStrategy(PkgPackage package)
			: base(package)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(PkgPackage), PkgPackageSchema.KP_KP_ParentPackage, BusinessObject.PK);
			Factory.AddFetchHint(typeof(PkgPackageItemDivot), PkgPackageItemDivotSchema.KI_KP_Package, BusinessObject.PK);
			Factory.AddFetchHint(typeof(PkgPackageScreening), PkgPackageScreeningSchema.KPS_KP_Package, BusinessObject.PK);

			if (!Package.KP_KPH_PackageHeader.IsEmpty)
			{
				Factory.AddFetchHint(PkgPackageHeaderSchema.Constants.TableName, Package.KP_KPH_PackageHeader); // tested in PkgPackageJobFetchStrategy
			}
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			var requireStmALog = false;
			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case PkgPackage.Schema.PackageStatus:
						requireStmALog = true;
						break;

					default:
						break;
				}
			}

			if (requireStmALog)
			{
				// Tested in PickEntryForm.TestPackageViewTabDbHits
				var stmALogQuery = new ZQuery(StmALogSchema.SL_Parent, Package.PK);
				Factory.AddFetchHint(StmALogSchema.Instance, stmALogQuery);
			}
		}

		protected override void FetchForDeleteCore()
		{
			base.FetchForDeleteCore();

			// Tested in PkgPackageTest.TestDelete_MultiplePackages_DbHits
			Factory.AddFetchHint(StmDefaultPrinterSchema.SDP_SubjectID, Package.PK);
			Factory.AddFetchHint(JobServiceLinkSchema.ESL_ParentID, Package.PK);

			if (Package.ShouldPackTrackedPackagesViaDivot)
			{
				Factory.AddFetchHint(PkgPackageHandlingUnitDivotSchema.KPD_KP_Package, Package.PK);
				Factory.AddFetchHint(PkgPackageHandlingUnitDivotSchema.KPD_KP_HandlingUnit, Package.PK);
			}
		}

		PkgPackage Package
		{
			get { return (PkgPackage)BusinessObject; }
		}
	}
}

