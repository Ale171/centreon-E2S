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

namespace Centreon_EventLog_2_Syslog
{
    class Filter
    {
        private string[] _EventLogSources = null;
        private string[] _EventLogID = null;
        private string[] _User = null;
        private string[] _Computer = null;
        private string[] _EventLogType = null;
        private string[] _EventLogDescriptions = null;
        private string _EventLogName = null;
        private string _SyslogLevel = null;
        private string _SyslogFacility = null;
        private Boolean _IsEmpty = true;

        /// <summary>
        /// Get or set EventLog Sources
        /// </summary>
        public string[] EventLogSources
        {
            get
            {
                return _EventLogSources;
            }
            set
            {
                this._EventLogSources = value;
                if (value.ToString().CompareTo("*") != 0)
                {
                    this._IsEmpty = false;
                }
            }
        }

        /// <summary>
        /// Get or set EventLog IDs
        /// </summary>
        public string[] EventLogID
        {
            get
            {
                return _EventLogID;
            }
            set
            {
                this._EventLogID = value;
                if (value.ToString().CompareTo("*") != 0)
                {
                    this._IsEmpty = false;
                }
            }
        }

        /// <summary>
        /// Get or set EventLog User
        /// </summary>
        public string[] User
        {
            get
            {
                return _User;
            }
            set
            {
                this._User = value;
                if (value.ToString().CompareTo("*") != 0)
                {
                    this._IsEmpty = false;
                }
            }
        }

        /// <summary>
        /// Get or set EventLog computer that generate event
        /// </summary>
        public string[] Computer
        {
            get
            {
                return _Computer;
            }
            set
            {
                this._Computer = value;
                if (value.ToString().CompareTo("*") != 0)
                {
                    this._IsEmpty = false;
                }
            }
        }

        /// <summary>
        /// Get or set EventLog type
        /// </summary>
        public string[] EventLogType
        {
            get
            {
                return _EventLogType;
            }
            set
            {
                this._EventLogType = value;
                if (value.ToString().CompareTo("*") != 0)
                {
                    this._IsEmpty = false;
                }
            }
        }

        /// <summary>
        /// Get or set EventLog Descriptions
        /// </summary>
        public string[] EventLogDescriptions
        {
            get
            {
                return _EventLogDescriptions;
            }
            set
            {
                this._EventLogDescriptions = value;
                if (value.ToString().CompareTo("*") != 0)
                {
                    this._IsEmpty = false;
                }
            }
        }

        /// <summary>
        /// Get or set EventLog name
        /// </summary>
        public string EventLogName
        {
            get
            {
                return _EventLogName;
            }
            set
            {
                this._EventLogName = value;
                if (value.ToString().CompareTo("*") != 0)
                {
                    this._IsEmpty = false;
                }
            }
        }

        /// <summary>
        /// Get or set syslog level for forward eventLog
        /// </summary>
        public string SyslogLevel
        {
            get
            {
                return _SyslogLevel;
            }
            set
            {
                this._SyslogLevel = value;
                if ((value.ToString().CompareTo("") == 0) || (value.ToString().CompareTo("*") == 0))
                {
                    this._IsEmpty = true;
                }
            }
        }

        /// <summary>
        /// Get or set syslog facility for forward eventLog
        /// </summary>
        public string SyslogFacility
        {
            get
            {
                return _SyslogFacility;
            }
            set
            {
                this._SyslogFacility = value;
                if ((value.ToString().CompareTo("") == 0) || (value.ToString().CompareTo("*") == 0))
                {
                    this._IsEmpty = true;
                }
            }
        }

        /// <summary>
        /// Print all infomration not null
        /// </summary>
        /// <returns>String with all informations</returns>
        override public String ToString()
        {
            String syslogMessage = "";

            if (_EventLogSources != null)
            {
                syslogMessage = syslogMessage + "EventLogSources: ";
                foreach (String value in _EventLogSources)
                {
                    syslogMessage = syslogMessage + value + ", ";
                }
            }
            if (_EventLogID != null)
            {
                syslogMessage = syslogMessage + "EventLogID: ";
                foreach (String value in _EventLogID)
                {
                    syslogMessage = syslogMessage + value + ", ";
                }
            }
            if (_User != null)
            {
                syslogMessage = syslogMessage + "User: ";
                foreach (String value in _User)
                {
                    syslogMessage = syslogMessage + value + ", ";
                }
            }
            if (_Computer != null)
            {
                syslogMessage = syslogMessage + "Computer: ";
                foreach (String value in _Computer)
                {
                    syslogMessage = syslogMessage + value + ", ";
                }
            }
            if (_EventLogType != null)
            {
                syslogMessage = syslogMessage + "EventLogType: ";
                foreach (String value in _EventLogType)
                {
                    syslogMessage = syslogMessage + value + ", ";
                }
            }
            if (_EventLogDescriptions != null)
            {
                syslogMessage = syslogMessage + "EventLogDescriptions: ";
                foreach (String value in EventLogDescriptions)
                {
                    syslogMessage = syslogMessage + value + ", ";
                }
            }
            if (_EventLogName != null)
                syslogMessage = syslogMessage + "EventLogName: " + _EventLogName + ", ";

            if (_SyslogLevel != null)
                syslogMessage = syslogMessage + "SyslogLevel: " + _SyslogLevel + ", ";

            if (_SyslogFacility != null)
                syslogMessage = syslogMessage + "SyslogFacility: " + _SyslogFacility + ", ";

            return syslogMessage;
        }

        /// <summary>
        /// To inform if at least one filter exists
        /// </summary>
        /// <returns>False if element permit to filter events</returns>
        public Boolean IsEmpty()
        {
            return this._IsEmpty;
        }
    }
}
