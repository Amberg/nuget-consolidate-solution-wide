﻿using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NugetConsolidate.Service
{
	public class ProcessRunner
	{
		public RunStatus Run(string process, string workingDirectory, string[] arguments)
		{
			var psi = new ProcessStartInfo(process, string.Join(" ", arguments))
			{
				WorkingDirectory = workingDirectory,
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};

			var p = new Process();
			try
			{
				p.StartInfo = psi;
				p.Start();

				var output = new StringBuilder();
				var errors = new StringBuilder();
				var outputTask = ConsumeStreamReaderAsync(p.StandardOutput, output);
				var errorTask = ConsumeStreamReaderAsync(p.StandardError, errors);
				var processExited = p.WaitForExit(20000);

				if (processExited == false)
				{
					p.Kill();
					return new RunStatus(output.ToString(), errors.ToString(), exitCode: -1);
				}

				Task.WaitAll(outputTask, errorTask);
				return new RunStatus(output.ToString(), errors.ToString(), p.ExitCode);
			}
			catch (Win32Exception e)
			{
				return new RunStatus(e.Message, e.ToString(), e.NativeErrorCode);
			}
			finally
			{
				p.Dispose();
			}
		}

		private static async Task ConsumeStreamReaderAsync(StreamReader reader, StringBuilder lines)
		{
			await Task.Yield();
			string line;
			while ((line = await reader.ReadLineAsync()) != null)
			{
				lines.AppendLine(line);
			}
		}
	}
}