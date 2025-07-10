using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public interface ITariff
	{
		ZString Code
		{
			get;
			set;
		}

		ZString Unit1
		{
			get;
			set;
		}

		ZString Unit2
		{
			get;
			set;
		}

		ZString Unit3
		{
			get;
			set;
		}

		ZString ShortDescription
		{
			get;
			set;
		}

		IReadOnlyList<ZString> OGARequirements
		{
			get;
		}
	}
}
