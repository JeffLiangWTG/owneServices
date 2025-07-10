using System;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eTail.Business;

[Serializable]
public class CustomsStatusCodeNotExistInCodeListException : Exception
{
	public CustomsStatusCodeNotExistInCodeListException(string code, string codeType, string codeList, string countryCode) : base(
		(NoResString)"The Customs Status Code does not exist in the code list.")
	{
		Code = code;
		CodeType = codeType;
		CodeList = codeList;
		CountryCode = countryCode;
	}

#if NETFRAMEWORK
	protected CustomsStatusCodeNotExistInCodeListException(SerializationInfo info, StreamingContext context) : base(info, context)
	{
	}
#endif

	public string Code { get; }
	public string CodeType { get; }
	public string CodeList { get; }
	public string CountryCode { get; }
}
