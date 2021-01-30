using System.Diagnostics;
using System.Text;

namespace AutoLockHelper
{
    internal static class RedirectStandardOutput
    {
        internal static void RunCommand(string filename, string args)
        {
            using (Process process = new Process())
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    StandardOutputEncoding = Encoding.GetEncoding(437),
                    FileName = filename,
                    Arguments = args
                };

                process.StartInfo = startInfo;
                process.EnableRaisingEvents = true;

                process.ErrorDataReceived += Proc_DataReceived;
                process.OutputDataReceived += Proc_DataReceived;

                process.Start();

                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                process.WaitForExit();
            }
        }

        private static void Proc_DataReceived(object sender, DataReceivedEventArgs e)
        {
            EasyLogger.Info(e.Data);
        }
    }
}
