using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class ContainerToSelectFromForPrinting : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ContainerToSelectFromForPrinting(CommonContainer container)
			: base(container.Factory)
		{
			Argument.NotNull(container, "container");
			Container = container;
			containerNumber = container.JC_ContainerNum;
			jc_Calc_PrintDocumentForContainer = container.JC_ContainerMode == Core.Constants.ContainerModes.FCL;
		}

		#region Container

		public CommonContainer Container { get; private set; }

		#endregion

		#region JC_Calc_PrintDocumentForContainer

		protected ZBool jc_Calc_PrintDocumentForContainer;
		public ZBool JC_Calc_PrintDocumentForContainer
		{
			get { return jc_Calc_PrintDocumentForContainer; }
			set
			{
				jc_Calc_PrintDocumentForContainer = value;
				JC_Calc_PrintDocumentForContainerInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JC_Calc_PrintDocumentForContainerInfo
		{
			get { return GetZPropertyInfo(nameof(JC_Calc_PrintDocumentForContainer)); }
		}

		#endregion

		#region ContainerNumber

		ZString containerNumber;
		public ZString ContainerNumber
		{
			get { return containerNumber; }
			set
			{
				containerNumber = value;
				ContainerNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ContainerNumberInfo
		{
			get { return GetZPropertyInfo(nameof(ContainerNumber)); }
		}

		#endregion

		#region JC_EPANStatusForBinding

		public ZString JC_EPANStatusForBinding
		{
			get
			{
				return Container.JC_EPANStatus;
			}
		}

		public ZPropertyInfo JC_EPANStatusForBindingInfo
		{
			get { return GetZPropertyInfo(nameof(JC_EPANStatusForBinding)); }
		}

		#endregion
	}
}
