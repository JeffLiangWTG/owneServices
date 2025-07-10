using System.Diagnostics;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NL.Business;

[DebuggerDisplay("FallbackConfiguration. Start={Start}; End={End} InvocationReason={InvocationReason}; RevocationReason={RevocationReason}; Regularisation={Regularisation};" +
		" RegularisationCount={RegularisationCount}; RegularisationBatchSize={RegularisationBatchSize}; RegularisationPeriod={RegularisationPeriod}")]
[XmlSerializerAssembly("Enterprise.Customs.NL.Business.XmlSerializers")]
public class FallbackConfiguration : RegistryBusinessObjectTemplate
{
	#region Constants
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Time format value")]
	public const string DateFormat = "yyyy-MM-dd HH:mm:ss";
	#endregion

	#region Schema
	protected abstract class Schema
	{
		public const string Start = "Start";
		public const string End = "End";
		public const string InvocationReason = "InvocationReason";
		public const string RevocationReason = "RevocationReason";
		public const string Regularisation = "Regularisation";
		public const string RegularisationCount = "RegularisationCount";
		public const string RegularisationBatchSize = "RegularisationBatchSize";
		public const string RegularisationPeriod = "RegularisationPeriod";
	}
	#endregion

	#region Constructions and cloning
	protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
	{
		return new FallbackConfiguration(fallbackLevel, factory);
	}

	public FallbackConfiguration()
		: base()
	{
	}

	public FallbackConfiguration(BusinessObjectFactory factory)
	: base(factory)
	{
	}

	public FallbackConfiguration(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		: base(fallbackLevel, factory)
	{ }

	protected override void SetPKAndDefaults()
	{
		base.SetPKAndDefaults();
		fRegularisationPeriod = 60;
	}
	#endregion

	#region Start
	ZDateTime fStart;
	public ZDateTime Start
	{
		get { return fStart; }
		set
		{
			SetNonPersistentPropertyValue(StartInfo, ref fStart, value);
			if (!IsValidationSuspended)
			{
				ValidateStart();
				ValidateInvocationReason();
			}
		}
	}

	public ZPropertyInfo StartInfo
	{
		get { return GetZPropertyInfo(Schema.Start); }
	}

	public void ValidateStart()
	{
		StartInfo.ClearAllNotifications();

		if (Start.IsEmpty)
		{
			StartInfo.AddMessageError(StartDateNotEmpty);
		}
	}
	#endregion

	#region End
	ZDateTime fEnd;
	public ZDateTime End
	{
		get { return fEnd; }
		set
		{
			SetNonPersistentPropertyValue(EndInfo, ref fEnd, value);
			if (!IsValidationSuspended)
			{
				Regularisation = new ZDateTime(fEnd.Year, fEnd.Month, fEnd.Day, 22, 0, 0);
				ValidateEnd();
			}
		}
	}

	public ZPropertyInfo EndInfo
	{
		get { return GetZPropertyInfo(Schema.End); }
	}

	public void ValidateEnd()
	{
		EndInfo.ClearAllNotifications();

		if (!End.IsEmpty)
		{
			if (Start.IsEmpty)
			{
				EndInfo.AddMessageError(StartDateNotEmpty);
			}
			else if (End <= Start)
			{
				EndInfo.AddMessageError(EndAfterStart);
			}
		}
		else if (!Regularisation.IsEmpty && !RegularisationPeriod.IsEmpty && !RevocationReason.IsEmpty)
		{
			EndInfo.AddMessageError(EndDateNotEmpty);
		}
	}
	#endregion

	#region Regularisation
	ZDateTime fRegularisation;
	public ZDateTime Regularisation
	{
		get { return fRegularisation; }
		set
		{
			SetNonPersistentPropertyValue(RegularisationInfo, ref fRegularisation, value);
			if (!IsValidationSuspended)
			{
				ValidateRegularisation();
			}
		}
	}

	public ZPropertyInfo RegularisationInfo
	{
		get { return GetZPropertyInfo(Schema.Regularisation); }
	}

	public void ValidateRegularisation()
	{
		RegularisationInfo.ClearAllNotifications();

		if (!End.IsEmpty)
		{
			if (Regularisation.IsEmpty)
			{
				RegularisationInfo.AddMessageError(RegularisationNotEmpty);
			}
			else if (Regularisation < End)
			{
				RegularisationInfo.AddMessageError(RegularisationAfterEnd);
			}
			else if (Start.IsEmpty)
			{
				RegularisationInfo.AddMessageError(StartDateNotEmpty);
			}
		}
	}
	#endregion

	#region RegularisationPeriod
	ZInt fRegularisationPeriod;
	public ZInt RegularisationPeriod
	{
		get { return fRegularisationPeriod; }
		set
		{
			SetNonPersistentPropertyValue(RegularisationPeriodInfo, ref fRegularisationPeriod, value);
			if (!IsValidationSuspended)
			{
				ValidateRegularisationPeriod();
			}
		}
	}

	public ZPropertyInfo RegularisationPeriodInfo
	{
		get { return GetZPropertyInfo(Schema.RegularisationPeriod); }
	}

	public void ValidateRegularisationPeriod()
	{
		RegularisationPeriodInfo.ClearAllNotifications();

		if (!End.IsEmpty)
		{
			if (RegularisationPeriod.IsEmpty || RegularisationPeriod == ZInt.Zero)
			{
				RegularisationPeriodInfo.AddMessageError(RegularisationPeriodNotEmpty);
			}
			else if (Start.IsEmpty)
			{
				RegularisationPeriodInfo.AddMessageError(StartDateNotEmpty);
			}
		}
	}
	#endregion

	#region InvocationReason

	ZString fInvocationReason;
	[MaxLength(70)]
	public ZString InvocationReason
	{
		get { return fInvocationReason; }
		set
		{
			SetNonPersistentPropertyValue(InvocationReasonInfo, ref fInvocationReason, value);
			if (!IsValidationSuspended)
			{
				ValidateInvocationReason();
				ValidateStart();
			}
		}
	}

	public ZPropertyInfo InvocationReasonInfo
	{
		get { return GetZPropertyInfo(Schema.InvocationReason); }
	}

	public void ValidateInvocationReason()
	{
		InvocationReasonInfo.ClearAllNotifications();

		if (InvocationReason.IsEmpty)
		{
			InvocationReasonInfo.AddMessageError(InvocationReasonNotEmpty);
		}
	}
	#endregion

	#region RevocationReason

	ZString fRevocationReason;
	[MaxLength(70)]
	public ZString RevocationReason
	{
		get { return fRevocationReason; }
		set
		{
			SetNonPersistentPropertyValue(RevocationReasonInfo, ref fRevocationReason, value);
			if (!IsValidationSuspended)
			{
				ValidateRevocationReason();
			}
		}
	}

	public ZPropertyInfo RevocationReasonInfo
	{
		get { return GetZPropertyInfo(Schema.RevocationReason); }
	}

	public void ValidateRevocationReason()
	{
		RevocationReasonInfo.ClearAllNotifications();

		if (!End.IsEmpty && RevocationReason.IsEmpty)
		{
			RevocationReasonInfo.AddMessageError(RevocationReasonNotEmpty);
		}
	}
	#endregion

	#region RegularisationCount
	ZDecimal fRegularisationCount;

	public ZDecimal RegularisationCount
	{
		get { return fRegularisationCount; }
		set
		{
			SetNonPersistentPropertyValue(RegularisationCountInfo, ref fRegularisationCount, value);
			if (!IsValidationSuspended)
			{
				ValidateRegularisationPeriod();
			}
		}
	}

	public ZPropertyInfo RegularisationCountInfo
	{
		get { return GetZPropertyInfo(Schema.RegularisationCount); }
	}

	public void ValidateRegularisationCount()
	{
		RegularisationCountInfo.ClearAllNotifications();
	}
	#endregion

	#region RegularisationBatchSize
	ZDecimal fRegularisationBatchSize;

	public ZDecimal RegularisationBatchSize
	{
		get { return fRegularisationBatchSize; }
		set
		{
			SetNonPersistentPropertyValue(RegularisationBatchSizeInfo, ref fRegularisationBatchSize, value);
			if (!IsValidationSuspended)
			{
				ValidateRegularisationBatchSize();
			}
		}
	}

	public ZPropertyInfo RegularisationBatchSizeInfo
	{
		get { return GetZPropertyInfo(Schema.RegularisationBatchSize); }
	}

	public void ValidateRegularisationBatchSize()
	{
		RegularisationBatchSizeInfo.ClearAllNotifications();
	}
	#endregion

	public void ValidateAll()
	{
		ValidateStart();
		ValidateEnd();
		ValidateRegularisation();
		ValidateRegularisationPeriod();
		ValidateInvocationReason();
		ValidateRevocationReason();
		ValidateRegularisationCount();
		ValidateRegularisationBatchSize();
	}

	protected override void RunPreSaveValidationCore()
	{
		base.RunPreSaveValidationCore();
		ValidateAll();
	}

	#region Xml Serialisation
	protected sealed override void WriteElements(XmlWriter writer)
	{
		base.WriteElements(writer);
		writer.WriteElementString(Schema.Start, Start.ToString(DateFormat, CultureInfo.InvariantCulture));
		writer.WriteElementString(Schema.End, End.ToString(DateFormat, CultureInfo.InvariantCulture));
		writer.WriteElementString(Schema.Regularisation, Regularisation.ToString(DateFormat, CultureInfo.InvariantCulture));
		writer.WriteElementString(Schema.RegularisationPeriod, RegularisationPeriod.ToString());
		writer.WriteElementString(Schema.InvocationReason, InvocationReason);
		writer.WriteElementString(Schema.RevocationReason, RevocationReason);
		writer.WriteElementString(Schema.RegularisationCount, RegularisationCount.ToString());
		writer.WriteElementString(Schema.RegularisationBatchSize, RegularisationBatchSize.ToString());
	}

	protected sealed override void ReadElements(XmlReaderWrapper reader)
	{
		Start = reader.ReadElementStringAsZDateTime(Schema.Start, DateFormat);
		End = reader.ReadElementStringAsZDateTime(Schema.End, DateFormat);
		Regularisation = reader.ReadElementStringAsZDateTime(Schema.Regularisation, DateFormat);
		RegularisationPeriod = reader.ReadElementStringAsZInt(Schema.RegularisationPeriod);
		InvocationReason = reader.ReadElementString(Schema.InvocationReason);
		RevocationReason = reader.ReadElementString(Schema.RevocationReason);
		RegularisationCount = reader.ReadElementStringAsZDecimal(Schema.RegularisationCount);
		RegularisationBatchSize = reader.ReadElementStringAsZDecimal(Schema.RegularisationBatchSize);
	}

	#endregion

	#region Messages error validation
	public static MultilingualString StartDateNotEmpty => ResString.GetMultilingualString("FallbackConfiguration-StartDateNotEmpty", "Start date cannot be empty.");

	public static MultilingualString EndDateNotEmpty => ResString.GetMultilingualString("FallbackConfiguration-EndDateNotEmpty", "End date cannot be empty.");

	public static MultilingualString EndAfterStart => ResString.GetMultilingualString("FallbackConfiguration-EndAfterStart", "End date should be after start date.");

	public static MultilingualString InvocationReasonNotEmpty => ResString.GetMultilingualString("FallbackConfiguration-InvocationReasonNotEmpty", "Invocation reason cannot be empty.");

	public static MultilingualString RevocationReasonNotEmpty => ResString.GetMultilingualString("FallbackConfiguration-RevocationReasonNotEmpty", "End fallback is given, revocation reason cannot be empty.");

	public static MultilingualString RegularisationNotEmpty => ResString.GetMultilingualString("FallbackConfiguration-RegularisationNotEmpty", "End fallback is given, regularization date cannot be empty.");

	public static MultilingualString RegularisationPeriodNotEmpty => ResString.GetMultilingualString("FallbackConfiguration-RegularisationPeriodNotEmpty", "End fallback is given, regularization period cannot be empty.");

	public static MultilingualString RegularisationAfterEnd => ResString.GetMultilingualString("FallbackConfiguration-RegularisationAfterEnd", "Fallback regularization date should be after Fallback End date.");

	#endregion
}
