using System;
using System.Collections.Immutable;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.US.Messaging.Business
{
	[Immutable]
	public partial class ACEError
	{
		ACEError()
		{
			aCEErrorCollection = ImmutableDictionary.CreateRange(GetValueForCollection());
		}

		readonly static Lazy<ACEError> instance = new Lazy<ACEError>(() => new ACEError(), true);
		readonly ImmutableDictionary<string, MessageError> aCEErrorCollection;

		public static ACEError Instance
		{
			get { return instance.Value; }
		}

		public MessageError GetErrorInfoByCode(ZString errorCode)
		{
			MessageError messageError;
			aCEErrorCollection.TryGetValue(errorCode, out messageError);
			return messageError;
		}

		internal int ACEErrorCollectionCount
		{
			get { return aCEErrorCollection.Count; }
		}
	}
}
