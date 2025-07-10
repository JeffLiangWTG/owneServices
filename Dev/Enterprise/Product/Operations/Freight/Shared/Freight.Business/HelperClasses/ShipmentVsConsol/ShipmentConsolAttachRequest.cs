using System;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class ShipmentConsolAttachRequest : IShipmentConsolAttachRequest
	{
		public ShipmentConsolAttachRequest(Func<ZString> errorsGetter, Func<ZString> warningsGetter)
		{
			error = new Lazy<ZString>(errorsGetter ?? (() => { return ZString.Empty; }));
			warning = new Lazy<ZString>(warningsGetter ?? (() => { return ZString.Empty; }));
		}

		readonly Lazy<ZString> error;
		readonly Lazy<ZString> warning;

		public ZString Errors
		{
			get { return error.Value; }
		}

		public ZString Warnings
		{
			get { return warning.Value; }
		}
	}
}
