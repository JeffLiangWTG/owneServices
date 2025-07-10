using System;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Manifest.Business
{
	//Bring me down to base Asycuda if I'm required by more than just NZ.
	sealed internal class CannotOverrideAddressSecurityCheckpoint : SecurityCheckpoint
	{
		internal CannotOverrideAddressSecurityCheckpoint()
			: base("Never Allow", ResString.GetMultilingualString("3E56BC97-859C-4B15-8686-375FFEB1087E", "Cannot override address"), null, null, false)// SecurityCheckpoint code should be English
		{
		}

		public override bool IsAllowed => false;

		public override bool Visible => false;

		public override void AddChild(SecurityCheckpoint child) => throw new NotSupportedException("AddChild() is not supported by CannotOverrideAddressSecurityCheckpoint.");

		public override void ShowError() => throw new NotSupportedException("ShowError() is not supported by CannotOverrideAddressSecurityCheckpoint.");

		public override MultilingualString ErrorMessageForNotAllowed => ResString.GetMultilingualString("6D2F9714-8B81-47DC-A2AC-38C364158D97", "Overriding this address is not allowed");
	}
}
