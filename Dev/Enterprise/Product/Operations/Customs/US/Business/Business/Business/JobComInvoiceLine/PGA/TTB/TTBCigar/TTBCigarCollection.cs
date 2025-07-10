using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class TTBCigarCollection : DependentCusAddInfoCollection<TTBCigar, TTBLine>
	{
		public TTBCigarCollection(TTBLine master)
			: base(master, CusAddInfoTypeAttribute.Codes.USTTBCigar)
		{
		}

		protected override bool AllowNewCore
		{
			get
			{
				var result = base.AllowNewCore;
				if (result)
				{
					var ttbLine = Master;
					result = ttbLine != null && ttbLine.IsTobaccoProgramTypeAndNotPaperOrTube;
				}
				return result;
			}
		}
	}
}
