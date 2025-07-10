using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinzorTestFramework;

public static class FormExtensions
{
	public static async Task CloseHandlerAsync(this Form form)
	{
		await form.CloseHandlerAsync();
	}
}
