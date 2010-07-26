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
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Centreon_EventLog_2_Syslog
{
    [RunInstaller(true)]
    public class MyProjectInstaller : Installer
    {
        public MyProjectInstaller()
            : base()
        {
            this.Committed += new InstallEventHandler(MyProjectInstaller_Committed);
            this.Committing += new InstallEventHandler(MyProjectInstaller_Committing);

            ServiceInstaller myService = new ServiceInstaller();
            ServiceProcessInstaller monInstallProcess = new ServiceProcessInstaller();

            myService.ServiceName = "Centreon-E2S";
            myService.DisplayName = "Centreon EventLog to Syslog";
            myService.Description = "This service course with regular interval the Windows events in order to seek events corresponding to the rules described by user. So correspondence are found, then the events are translated in syslog format for then being sent to a syslog server.";

            this.Installers.Add(myService);

            monInstallProcess.Account = System.ServiceProcess.ServiceAccount.LocalSystem;

            myService.StartType = ServiceStartMode.Automatic;

            this.Installers.Add(monInstallProcess);
        }

        private void MyProjectInstaller_Committing(object sender, InstallEventArgs e)
        {
            Console.WriteLine("");
            Console.WriteLine("Committing Event occured.");
            Console.WriteLine("");
        }

        private void MyProjectInstaller_Committed(object sender, InstallEventArgs e)
        {
            Console.WriteLine("");
            Console.WriteLine("Committed Event occured.");
            Console.WriteLine("");
        }

        public override void Install(IDictionary savedState)
        {
            base.Install(savedState);
        }

        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);
        }

        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
        }
    }
}

