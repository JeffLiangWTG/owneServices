using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.ZA.Business.MessageBuilders;

namespace Enterprise.Customs.ZA.Business
{
	public class CusContainer : BaseCusContainer, Integration.Customs.ZA.ICusContainer, IContainerInformation
	{
		public CusContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Typed 'New' methods, functions and properties

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		public new CusContainerLookups Lookups
		{
			get { return (CusContainerLookups)base.Lookups; }
		}

		public new CusContainerValidation Validation
		{
			get { return (CusContainerValidation)base.Validation; }
		}

		#endregion

		#region Overrides

		protected override System.Type GetJobDeclarationType()
		{
			return typeof(JobDeclaration);
		}

		protected override Customs.Business.CusContainerValidation GetNewValidation()
		{
			return new CusContainerValidation(this);
		}

		protected override Customs.Business.CusContainerLookups GetNewLookups()
		{
			return new CusContainerLookups(this);
		}

		#endregion

		#region New Properties

		public ZBool IsContainerToBeAdvised
		{
			get
			{
				ZString containerNumber = CO_ContainerNumber;
				return !(Declaration != null
						 && Declaration.IsExport
						 && containerNumber.StartsWith("TBA")
						 && containerNumber.Length >= 4
						 && containerNumber.Substring(3, 1).IsNumbersOnlyOrEmpty);
			}
		}

		#endregion

		#region IContainerInformation

		ZString IContainerInformation.ContainerNumber
		{
			get
			{
				var result = CO_ContainerNumber;

				if (!CusContainerValidation.IsValidContainerNumberForZA(result))
				{
					return "NONU-" + result;
				}

				return result;
			}
		}

		ZString IContainerInformation.FirstSealNumber => CO_Seal;

		ZString IContainerInformation.SecondSealNumber => CO_SecondSeal;

		ZString IContainerInformation.ContainerMode => CO_FCL_LCL_AIR.TranslateToWCOContainerModeCode();

		#endregion
	}
}
