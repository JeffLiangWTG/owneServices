using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	[Serializable]
	class FormatConversionException : Exception
	{
#if NETFRAMEWORK
		protected FormatConversionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif

		internal FormatConversionException(Exception ex, string fieldValueCouldNotConvert)
			: base((NoResString)"Could not convert a value", ex) // Exception message not shown to user.
		{
			FieldValueCouldNotConvert = fieldValueCouldNotConvert;
		}

		internal string FieldValueCouldNotConvert { get; }
	}
}
