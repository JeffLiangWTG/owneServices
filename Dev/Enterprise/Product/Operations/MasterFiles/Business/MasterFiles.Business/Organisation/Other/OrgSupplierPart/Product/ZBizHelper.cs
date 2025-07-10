using System;
using CargoWise.Types;
using Enterprise.Core;

//using Enterprise.SharedComponents;

namespace Enterprise.MasterFiles.Business
{
	public class ZCusBizHelper
	{
		protected ZCusBizHelper()
		{
		}

		[ThreadStatic]
		protected static ZCusBizHelper fInstance;
		public static ZCusBizHelper GetInstance()
		{
			if (fInstance == null)
			{
				fInstance = new ZCusBizHelper();
			}
			return fInstance;
		}

		public bool StandardConvertible(ZString fromUQ, ZString toUQ)
		{
			bool result = false;

			result |= Constants.Weight.ContainsCode(fromUQ) && Constants.Weight.ContainsCode(toUQ);

			result |= Constants.Length.ContainsCode(fromUQ) && Constants.Length.ContainsCode(toUQ);

			result |= Constants.Volume.ContainsCode(fromUQ) && Constants.Volume.ContainsCode(toUQ);

			return result;
		}

		public ZDecimal GetConversionFactor(ZString fromUQ, ZString toUQ)
		{
			ZDecimal result = 0m;
			if (Constants.Weight.ContainsCode(fromUQ) && Constants.Weight.ContainsCode(toUQ))
			{
				result = Constants.Weight.Convert(1m, fromUQ, toUQ);
			}
			else if (Constants.Length.ContainsCode(fromUQ) && Constants.Length.ContainsCode(toUQ))
			{
				result = Constants.Length.Convert(1m, fromUQ, toUQ);
			}
			else if (Constants.Volume.ContainsCode(fromUQ) && Constants.Volume.ContainsCode(toUQ))
			{
				result = Constants.Volume.Convert(1m, fromUQ, toUQ);
			}
			return result;
		}
	}
}