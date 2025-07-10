using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusPermitCusDecProcessorForTesting : CusPermitCusDecProcessor<EDIMessage>
	{
		public CusPermitCusDecProcessorForTesting(CusEntryHeader header) : base(header)
		{
		}

		public new IList<PermitRecord> PermitRecords => base.PermitRecords;

		protected override IList<PermitRecord> GetPermitRecords()
		{
			var factory = new BusinessObjectFactory();
			var permitRecords = new List<PermitRecord>();
			var managedPermit = factory.NewMoq<BaseCusPermitHeader>();
			managedPermit.Setup(m => m.CPH_Number).Returns("123");
			managedPermit.Setup(m => m.IsTransactionsApplicable()).Returns(true);

			var notManagedPermit = factory.NewMoq<BaseCusPermitHeader>();
			notManagedPermit.Setup(m => m.CPH_Number).Returns("ABC");
			notManagedPermit.Setup(m => m.IsTransactionsApplicable()).Returns(false);

			permitRecords.Add(new PermitRecord
			{
				PermitHeader = managedPermit.Object
			});

			permitRecords.Add(new PermitRecord
			{
				PermitHeader = notManagedPermit.Object
			});

			permitRecords.Add(new PermitRecord
			{
				PermitHeader = null
			});

			return permitRecords;
		}
	}
}
