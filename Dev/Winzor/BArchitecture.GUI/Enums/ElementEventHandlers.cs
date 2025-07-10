namespace WinzorFramework.Enums;

[Flags]
public enum ElementEventHandlers
{
	None = 0,
	TextBoxDisableHomeEndKeyWhenSelectAll = 1 << 0,
	TextBoxStartTyping = 1 << 1,
	GridUpdateTextAreaHeightWhenFocusin = 1 << 2,
}
