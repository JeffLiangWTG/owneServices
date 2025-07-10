using System;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	sealed class CannotOverrideAddressSecurityCheckpoint : SecurityCheckpoint
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SecurityCheckpoint code should be English")]
		internal CannotOverrideAddressSecurityCheckpoint()
			: base("Never Allow", ResString.GetMultilingualString("85F78464-54C5-491E-8393-5813FC937B57", "Cannot override address"), null, null, false)
		{
		}

		public override bool IsAllowed
		{
			get { return false; }
		}

		public override bool Visible
		{
			get { return false; }
		}

		public override void AddChild(SecurityCheckpoint child)
		{
			throw new NotSupportedException("AddChild() is not supported by CannotOverrideAddressSecurityCheckpoint.");
		}

		public override void ShowError()
		{
			throw new NotSupportedException("ShowError() is not supported by CannotOverrideAddressSecurityCheckpoint.");
		}

		public override MultilingualString ErrorMessageForNotAllowed
		{
			get { return ResString.GetMultilingualString("a42e598d-4cdb-452e-968e-7d84cd0052b5", "Overriding this address is not allowed"); }
		}
	}
}
