using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Freight.Forwarding.DataTransfer.ForwardingPkgPackageDataObjectWriter;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ForwardingPkgPackageDataObjectWriter : DataObjectWriter<PkgPackageWrapper, PackingLine>
	{
		public ForwardingPkgPackageDataObjectWriter(IContainerLinkManager<ForwardingConsol> containerLinkManager, IDataWritingManager manager, int startPacklineLink = 1) : base(manager)
		{
			this.containerLinkManager = containerLinkManager;
			this.startPacklineLink = startPacklineLink;
			helper = new PkgPackageJobDataObjectWriterHelper(manager, null, assertNotNullPackingParent: false);
		}

		readonly IContainerLinkManager<ForwardingConsol> containerLinkManager;
		readonly int startPacklineLink;
		readonly PkgPackageJobDataObjectWriterHelper helper;

		protected override PackingLine PopulateDataObject(PkgPackageWrapper sourceBO)
		{
			var packageDataObject = PopulatePkgPackageDataObject(sourceBO.Package);
			ProcessContainerNumberAndLink(sourceBO.PackLine, packageDataObject);
			return packageDataObject;
		}

		PackingLine PopulatePkgPackageDataObject(PkgPackage package)
		{
			var packageDataObject = helper.PopulatePkgPackageDataObject(package, helper.LinksDictionary.Count + startPacklineLink, new ForwardingPkgPackageOutturnProvider(package));
			if (packageDataObject.PackingLineCollection == null)
			{
				packageDataObject.SetPackingLineCollection(() => new List<PackingLine>());
			}

			var innerPackages = package.HandlingUnitPackedPackages.Any() ? package.HandlingUnitPackedPackages : package.Packages;
			packageDataObject.PackingLineCollection.AddRange(innerPackages.Select(PopulatePkgPackageDataObject));
			return packageDataObject;
		}

		void ProcessContainerNumberAndLink(ForwardingPackLine packLine, PackingLine packageDataObject)
		{
			if (containerLinkManager.Consol != null)
			{
				var container = packLine.GetContainer(containerLinkManager.Consol);

				if (container == null && containerLinkManager.Consol.CouldBeAttachedToMultiAWBMaster && containerLinkManager.Consol.MasterConsol != null)
				{
					container = packLine.GetContainer(containerLinkManager.Consol.MasterConsol);
				}

				if (container != null)
				{
					containerLinkManager.SetContainerLink(container, packageDataObject);
				}

				packageDataObject.ContainerNumber = container != null ? container.JC_ContainerNum : null;
			}
		}

		public class PkgPackageWrapper : NonPersistentBusinessObject
		{
			public PkgPackage Package { get; set; }

			public ForwardingPackLine PackLine { get; set; }
		}
	}
}

