using System.Collections.Concurrent;

namespace System.Windows.Input;

public static class Keyboard
{
	public static bool IsKeyDown(Key key) => KeyDownStatus.TryGetValue(key, out var status) && status;

	public static bool IsKeyToggled(Key key)
	{
		switch (key)
		{
			case Key.CapsLock:
				return IsCapsLockOn;
			default:
				return false;
		}
	}

	public static bool IsCapsLockOn { get; set; }

	public static ConcurrentDictionary<Key, bool> KeyDownStatus = new();
}
