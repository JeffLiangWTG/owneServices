using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	/// <summary>
	/// Summary description for IPackLineCollection.
	/// </summary>
	public interface IPackLineCollection : IEnumerable
	{
		PackLineCollectionCalculator Totals { get; }
		BusinessObjectFactory Factory { get; }

		int Count { get; }
		ZString MasterPackagesUnit { get; }
		ZString MasterWeightUnit { get; }
		ZString MasterVolumeUnit { get; }
	}
}
