using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ReYaml
{
#if !ISDLL
	// Класс-заглушка для тестирования приложения в режиме
	public class DllExport : Attribute {
		public CallingConvention CallingConvention;
		public DllExport(string v, CallingConvention c) { }
		public DllExport(string v) { }
	};
#endif

	/// <summary>
	/// Класс нижнего слоя взаимодействия с движком Армы.
	/// </summary>
	public static class RVApi
	{
		static Version version = new Version(0, 1);

		[DllExport("RVExtensionVersion", CallingConvention = CallingConvention.Winapi)]
		public static void RvExtensionVersion(StringBuilder output, int outputSize) { output.Append(version.ToString()); }		

		[DllExport("RVExtension", CallingConvention = CallingConvention.Winapi)]
		public static void RvExtension(StringBuilder output, int outputSize, [MarshalAs(UnmanagedType.LPStr)] string function)
		{
#if ISDLL
			callback("1", "2", "3");
#else
			output.Append("Relicta Yaml Parser. Version: " + version.ToString());
#endif
		}

		/// <summary>
		/// Функция вызывается из движка Армы. Выполняется в основном потоке. Сликом долгие действия залочат процесс до конца выполнения этой функции.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="outputSize"></param>
		/// <param name="function"></param>
		/// <param name="args"></param>
		[DllExport("RVExtensionArgs", CallingConvention = CallingConvention.Winapi)]
		public static int RvExtensionArgs(StringBuilder output, int outputSize,
		   [MarshalAs(UnmanagedType.LPStr)] string function,
		   [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 4)] string[] args, int argCount)
		{
			CommandProcessor.ParseCommand(output, outputSize, function, args);
			return 0;
		}

		public static ExtensionCallback callback;
		public delegate int ExtensionCallback([MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string function, [MarshalAs(UnmanagedType.LPStr)] string data);

		[DllExport("RVExtensionRegisterCallback", CallingConvention = CallingConvention.Winapi)]
		public static void RVExtensionRegisterCallback([MarshalAs(UnmanagedType.FunctionPtr)] ExtensionCallback func)
		{
			callback = func;
		}

	}
}
