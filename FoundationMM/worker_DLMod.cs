﻿using SharpSvn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace FoundationMM
{
    public partial class Window : Form
    {
        private void dlModWorkerStarter_DoWork(object sender, DoWorkEventArgs e)
        {
            List<ListViewItem> mods = (List<ListViewItem>)e.Argument;

            foreach (ListViewItem item in mods)
            {
                tabControl1.Invoke((MethodInvoker)delegate { tabControl1.Enabled = false; });
                string remLocation = "https://github.com/Clef-0/FMM-Mods/trunk/" + item.SubItems[5].Text;
                Debug.WriteLine(remLocation);
                string locLocation = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "mods", "tagmods", item.SubItems[5].Text.Replace("/", "\\"));
                Debug.WriteLine(locLocation);
                dlModWorker.RunWorkerAsync(new string[] { remLocation, locLocation });
                do {
                    Thread.Sleep(100);
                } while (dlModWorker.IsBusy);
            }
            tabControl1.Invoke((MethodInvoker)delegate { tabControl1.Enabled = true; });
        }

        private void dlModWorkerStarter_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Selected mods downloaded.\nRefresh your \"My Mods\" window.");
            tabControl1.Invoke((MethodInvoker)delegate { tabControl1.Enabled = true; });
        }

        private void dlModWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            SvnClient client = new SvnClient();

            string[] args = (string[])e.Argument;
            string remLocation = args[0];
            string locLocation = args[1];
            remLocation = remLocation.Replace(System.IO.Path.GetFileName(new Uri(remLocation).LocalPath), "");

            locLocation = Path.GetDirectoryName(locLocation);
            Debug.WriteLine(remLocation + "  &&  " + locLocation);

            BackgroundWorker worker = sender as BackgroundWorker;
            if (Directory.Exists(locLocation))
            {
                Directory.Delete(locLocation, true);
            }
            Directory.CreateDirectory(locLocation);
            if (Directory.Exists(Path.Combine(locLocation, ".svn")))
            {
                client.CleanUp(locLocation);
            }
                percentageLabel.Text = "Download in progress...";
                client.CheckOut(new Uri(remLocation), locLocation);

            //exeProcessDirectory(locLocation);
        }

        private void exeProcessDirectory(string targetDirectory)
        {
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                exeProcessFile(fileName);

            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                exeProcessDirectory(subdirectory);
        }

        private void exeProcessFile(string path)
        {
            if (Path.GetExtension(path) == ".exe")
            {
                locatedFMMInstallers.Add(path);
            }
        }

        private void dlModWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!(e.Error == null))
            {
                percentageLabel.Text = ("Error: " + e.Error.Message);
            }
            else
            {
                percentageLabel.Text = "";
            }
        }
    }
}
