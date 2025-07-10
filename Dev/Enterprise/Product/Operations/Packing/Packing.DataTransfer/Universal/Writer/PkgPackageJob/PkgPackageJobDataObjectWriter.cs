using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Packing.DataTransfer.Universal
{
	public class PkgPackageJobDataObjectWriter : DataObjectWriter<PkgPackageJob, Shipment>
	{
		public PkgPackageJobDataObjectWriter(IDataWritingManager manager) : base(manager) { }

		protected override Shipment PopulateDataObject(PkgPackageJob packageJobBO)
		{
			var pkgPackageJobData = new Shipment(writeManager.WriterStrategy);

			var dataSource = DataContextFactory.New();
			dataSource.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			pkgPackageJobData.DataContext = dataSource;

			helper = new PkgPackageJobDataObjectWriterHelper(writeManager, packageJobBO.ParentJob, packageJobBO);
			helper.PopulateDataObject(pkgPackageJobData);

			return pkgPackageJobData;
		}

		PkgPackageJobDataObjectWriterHelper helper;

		public Dictionary<ZGuid, ZInt> LinksDictionary => helper == null ? new Dictionary<ZGuid, ZInt>() : helper.LinksDictionary;
	}
}
