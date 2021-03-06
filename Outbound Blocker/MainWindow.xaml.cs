﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using NetFwTypeLib;
using System.Diagnostics;

namespace Yep
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int createdRules = 0;
        string blockedEXE;
        private void deleteRule(string rulename)
        {
            Process p = new Process();
            p.StartInfo.FileName = "netsh.exe";
            p.StartInfo.Arguments = string.Format("advfirewall firewall delete rule name=\"{0}\"", rulename);
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            // I opted to use netsh for the deletion of the rule because it allows for deletion of rules with simply their names, which the program controls
            //(and I failed to find documentation regarding the deletion of rules with the Windows Firewall API)
        }
        private void createRule(string appPath)
        {
            if (string.IsNullOrEmpty(appPath) != true)
            {

                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
                    Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                INetFwRule firewallRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
                firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
                firewallRule.Description = String.Format("Block {0}", appPath);
                firewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
                firewallRule.ApplicationName = @appPath;
                firewallRule.Enabled = true;
                firewallRule.InterfaceTypes = "All";
                firewallRule.Name = appPath;
                // I opted to use the Windows Firewall API for creating the rule because it allows one to create (unattractively named, though easily identifiable) rules without
                // requiring information beyond the location of the program that needs its outbound connection blocked, which is retrieved from the OpenFileDialog and assigned to blockedEXE
                firewallPolicy.Rules.Add(firewallRule);
            }
        }
        public MainWindow()
        {
            InitializeComponent();
        }

        private void dialogLauncher_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Executable Files (*.exe)|*.exe";
            if (openFileDialog.ShowDialog() == true)
                selectedPath.Text = openFileDialog.FileName;
            blockedEXE = selectedPath.Text;
        }

        private void blockButton_Click(object sender, RoutedEventArgs e)
        {
            if (createdRules == 0)
            {
                createRule(blockedEXE);  
                createdRules = 1;
            }
        }

        private void unblockButton_Click(object sender, RoutedEventArgs e)
        {
            deleteRule(blockedEXE);
            createdRules = 0;
        }

        private void selectedPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            blockedEXE = selectedPath.Text;
        }
    }
}
