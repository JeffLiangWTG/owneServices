using System.Globalization;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NO.Business;

public sealed class ReferenceNumberWrapper
{
	public const string Version01AsString = "01";

	ZString orgCode;
	public ZString OrgCode
	{
		get => orgCode;
		set
		{
			orgCode = value;
			IsValid &= orgCode.Length == 9;
			IsValid &= orgCode.IsNumbersOnlyOrEmpty;
		}
	}

	ZDateTime datetime;
	public ZDateTime DateTime
	{
		get => datetime;
		set
		{
			datetime = value;
			IsValid &= datetime.IsValid;
		}
	}
	public ZString Date
	{
		get => DateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
		set
		{
			if (ZDateTime.TryParseExact(value, out var dateTime, "yyyyMMdd"))
			{
				DateTime = dateTime;
			}
			else
			{
				IsValid = false;
			}
		}
	}

	ZString sequence;
	public ZString Sequence
	{
		get => sequence;
		set
		{
			sequence = value;
			IsValid &= sequence.Length == 6;
			IsValid &= sequence.IsNumbersOnlyOrEmpty;
		}
	}

	public void TryIncrementVersion()
	{
		Version++;
	}

	public ZByte Version
	{
		get => version;
		private set
		{
			version = value;
			IsValid &= version > 0;
			IsValid &= version < 100;
		}
	}
	ZByte version;

	public ZString VersionAsString
	{
		get => Version.ToString("00");
	}

	public ZString FallbackValue { get; set; }

	public ReferenceNumberWrapper(ZString value)
	{
		SetSplitValue(value);
	}

	public ZBool IsValid { get; private set; }

	public ZString ReferenceNumber => IsValid ? (ZString)$"{OrgCode}{Date}{Sequence}{VersionAsString}" : FallbackValue;

	public override string ToString() => ReferenceNumber;

	public void GenerateFrom(BusinessObjectFactory factory, OrgHeader declarant, ZDateTime declarantDateTime, ZString fallbackValue)
	{
		Clear();
		FallbackValue = fallbackValue;
		if (factory is not null)
		{
			using var transactionManager = Db.Connection.BeginTransactionWithManager();
			IsValid = true;
			OrgCode = declarant?.CustomsCodes?.GetCustomsRegNo(OrgCusCode.CodeTypes.OrganizationNumber) ?? ZString.Empty;
			DateTime = declarantDateTime;
			Sequence = Env.NumberFountains.NODecReferenceNumber($"{OrgCode}-{Date}").GetNextFormatted(factory);
			version = 1;
			transactionManager.CommitTransaction();
		}
	}

	public void SetSplitValue(ZString value)
	{
		if (value.IsEmpty)
		{
			Clear();
		}
		else if (value.Length == 25)
		{
			IsValid = true;
			FallbackValue = value;
			OrgCode = value.SubstringSafe(0, 9);
			Date = value.SubstringSafe(9, 8);
			Sequence = value.SubstringSafe(17, 6);
			Version = ZByte.ParseSafe(value.SubstringSafe(23, 2), 100);
		}
		else
		{
			IsValid = false;
			FallbackValue = value;
		}
	}

	public void Clear()
	{
		IsValid = false;
		FallbackValue = ZString.Empty;

		OrgCode = ZString.Empty;
		Date = ZString.Empty;
		Sequence = ZString.Empty;
		version = ZByte.Zero;
	}

	internal const int ReferenceNumberWithoutVersionLength = 23;
}
