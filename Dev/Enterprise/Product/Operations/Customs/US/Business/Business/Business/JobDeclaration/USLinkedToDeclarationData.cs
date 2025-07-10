using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USLinkedToDeclarationData : Customs.Business.LinkedToDeclarationData
	{
		public USLinkedToDeclarationData(JobDeclaration declaration)
			: base(declaration)
		{
			IsACE = declaration.IsACE;
			IsACECargoCertificationMode = declaration.IsACECargoCertificationMode;
			CanHavePGAFDA = declaration.CanHavePGAFDA;
			ACECargoReleaseType = IsACE ? declaration.US_CargoReleaseType : ZString.Empty;
		}

		public bool IsACE { get; private set; }
		public bool IsACECargoCertificationMode { get; private set; }
		public bool CanHavePGAFDA { get; private set; }
		public ZString ACECargoReleaseType { get; private set; }
	}
}
