using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.SG.V4.Business
{
	public class DocPrintPermitContainers : DocumentWrapper
	{
		protected DocPrintPermitContainers(PrintPermitContainers containers, BusinessObjectFactory factoryToWrap)
			: base(containers, factoryToWrap)
		{
		}

		public static DocPrintPermitContainers New(PrintPermitContainers containers, BusinessObjectFactory factoryToWrap)
		{
			return containers != null ? new DocPrintPermitContainers(containers, factoryToWrap) : null;
		}

		protected IPrintPermitContainers inContainers
		{
			get { return ((PrintPermitContainers)WrappedObject).inContainers; }
		}

		#region IPrintPermitContainers

		public ZString ContainerIdentifier1
		{
			get { return inContainers != null ? inContainers.ContainerIdentifier1 : new ZString(""); }
		}

		public ZString ContainerSequenceNumber1
		{
			get { return inContainers != null ? inContainers.ContainerSequenceNumber1 : new ZString(""); }
		}

		public ZString ContainerIdentifier2
		{
			get { return inContainers != null ? inContainers.ContainerIdentifier2 : new ZString(""); }
		}

		public ZString ContainerSequenceNumber2
		{
			get { return inContainers != null ? inContainers.ContainerSequenceNumber2 : new ZString(""); }
		}

		#endregion
	}

	public class DocPrintPermitContainersCollection : DocumentWrapperCollection
	{
		public DocPrintPermitContainersCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocPrintPermitContainers this[int index]
		{
			get { return (DocPrintPermitContainers)base[index]; }
		}
	}
}
