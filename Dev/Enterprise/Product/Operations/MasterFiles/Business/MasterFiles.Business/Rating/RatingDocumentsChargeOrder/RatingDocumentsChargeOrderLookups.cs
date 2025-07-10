using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Rating
{
	public class RatingDocumentsChargeOrderLookups : AutoRatingDocumentsChargeOrderLookups
	{
		public RatingDocumentsChargeOrderLookups(AutoRatingDocumentsChargeOrder parent) : base(parent)
		{
		}

		#region RatingDocumentsTypeList

		public CodeDescriptionPairList RatingDocumentsTypeList
		{
			get
			{
				var fRatingDocumentsTypeList = new CodeDescriptionPairList();
				fRatingDocumentsTypeList.Add(RatingDocumentsType.ALL);
				fRatingDocumentsTypeList.Add(RatingDocumentsType.FDC);
				fRatingDocumentsTypeList.Add(RatingDocumentsType.DRF);
				fRatingDocumentsTypeList.Add(RatingDocumentsType.DAP);
				fRatingDocumentsTypeList.Add(RatingDocumentsType.DNA);

				return fRatingDocumentsTypeList;
			}
		}

		public static class RatingDocumentsType
		{
			public static CodeDescriptionPair ALL => new CodeDescriptionPair("ALL", Res.GetString("186B05E9-6653-4064-85E9-6CDF6E4E0ECA", "All Documents"));

			public static CodeDescriptionPair FDC => new CodeDescriptionPair("FDC", Res.GetString("C8714F34-7D2A-49EE-8E3C-46F6013DD40F", "Final Documents"));

			public static CodeDescriptionPair DRF => new CodeDescriptionPair("DRF", Res.GetString("63AD17FE-45F6-4A96-94A2-D6626EF1E5C0", "Draft Documents (Not Approved and Approved)"));

			public static CodeDescriptionPair DAP => new CodeDescriptionPair("DAP", Res.GetString("425506BF-0122-4BE0-98E9-16409BA7C3D6", "Draft Documents (Approved)"));

			public static CodeDescriptionPair DNA => new CodeDescriptionPair("DNA", Res.GetString("68F9887E-1208-4527-9A73-AACD91AE5BCE", "Draft Documents (Not Approved)"));
		}

		#endregion
	}
}
