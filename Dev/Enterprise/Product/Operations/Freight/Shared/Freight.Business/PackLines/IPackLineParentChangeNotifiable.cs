using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public interface IPackLineParentChangeNotifiable
	{
		void NotifyWeightChanged(ZDecimal oldWeight, ZDecimal newWeight);
		void NotifyWeightUQChanged(ZString oldWeightUQ, ZString newWeightUQ);

		void NotifyVolumeChanged(ZDecimal oldVolume, ZDecimal newVolume);
		void NotifyVolumeUQChanged(ZString oldVolumeUQ, ZString newVolumeUQ);

		void NotifyPackageCountChanged(ZInt oldCount, ZInt newCount);
		void NotifyPackageTypeChanged(ZString oldType, ZString newType);
		void NotifyLoadingMetersChanged(ZDecimal oldValue, ZDecimal newValue);
		void NotifyDescriptionChanged(ZString oldValue, ZString newValue);
	}
}
