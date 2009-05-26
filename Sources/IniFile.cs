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
using System.IO;
using System.Text;
using System.Collections; 

namespace centreon_eventLog_syslog
{
    class IniFile
    {
        private string sFileName;
        private SyslogServer _Syslog = null;
        private ArrayList _RulesList = null;
        private ArrayList _EventLogsNames = null;
        private Hashtable _ArrayRulesLists = new Hashtable();
        private int _RefreshTime = 1;
   
        /// <summary>
        /// Create a new instance of IniFile and load it
        /// </summary>
        /// <param name="fileName">Name and path of ini file</param>
        public IniFile(string fileName, Debug debug)
        {
            sFileName = fileName;
            String exepath = Environment.GetCommandLineArgs()[0];
            String exedir = exepath.Substring(0, exepath.LastIndexOf('\\'));
            fileName = exedir + "\\" + fileName;

            if (File.Exists(fileName))
                Load(fileName, debug);
        } 

        /// <summary>
        /// Charge un fichier INI
        /// </summary>
        /// <param name="fileName">Nom du fichier à charger</param>
        /// <param name="debug">Debug objet instance</param>
        private void Load(string fileName, Debug debug)
        {
            Boolean isSyslog = false;
            Boolean isRule = false;
            Boolean isProgramOptions = false;

            Rule rule = null;
            StreamReader str = new StreamReader(File.Open(fileName,FileMode.Open));

            string fichier = str.ReadToEnd();

            string[] lignes = fichier.Split('\r','\n');

            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i];


                if (ligne.StartsWith("[") && ligne.EndsWith("]"))
                {
                    if (ligne.CompareTo("[Syslog]") == 0)
                    {
                        _Syslog = new SyslogServer();
                        isSyslog = true;
                        isProgramOptions = false;
                        isRule = false;
                    }
                    else if ((ligne.CompareTo("[Rule]") == 0) || (ligne.CompareTo("[LastRule]") == 0))
                    {
                        // Access to get rule's options
                        isRule = true;

                        // Create a rule object
                        if(rule == null) 
                            rule = new Rule();

                        if (ligne.CompareTo("[LastRule]") == 0)
                        {
                            isProgramOptions = false;
                            isSyslog = false;
                            isRule = false;
                        }

                        // object was not empty
                        if ((rule.isEmpty != true) && (rule.EventLogName == null))
                        {
                            if (debug.isValid)
                            {
                                debug.log("[ERROR]\t\tThe following rule must have \'EventLogName\' parameter fieled");
                                debug.log("[ERROR]\t\t" + rule.ToString());
                            }
                        }
                        else if ((rule.isEmpty != true) && (rule.EventLogName != null))
                        {
                            if (_ArrayRulesLists[rule.EventLogName] != null)
                            {
                                _RulesList = (ArrayList)_ArrayRulesLists[rule.EventLogName];
                                _RulesList.Add(rule);
                                _ArrayRulesLists.Remove(rule.EventLogName);
                                _ArrayRulesLists.Add(rule.EventLogName, _RulesList);
                                rule = new Rule();
                            }
                            else
                            {
                                _RulesList = new ArrayList();
                                _RulesList.Add(rule);
                                _ArrayRulesLists.Add(rule.EventLogName, _RulesList);
                                rule = new Rule();
                            }
                        }
                        else
                        {
                            rule = new Rule();
                        }
                    }
                    else if (ligne.CompareTo("[Program]") == 0)
                    {
                        isProgramOptions = true;
                    }
                }
                else if ((ligne != "") && !(ligne.StartsWith(";")))
                {
                    if (isSyslog)
                    {
                        if (ligne.Contains("Server"))
                        {
                            String[] value = ligne.Split('=');
                            _Syslog.Server = value[1];
                        }
                        else if (ligne.Contains("Protocol"))
                        {
                            String[] value = ligne.Split('=');
                            _Syslog.Protocol = value[1];
                        }
                        else if (ligne.Contains("Port"))
                        {
                            String[] value = ligne.Split('=');
                            try
                            {
                                _Syslog.Port = Convert.ToInt16(value[1]);
                            }
                            catch (System.Exception e)
                            {
                                if (debug.isValid)
                                {
                                    debug.log("[ERROR]\tUnable to load syslog port in general section");
                                }
                            }

                        }

                        if (_Syslog.isValid())
                        {
                            isSyslog = false;

                        }
                    }
                    else if (isProgramOptions)
                    {
                        if (ligne.Contains("Debug"))
                        {
                            String[] value = ligne.Split('=');
                            if (value[1].CompareTo("0") == 0)
                                debug.isValid = false;
                            else
                            {
                                debug.isValid = true;
                            }
                        }
                        else if (ligne.Contains("RefreshInterval"))
                        {
                            String[] value = ligne.Split('=');
                            try
                            {
                                _RefreshTime = Convert.ToInt16(value[1]);
                            }
                            catch (System.Exception e)
                            {
                                _RefreshTime = 1;
                            }
                            isProgramOptions = false;
                        }
                    }
                    else if (isRule)
                    {
                        if (ligne.Contains("EventLogName"))
                        {
                            String[] value = ligne.Split('=');
                            rule.EventLogName = value[1];

                            if (this._EventLogsNames == null)
                                this._EventLogsNames = new ArrayList();
                            if(this._EventLogsNames.Contains(rule.EventLogName) == false)
                                this._EventLogsNames.Add(rule.EventLogName);
                        }
                        else if (ligne.Contains("EventLogSources"))
                        {
                            String[] Gvalue = ligne.Split('=');
                            String[] values = Gvalue[1].Split(',');
                            rule.EventLogSources = values;
                        }
                        else if (ligne.Contains("EventLogID"))
                        {
                            String[] Gvalue = ligne.Split('=');
                            String[] values = Gvalue[1].Split(',');
                            rule.EventLogID = values;
                        }
                        else if (ligne.Contains("User"))
                        {
                            String[] Gvalue = ligne.Split('=');
                            String[] values = Gvalue[1].Split(',');
                            rule.User = values;
                        }
                        else if (ligne.Contains("Computer"))
                        {
                            String[] Gvalue = ligne.Split('=');
                            String[] values = Gvalue[1].Split(',');
                            rule.Computer = values;
                        }
                        else if (ligne.Contains("EventLogType"))
                        {
                            String[] Gvalue = ligne.Split('=');
                            String[] values = Gvalue[1].Split(',');
                            rule.EventLogType = values;
                        }
                        else if (ligne.Contains("EventLogDescriptions"))
                        {
                            String[] Gvalue = ligne.Split('=');
                            String[] values = Gvalue[1].Split(',');
                            rule.EventLogDescriptions = values;
                        }
                        else if (ligne.Contains("SyslogLevel"))
                        {
                            String[] value = ligne.Split('=');
                            rule.SyslogLevel = value[1];
                        }
                        else if (ligne.Contains("SyslogFacility"))
                        {
                            String[] value = ligne.Split('=');
                            rule.SyslogFacility = value[1];
                        }
                    }

                }
            }
            str.Close();
        }

        /// <summary>
        /// Get SyslogServer object
        /// </summary>
        /// <returns>SyslogServer serveur object loaded in INI file</returns>
        public SyslogServer getSyslog()
        {
            return this._Syslog;

        }

        /// <summary>
        /// Get list of Rules contained in INI file
        /// </summary>
        /// <param name="eventLogName">Name of eventLog</param>
        /// <returns>List of rules</returns>
        public ArrayList getRulesList(String eventLogName)
        {
            return (ArrayList)this._ArrayRulesLists[eventLogName];
        }

        /// <summary>
        /// Get list of eventLog name contained in rules
        /// </summary>
        /// <returns>List of eventLog names</returns>
        public ArrayList getEventLogNames()
        {
            return this._EventLogsNames;
        }

        /// <summary>
        /// Get Refresh time for main process
        /// </summary>
        public int RefreshTime
        {
            get 
            {
                return _RefreshTime;
            }
        }
    }
}
