using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class AdditionalInformationLookups : Customs.Business.CusCodeDataLookups
	{
		public AdditionalInformationLookups(AdditionalInformation parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList CY_CodeList
		{
			get
			{
				var date = ZDateTime.Today;
				var isExport = false;
				var isLine1 = false;
				var entryLine = Parent.Parent;
				if (entryLine != null)
				{
					var entryHeader = entryLine.Header;
					if (entryHeader != null)
					{
						isExport = entryHeader.Declaration?.IsExport ?? ZBool.False;
						date = entryHeader.EntryInstructionAssessmentDate;
					}
					isLine1 = entryLine.IsLine1;
				}
				return ZARefCusCodeListTypes.GetAdditionalInformationList(Factory, date, isExport, isLine1);
			}
		}

		protected new AdditionalInformation Parent
		{
			get { return (AdditionalInformation)base.Parent; }
		}
	}
}
