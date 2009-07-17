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

namespace centreon_eventLog_syslog
{
    class Debug
    {
        private Boolean _isValid;
        private Boolean _isActive;
        private String _FileName = "debug.log";
        private static StreamWriter stw;

        /// <summary>
        /// Simple constructor
        /// </summary>
        public Debug()
        {
            String exepath = Environment.GetCommandLineArgs()[0];
            String exedir = exepath.Substring(0, exepath.LastIndexOf('\\'));
            _FileName = exedir + "\\" + _FileName;

            _isValid = false;
            _isActive = false;

           stw = new StreamWriter(File.Open(_FileName, FileMode.Append));
        }

        /// <summary>
        /// Active or not program debug
        /// </summary>
        public Boolean isValid
        {
            get
            {
                return _isValid;
            }
            set
            {
                _isValid = value;
            }
        }

        /// <summary>
        /// defined if an application wrtie some informations in log file
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

        /// <summary>
        /// Log message into debug file
        /// </summary>
        /// <param name="value">messge to log</param>
        public void log(String value)
        {
            _isActive = true;
            lock (this)
            {
                stw.WriteLine(value);
            }
            _isActive = false;
            
        }

        /// <summary>
        /// Close StreamWriter
        /// </summary>
        public void close()
        {
            stw.Close();
        }
    }
}
