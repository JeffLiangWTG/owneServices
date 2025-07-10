using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.DocumentPrinting.DocAmount
{
	internal sealed class EmptyDocAmount : DocAmount
	{
		internal EmptyDocAmount()
		{
		}

		public override MultilingualString AmountAsString => (NoResString)ZString.Empty;
	}
}
