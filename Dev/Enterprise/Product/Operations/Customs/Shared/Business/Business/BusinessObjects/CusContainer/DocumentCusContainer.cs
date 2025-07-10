using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// Summary description for DocumentCusContainer.
	/// </summary>
	public class DocumentCusContainer : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public class Schema : BaseCusContainer.Schema
		{
			public const string PrintContainer = "PrintContainer";
		}

		#endregion

		public DocumentCusContainer(BusinessObjectFactory factory) : base(factory)
		{
		}

		#region Related Objects

		#region Container

		BaseCusContainer fContainer;
		public BaseCusContainer Container
		{
			get { return fContainer; }
			set { fContainer = value; }
		}

		#endregion

		#endregion

		#region Overrides

		protected override void SetDefaultValues()
		{
			fPrintContainer = ZBool.True;
			base.SetDefaultValues();
		}

		#endregion

		#region Properties

		public ZBool PrintContainer
		{
			get { return fPrintContainer; }
			set
			{
				fPrintContainer = value;
				PrintContainerInfo.RefreshBinding();
			}
		}
		ZBool fPrintContainer;

		public ZPropertyInfo PrintContainerInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.PrintContainer); }
		}

		#endregion
	}
}
