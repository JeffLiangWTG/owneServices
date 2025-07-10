using System.Collections.Generic;

namespace Enterprise.Customs.PL.Business.Declaration;

public interface ITranCircumstanceSupporter
{
	IEnumerable<TranCircumstance> TranCircumstances { get; }
}
