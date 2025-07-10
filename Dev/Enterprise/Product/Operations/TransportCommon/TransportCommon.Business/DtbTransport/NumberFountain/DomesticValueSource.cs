using System.Collections;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Business.Common;
using Keys = Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement.Keys;

namespace Enterprise.TransportCommon.Business
{
	public sealed class DomesticValueSource : IEnumerable<INumberGeneratorValueProvider>
	{
		public DomesticValueSource(AutoDtbBooking transport)
		{
			this.transport = transport;

			providers = new INumberGeneratorValueProvider[]
			{
				new NumberGeneratorValueProvider(Keys.ServiceLevel, ServiceLevel),
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

		string ServiceLevel(NumberGenerator generator, string detail)
		{
			return transport.KM_RS_NKServiceLevel;
		}

		readonly INumberGeneratorValueProvider[] providers;
		readonly AutoDtbBooking transport;
	}
}
