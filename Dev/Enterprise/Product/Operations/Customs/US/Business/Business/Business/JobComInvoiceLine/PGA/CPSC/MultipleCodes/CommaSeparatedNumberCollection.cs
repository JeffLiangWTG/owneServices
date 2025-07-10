using System.Collections.Generic;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public abstract class CommaSeparatedNumberCollection : NonPersistentBusinessObjectCollection<CommaSeparatedNumber>
	{
		protected CommaSeparatedNumberCollection(ZString commaSeparatedCodes, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (var number in commaSeparatedCodes.Split(',').Select(x => x.Trim()).Where(x => !x.IsEmpty))
			{
				var multipleCode = AddNew();
				multipleCode.Number = number.Trim().Left(multipleCode.NumberMaxLength);
			}
		}

		public ZString GetCodesAsCommaSeparatedString()
		{
			var builder = new ZStringBuilder();
			foreach (var code in GetCodesAsCollection())
			{
				builder.Append(code);
			}
			ZString result = builder.ToStringWithDelimiterBetweenAppends(",");
			return result;
		}

		IEnumerable<ZString> GetCodesAsCollection()
		{
			return this.Cast<CommaSeparatedNumber>().Select(x => x.Number.Trim()).Where(x => !x.IsEmpty);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return CreatePersistentBusinessObjectCore();
		}

		public abstract CommaSeparatedNumber CreatePersistentBusinessObjectCore();

		public bool HasRepeatElement
		{
			get { return this.Cast<CommaSeparatedNumber>().DistinctBy(x => x.Number).Count() != Count; }
		}
	}
}

