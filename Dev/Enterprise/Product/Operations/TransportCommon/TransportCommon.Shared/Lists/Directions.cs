using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportCommon.Shared
{
	public class Directions
	{
		public static ZString GetDirectionCodeFromDtbBookingDirection(DtbBookingDirection dtbDirection)
		{
			switch (dtbDirection)
			{
				case DtbBookingDirection.PIC:
					return Constants.CartageDirection.Origin;
				case DtbBookingDirection.DLV:
					return Constants.CartageDirection.Destination;
				default:
					return "";
			}
		}

		#region List

		public CodeDescriptionPairList List
		{
			get
			{
				if (list == null)
				{
					list = new CodeDescriptionPairList();
					list.AddPair(Constants.CartageDirection.Import, Constants.CartageDirectionDescription.Import);
					list.AddPair(Constants.CartageDirection.Export, Constants.CartageDirectionDescription.Export);
					list.AddPair(Constants.CartageDirection.Origin, Constants.CartageDirectionDescription.Origin);
					list.AddPair(Constants.CartageDirection.Destination, Constants.CartageDirectionDescription.Destination);
					list.AddPair(Constants.CartageDirection.Local, Constants.CartageDirectionDescription.Local);
				}
				return list;
			}
		}

		CodeDescriptionPairList list;

		#endregion
	}
}
