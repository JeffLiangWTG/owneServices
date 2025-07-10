using System.Runtime.InteropServices;
using System.Security;

namespace Enterprise.MasterFiles.GUI;

public static class SecureStringExtensions
{
	public static string ToInsecureString(this SecureString str)
	{
		if (str == null)
		{
			return string.Empty;
		}

		var bstr = Marshal.SecureStringToBSTR(str);
		try
		{
			return Marshal.PtrToStringBSTR(bstr);
		}
		finally
		{
			Marshal.ZeroFreeBSTR(bstr);
		}
	}
}
