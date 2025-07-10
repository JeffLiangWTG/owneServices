using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.eTail.Integration
{
	public interface IHVLVFWBDGCodeProvider
	{
		/// <summary>
		/// Returns an unmodifiable collection of the distinct dangerous goods DG codes for this item.
		/// </summary>
		IReadOnlyCollection<ZString> DGCodes { get; }

		/// <summary>
		/// Returns an unmodifiable collection of the distinct dangerous goods UNNO values for this item.
		/// </summary>
		IReadOnlyCollection<ZString> DGUNNOValues();
	}
}
