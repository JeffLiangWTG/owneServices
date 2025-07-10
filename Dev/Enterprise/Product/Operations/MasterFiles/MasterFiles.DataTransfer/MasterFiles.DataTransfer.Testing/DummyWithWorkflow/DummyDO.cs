using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Testing
{
	[RootElement("UniversalDummyDO")]
	public class DummyDO : ITopLevelDataObject
	{
		IDataContextDataObject ITopLevelDataObject.DataContext
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[MaxLength(50)]
		public ZString GotMeAValue
		{
			get { return "HERE'S LUCY!!!!"; }
		}

		public IEnumerable<IMessageNumber> MessageNumberCollection => throw new NotImplementedException();

		#region IDisposable Support
		bool disposedValue; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				disposedValue = true;
			}
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			Dispose(true);
		}

		public void SetWriterStrategy(IDataObjectWriterStrategy strategy)
		{
		}

		public void SetMessageNumber(MessageNumberType type, ZString value)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
