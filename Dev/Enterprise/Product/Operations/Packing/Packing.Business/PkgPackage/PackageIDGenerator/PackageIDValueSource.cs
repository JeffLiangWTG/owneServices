using System.Collections;
using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Keys = Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement.Keys;

namespace Enterprise.Packing.Business
{
	public sealed class PackageIDValueSource : IEnumerable<INumberGeneratorValueProvider>
	{
		public PackageIDValueSource(PkgPackageJob packageJob)
		{
			this.packageJob = Argument.NotNull(packageJob, nameof(packageJob));

			providers = new INumberGeneratorValueProvider[]
			{
				new NumberGeneratorValueProvider(Keys.JobNo, JobNo)
			};
		}

		#region IEnumerable<INumberGeneratorValueProvider> Members

		public IEnumerator<INumberGeneratorValueProvider> GetEnumerator()
		{
			return ((IEnumerable<INumberGeneratorValueProvider>)providers).GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region JobNo

		string JobNo(NumberGenerator generator, string detail)
		{
			var parentJob = packageJob.ParentJob;
			return (parentJob != null) ? packageJob.ParentJob.JobNo : packageJob.KJ_JobID;
		}

		#endregion

		readonly INumberGeneratorValueProvider[] providers;
		readonly PkgPackageJob packageJob;
	}
}
