using System.Collections;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using static Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement;

namespace Enterprise.TransportConsignment.Business
{
	public sealed class LTValueSource : IEnumerable<INumberGeneratorValueProvider>
	{
		public LTValueSource(DtbConsignment consignment)
		{
			this.consignment = consignment;

			providers = new INumberGeneratorValueProvider[]
			{
				new NumberGeneratorValueProvider(Keys.ServiceLevel, ServiceLevel),
			};
		}

		#region IEnumerable<INumberGeneratorValueProvider> Members

		public IEnumerator<INumberGeneratorValueProvider> GetEnumerator()
		{
			return providers.GetEnumerator();
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
			return consignment.LTC_RS_NKServiceLevel;
		}

		readonly IReadOnlyCollection<INumberGeneratorValueProvider> providers;
		readonly DtbConsignment consignment;
	}
}
