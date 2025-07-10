using System.Collections.Immutable;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public partial class GovernmentAgencyIDList
	{
		static readonly ImmutableArray<string> GovernmentAgencyIDIsBetweenR9901AndR9920 = ImmutableArray.Create(
			Codes.R9901, Codes.R9902, Codes.R9903, Codes.R9904, Codes.R9905,
			Codes.R9906, Codes.R9907, Codes.R9908, Codes.R9909, Codes.R9910,
			Codes.R9911, Codes.R9912, Codes.R9913, Codes.R9914, Codes.R9915,
			Codes.R9916, Codes.R9917, Codes.R9918, Codes.R9919, Codes.R9920);

		static readonly ImmutableArray<string> GovernmentAgencyIDIsBetweenR9921AndR9950 = ImmutableArray.Create(
			Codes.R9921, Codes.R9922, Codes.R9923, Codes.R9924, Codes.R9925,
			Codes.R9926, Codes.R9927, Codes.R9928, Codes.R9929, Codes.R9930,
			Codes.R9931, Codes.R9932, Codes.R9933, Codes.R9934, Codes.R9935,
			Codes.R9936, Codes.R9937, Codes.R9938, Codes.R9939, Codes.R9940,
			Codes.R9941, Codes.R9942, Codes.R9943, Codes.R9944, Codes.R9945,
			Codes.R9946, Codes.R9947, Codes.R9948, Codes.R9949, Codes.R9950);

		static readonly ImmutableArray<string> GovernmentAgencyIDIsBetweenR9951AndR9980 = ImmutableArray.Create(
			Codes.R9951, Codes.R9952, Codes.R9953, Codes.R9954, Codes.R9955,
			Codes.R9956, Codes.R9957, Codes.R9958, Codes.R9959, Codes.R9960,
			Codes.R9961, Codes.R9962, Codes.R9963, Codes.R9964, Codes.R9965,
			Codes.R9966, Codes.R9967, Codes.R9968, Codes.R9969, Codes.R9970,
			Codes.R9971, Codes.R9972, Codes.R9973, Codes.R9974, Codes.R9975,
			Codes.R9976, Codes.R9977, Codes.R9978, Codes.R9979, Codes.R9980);

		static readonly ImmutableArray<string> GovernmentAgencyIDIsBetweenR9981AndR9999 = ImmutableArray.Create(
			Codes.R9981, Codes.R9982, Codes.R9983, Codes.R9984, Codes.R9985,
			Codes.R9986, Codes.R9987, Codes.R9988, Codes.R9989, Codes.R9990,
			Codes.R9991, Codes.R9992, Codes.R9993, Codes.R9994, Codes.R9995,
			Codes.R9996, Codes.R9997, Codes.R9998, Codes.R9999);

		public static bool IsGovernmentAgencyIDBetweenR9901AndR9920(ZString governmentAgencyID) => GovernmentAgencyIDIsBetweenR9901AndR9920.Contains(governmentAgencyID);

		public static bool IsGovernmentAgencyIDBetweenR9921AndR9950(ZString governmentAgencyID) => GovernmentAgencyIDIsBetweenR9921AndR9950.Contains(governmentAgencyID);

		public static bool IsGovernmentAgencyIDBetweenR9951AndR9980(ZString governmentAgencyID) => GovernmentAgencyIDIsBetweenR9951AndR9980.Contains(governmentAgencyID);

		public static bool IsGovernmentAgencyIDBetweenR9981AndR9999(ZString governmentAgencyID) => GovernmentAgencyIDIsBetweenR9981AndR9999.Contains(governmentAgencyID);
	}
}
