using System;
using System.Runtime.InteropServices;
using NLog;

namespace Yutaka.Updater
{
	class Program
	{
		#region Fields
		#region Static Externs
		[DllImport("kernel32.dll")]
		static extern IntPtr GetConsoleWindow();
		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
		const int SW_HIDE = 0;
		#endregion Static Externs

		// Constants //
		const string PROGRAM_NAME = "Rcw.SqlAntiHack";
		const string TIMESTAMP = @"[HH:mm:ss] ";

		// PIVs //
		private static DateTime startTime = DateTime.Now;
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private static int errorCount = 0;
		private static int totalCount = 0;
		private static int errorCountThreshold = 7;
		private static double errorPerThreshold = 0.07;
		#endregion Fields

		// Config/Settings //
		private static bool consoleOut = false; // default = false //

		#region Private Helpers
		private static void Process()
		{

		}
		#endregion Private Helpers

		#region Methods
		static void Main(string[] args)
		{
			StartProgram();
			Process();
			EndProgram();
		}

		static void StartProgram()
		{
			var log = String.Format("Starting {0} program", PROGRAM_NAME);
			logger.Info(log);

			if (consoleOut) {
				Console.Clear();
				Console.Write("{0}{1}", DateTime.Now.ToString(TIMESTAMP), log);
			}

			else {
				var handle = GetConsoleWindow();
				ShowWindow(handle, SW_HIDE); // hide window //
			}
		}

		static void EndProgram()
		{
			var endTime = DateTime.Now;
			var ts = endTime - startTime;
			var errorPer = (double) errorCount/totalCount;

			if (errorCount > errorCountThreshold && errorPer > errorPerThreshold)
				logger.Error("The number of errors is above the threshold.");

			var totalTime = "";
			if (ts.TotalHours > .9999)
				totalTime += String.Format("{0}h ", ts.TotalHours);
			if (ts.TotalMinutes > .9999)
				totalTime += String.Format("{0}m ", ts.TotalMinutes);
			totalTime += String.Format("{0}s", ts.TotalSeconds);

			var log = new string[4];
			log[0] = "Ending program";
			log[1] = String.Format("It took {0} to complete", totalTime);
			log[2] = String.Format("Total: {0}", totalCount);
			log[3] = String.Format("Errors: {0} ({1}){2}{2}", errorCount, errorPer.ToString("P"), Environment.NewLine);

			for (int i = 0; i < log.Length; i++) {
				logger.Info(log[i]);
			}

			if (consoleOut) {
				Console.Write("\n");
				for (int i = 0; i < log.Length; i++) {
					Console.Write("\n{0}{1}", DateTime.Now.ToString(TIMESTAMP), log[i]);
				}
				Console.Write("\n.... Press any key to close the program ....");
				Console.ReadKey(true);
			}

			LogManager.Shutdown(); // Flush and close down internal threads and timers
			Environment.Exit(0); // in case you want to call this method outside of a standard successful program completion, this line will close the app //
		}
		#endregion Methods
	}
}