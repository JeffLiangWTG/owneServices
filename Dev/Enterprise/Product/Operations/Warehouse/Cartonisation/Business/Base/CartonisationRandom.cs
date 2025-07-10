using System;

namespace Enterprise.Warehouse.Cartonisation.Business
{
	public class CartonisationRandom : IRandom
	{
		#region IRandom Members

		[ThreadStatic]
		static Random LocalRandom;

#if DEBUG
		protected virtual
#endif
		Random GetNewRandom()
		{
			return new Random(Guid.NewGuid().GetHashCode());
		}

		public int Next(int min, int max)
		{
			InitialiseRandom();
			return LocalRandom.Next(min, max);
		}

		public int Next(int max)
		{
			InitialiseRandom();
			return LocalRandom.Next(max);
		}

		// Initialise as a random using Guid.NewGuid as a seed
		// https://stackoverflow.com/questions/3049467/is-c-sharp-random-number-generator-thread-safe
		void InitialiseRandom()
		{
			if (LocalRandom == null)
			{
				LocalRandom = GetNewRandom();
			}
		}

		#endregion
	}
}
