//##################################################################################
//# Copyright 2005-2009 MERETHIS
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
//# Last modification by : Laurent Pinsivy
//# Last modification date : 2009.02.24
//#
//# Dependent plugin : .NET Framework
//# Dependent plugin version : 2.0.x
//#
//####################################################################################
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace centreon_eventLog_syslog
{
    class Program
    {
        private static IniFile configurationFile = null;
        private static SyslogServer syslog = null;
        private static ArrayList rulesList = null;
        private static ArrayList eventLogNames = null;
        private static ArrayList threadList = new ArrayList();
        public static Debug debug = null;
        public static DateTime lastExecTime;
        public static DateTime maxExecTime;
        public static DateTime nextCheck;
        private static ManualResetEvent[] doneEvents = null;
        private Boolean _isActive = true;
        
        /// <summary>
        /// Simple constructor
        /// </summary>
        public Program()
        {
        }

        /// <summary>
        /// Start principal process
        /// </summary>
        public void Start()
        {
            if (ProcessAlreadyExist() == true)
                System.Environment.Exit(-1);

            String fileName = "config.ini";
            
            lastExecTime = DateTime.Now.AddMinutes(-120);
            maxExecTime = DateTime.Now;

            do
            {
                debug = new Debug();
                IniFile init = new IniFile(fileName, debug);
                
                LoadIniFile(fileName, debug);
                WaitHandle.WaitAll(doneEvents);

                if (debug.isValid) debug.log("[Program] \tFinish at " + DateTime.Now);
                debug.close();

                nextCheck = maxExecTime.AddMinutes(init.RefreshTime);

                int now = DateTime.Now.Minute * 60 + DateTime.Now.Second;
                int sleep = ((nextCheck.Minute * 60 + nextCheck.Second) - now);

                do
                {
                    Thread.Sleep(1000);
                    if (_isActive == false)
                        break;
                    sleep--;
                }
                while(sleep > 0);

                /*
                if (sleep > 0)
                {
                    Thread.Sleep(sleep * 1000);
                }*/

                lastExecTime = maxExecTime;
                maxExecTime = DateTime.Now;
            }
            while (_isActive);
        }

        /// <summary>
        /// Create an IniFile object with INI file configuration
        /// </summary>
        /// <param name="fileName">Name of INI file</param>
        /// <param name="debug">Debug objet instance</param>
        private static void LoadIniFile(String fileName, Debug debug)
        {
            configurationFile = new IniFile(fileName, debug);
            syslog = configurationFile.getSyslog();
            eventLogNames = configurationFile.getEventLogNames();
            doneEvents = new ManualResetEvent[eventLogNames.Count];

            if (debug.isValid)
            {
                debug.log("");
                debug.log("[Program] \tStart at " + DateTime.Now);
            }

            try
            {
                int i = 0;
                foreach (String eventLog in eventLogNames)
                {
                    try
                    {
                        rulesList = configurationFile.getRulesList(eventLog);
                        ThreadFilter[] filterArray = new ThreadFilter[eventLogNames.Count];
                        
                        doneEvents[i] = new ManualResetEvent(false);
                        ThreadFilter tf = new ThreadFilter(eventLog, syslog, rulesList, debug, lastExecTime, maxExecTime, doneEvents[i]);
                        filterArray[i] = tf;

                        ThreadPool.QueueUserWorkItem(tf.ThreadLoop, i);
                    }
                    catch (System.NullReferenceException e)
                    {
                        if (debug.isValid) debug.log("[ERROR]\t\tProblem during loading " + eventLog + "package rules");
                    }
                    i++;
                }
            }
            catch (System.NullReferenceException e)
            {
                if (debug.isValid) debug.log("[ERROR]\t\tAll rules are wrong, program abort");
                    System.Environment.Exit(-1);
            }
        }

        /// <summary>
        /// Test if exist process exist
        /// </summary>
        /// <returns>True if process exist</returns>
        private static bool ProcessAlreadyExist() 
	    { 
		    Process currentProcess = Process.GetCurrentProcess(); 
    		
		    foreach (Process p in Process.GetProcesses()) 
		    { 
			    if (p.Id != currentProcess.Id && p.ProcessName.Equals(currentProcess.ProcessName) == true) 
				    return true; 
		    } 
		    return false; 
	    }

        /// <summary>
        /// Get or set isActive value
        /// If value is false, principal process run by Start method finish
        /// </summary>
        public Boolean isActive
        {
            get
            {
                return _isActive;
            }

            set
            {
                _isActive = value;
            }
        }
    }
}
