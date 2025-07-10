using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class DocumentContainer : NonPersistentBusinessObject
	{
		public DocumentContainer(CommonContainer container)
			: base(container.Factory)
		{
			this.container = container;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public static class Schema
		{
			public const string PrintContainer = "PrintContainer";
		}

		public CommonContainer Container
		{
			get { return container; }
		}
		readonly CommonContainer container;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			PrintContainer = true;
		}

		public ZBool PrintContainer
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return printContainer; }
			set { SetNonPersistentPropertyValue(PrintContainerInfo, ref printContainer, value); }
		}
		ZBool printContainer;

		public ZPropertyInfo PrintContainerInfo
		{
			get { return GetZPropertyInfo(Schema.PrintContainer); }
		}
	}
}
