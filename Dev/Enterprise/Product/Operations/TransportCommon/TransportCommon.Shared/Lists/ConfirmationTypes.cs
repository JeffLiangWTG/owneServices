using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportCommon.Shared
{
	public class ConfirmationTypes
	{
		#region Codes

		public static class Codes
		{
			public const string PickUp = "PIC";
			public const string Delivery = "DLV";
			public const string ConNoteNo = "CON";
		}

		#endregion

		#region Descriptions

		public static class Descriptions
		{
			public static MultilingualString PickUp
			{
				get { return ResString.GetMultilingualString("61557610-3b98-42f0-9ab6-ed3d812830c7", "Pickup"); }
			}
			public static MultilingualString Delivery
			{
				get { return ResString.GetMultilingualString("02869588-9181-4591-87e4-2172d275e0d4", "Delivery"); }
			}
			public static MultilingualString ConnoteNo
			{
				get { return ResString.GetMultilingualString("b56a65cd-7059-4998-98c0-bda89a8ca5f0", "Connote #"); }
			}
		}

		#endregion

		#region List

		public CodeDescriptionPairList List
		{
			get
			{
				if (list == null)
				{
					list = new CodeDescriptionPairList();

					list.AddPair(Codes.PickUp, Descriptions.PickUp);
					list.AddPair(Codes.Delivery, Descriptions.Delivery);
					list.AddPair(Codes.ConNoteNo, Descriptions.ConnoteNo);
				}

				return list;
			}
		}

		CodeDescriptionPairList list;

		#endregion
	}
}
