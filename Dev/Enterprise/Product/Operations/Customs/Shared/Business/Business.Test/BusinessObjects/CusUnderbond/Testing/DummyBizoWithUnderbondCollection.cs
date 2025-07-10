using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;

namespace Enterprise.Customs.Business.Testing
{
	public class DummyBizoWithUnderbondCollection : DummyBusinessObject, ICusUnderbondDependentCollectionParent
	{
		public DummyBizoWithUnderbondCollection(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		CusUnderbondCollection underbonds;
		public CusUnderbondCollection Underbonds
		{
			get
			{
				if (underbonds == null)
				{
					underbonds = new CusUnderbondCollectionWithProvider(this);
					underbonds.Load();
				}
				return underbonds;
			}
		}

		ZString IOutturnableLine.UnderbondHumanReadableName
		{
			get { return "Dummy Underbond Biz Obj"; }
		}

		ZString ICusUnderbondDependentCollectionParent.Details
		{
			get { return "Dummy Underbond Biz Obj Details"; }
		}

		bool ICusUnderbondDependentCollectionParent.CanSendWithoutDelay
		{
			get { return true; }
		}

		public IOutturnableLine[] OutturnableLines
		{
			get
			{
				return fOutturnableLines;
			}
			set
			{
				fOutturnableLines = value;
			}
		}
		IOutturnableLine[] fOutturnableLines = Array.Empty<IOutturnableLine>();

		#region IOutturnableLine Members

		ZString IOutturnableLine.CargoStatus
		{
			get { return "WTO"; }
		}

		ZInt IOutturnableLine.PackagesManifested
		{
			get { return 0; }
		}

		#endregion

		public bool UsesTranshipmentPortOnUnderbond
		{
			get { return usesTranshipmentPortOnUnderbond; }
			set { usesTranshipmentPortOnUnderbond = value; }
		}
		bool usesTranshipmentPortOnUnderbond;

		public ZString DefaultTranshipmentPort
		{
			get { return defaultTranshipmentPort; }
			set { defaultTranshipmentPort = value; }
		}
		ZString defaultTranshipmentPort;
	}
}
