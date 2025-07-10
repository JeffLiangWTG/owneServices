using System;
using System.IO;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.MessageDelivery
{
	public interface IErrorNotifier<T>
	{
		void Notify(BusinessObjectFactory factory, string message, T source, Stream sentMessageData, Stream responseMessageData);
		void Notify(BusinessObjectFactory factory, Exception ex, T source, Stream sentMessageData, Stream responseMessageData);

		void Notify(BusinessObjectFactory factory, Exception ex, string environmentStackTrace, T source, Stream sentMessageData, Stream responseMessageData);
	}
}
