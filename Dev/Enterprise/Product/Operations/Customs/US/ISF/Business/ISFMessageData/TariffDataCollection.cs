using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.ISF.Business
{
	class TariffDataCollection : IEnumerable<ITariffData>
	{
		public enum MergeStyle { NotMerge, Merge }

		public TariffDataCollection(MergeStyle mergeStyle)
		{
			this.mergeStyle = mergeStyle;
		}
		readonly MergeStyle mergeStyle;

		List<ITariffData> List
		{
			get { return fList ?? (fList = new List<ITariffData>()); }
		}
		List<ITariffData> fList;

		public void Add(ZString countryOfOrigin, ZString tariffNumber)
		{
			TariffData data = new TariffData() { CountryOfOrigin = countryOfOrigin, HarmonizedTariffNumber = tariffNumber };
			if (mergeStyle == MergeStyle.NotMerge || !List.Contains(data))
			{
				List.Add(data);
			}
		}

		#region IEnumerable<ITariffData> Members

		public IEnumerator<ITariffData> GetEnumerator()
		{
			return List.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return List.GetEnumerator();
		}

		#endregion
	}
}
