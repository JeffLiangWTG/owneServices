using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Integration
{
	public interface IFreightRatingHelper
	{
		FreightMode CalculateFreightMode(ZString transportMode, ZString containerMode, Func<FreightMode> fallbackContainerFrieghtModeGetter);
	}
}
