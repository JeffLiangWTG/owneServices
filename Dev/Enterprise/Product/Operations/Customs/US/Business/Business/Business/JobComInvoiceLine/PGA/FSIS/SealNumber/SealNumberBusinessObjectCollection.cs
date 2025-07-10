using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class SealNumberBusinessObjectCollection : NonPersistentBusinessObjectCollection<SealNumberBusinessObject>
	{
		public SealNumberBusinessObjectCollection(ZString commaSeperateSealNumbers)
		{
			foreach (var sealNumber in commaSeperateSealNumbers.Split(',').Select(x => x.Trim()).Where(x => !x.IsEmpty))
			{
				Add(new SealNumberBusinessObject(sealNumber));
			}
		}

		public ZString GetSealNumbersAsCommaSepereateString()
		{
			var builder = new ZStringBuilder();
			foreach (var sealNumber in GetSealNumbersAsCollection())
			{
				builder.Append(sealNumber);
			}
			ZString result = builder.ToStringWithDelimiterBetweenAppends(",");
			return result.Left(USFSISLineAddInfo.Schema.US_SealNumbersMaxLength);
		}

		public IEnumerable<ZString> GetSealNumbersAsCollection()
		{
			return this.Cast<SealNumberBusinessObject>().Select(x => x.SealNumber.Trim()).Where(x => !x.IsEmpty);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SealNumberBusinessObject(ZString.Empty);
		}
	}
}
