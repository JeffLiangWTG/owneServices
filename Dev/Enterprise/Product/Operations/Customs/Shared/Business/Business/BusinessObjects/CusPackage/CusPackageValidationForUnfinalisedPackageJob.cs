using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;

namespace Enterprise.Customs.Business
{
	public class CusPackageValidationForUnfinalisedPackageJob : PkgPackageValidationForUnfinalisedPackageJob
	{
		public CusPackageValidationForUnfinalisedPackageJob(CusPackage parent)
			: base(parent)
		{
		}

		protected new CusPackage Parent => (CusPackage)base.Parent;

		CustomLabelPropertyValidation CustomLabelPropertyValidation => customLabelPropertyValidation ?? (customLabelPropertyValidation = new CustomLabelPropertyValidation(Parent.Factory));
		CustomLabelPropertyValidation customLabelPropertyValidation;

		CusPackageCustomLabelsProvider CustomLabelsProvider
		{
			get
			{
				if (customLabelsProvider == null)
				{
					var packingList = Parent.PackingList;
					if (packingList != null)
					{
						customLabelsProvider = new CusPackageCustomLabelsProvider(packingList);
					}
				}
				return customLabelsProvider;
			}
		}
		CusPackageCustomLabelsProvider customLabelsProvider;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCustomAttribute1();
			ValidateCustomAttribute2();
			ValidateCustomDate1();
			ValidateCustomDate2();
			ValidateCustomDecimal1();
			ValidateCustomDecimal2();
			ValidateNetWeight();
		}

		protected override void CheckKP_MarksAndNumbers()
		{
			base.CheckKP_MarksAndNumbers();
			MandatoryValidation.CheckEntered(Parent.KP_MarksAndNumbersInfo);
		}

		public void ValidateCustomAttribute1()
		{
			ValidateCalculatedProperty(Parent.CustomAttribute1Info);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Validators used via reflection")]
		void CheckCustomAttribute1()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.CustomAttribute1Info);
		}

		public void ValidateCustomAttribute2()
		{
			ValidateCalculatedProperty(Parent.CustomAttribute2Info);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Validators used via reflection")]
		void CheckCustomAttribute2()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.CustomAttribute2Info);
		}

		public void ValidateCustomDate1()
		{
			ValidateCalculatedProperty(Parent.CustomDate1Info);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Validators used via reflection")]
		void CheckCustomDate1()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.CustomDate1Info);
		}

		public void ValidateCustomDate2()
		{
			ValidateCalculatedProperty(Parent.CustomDate2Info);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Validators used via reflection")]
		void CheckCustomDate2()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.CustomDate2Info);
		}

		public void ValidateCustomDecimal1()
		{
			ValidateCalculatedProperty(Parent.CustomDecimal1Info);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Validators used via reflection")]
		void CheckCustomDecimal1()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.CustomDecimal1Info);
		}

		public void ValidateCustomDecimal2()
		{
			ValidateCalculatedProperty(Parent.CustomDecimal2Info);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Validators used via reflection")]
		void CheckCustomDecimal2()
		{
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.CustomDecimal2Info);
		}

		public void ValidateNetWeight()
		{
			ValidateCalculatedProperty(Parent.NetWeightInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Validators used via reflection")]
		void CheckNetWeight()
		{
			var totalNetWeight = Parent.CalculateAllPackedItemDivotsNetWeight();
			if (Parent.NetWeight != totalNetWeight)
			{
				Parent.NetWeightInfo.AddWarning(Res.GetString("9460667A-4D6A-4C24-9081-EB8116CF1D33", "The sum of Packed Item Net Weight {0} {1} does not balance with the Invoice Line Net Weight {2} {3}.", Parent.NetWeight, Parent.KP_WeightUQ, totalNetWeight, Parent.KP_WeightUQ));
			}
		}
	}
}
