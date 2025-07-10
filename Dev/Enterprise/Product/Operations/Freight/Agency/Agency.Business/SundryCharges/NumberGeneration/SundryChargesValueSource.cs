using System.Collections;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Keys = Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement.Keys;

namespace Enterprise.Freight.Agency.Business
{
	internal sealed class SundryChargesValueSource : IEnumerable<INumberGeneratorValueProvider>
	{
		public SundryChargesValueSource(SundryCharges sundry)
		{
			this.sundry = sundry;

			providers = new INumberGeneratorValueProvider[]
			{
				new NumberGeneratorValueProvider(Keys.SundryChargesType, Type),
				new NumberGeneratorValueProvider(Keys.SundryChargesMode, Mode),
				new NumberGeneratorValueProvider(Keys.SundryChargesActivity, Activity),
			};
		}

		#region IEnumerable<INumberGeneratorValueProvider> Members

		public IEnumerator<INumberGeneratorValueProvider> GetEnumerator()
		{
			return ((IEnumerable<INumberGeneratorValueProvider>)providers).GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		string Type(NumberGenerator generator, string detail)
		{
			return sundry.D4_SundriesJobType;
		}

		string Mode(NumberGenerator generator, string detail)
		{
			return sundry.D4_SundryJobMode;
		}

		string Activity(NumberGenerator generator, string detail)
		{
			return sundry.D4_SundryJobActivity;
		}

		readonly INumberGeneratorValueProvider[] providers;
		readonly SundryCharges sundry;
	}
}


