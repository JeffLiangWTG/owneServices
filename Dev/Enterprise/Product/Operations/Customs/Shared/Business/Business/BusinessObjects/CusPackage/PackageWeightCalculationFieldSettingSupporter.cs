using System;

namespace Enterprise.Customs.Business
{
	public interface IPackageWeightCalculationFieldSettingSupporter
	{
		void Start(object type);
		void Stop(object type);
	}

	public class PackageWeightCalculationFieldSettingSupporter : IDisposable
	{
		public PackageWeightCalculationFieldSettingSupporter(IPackageWeightCalculationFieldSettingSupporter supporter, object type)
		{
			this.supporter = supporter;
			this.type = type;
			supporter.Start(type);
		}

		readonly IPackageWeightCalculationFieldSettingSupporter supporter;
		readonly object type;

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				supporter.Stop(type);
			}
		}

		void IDisposable.Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}

	public enum PackageWeightCalculationFieldSettingType
	{
		KP_Weight,
		KP_PackageQty,
		UnitGrossWeight,
		NetWeight,
		UnitNetWeight,
		KP_TareWeight,
		KP_DunnageWeight
	}
}
