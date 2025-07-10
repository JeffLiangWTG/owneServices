using System;
using CargoWise.Application;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin
{
	public class DocDataObjectPostProcessor<T> : IForwardingDocDataObjectProvider
		where T : class
	{
		readonly IForwardingDocDataObjectProvider parentProvider;
		readonly Func<T, T> processor;

		public DocDataObjectPostProcessor(IForwardingDocDataObjectProvider parentProvider, Func<T, T> processor)
		{
			this.parentProvider = parentProvider;
			this.processor = processor;
		}

		public object GetDocDataObject(object parent, string dataCotext, IDocDataObjectParameters parameters)
		{
			var obj = parentProvider.GetDocDataObject(parent, dataCotext, parameters);
			var castObj = obj != null
				? obj as T ?? throw new InvalidCastException($"{obj.GetType().FullName} cannot be cast to a {typeof(T).FullName}")
				: null;
			return processor.Invoke(castObj);
		}

		public static IDisposable InjectInToObjectFactory(Func<T, T> processor)
			=> ObjectFactory.Substitute<IForwardingDocDataObjectProvider>(new DocDataObjectPostProcessor<T>(ObjectFactory.Get<IForwardingDocDataObjectProvider>(), processor));
	}
}
