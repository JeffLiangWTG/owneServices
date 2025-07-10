using System;
using System.Collections.Immutable;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.US.Messaging.Business
{
	[Immutable]
	public partial class ABIError
	{
		ABIError()
		{
			aBIErrorCollection = ImmutableDictionary.CreateRange(GetValueFromCollection());
		}

		readonly static Lazy<ABIError> instance = new Lazy<ABIError>(() => new ABIError(), true);
		readonly ImmutableDictionary<string, MessageError> aBIErrorCollection;

		public static ABIError Instance
		{
			get { return instance.Value; }
		}

		internal int ABIErrorCollectionCount
		{
			get { return aBIErrorCollection.Count; }
		}

		public MessageError GetErrorInfoByCode(ZString errorCode)
		{
			MessageError messageError;
			aBIErrorCollection.TryGetValue(errorCode, out messageError);
			return messageError;
		}
	}
}
