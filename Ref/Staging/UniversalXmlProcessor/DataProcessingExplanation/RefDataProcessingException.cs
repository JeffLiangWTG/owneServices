using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CargoWise.RefDbRepo.DataProcessingExplanation;

[Serializable]
public class RefDataProcessingException : Exception
{
	RefDataProcessingException() { }

	RefDataProcessingException(string message) : base(message) { }

	RefDataProcessingException(string message, Exception innerException) : base(message, innerException) { }

	/// <summary>
	/// Don't use this constructor, except for purely message without any type, property name or table prefix
	/// </summary>
	/// <param name="message"></param>
	/// <param name="errorCode"></param>
	public RefDataProcessingException(string message, string errorCode)
		: base(GetMessageWithReference(message, errorCode))
	{ }

	RefDataProcessingException(string message, string errorCode, Exception innerException)
		: base(GetMessageWithReference(message, errorCode), innerException)
	{ }

	public RefDataProcessingException(string message, string errorCode, IEnumerable<Type> entityTypes)
		: this(Transform(message, [msg => TransformCore(msg, entityTypes, NonPersistentTransformer.TransformToPersistentType)]), errorCode)
	{ }

	public RefDataProcessingException(string message, string errorCode, IEnumerable<Type> entityTypes,
		IEnumerable<string> propertyNames)
		: this(Transform(message,
			[
				msg => TransformCore(msg, entityTypes, NonPersistentTransformer.TransformToPersistentType),
				msg => TransformCore(msg, propertyNames, NonPersistentTransformer.TransformToPersistentPropertyName)
				]),
			errorCode)
	{ }

	public RefDataProcessingException(string message, string errorCode, IEnumerable<string> propertyNames)
		: this(
			Transform(message, [msg => TransformCore(msg, propertyNames, NonPersistentTransformer.TransformToPersistentPropertyName)]),
			errorCode)
	{ }

	public RefDataProcessingException(string message, string errorCode, string tablePrefix,
		params string[] tablePrefixs)
		: this(
			Transform(message,
			[msg => TransformCore(msg, [tablePrefix, .. tablePrefixs], NonPersistentTransformer.TransformToPersistentTablePrefix)]),
			errorCode)
	{ }

	static string GetMessageWithReference(string message, string errorCode)
	{
		var strBuilder = new StringBuilder();
		strBuilder.AppendLine(message);
		if (!string.IsNullOrEmpty(errorCode))
		{
			strBuilder.Append(CultureInfo.InvariantCulture, $"ErrorCode: {errorCode}");
			if (!string.IsNullOrEmpty(ErrorConstants.ErrorReferenceUrl))
			{
				strBuilder.Append(CultureInfo.InvariantCulture,
					$", please check more details on {GetWikiLink(ErrorConstants.ErrorReferenceUrl, errorCode)}");
			}
		}

		return strBuilder.ToString();
	}

	static string GetWikiLink(string baseUri, string errorCode)
	{
		return
			$"{baseUri}?anchor={errorCode.ToLower(CultureInfo.CurrentCulture)}%3A-{ErrorConstants.GetErrorNameByCode(errorCode).ToLower(CultureInfo.CurrentCulture)}";
	}

	static string Transform(string message, List<Func<string, string>> funcs)
	{
		funcs.ForEach(x => message = x(message));
		return message;
	}

	static string TransformCore(string message, IEnumerable<string> underTransformObjects, Func<string, IEnumerable<string>, bool, string> func)
	{
		// we don't need to get full pair Rate/Cond and App (always get App) in current scenarios. we keep it here if it is needed in the future
		return func(message, underTransformObjects, false);
	}
	static string TransformCore(string message, IEnumerable<Type> underTransformObjects, Func<string, IEnumerable<Type>, bool, string> func)
	{
		// we don't need to get full pair Rate/Cond and App (always get App) in current scenarios. we keep it here if it is needed in the future
		return func(message, underTransformObjects, false);
	}
}
