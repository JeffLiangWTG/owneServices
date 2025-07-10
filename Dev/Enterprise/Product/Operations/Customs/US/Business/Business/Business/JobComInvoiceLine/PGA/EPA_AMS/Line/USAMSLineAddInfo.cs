using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USAMSLine)]
	public class USAMSLineAddInfo : AutoUSAMSLineAddInfo
	{
		public USAMSLineAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public AMSLine AMSLine => Parent as AMSLine;

		protected override USAMSLineAddInfoValidation GetNewValidation()
		{
			if (AMSLine != null)
			{
				var amsHeader = AMSLine.Parent;
				var program = amsHeader != null ? amsHeader.US_Program : ZString.Empty;
				switch (program)
				{
					case AMSProgramList.Codes.EG1:
						return new USAMSEG1AddInfoValidation(this);
					case AMSProgramList.Codes.EG2:
						return new USAMSEG2AddInfoValidation(this);
					case AMSProgramList.Codes.MO2:
						return new USAMSMO2AddInfoValidation(this);
					case AMSProgramList.Codes.MO3:
						return new USAMSMO3AddInfoValidation(this);
					case AMSProgramList.Codes.MO5:
					case AMSProgramList.Codes.PN1:
						return new USAMSMO5AddInfoValidation(this);
					case AMSProgramList.Codes.MO1:
						return new USAMSMO1AddInfoValidation(this);
					case AMSProgramList.Codes.OR2:
						return new USAMSOR2AddInfoValidation(this);
				}
			}
			return base.GetNewValidation();
		}

		public OrgAddress FinalHandlerAddress
		{
			get
			{
				return Factory.GetCachedValue(US_OA_FinalHandler.ToString(), () =>
				{
					return Factory.Load<OrgAddress>(US_OA_FinalHandler);
				});
			}
		}

		public OrgAddress CerFinalHandlerAddress
		{
			get
			{
				return Factory.GetCachedValue(US_OA_CerFinalHandler.ToString(), () =>
				{
					return Factory.Load<OrgAddress>(US_OA_CerFinalHandler);
				});
			}
		}
	}
}
