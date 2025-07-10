using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.US.DIS.Business
{
	class DISToxicSubstanceDataWrapper : IDISToxicSubstanceData
	{
		public DISToxicSubstanceDataWrapper(DISToxicSubstanceData data)
		{
			this.data = data;
		}

		readonly DISToxicSubstanceData data;

		ZString IDISToxicSubstanceData.CASNumber
		{
			get
			{
				var casNumbers = from DISAdditionalNumber one in data.CASNumbers
								 orderby one.Number
								 select one.Number;

				return new ZStringBuilder(casNumbers).ToStringWithDelimiterBetweenAppends(", ");
			}
		}

		ZString IDISToxicSubstanceData.EPARegoNumber
		{
			get { return data.EPARegistrationNumber; }
		}

		ZString IDISToxicSubstanceData.EPAProducerEstNumber
		{
			get { return data.EPAProducerEstNumber; }
		}
	}
}
