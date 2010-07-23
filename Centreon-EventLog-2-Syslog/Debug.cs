using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Centreon_EventLog_2_Syslog
{
    /// <summary>
    /// Write debug informations into log file
    /// </summary>
    class Debug
    {
        private String _FileName = "Debug.log";
        private String _Path = "";
        private DebugInformations _DebugInfo;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">Path and file name of log</param>
        /// <param name="debInf">Debug informations includng level, verbose, rotation information</param>
        public Debug(String fileName, ref DebugInformations debInf)
        {
            if (fileName.CompareTo("") == 0)
            {
                fileName = this._FileName;
            }

            Regex re = new Regex(@"\w+:\\\w+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

            if (re.IsMatch(fileName))
            {

                this._Path = fileName.Substring(0, fileName.LastIndexOf('\\'));
                if (System.IO.Directory.Exists(this._Path))
            	{
                    this._FileName = fileName;
            	}
            	else
            	{
                    String exepath = Environment.GetCommandLineArgs()[0];
                    this._Path = exepath.Substring(0, exepath.LastIndexOf('\\'));
                    this._FileName = this._Path + "\\" + fileName;
            	}
            }
            else
            {
                String exepath = Environment.GetCommandLineArgs()[0];
                this._Path = exepath.Substring(0, exepath.LastIndexOf('\\'));
                this._FileName = this._Path + "\\" + fileName;
            }

            this._DebugInfo = debInf;
            this._DebugInfo.Level = 1;
        }

        /// <summary>
        /// Write error into log file
        /// </summary>
        /// <param name="processName">Name of the process which wants to write error</param>
        /// <param name="line">Error to write</param>
        /// <param name="dtError">DateTime when of error</param>
        /// <returns></returns>
        public Boolean Write(String processName, String line, DateTime dtError)
        {
            if (this._DebugInfo.Level != 0)
            {
                StreamWriter stw;

                if (dtError == null)
                {
                    dtError = DateTime.Now;
                }

                try
                {
                    long debugFileSize = 0;
                    if (File.Exists(this._FileName))
                    {
                        FileInfo f = new FileInfo(this._FileName);
                        debugFileSize = f.Length;
                    }

                    lock (this)
                    {
                        if (debugFileSize > (this._DebugInfo.MaxSize * 1024 * 1024))
                        {
                            rotateFile();
                        }
                        stw = new StreamWriter(File.Open(this._FileName, FileMode.Append));
                        stw.WriteLine(String.Format("[{0}.{1}] {2} :: {3}", dtError, dtError.Millisecond, processName, line));
                        stw.Close();
                    }
                }
                catch (System.ObjectDisposedException ode)
                {
                    Console.WriteLine(ode.Message);
                }
                catch (System.IO.IOException ioe)
                {
                    Console.WriteLine(ioe.Message);
                }
            }

            return true;
        }

        /// <summary>
        /// Make rotation of debug file
        /// </summary>
        private void rotateFile()
        {
            String[] files = Directory.GetFiles(this._Path);

            // Count available debug files
            int nbDebufFile = 0;
            foreach (String file in files)
            {
                Regex re = new Regex(@"\w+:\\\w+\\Debug.log.*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

                if (re.IsMatch(file))
                {
                    nbDebufFile++;
                }
            }

            // Delete old debug file
            if (nbDebufFile++ > this._DebugInfo.FileNumber)
            {
                for (int i = nbDebufFile; i > 0; i--)
                {
                    foreach (String file in files)
                    {
                        Regex re = new Regex(@"\w+:\\\w+\\Debug.log." + i, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
                        String destFile = file.Substring(0, file.LastIndexOf('\\')) + "Debug.log." + (i - 1);

                        if (re.IsMatch(file))
                        {
                            if (i != nbDebufFile)
                            {
                                File.Copy(file, destFile, true);
                            }
                            File.Delete(file);
                        }
                    }
                }
            }

            // Rename debug files
        }

        /// <summary>
        /// Get or Set Name of debug file
        /// </summary>
        public String FileName
        {
            get
            {
                return this._FileName;
            }
            set
            {
                this._FileName = value;
            }
        }
    }
}
