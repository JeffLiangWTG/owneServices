using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business.Testing
{
	sealed class MockIContainer : NonPersistentBusinessObject, IContainer
	{
		public ZString ContainerMode;
		ZString IContainer.ContainerMode
		{
			get { return this.ContainerMode; }
		}

		bool fIsQuarantineRequired;
		public ZBool IsQuarantineRequired
		{
			get { return this.fIsQuarantineRequired; }
			set { this.fIsQuarantineRequired = value; }
		}
		ZBool IContainer.IsQuarantineRequired
		{
			get { return this.fIsQuarantineRequired; }
		}

		bool fIsCustomsHold;
		public ZBool IsCustomsHold
		{
			get { return this.fIsCustomsHold; }
			set { this.fIsCustomsHold = value; }
		}
		ZBool IContainer.IsCustomsHold
		{
			get { return this.fIsCustomsHold; }
		}

		bool fIsFumigationRequired;
		public ZBool IsFumigationRequired
		{
			get { return this.fIsFumigationRequired; }
			set { this.fIsFumigationRequired = value; }
		}
		ZBool IContainer.IsFumigationRequired
		{
			get { return this.fIsFumigationRequired; }
		}
	}
}
