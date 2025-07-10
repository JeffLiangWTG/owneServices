using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class ExchangeHedgeMethodofPaymentCodeListParser : BaseRefCusCodeListParser<Stream>
	{
		public ExchangeHedgeMethodofPaymentCodeListParser(string dataSource) : base(dataSource)
		{
		}

		protected override string CodeType => Constants.RefCusCodeTypes.CustomsMethodOfPayment.Code;

		protected override bool HasAttributes => false;

		protected override IEnumerable<RefCusCodeList> GetRefCusCodeLists(Stream dataSource)
		{
			Argument.NotNull(dataSource, nameof(dataSource));

			var result = new List<RefCusCodeList>();

			var xml = XDocument.Load(dataSource);

			Contract.Assume(xml != null);
			var methodOfPayments = xml.Root.Descendants(ExchangeHedgeMethodofPaymentCodeListConstants.TagMethodOfPayment);

			foreach (XElement element in methodOfPayments)
			{
				var refCusCodeList = new RefCusCodeList
				{
					ZZD_Code = element.GetElementValueAsString(ExchangeHedgeMethodofPaymentCodeListConstants.TagMethodOfPaymentCode, 35),
					ZZD_Description = element.GetElementValueAsString(ExchangeHedgeMethodofPaymentCodeListConstants.TagMethodOfPaymentDescription, 2000)
				};

				refCusCodeList.ZZD_StartDate = element.GetElementValueAsDateTime(ExchangeHedgeMethodofPaymentCodeListConstants.TagMethodOfPaymentStartDate);

				result.Add(refCusCodeList);
			}
			return result;
		}

		protected override IEnumerable<RefCusCodeType> GetRefCusCodeType()
		{
			yield return new RefCusCodeType()
			{
				ZZK_CodeType = CodeType,
				ZZK_Description = Constants.RefCusCodeTypes.CustomsMethodOfPayment.Description
			};
		}
	}
}
