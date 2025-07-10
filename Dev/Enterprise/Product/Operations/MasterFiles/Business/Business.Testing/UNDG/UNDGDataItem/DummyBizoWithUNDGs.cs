using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DummyBizoWithUNDGs : DummyBusinessObject, IUNDGDataItemProvider
	{
		public DummyBizoWithUNDGs(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public UNDGDataItemCollection UNDGs
		{
			get { return fUNDGs ?? (fUNDGs = new UNDGDataItemCollection(this)); }
		}
		UNDGDataItemCollection fUNDGs;

		bool IUNDGDataItemProvider.NeedFetchHintForLoad => true;
	}
}
