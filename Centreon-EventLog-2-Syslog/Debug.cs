//##################################################################################
//# Copyright 2005-2010 MERETHIS
//# Centreon is developped by : Julien Mathis and Romain Le Merlus under
//# GPL Licence 2.0.
//# 
//# This program is free software; you can redistribute it and/or modify it under 
//# the terms of the GNU General Public License as published by the Free Software 
//# Foundation ; either version 2 of the License.
//# 
//# This program is distributed in the hope that it will be useful, but WITHOUT ANY
//# WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A 
//# PARTICULAR PURPOSE. See the GNU General Public License for more details.
//# 
//# You should have received a copy of the GNU General Public License along with 
//# this program; if not, see <http://www.gnu.org/licenses>.
//# 
//# Linking this program statically or dynamically with other modules is making a 
//# combined work based on this program. Thus, the terms and conditions of the GNU 
//# General Public License cover the whole combination.
//# 
//# As a special exception, the copyright holders of this program give MERETHIS 
//# permission to link this program with independent modules to produce an executable, 
//# regardless of the license terms of these independent modules, and to copy and 
//# distribute the resulting executable under terms of MERETHIS choice, provided that 
//# MERETHIS also meet, for each linked independent module, the terms  and conditions 
//# of the license of that module. An independent module is a module which is not 
//# derived from this program. If you modify this program, you may extend this 
//# exception to your version of the program, but you are not obliged to do so. If you
//# do not wish to do so, delete this exception statement from your version.
//# 
//# For more information : contact@centreon.com
//# 
//# SVN : $URL
//# SVN : $Id :
//#
//####################################################################################
//#
//# Dependent plugin : .NET Framework
//# Dependent plugin version : 2.0.x
//#
//####################################################################################
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
                catch (System.Exception e)
                {
                    Console.WriteLine(e.Message);
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
                //Regex re = new Regex(@"\w+:\\\w+\\Debug.log.\d", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
                Regex re = new Regex(@"Debug.log.\d$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

                if (re.IsMatch(file))
                {
                    nbDebufFile++;
                }
            }

            // Delete old debug file
            if (nbDebufFile >= this._DebugInfo.FileNumber)
            {
                foreach (String file in files)
                {
                    String pattern = "Debug.log.";
                    String fileNumber = file.Substring(file.LastIndexOf(pattern) + pattern.Length, 1);
                    int ifileNumber = 0;

                    try
                    {
                        ifileNumber = Convert.ToInt32(fileNumber);
                        if (ifileNumber >= this._DebugInfo.FileNumber)
                        {
                            File.Delete(file);
                        }
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
            // Rename debug files
            // TODO : faire la rotation :D

            // Debug.log.5 => delete
            // Debug.log.4 => Debug.log.5
            // Debug.log.3 => Debug.log.4
            // Debug.log.2 => Debug.log.1
            // Debug.log => Debug.log.1

            for (int i = nbDebufFile; i > 1; i--)
            {
                try
                {
                    File.Move(this._Path + "\\Debug.log." + (i - 1), this._Path + "\\Debug.log." + i);
                }
                catch (Exception e)
                {
                }
            }

            try
            {
                File.Move(this._Path + "\\Debug.log", this._Path + "\\Debug.log.1");
            }
            catch (Exception e)
            {
            }
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
