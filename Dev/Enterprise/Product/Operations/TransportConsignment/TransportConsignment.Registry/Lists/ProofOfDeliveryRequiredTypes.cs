using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportConsignment.Registry
{
	public class ProofOfDeliveryRequiredTypes : CodeDescriptionPairList
	{
		#region Codes

		public abstract class Codes
		{
			public const string None = "NON";
			public const string SignatureRequired = "SIG";
			public const string PhotoRequired = "PHO";
			public const string SignatureOrPhotoRequired = "SOP";
			public const string SignatureAndPhotoRequired = "SAP";
		}

		#endregion

		#region Descriptions

		public abstract class Descriptions
		{
			public static MultilingualString None { get { return ResString.GetMultilingualString("2c30fe92-db72-4897-93cc-94e8a50b4aea", "None"); } }
			public static MultilingualString SignatureRequired { get { return ResString.GetMultilingualString("c9914e86-737f-425e-8b10-a59fd7277edc", "Signature"); } }
			public static MultilingualString PhotoRequired { get { return ResString.GetMultilingualString("d18553fe-2426-48a7-afd2-c0f885cb38eb", "Photo"); } }
			public static MultilingualString SignatureOrPhotoRequired { get { return ResString.GetMultilingualString("41e40094-dc8c-40cb-979d-e405a359f958", "Signature or Photo"); } }
			public static MultilingualString SignatureAndPhotoRequired { get { return ResString.GetMultilingualString("161cb4c0-13c4-4508-8e9f-8c3c13301f93", "Signature and Photo"); } }
		}

		#endregion

		#region ProofOfDeliveryRequiredTypes

		public ProofOfDeliveryRequiredTypes()
		{
			AddPair(Codes.None, Descriptions.None);
			AddPair(Codes.SignatureRequired, Descriptions.SignatureRequired);
			AddPair(Codes.PhotoRequired, Descriptions.PhotoRequired);
			AddPair(Codes.SignatureOrPhotoRequired, Descriptions.SignatureOrPhotoRequired);
			AddPair(Codes.SignatureAndPhotoRequired, Descriptions.SignatureAndPhotoRequired);
		}

		#endregion
	}
}
