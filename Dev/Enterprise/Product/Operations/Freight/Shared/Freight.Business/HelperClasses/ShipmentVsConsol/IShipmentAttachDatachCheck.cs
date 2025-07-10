
namespace Enterprise.Freight.Business
{
	using System.Collections.Generic;
	using CargoWise.EntityFramework;

	public interface IShipmentAttachDetachCheck
	{
		/// <summary>
		///     Checks whether it is possible to attach/detach <paramref name="shipments"/>/<paramref name="consols"/> to a <paramref name="parent"/>.
		/// </summary>
		/// <param name="action">
		///     An user concrete action.
		/// </param>
		/// <param name="shipments">
		///     A list of shipments to attach/detach if the <paramref name="parent"/> is <see cref="CommonConsol"/>.
		/// </param>
		/// <param name="consols">
		///     A list of consols to attach/detach if the <paramref name="parent"/> is <see cref="CommonShipment"/>.
		/// </param>
		/// <param name="parent">
		///     An object attach/detach to.
		/// </param>
		/// <param name="listFormatter">
		///     A formatter to use for formatting the <paramref name="shipments"/> and <paramref name="consols"/> in the error message.
		/// </param>
		/// <returns>
		///     An error message if attach/detach is not possible; otherwise, <see cref="string.Empty"/>.
		/// </returns>
		string Check(
			AttachDetachAction action,
			IEnumerable<CommonShipment> shipments,
			IEnumerable<CommonConsol> consols,
			BusinessObject parent,
			ShipmentVsConsolMessageHelper.ListFormatterDelegate listFormatter);
	}
}
