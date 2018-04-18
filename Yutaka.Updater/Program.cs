using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using NLog;
using Yutaka.Diagnostics;
using Yutaka.IO;

namespace Yutaka.Updater
{
	class Program
	{
		// Config/Settings //
		private static bool consoleOut = false; // default = false //

		#region Fields
		#region Static Externs
		[DllImport("kernel32.dll")]
		static extern IntPtr GetConsoleWindow();
		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
		const int SW_HIDE = 0;
		#endregion Static Externs

		// Constants //
		const string PROGRAM_NAME = "Yutaka.Updater";
		const string TIMESTAMP = @"[HH:mm:ss] ";

		// PIVs //
		private static DateTime startTime = DateTime.Now;
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private static int errorCount = 0;
		private static int totalCount = 0;
		private static int errorCountThreshold = 7;
		private static double errorPerThreshold = 0.07;
		private static string ninitePath;
		#endregion Fields

		#region Private Helpers
		private static void Updater()
		{
			var isSkypeRunning = false;
			var isGreenshotRunning = false;
			var skypePath = "";
			var greenshotPath = "";

			var greenshotProc = Process.GetProcessesByName("Greenshot").FirstOrDefault();
			if (greenshotProc != null) {
				isGreenshotRunning = true;
				greenshotPath = greenshotProc.MainModule.FileName;
				logger.Info(greenshotPath);
				ProcessHelper.EndProcessesByName("Greenshot");
			}

			var skypeProc = Process.GetProcessesByName("Skype").FirstOrDefault();
			if (skypeProc != null) {
				isSkypeRunning = true;
				skypePath = skypeProc.MainModule.FileName;
				logger.Info(skypePath);
				ProcessHelper.EndProcessesByName("Skype");
			}

			Process.Start(new ProcessStartInfo("wuapp.exe"));
			Process.Start(new ProcessStartInfo(ninitePath));
			Thread.Sleep(120000);

			if (isGreenshotRunning)
				Process.Start(new ProcessStartInfo(greenshotPath));
			if (isSkypeRunning)
				Process.Start(new ProcessStartInfo(skypePath));
		}
		#endregion Private Helpers

		#region Methods
		static void Main(string[] args)
		{
			if (args == null || args.Length < 1) {
				Console.Write("Path to Ninite must be given as a argument.");
				Console.Write("\n.... Press any key to close the program ....");
				Console.ReadKey(true);
				Environment.Exit(0);
			}

			ninitePath = args[0];
			StartProgram();
			Updater();
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
				totalTime += String.Format("{0}h ", ts.Hours);
			if (ts.TotalMinutes > .9999)
				totalTime += String.Format("{0}m ", ts.Minutes);
			totalTime += String.Format("{0}.{0:D3}s", ts.Seconds, ts.Milliseconds);

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