using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportCommon.Shared
{
	public class InstructionTypes
	{
		#region Codes

		public static class Codes
		{
			public const string PickUp = "PIC";
			public const string Delivery = "DLV";
			public const string Multi = "MLT";
		}

		#endregion

		#region Descriptions

		public static class Descriptions
		{
			public static MultilingualString PickUp
			{
				get { return ResString.GetMultilingualString("6c8eace8-9517-4c51-a423-aa3f3181fd95", "Pickup"); }
			}
			public static MultilingualString Delivery
			{
				get { return ResString.GetMultilingualString("3d269816-0e09-4672-8f5a-998f16343735", "Delivery"); }
			}
			public static MultilingualString Multi
			{
				get { return ResString.GetMultilingualString("b6852478-8d2a-4127-a18f-ba69aa4ff6e6", "Multi"); }
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
					list.AddPair(Codes.Multi, Descriptions.Multi);
				}

				return list;
			}
		}

		CodeDescriptionPairList list;

		#endregion
	}
}
